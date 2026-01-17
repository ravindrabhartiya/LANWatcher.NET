using FluentAssertions;
using LanWatcher.Models;

namespace LanWatcher.Tests;

public class NetworkDeviceTests
{
    [Fact]
    public void NewDevice_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var device = new NetworkDevice();

        // Assert
        device.IpAddress.Should().BeEmpty();
        device.HostName.Should().Be("Unknown");
        device.MacAddress.Should().Be("Unknown");
        device.IsOnline.Should().BeFalse();
        device.OpenPorts.Should().BeEmpty();
        device.DeviceType.Should().Be(DeviceType.Unknown);
    }

    [Theory]
    [InlineData(DeviceType.Router, "🌐")]
    [InlineData(DeviceType.Printer, "🖨️")]
    [InlineData(DeviceType.Camera, "📷")]
    [InlineData(DeviceType.WebServer, "🖥️")]
    [InlineData(DeviceType.FileServer, "📁")]
    [InlineData(DeviceType.SmartTV, "📺")]
    [InlineData(DeviceType.SmartHome, "🏠")]
    [InlineData(DeviceType.GameConsole, "🎮")]
    [InlineData(DeviceType.Phone, "📱")]
    [InlineData(DeviceType.Computer, "💻")]
    [InlineData(DeviceType.DatabaseServer, "🗄️")]
    [InlineData(DeviceType.MailServer, "📧")]
    [InlineData(DeviceType.MediaServer, "🎬")]
    [InlineData(DeviceType.Unknown, "❓")]
    public void DeviceIcon_ShouldReturnCorrectIcon_ForDeviceType(DeviceType deviceType, string expectedIcon)
    {
        // Arrange
        var device = new NetworkDevice { DeviceType = deviceType };

        // Act & Assert
        device.DeviceIcon.Should().Be(expectedIcon);
    }

    [Theory]
    [InlineData(DeviceType.Router, "#4CAF50")]
    [InlineData(DeviceType.Printer, "#9C27B0")]
    [InlineData(DeviceType.Camera, "#F44336")]
    [InlineData(DeviceType.Unknown, "#9E9E9E")]
    public void DeviceColor_ShouldReturnCorrectColor_ForDeviceType(DeviceType deviceType, string expectedColor)
    {
        // Arrange
        var device = new NetworkDevice { DeviceType = deviceType };

        // Act & Assert
        device.DeviceColor.Should().Be(expectedColor);
    }

    [Fact]
    public void Device_WithOpenPorts_ShouldStorePortsCorrectly()
    {
        // Arrange
        var device = new NetworkDevice
        {
            IpAddress = "192.168.1.1",
            OpenPorts = new List<PortInfo>
            {
                new() { Port = 80, ServiceName = "HTTP", IsOpen = true },
                new() { Port = 443, ServiceName = "HTTPS", IsOpen = true }
            }
        };

        // Assert
        device.OpenPorts.Should().HaveCount(2);
        device.OpenPorts.Should().Contain(p => p.Port == 80);
        device.OpenPorts.Should().Contain(p => p.Port == 443);
    }
}
