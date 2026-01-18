using LanWatcher.Models;

namespace LanWatcher.Services;

public interface IScanService
{
    ScanProgress Progress { get; }
    bool IsScanning { get; }
    List<NetworkDevice> Devices { get; }
    ScanOptions Options { get; set; }
    event EventHandler<ScanProgress>? OnProgressChanged;
    event EventHandler<NetworkDevice>? OnDeviceFound;
    event EventHandler<List<NetworkDevice>>? OnScanComplete;
    Task StartScanAsync();
    Task RefreshKnownDevicesAsync();
    void StopScan();
    string GetLocalIpRange();
}

public class ScanService : IScanService
{
    private readonly INetworkScanner _scanner;
    private readonly IDeviceRepository _repository;
    private readonly ILogger<ScanService> _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    public ScanProgress Progress { get; private set; } = new();
    public bool IsScanning => Progress.IsScanning;
    public List<NetworkDevice> Devices => _repository.GetAllDevices();
    public ScanOptions Options { get; set; }

    public event EventHandler<ScanProgress>? OnProgressChanged;
    public event EventHandler<NetworkDevice>? OnDeviceFound;
    public event EventHandler<List<NetworkDevice>>? OnScanComplete;

    public ScanService(INetworkScanner scanner, IDeviceRepository repository, ILogger<ScanService> logger)
    {
        _scanner = scanner;
        _repository = repository;
        _logger = logger;

        Options = new ScanOptions
        {
            IpRange = _scanner.GetLocalIpRange()
        };

        _scanner.OnProgressChanged += (s, p) =>
        {
            Progress = p;
            OnProgressChanged?.Invoke(this, p);
        };

        _scanner.OnDeviceFound += (s, d) =>
        {
            _repository.AddOrUpdateDevice(d);
            OnDeviceFound?.Invoke(this, d);
        };
    }

    public string GetLocalIpRange()
    {
        return _scanner.GetLocalIpRange();
    }

    public async Task StartScanAsync()
    {
        if (IsScanning)
        {
            _logger.LogWarning("Scan already in progress");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        // Don't clear devices - we want to merge with existing data

        Progress = new ScanProgress
        {
            IsScanning = true,
            StartTime = DateTime.Now
        };
        OnProgressChanged?.Invoke(this, Progress);

        try
        {
            _logger.LogInformation("Starting network scan with options: {Options}", 
                System.Text.Json.JsonSerializer.Serialize(Options));

            var devices = await _scanner.ScanNetworkAsync(Options, _cancellationTokenSource.Token);
            _repository.UpdateDevices(devices);

            OnScanComplete?.Invoke(this, _repository.GetAllDevices());
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Scan was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during network scan");
        }
        finally
        {
            Progress.IsScanning = false;
            OnProgressChanged?.Invoke(this, Progress);
        }
    }

    public void StopScan()
    {
        _cancellationTokenSource?.Cancel();
        _logger.LogInformation("Scan stop requested");
    }

    public async Task RefreshKnownDevicesAsync()
    {
        if (IsScanning)
        {
            _logger.LogWarning("Scan already in progress");
            return;
        }

        var knownDevices = _repository.GetAllDevices();
        if (!knownDevices.Any())
        {
            _logger.LogInformation("No known devices to refresh");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();

        Progress = new ScanProgress
        {
            IsScanning = true,
            StartTime = DateTime.Now,
            TotalAddresses = knownDevices.Count
        };
        OnProgressChanged?.Invoke(this, Progress);

        try
        {
            _logger.LogInformation("Refreshing {Count} known devices", knownDevices.Count);

            var refreshedDevices = new List<NetworkDevice>();
            
            foreach (var device in knownDevices)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                Progress.CurrentIp = device.IpAddress;
                Progress.CurrentAction = $"Checking {device.IpAddress}...";
                OnProgressChanged?.Invoke(this, Progress);

                var refreshed = await _scanner.ScanDeviceAsync(device.IpAddress, Options, _cancellationTokenSource.Token);
                
                if (refreshed.IsOnline)
                {
                    refreshedDevices.Add(refreshed);
                    _repository.AddOrUpdateDevice(refreshed);
                    OnDeviceFound?.Invoke(this, refreshed);
                }
                else
                {
                    // Mark device as offline
                    device.IsOnline = false;
                    _repository.AddOrUpdateDevice(device);
                }

                Progress.IncrementScanned();
                OnProgressChanged?.Invoke(this, Progress);
            }

            OnScanComplete?.Invoke(this, _repository.GetAllDevices());
            _logger.LogInformation("Refresh complete. {Online} of {Total} devices online", 
                refreshedDevices.Count, knownDevices.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Refresh was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during device refresh");
        }
        finally
        {
            Progress.IsScanning = false;
            OnProgressChanged?.Invoke(this, Progress);
        }
    }
}
