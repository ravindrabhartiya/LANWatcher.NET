using LanWatcher.Models;

namespace LanWatcher.Services;

public interface IDeviceRepository
{
    List<NetworkDevice> GetAllDevices();
    void AddOrUpdateDevice(NetworkDevice device);
    void ClearDevices();
    NetworkDevice? GetDevice(string ipAddress);
    void UpdateDevices(List<NetworkDevice> devices);
    event EventHandler<List<NetworkDevice>>? OnDevicesChanged;
}

public class DeviceRepository : IDeviceRepository
{
    private readonly Dictionary<string, NetworkDevice> _devices = new();
    private readonly object _lock = new();

    public event EventHandler<List<NetworkDevice>>? OnDevicesChanged;

    public List<NetworkDevice> GetAllDevices()
    {
        lock (_lock)
        {
            return _devices.Values.OrderBy(d => 
            {
                var parts = d.IpAddress.Split('.');
                return parts.Length == 4 && int.TryParse(parts[3], out var last) ? last : 999;
            }).ToList();
        }
    }

    public void AddOrUpdateDevice(NetworkDevice device)
    {
        lock (_lock)
        {
            _devices[device.IpAddress] = device;
        }
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
            _devices.Clear();
            foreach (var device in devices)
            {
                _devices[device.IpAddress] = device;
            }
        }
        OnDevicesChanged?.Invoke(this, GetAllDevices());
    }
}
