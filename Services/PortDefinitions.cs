using LanWatcher.Models;

namespace LanWatcher.Services;

public static class PortDefinitions
{
    // Common ports for quick scanning
    public static readonly int[] CommonPorts = new[]
    {
        20, 21, 22, 23, 25, 53, 80, 110, 111, 135, 139, 143, 443, 445, 
        465, 514, 548, 554, 587, 631, 993, 995, 1080, 1433, 1521, 1723, 
        1883, 3000, 3306, 3389, 4000, 5000, 5432, 5900, 5901, 6379, 
        7000, 8000, 8008, 8080, 8081, 8443, 8888, 9000, 9090, 9100, 
        10000, 27017, 32400, 49152
    };

    // Extended port list for comprehensive scanning
    public static readonly int[] ExtendedPorts = new[]
    {
        1, 7, 9, 11, 13, 15, 17, 18, 19, 20, 21, 22, 23, 25, 37, 42, 43, 
        49, 53, 70, 79, 80, 81, 82, 83, 84, 85, 88, 89, 90, 99, 100, 
        106, 109, 110, 111, 113, 119, 123, 135, 137, 138, 139, 143, 
        144, 161, 162, 175, 179, 199, 211, 212, 222, 255, 256, 259, 
        264, 280, 311, 389, 407, 443, 444, 445, 458, 465, 500, 512, 
        513, 514, 515, 520, 524, 541, 543, 548, 554, 563, 587, 593, 
        625, 631, 636, 646, 691, 860, 873, 902, 990, 993, 995, 1025, 
        1080, 1241, 1311, 1433, 1434, 1494, 1521, 1720, 1723, 1755, 
        1883, 1900, 2000, 2001, 2049, 2121, 2717, 3000, 3128, 3306, 
        3389, 3632, 4000, 4443, 4567, 4899, 5000, 5001, 5009, 5050, 
        5060, 5101, 5190, 5357, 5432, 5631, 5666, 5800, 5900, 5901, 
        5984, 5985, 6000, 6001, 6379, 6665, 6666, 6667, 6668, 6669, 
        7000, 7001, 7002, 8000, 8008, 8080, 8081, 8088, 8443, 8888, 
        9000, 9001, 9090, 9100, 9200, 9999, 10000, 10050, 11211, 
        13579, 27017, 32400, 49152, 50000
    };

    public static readonly Dictionary<int, string> PortServices = new()
    {
        { 20, "FTP Data" },
        { 21, "FTP Control" },
        { 22, "SSH" },
        { 23, "Telnet" },
        { 25, "SMTP" },
        { 53, "DNS" },
        { 80, "HTTP" },
        { 81, "HTTP Alternate" },
        { 110, "POP3" },
        { 111, "RPC" },
        { 123, "NTP" },
        { 135, "MS RPC" },
        { 137, "NetBIOS Name" },
        { 138, "NetBIOS Datagram" },
        { 139, "NetBIOS Session" },
        { 143, "IMAP" },
        { 161, "SNMP" },
        { 389, "LDAP" },
        { 443, "HTTPS" },
        { 445, "SMB/CIFS" },
        { 465, "SMTPS" },
        { 514, "Syslog" },
        { 515, "LPD Printing" },
        { 548, "AFP (Apple)" },
        { 554, "RTSP (Streaming)" },
        { 587, "SMTP Submission" },
        { 631, "IPP (Printing)" },
        { 636, "LDAPS" },
        { 993, "IMAPS" },
        { 995, "POP3S" },
        { 1080, "SOCKS Proxy" },
        { 1433, "MS SQL Server" },
        { 1521, "Oracle DB" },
        { 1723, "PPTP VPN" },
        { 1883, "MQTT (IoT)" },
        { 1900, "UPnP/SSDP" },
        { 2049, "NFS" },
        { 3000, "Node.js/Dev Server" },
        { 3128, "Squid Proxy" },
        { 3306, "MySQL" },
        { 3389, "RDP" },
        { 4000, "ICQ/Custom" },
        { 5000, "UPnP/Flask" },
        { 5001, "Synology DSM" },
        { 5060, "SIP" },
        { 5222, "XMPP" },
        { 5357, "WSDAPI" },
        { 5432, "PostgreSQL" },
        { 5900, "VNC" },
        { 5901, "VNC Display 1" },
        { 5984, "CouchDB" },
        { 5985, "WinRM HTTP" },
        { 6379, "Redis" },
        { 6667, "IRC" },
        { 7000, "Cassandra" },
        { 8000, "HTTP Alt/Dev" },
        { 8008, "HTTP Alt" },
        { 8080, "HTTP Proxy" },
        { 8081, "HTTP Alt" },
        { 8443, "HTTPS Alt" },
        { 8888, "HTTP Alt/Jupyter" },
        { 9000, "SonarQube/PHP-FPM" },
        { 9090, "Prometheus/Cockpit" },
        { 9100, "JetDirect (Printer)" },
        { 9200, "Elasticsearch" },
        { 10000, "Webmin" },
        { 11211, "Memcached" },
        { 27017, "MongoDB" },
        { 32400, "Plex Media Server" },
        { 49152, "Windows RPC" }
    };

    public static string GetServiceName(int port)
    {
        return PortServices.TryGetValue(port, out var service) ? service : $"Port {port}";
    }

    // Port signatures for device type detection
    public static readonly Dictionary<int[], DeviceType> DeviceSignatures = new()
    {
        { new[] { 80, 443, 53 }, DeviceType.Router },
        { new[] { 9100, 515, 631 }, DeviceType.Printer },
        { new[] { 554, 8080, 80 }, DeviceType.Camera },
        { new[] { 445, 139, 137 }, DeviceType.FileServer },
        { new[] { 8008, 8443, 9000 }, DeviceType.SmartTV },
        { new[] { 1883, 8883 }, DeviceType.SmartHome },
        { new[] { 3074, 3478, 3480 }, DeviceType.GameConsole },
        { new[] { 62078, 5353 }, DeviceType.Phone },
        { new[] { 3389, 22, 5900 }, DeviceType.Computer },
        { new[] { 3306, 5432, 1433, 27017 }, DeviceType.DatabaseServer },
        { new[] { 25, 465, 587, 110, 143 }, DeviceType.MailServer },
        { new[] { 32400, 8096, 8920 }, DeviceType.MediaServer },
        { new[] { 80, 443 }, DeviceType.WebServer }
    };
}
