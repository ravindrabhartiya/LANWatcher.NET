using FluentAssertions;
using LanWatcher.Services;

namespace LanWatcher.Tests;

public class PortDefinitionsTests
{
    [Fact]
    public void CommonPorts_ShouldContainEssentialPorts()
    {
        // Assert
        PortDefinitions.CommonPorts.Should().Contain(80);   // HTTP
        PortDefinitions.CommonPorts.Should().Contain(443);  // HTTPS
        PortDefinitions.CommonPorts.Should().Contain(22);   // SSH
        PortDefinitions.CommonPorts.Should().Contain(21);   // FTP
        PortDefinitions.CommonPorts.Should().Contain(3389); // RDP
        PortDefinitions.CommonPorts.Should().Contain(445);  // SMB
    }

    [Fact]
    public void ExtendedPorts_ShouldContainMorePorts_ThanCommonPorts()
    {
        // Assert
        PortDefinitions.ExtendedPorts.Length.Should().BeGreaterThan(PortDefinitions.CommonPorts.Length);
    }

    [Theory]
    [InlineData(80, "HTTP")]
    [InlineData(443, "HTTPS")]
    [InlineData(22, "SSH")]
    [InlineData(21, "FTP Control")]
    [InlineData(3389, "RDP")]
    [InlineData(445, "SMB/CIFS")]
    [InlineData(3306, "MySQL")]
    [InlineData(5432, "PostgreSQL")]
    [InlineData(27017, "MongoDB")]
    [InlineData(6379, "Redis")]
    [InlineData(9100, "JetDirect (Printer)")]
    [InlineData(554, "RTSP (Streaming)")]
    [InlineData(32400, "Plex Media Server")]
    public void GetServiceName_ShouldReturnCorrectServiceName(int port, string expectedService)
    {
        // Act
        var serviceName = PortDefinitions.GetServiceName(port);

        // Assert
        serviceName.Should().Be(expectedService);
    }

    [Fact]
    public void GetServiceName_ShouldReturnPortNumber_ForUnknownPort()
    {
        // Arrange
        var unknownPort = 12345;

        // Act
        var serviceName = PortDefinitions.GetServiceName(unknownPort);

        // Assert
        serviceName.Should().Be($"Port {unknownPort}");
    }

    [Fact]
    public void PortServices_ShouldContainCommonServices()
    {
        // Assert
        PortDefinitions.PortServices.Should().ContainKey(80);
        PortDefinitions.PortServices.Should().ContainKey(443);
        PortDefinitions.PortServices.Should().ContainKey(22);
        PortDefinitions.PortServices.Should().ContainKey(25);
    }

    [Fact]
    public void DeviceSignatures_ShouldContainDeviceTypes()
    {
        // Assert
        PortDefinitions.DeviceSignatures.Should().NotBeEmpty();
        PortDefinitions.DeviceSignatures.Values.Should().Contain(LanWatcher.Models.DeviceType.Printer);
        PortDefinitions.DeviceSignatures.Values.Should().Contain(LanWatcher.Models.DeviceType.Router);
        PortDefinitions.DeviceSignatures.Values.Should().Contain(LanWatcher.Models.DeviceType.Camera);
    }
}
