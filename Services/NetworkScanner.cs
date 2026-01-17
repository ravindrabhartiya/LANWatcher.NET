using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using LanWatcher.Models;

namespace LanWatcher.Services;

public interface INetworkScanner
{
    event EventHandler<ScanProgress>? OnProgressChanged;
    event EventHandler<NetworkDevice>? OnDeviceFound;
    Task<List<NetworkDevice>> ScanNetworkAsync(ScanOptions options, CancellationToken cancellationToken = default);
    Task<NetworkDevice> ScanDeviceAsync(string ipAddress, ScanOptions options, CancellationToken cancellationToken = default);
    string GetLocalIpRange();
}

public class NetworkScanner : INetworkScanner
{
    private readonly ILogger<NetworkScanner> _logger;
    private SemaphoreSlim _semaphore = null!;
    private ScanProgress _progress = new();

    public event EventHandler<ScanProgress>? OnProgressChanged;
    public event EventHandler<NetworkDevice>? OnDeviceFound;

    public NetworkScanner(ILogger<NetworkScanner> logger)
    {
        _logger = logger;
    }

    public string GetLocalIpRange()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    var parts = ip.ToString().Split('.');
                    if (parts.Length >= 3)
                    {
                        return $"{parts[0]}.{parts[1]}.{parts[2]}";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get local IP range");
        }
        return "192.168.1";
    }

    public async Task<List<NetworkDevice>> ScanNetworkAsync(ScanOptions options, CancellationToken cancellationToken = default)
    {
        var devices = new List<NetworkDevice>();
        _progress = new ScanProgress
        {
            TotalAddresses = options.EndAddress - options.StartAddress + 1,
            IsScanning = true,
            StartTime = DateTime.Now
        };

        // Create a fresh semaphore for each scan with the configured parallelism
        _semaphore = new SemaphoreSlim(options.MaxParallelScans, options.MaxParallelScans);

        _logger.LogInformation("Starting network scan on {IpRange}.{Start}-{End}", 
            options.IpRange, options.StartAddress, options.EndAddress);

        UpdateProgress("Initializing scan...");

        var tasks = new List<Task<NetworkDevice?>>();

        for (int i = options.StartAddress; i <= options.EndAddress; i++)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var ipAddress = $"{options.IpRange}.{i}";
            tasks.Add(ScanSingleHostAsync(ipAddress, options, cancellationToken));
        }

        // Process results as they complete using Task.WhenAll for maximum parallelism
        var results = await Task.WhenAll(tasks);
        
        foreach (var device in results)
        {
            if (device != null && device.IsOnline)
            {
                devices.Add(device);
            }
        }

        _progress.IsScanning = false;
        UpdateProgress("Scan complete!");

        _logger.LogInformation("Scan complete. Found {Count} devices", devices.Count);
        return devices;
    }

    private async Task<NetworkDevice?> ScanSingleHostAsync(string ipAddress, ScanOptions options, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var device = await ScanDeviceAsync(ipAddress, options, cancellationToken);
            
            _progress.IncrementScanned();
            _progress.CurrentIp = ipAddress;
            
            if (device.IsOnline)
            {
                _progress.IncrementDevicesFound();
                OnDeviceFound?.Invoke(this, device);
                UpdateProgress($"Found device at {ipAddress}");
            }
            else
            {
                UpdateProgress($"Scanning {ipAddress}...");
            }

            return device;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<NetworkDevice> ScanDeviceAsync(string ipAddress, ScanOptions options, CancellationToken cancellationToken = default)
    {
        var device = new NetworkDevice
        {
            IpAddress = ipAddress,
            LastSeen = DateTime.Now
        };

        try
        {
            // Ping the device
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ipAddress, options.PingTimeout);

            if (reply.Status == IPStatus.Success)
            {
                device.IsOnline = true;
                device.ResponseTime = (int)reply.RoundtripTime;
                device.LastSeen = DateTime.Now;

                // Try to resolve hostname
                try
                {
                    var hostEntry = await Dns.GetHostEntryAsync(ipAddress);
                    device.HostName = hostEntry.HostName;
                }
                catch
                {
                    device.HostName = "Unknown";
                }

                // Scan ports if enabled
                if (options.ScanPorts)
                {
                    var portsToScan = options.CustomPorts.Count > 0 
                        ? options.CustomPorts.ToArray()
                        : options.QuickScan 
                            ? PortDefinitions.CommonPorts 
                            : PortDefinitions.ExtendedPorts;

                    device.OpenPorts = await ScanPortsAsync(ipAddress, portsToScan, options.PortTimeout, cancellationToken);
                    _progress.AddPortsScanned(portsToScan.Length);
                }

                // Detect device type
                device.DeviceType = DetectDeviceType(device);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error scanning {IpAddress}", ipAddress);
        }

        return device;
    }

    private async Task<List<PortInfo>> ScanPortsAsync(string ipAddress, int[] ports, int timeout, CancellationToken cancellationToken)
    {
        var openPorts = new List<PortInfo>();
        var portTasks = ports.Select(port => ScanPortAsync(ipAddress, port, timeout, cancellationToken));
        var results = await Task.WhenAll(portTasks);
        
        openPorts.AddRange(results.Where(p => p.IsOpen));
        return openPorts;
    }

    private async Task<PortInfo> ScanPortAsync(string ipAddress, int port, int timeout, CancellationToken cancellationToken)
    {
        var portInfo = new PortInfo
        {
            Port = port,
            ServiceName = PortDefinitions.GetServiceName(port),
            IsOpen = false
        };

        try
        {
            using var client = new TcpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            await client.ConnectAsync(ipAddress, port, cts.Token);
            portInfo.IsOpen = true;

            // Try to grab banner for certain services
            if (port == 80 || port == 8080 || port == 8000)
            {
                portInfo.Banner = await TryGetHttpBannerAsync(client, ipAddress);
            }
            else if (port == 22)
            {
                portInfo.Banner = await TryGetBannerAsync(client);
            }
        }
        catch
        {
            // Port is closed or filtered
        }

        return portInfo;
    }

    private async Task<string> TryGetBannerAsync(TcpClient client)
    {
        try
        {
            client.ReceiveTimeout = 500;
            var stream = client.GetStream();
            var buffer = new byte[256];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    private async Task<string> TryGetHttpBannerAsync(TcpClient client, string ipAddress)
    {
        try
        {
            var stream = client.GetStream();
            var request = $"HEAD / HTTP/1.1\r\nHost: {ipAddress}\r\nConnection: close\r\n\r\n";
            var requestBytes = Encoding.ASCII.GetBytes(request);
            await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            // Extract Server header
            var lines = response.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("Server:", StringComparison.OrdinalIgnoreCase))
                {
                    return line.Substring(7).Trim();
                }
            }
        }
        catch
        {
            // Ignore banner grab failures
        }
        return string.Empty;
    }

    private DeviceType DetectDeviceType(NetworkDevice device)
    {
        var openPortNumbers = device.OpenPorts.Select(p => p.Port).ToHashSet();

        // Check for specific device signatures
        if (openPortNumbers.Contains(9100) || openPortNumbers.Contains(515) || openPortNumbers.Contains(631))
            return DeviceType.Printer;

        if (openPortNumbers.Contains(554) && (openPortNumbers.Contains(80) || openPortNumbers.Contains(8080)))
            return DeviceType.Camera;

        if (openPortNumbers.Contains(32400) || openPortNumbers.Contains(8096))
            return DeviceType.MediaServer;

        if (openPortNumbers.Contains(1883) || openPortNumbers.Contains(8883))
            return DeviceType.SmartHome;

        if (openPortNumbers.Contains(3306) || openPortNumbers.Contains(5432) || 
            openPortNumbers.Contains(1433) || openPortNumbers.Contains(27017))
            return DeviceType.DatabaseServer;

        if (openPortNumbers.Contains(25) || openPortNumbers.Contains(465) || 
            openPortNumbers.Contains(587) || openPortNumbers.Contains(143))
            return DeviceType.MailServer;

        if (openPortNumbers.Contains(445) && openPortNumbers.Contains(139))
            return DeviceType.FileServer;

        if (openPortNumbers.Contains(3389) || openPortNumbers.Contains(5900))
            return DeviceType.Computer;

        if (openPortNumbers.Contains(80) && openPortNumbers.Contains(53))
            return DeviceType.Router;

        if (openPortNumbers.Contains(62078) || openPortNumbers.Contains(5353))
            return DeviceType.Phone;

        if (openPortNumbers.Contains(8008) && openPortNumbers.Contains(8443))
            return DeviceType.SmartTV;

        if (openPortNumbers.Contains(80) || openPortNumbers.Contains(443) || openPortNumbers.Contains(8080))
            return DeviceType.WebServer;

        return DeviceType.Unknown;
    }

    private void UpdateProgress(string message)
    {
        _progress.CurrentAction = message;
        OnProgressChanged?.Invoke(this, _progress);
    }
}
