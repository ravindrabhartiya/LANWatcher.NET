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
    private string _ipRange = "192.168.1";
    
    public string IpRange 
    { 
        get => _ipRange;
        set => _ipRange = CleanIpRange(value);
    }
    public int StartAddress { get; set; } = 1;
    public int EndAddress { get; set; } = 254;
    public int PingTimeout { get; set; } = 1000; // milliseconds
    public int PortTimeout { get; set; } = 500; // milliseconds
    public int MaxParallelScans { get; set; } = 50;
    public bool ScanPorts { get; set; } = true;
    public bool QuickScan { get; set; } = true; // Only scan common ports
    public List<int> CustomPorts { get; set; } = new();

    /// <summary>
    /// Cleans the IP range input without forcing normalization to 3 octets.
    /// Preserves 2-octet inputs like "192.168" for broad scanning.
    /// </summary>
    private static string CleanIpRange(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "192.168.1";

        // Remove any trailing dots
        input = input.TrimEnd('.');
        
        var parts = input.Split('.');
        
        // Filter out empty parts and validate each octet is a number
        var validParts = new List<string>();
        foreach (var part in parts)
        {
            if (int.TryParse(part, out int octet) && octet >= 0 && octet <= 255)
            {
                validParts.Add(octet.ToString());
            }
        }

        // Return the cleaned value, preserving octet count (2 or 3 octets)
        return validParts.Count switch
        {
            0 => "192.168.1",
            1 => "192.168.1", // Single octet not useful, default to common range
            2 => $"{validParts[0]}.{validParts[1]}", // Preserve 2 octets for broad scan
            3 => $"{validParts[0]}.{validParts[1]}.{validParts[2]}",
            _ => $"{validParts[0]}.{validParts[1]}.{validParts[2]}" // 4+ octets, use first 3
        };
    }

    /// <summary>
    /// Gets the number of octets in the current IP range.
    /// </summary>
    public int GetOctetCount()
    {
        var parts = IpRange.Split('.');
        return parts.Count(p => int.TryParse(p, out int v) && v >= 0 && v <= 255);
    }

    public bool IsValidIpRange()
    {
        var octetCount = GetOctetCount();
        return octetCount >= 2 && octetCount <= 3;
    }
}
