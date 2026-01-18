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
    public string DeviceIcon => GetDeviceIcon();
    public string DeviceColor => GetDeviceColor();

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
