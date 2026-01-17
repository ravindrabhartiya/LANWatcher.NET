namespace LanWatcher.Models;

public class ScanProgress
{
    private int _scannedAddresses;
    private int _devicesFound;
    private int _portsScanned;

    public int TotalAddresses { get; set; }
    
    public int ScannedAddresses
    {
        get => _scannedAddresses;
        set => _scannedAddresses = value;
    }
    
    public int DevicesFound
    {
        get => _devicesFound;
        set => _devicesFound = value;
    }
    
    public int PortsScanned
    {
        get => _portsScanned;
        set => _portsScanned = value;
    }

    public void IncrementScanned() => Interlocked.Increment(ref _scannedAddresses);
    public void IncrementDevicesFound() => Interlocked.Increment(ref _devicesFound);
    public void AddPortsScanned(int count) => Interlocked.Add(ref _portsScanned, count);

    public string CurrentAction { get; set; } = string.Empty;
    public string CurrentIp { get; set; } = string.Empty;
    public bool IsScanning { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan ElapsedTime => DateTime.Now - StartTime;
    public double ProgressPercentage => TotalAddresses > 0 ? (double)ScannedAddresses / TotalAddresses * 100 : 0;
}

public class ScanOptions
{
    public string IpRange { get; set; } = "192.168.1";
    public int StartAddress { get; set; } = 1;
    public int EndAddress { get; set; } = 254;
    public int PingTimeout { get; set; } = 1000; // milliseconds
    public int PortTimeout { get; set; } = 500; // milliseconds
    public int MaxParallelScans { get; set; } = 50;
    public bool ScanPorts { get; set; } = true;
    public bool QuickScan { get; set; } = true; // Only scan common ports
    public List<int> CustomPorts { get; set; } = new();
}
