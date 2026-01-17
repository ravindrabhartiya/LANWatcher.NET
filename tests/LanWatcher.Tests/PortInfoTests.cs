using FluentAssertions;
using LanWatcher.Models;

namespace LanWatcher.Tests;

public class PortInfoTests
{
    [Fact]
    public void NewPortInfo_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var portInfo = new PortInfo();

        // Assert
        portInfo.Port.Should().Be(0);
        portInfo.ServiceName.Should().BeEmpty();
        portInfo.Protocol.Should().Be("TCP");
        portInfo.IsOpen.Should().BeFalse();
        portInfo.Banner.Should().BeEmpty();
    }

    [Fact]
    public void PortInfo_ShouldStoreValues()
    {
        // Arrange & Act
        var portInfo = new PortInfo
        {
            Port = 80,
            ServiceName = "HTTP",
            Protocol = "TCP",
            IsOpen = true,
            Banner = "Apache/2.4.41"
        };

        // Assert
        portInfo.Port.Should().Be(80);
        portInfo.ServiceName.Should().Be("HTTP");
        portInfo.Protocol.Should().Be("TCP");
        portInfo.IsOpen.Should().BeTrue();
        portInfo.Banner.Should().Be("Apache/2.4.41");
    }
}
