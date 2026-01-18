namespace LanWatcher.Models;

public class NetworkDevice
{
    public string IpAddress { get; set; } = string.Empty;
    public string HostName { get; set; } = "Unknown";
    public string MacAddress { get; set; } = "Unknown";
    public bool IsOnline { get; set; }
    public DateTime LastSeen { get; set; }
    public DateTime FirstDiscovered { get; set; } = DateTime.Now;
    public int DiscoveryCount { get; set; } = 1;
    public int ResponseTime { get; set; } // in milliseconds
    public List<PortInfo> OpenPorts { get; set; } = new();
    public DeviceType DeviceType { get; set; } = DeviceType.Unknown;
    
    // New enriched properties
    public string Manufacturer { get; set; } = "Unknown";
    public string OperatingSystem { get; set; } = "Unknown";
    public string NetBiosName { get; set; } = string.Empty;
    public string ConnectionType { get; set; } = "Unknown"; // WiFi, Ethernet, Virtual
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Unknown;
    public List<DateTime> OnlineHistory { get; set; } = new();
    public int Ttl { get; set; } // Time-to-live from ping response
    
    // Computed properties
    public string DeviceIcon => GetDeviceIcon();
    public string DeviceColor => GetDeviceColor();
    public string RiskIcon => GetRiskIcon();
    public string RiskColor => GetRiskColor();
    public string UptimeTrend => GetUptimeTrend();
    public int RiskScore => CalculateRiskScore();

    private string GetDeviceIcon()
    {
        return DeviceType switch
        {
            DeviceType.Router => "🌐",
            DeviceType.WebServer => "🖥️",
            DeviceType.Printer => "🖨️",
            DeviceType.Camera => "📷",
            DeviceType.FileServer => "📁",
            DeviceType.SmartTV => "📺",
            DeviceType.SmartHome => "🏠",
            DeviceType.GameConsole => "🎮",
            DeviceType.Phone => "📱",
            DeviceType.Computer => "💻",
            DeviceType.IoTDevice => "🔌",
            DeviceType.DatabaseServer => "🗄️",
            DeviceType.MailServer => "📧",
            DeviceType.MediaServer => "🎬",
            _ => "❓"
        };
    }

    private string GetDeviceColor()
    {
        return DeviceType switch
        {
            DeviceType.Router => "#4CAF50",
            DeviceType.WebServer => "#2196F3",
            DeviceType.Printer => "#9C27B0",
            DeviceType.Camera => "#F44336",
            DeviceType.FileServer => "#FF9800",
            DeviceType.SmartTV => "#00BCD4",
            DeviceType.SmartHome => "#8BC34A",
            DeviceType.GameConsole => "#E91E63",
            DeviceType.Phone => "#673AB7",
            DeviceType.Computer => "#3F51B5",
            DeviceType.IoTDevice => "#795548",
            DeviceType.DatabaseServer => "#607D8B",
            DeviceType.MailServer => "#009688",
            DeviceType.MediaServer => "#FF5722",
            _ => "#9E9E9E"
        };
    }

    private string GetRiskIcon()
    {
        return RiskLevel switch
        {
            RiskLevel.Critical => "🔴",
            RiskLevel.High => "🟠",
            RiskLevel.Medium => "🟡",
            RiskLevel.Low => "🟢",
            _ => "⚪"
        };
    }

    private string GetRiskColor()
    {
        return RiskLevel switch
        {
            RiskLevel.Critical => "#D32F2F",
            RiskLevel.High => "#F57C00",
            RiskLevel.Medium => "#FBC02D",
            RiskLevel.Low => "#388E3C",
            _ => "#9E9E9E"
        };
    }

    private int CalculateRiskScore()
    {
        int score = 0;
        var openPortNumbers = OpenPorts.Select(p => p.Port).ToHashSet();

        // Critical risk ports (remote access, unencrypted)
        if (openPortNumbers.Contains(23)) score += 30;  // Telnet (unencrypted)
        if (openPortNumbers.Contains(21)) score += 20;  // FTP (unencrypted)
        if (openPortNumbers.Contains(3389)) score += 15; // RDP
        if (openPortNumbers.Contains(5900)) score += 15; // VNC
        if (openPortNumbers.Contains(5901)) score += 15; // VNC
        if (openPortNumbers.Contains(5902)) score += 15; // VNC

        // High risk ports
        if (openPortNumbers.Contains(22)) score += 10;  // SSH (good but exposed)
        if (openPortNumbers.Contains(445)) score += 15; // SMB
        if (openPortNumbers.Contains(139)) score += 10; // NetBIOS
        if (openPortNumbers.Contains(135)) score += 10; // RPC
        if (openPortNumbers.Contains(1433)) score += 10; // SQL Server
        if (openPortNumbers.Contains(3306)) score += 10; // MySQL
        if (openPortNumbers.Contains(5432)) score += 10; // PostgreSQL
        if (openPortNumbers.Contains(27017)) score += 10; // MongoDB

        // Medium risk ports
        if (openPortNumbers.Contains(80)) score += 3;   // HTTP
        if (openPortNumbers.Contains(8080)) score += 5; // HTTP Alt
        if (openPortNumbers.Contains(8000)) score += 5; // HTTP Alt
        if (openPortNumbers.Contains(8443)) score += 3; // HTTPS Alt

        // Low risk - encrypted services
        if (openPortNumbers.Contains(443)) score += 1;  // HTTPS
        if (openPortNumbers.Contains(993)) score += 1;  // IMAPS
        if (openPortNumbers.Contains(995)) score += 1;  // POP3S

        return Math.Min(score, 100); // Cap at 100
    }

    private string GetUptimeTrend()
    {
        if (OnlineHistory.Count < 2)
            return "New";

        var recentHistory = OnlineHistory.TakeLast(10).ToList();
        var totalScans = DiscoveryCount;
        var onlineCount = recentHistory.Count;

        if (totalScans > 5)
        {
            var ratio = (double)onlineCount / Math.Min(totalScans, 10);
            if (ratio >= 0.9) return "Always On";
            if (ratio >= 0.5) return "Frequent";
            if (ratio >= 0.2) return "Sporadic";
            return "Rare";
        }

        return "Tracking...";
    }
}

public class PortInfo
{
    public int Port { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Protocol { get; set; } = "TCP";
    public bool IsOpen { get; set; }
    public string Banner { get; set; } = string.Empty;
}

public enum DeviceType
{
    Unknown,
    Router,
    WebServer,
    Printer,
    Camera,
    FileServer,
    SmartTV,
    SmartHome,
    GameConsole,
    Phone,
    Computer,
    IoTDevice,
    DatabaseServer,
    MailServer,
    MediaServer
}

public enum RiskLevel
{
    Unknown,
    Low,
    Medium,
    High,
    Critical
}
