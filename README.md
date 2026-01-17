# 🔍 LanWatcher.NET

A powerful network device discovery and port scanning tool built with **Blazor** and **.NET 9**. Discover all devices on your local network and identify what services they're running.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)
![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?style=flat-square&logo=blazor)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

## ✨ Features

- **🚀 Parallel Network Scanning** - Uses `Task.WhenAll` to scan hundreds of IP addresses simultaneously
- **🔌 Port Scanning** - Scans 50+ common ports using `TcpClient`
- **🎯 Device Type Detection** - Automatically identifies device types based on open ports
- **📊 Interactive Dashboard** - Real-time statistics and progress tracking
- **🗺️ Multiple Views** - Grid, List, and Topology network map views
- **⚠️ Risk Detection** - Highlights devices with potentially risky open ports

## 📷 Screenshots

### Network Map View
The interactive network map shows all discovered devices with their types and connections.

### Device Types Detected
| Icon | Type | Detection Criteria |
|------|------|-------------------|
| 🌐 | Router | Ports 80, 443, 53 |
| 🖥️ | Web Server | Ports 80, 443, 8080 |
| 🖨️ | Printer | Ports 9100, 515, 631 |
| 📷 | Camera | Ports 554, 8080 |
| 📁 | File Server | Ports 445, 139 |
| 📺 | Smart TV | Ports 8008, 8443 |
| 🏠 | Smart Home | Ports 1883 (MQTT) |
| 🎮 | Game Console | Ports 3074, 3478 |
| 💻 | Computer | Ports 3389, 22, 5900 |
| 🗄️ | Database | Ports 3306, 5432, 27017 |
| 📧 | Mail Server | Ports 25, 465, 587 |
| 🎬 | Media Server | Port 32400 (Plex) |

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/LanWatcher.NET.git
cd LanWatcher.NET
```

2. Build the project:
```bash
dotnet build
```

3. Run the application:
```bash
dotnet run
```

4. Open your browser and navigate to `http://localhost:5182`

## 🎮 Usage

1. **Configure Scan Settings**
   - Set the IP range (auto-detected from your network)
   - Choose Quick Scan (common ports) or Deep Scan (extended ports)
   - Adjust parallel scan count for performance

2. **Start Scanning**
   - Click "🚀 Start Scan" to begin discovery
   - Watch real-time progress as devices are found

3. **Explore Results**
   - Switch between Grid, List, and Topology views
   - Click on any device to see detailed port information
   - Review the stats dashboard for network overview

## 🏗️ Architecture

```
LanWatcher.NET/
├── Models/
│   ├── NetworkDevice.cs      # Device model with type detection
│   └── ScanProgress.cs       # Progress tracking & scan options
├── Services/
│   ├── NetworkScanner.cs     # Core scanning using Ping & TcpClient
│   ├── DeviceRepository.cs   # In-memory device storage
│   ├── ScanService.cs        # Orchestrates scanning operations
│   └── PortDefinitions.cs    # Port-to-service mappings
├── Components/
│   ├── DeviceCard.razor      # Individual device display
│   ├── NetworkMap.razor      # Grid/List/Topology views
│   ├── ProgressPanel.razor   # Scan progress display
│   ├── ScanControls.razor    # Scan configuration UI
│   └── StatsDashboard.razor  # Summary statistics
└── wwwroot/css/
    └── scanner.css           # Dark theme styling
```

## ⚡ Performance

LanWatcher.NET leverages C#'s async/await pattern with `Task.WhenAll` to achieve high-performance parallel scanning:

- **Parallel IP Scanning**: Configurable 10-100 concurrent connections
- **Efficient Port Scanning**: Non-blocking TCP connection attempts
- **Memory Efficient**: Uses semaphores to limit resource usage

## 🔒 Security Considerations

- This tool is designed for **scanning your own network only**
- Always obtain proper authorization before scanning networks
- Some networks may flag port scanning activity
- Use responsibly and ethically

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [Blazor](https://blazor.net/)
- Inspired by tools like Nmap and Angry IP Scanner
- Icons from native emoji support

## 📧 Contact

Your Name - [@yourtwitter](https://twitter.com/yourtwitter)

Project Link: [https://github.com/yourusername/LanWatcher.NET](https://github.com/yourusername/LanWatcher.NET)

---

⭐ Star this repo if you find it useful!
