using System.Text.Json;
using LanWatcher.Models;

namespace LanWatcher.Services;

public interface IDeviceRepository
{
    List<NetworkDevice> GetAllDevices();
    void AddOrUpdateDevice(NetworkDevice device);
    void ClearDevices();
    NetworkDevice? GetDevice(string ipAddress);
    void UpdateDevices(List<NetworkDevice> devices);
    void MergeDevices(List<NetworkDevice> devices);
    Task LoadFromFileAsync();
    Task SaveToFileAsync();
    event EventHandler<List<NetworkDevice>>? OnDevicesChanged;
}

public class DeviceRepository : IDeviceRepository
{
    private readonly Dictionary<string, NetworkDevice> _devices = new();
    private readonly object _lock = new();
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly ILogger<DeviceRepository> _logger;
    private readonly string _dataFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _saveScheduled = false;

    public event EventHandler<List<NetworkDevice>>? OnDevicesChanged;

    public DeviceRepository(ILogger<DeviceRepository> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _dataFilePath = Path.Combine(environment.ContentRootPath, "discovered_devices.json");
        _jsonOptions = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Load devices from file on startup
        _ = LoadFromFileAsync();
    }

    public async Task LoadFromFileAsync()
    {
        try
        {
            if (File.Exists(_dataFilePath))
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                var savedData = JsonSerializer.Deserialize<DeviceDataFile>(json, _jsonOptions);
                
                if (savedData?.Devices != null)
                {
                    lock (_lock)
                    {
                        foreach (var device in savedData.Devices)
                        {
                            // Mark previously discovered devices as offline until rescanned
                            device.IsOnline = false;
                            _devices[device.IpAddress] = device;
                        }
                    }
                    
                    _logger.LogInformation("Loaded {Count} devices from {Path}", 
                        savedData.Devices.Count, _dataFilePath);
                    
                    OnDevicesChanged?.Invoke(this, GetAllDevices());
                }
            }
            else
            {
                _logger.LogInformation("No existing device data file found at {Path}", _dataFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading devices from file: {Path}", _dataFilePath);
        }
    }

    public async Task SaveToFileAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            List<NetworkDevice> devicesToSave;
            lock (_lock)
            {
                devicesToSave = _devices.Values.ToList();
            }

            var dataFile = new DeviceDataFile
            {
                LastUpdated = DateTime.Now,
                Devices = devicesToSave
            };

            var json = JsonSerializer.Serialize(dataFile, _jsonOptions);
            await File.WriteAllTextAsync(_dataFilePath, json);
            
            _logger.LogInformation("Saved {Count} devices to {Path}", devicesToSave.Count, _dataFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving devices to file: {Path}", _dataFilePath);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Schedule a save operation with debouncing to avoid too many file writes
    /// </summary>
    private void ScheduleSave()
    {
        if (_saveScheduled) return;
        _saveScheduled = true;
        
        _ = Task.Run(async () =>
        {
            await Task.Delay(500); // Debounce for 500ms
            _saveScheduled = false;
            await SaveToFileAsync();
        });
    }

    public List<NetworkDevice> GetAllDevices()
    {
        lock (_lock)
        {
            return _devices.Values.OrderBy(d => 
            {
                var parts = d.IpAddress.Split('.');
                if (parts.Length == 4)
                {
                    // Sort by 3rd octet first, then by 4th octet
                    if (int.TryParse(parts[2], out var third) && int.TryParse(parts[3], out var fourth))
                    {
                        return third * 1000 + fourth;
                    }
                }
                return 999999;
            }).ToList();
        }
    }

    public void AddOrUpdateDevice(NetworkDevice device)
    {
        lock (_lock)
        {
            if (_devices.TryGetValue(device.IpAddress, out var existingDevice))
            {
                // Preserve historical data, update with new scan results
                device.FirstDiscovered = existingDevice.FirstDiscovered;
                device.DiscoveryCount = existingDevice.DiscoveryCount + 1;
            }
            else
            {
                device.FirstDiscovered = DateTime.Now;
                device.DiscoveryCount = 1;
            }
            
            _devices[device.IpAddress] = device;
        }
        
        // Schedule debounced save to avoid file contention
        ScheduleSave();
        OnDevicesChanged?.Invoke(this, GetAllDevices());
    }

    public void ClearDevices()
    {
        lock (_lock)
        {
            _devices.Clear();
        }
        OnDevicesChanged?.Invoke(this, new List<NetworkDevice>());
    }

    public NetworkDevice? GetDevice(string ipAddress)
    {
        lock (_lock)
        {
            return _devices.TryGetValue(ipAddress, out var device) ? device : null;
        }
    }

    public void UpdateDevices(List<NetworkDevice> devices)
    {
        lock (_lock)
        {
            // Merge with existing devices rather than clearing
            foreach (var device in devices)
            {
                if (_devices.TryGetValue(device.IpAddress, out var existingDevice))
                {
                    device.FirstDiscovered = existingDevice.FirstDiscovered;
                    device.DiscoveryCount = existingDevice.DiscoveryCount + 1;
                }
                else
                {
                    device.FirstDiscovered = DateTime.Now;
                    device.DiscoveryCount = 1;
                }
                
                _devices[device.IpAddress] = device;
            }
            
            // Mark devices not found in this scan as offline
            var scannedIps = devices.Select(d => d.IpAddress).ToHashSet();
            foreach (var kvp in _devices)
            {
                if (!scannedIps.Contains(kvp.Key))
                {
                    kvp.Value.IsOnline = false;
                }
            }
        }
        
        ScheduleSave();
        OnDevicesChanged?.Invoke(this, GetAllDevices());
    }

    public void MergeDevices(List<NetworkDevice> devices)
    {
        lock (_lock)
        {
            foreach (var device in devices)
            {
                if (_devices.TryGetValue(device.IpAddress, out var existingDevice))
                {
                    // Update existing device with new info
                    existingDevice.IsOnline = device.IsOnline;
                    existingDevice.LastSeen = device.LastSeen;
                    existingDevice.ResponseTime = device.ResponseTime;
                    existingDevice.HostName = device.HostName;
                    existingDevice.MacAddress = device.MacAddress;
                    existingDevice.OpenPorts = device.OpenPorts;
                    existingDevice.DeviceType = device.DeviceType;
                    existingDevice.DiscoveryCount++;
                }
                else
                {
                    device.FirstDiscovered = DateTime.Now;
                    device.DiscoveryCount = 1;
                    _devices[device.IpAddress] = device;
                }
            }
        }
        
        ScheduleSave();
        OnDevicesChanged?.Invoke(this, GetAllDevices());
    }
}

/// <summary>
/// Container for persisted device data
/// </summary>
public class DeviceDataFile
{
    public DateTime LastUpdated { get; set; }
    public List<NetworkDevice> Devices { get; set; } = new();
}
