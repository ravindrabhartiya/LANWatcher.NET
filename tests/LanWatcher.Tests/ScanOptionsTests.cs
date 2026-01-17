using FluentAssertions;
using LanWatcher.Models;

namespace LanWatcher.Tests;

public class ScanOptionsTests
{
    [Fact]
    public void NewScanOptions_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new ScanOptions();

        // Assert
        options.IpRange.Should().Be("192.168.1");
        options.StartAddress.Should().Be(1);
        options.EndAddress.Should().Be(254);
        options.PingTimeout.Should().Be(1000);
        options.PortTimeout.Should().Be(500);
        options.MaxParallelScans.Should().Be(50);
        options.ScanPorts.Should().BeTrue();
        options.QuickScan.Should().BeTrue();
        options.CustomPorts.Should().BeEmpty();
    }

    [Fact]
    public void ScanOptions_ShouldAcceptCustomValues()
    {
        // Arrange & Act
        var options = new ScanOptions
        {
            IpRange = "10.0.0",
            StartAddress = 10,
            EndAddress = 100,
            PingTimeout = 2000,
            PortTimeout = 1000,
            MaxParallelScans = 100,
            ScanPorts = false,
            QuickScan = false,
            CustomPorts = new List<int> { 22, 80, 443 }
        };

        // Assert
        options.IpRange.Should().Be("10.0.0");
        options.StartAddress.Should().Be(10);
        options.EndAddress.Should().Be(100);
        options.PingTimeout.Should().Be(2000);
        options.PortTimeout.Should().Be(1000);
        options.MaxParallelScans.Should().Be(100);
        options.ScanPorts.Should().BeFalse();
        options.QuickScan.Should().BeFalse();
        options.CustomPorts.Should().HaveCount(3);
        options.CustomPorts.Should().Contain(new[] { 22, 80, 443 });
    }

    [Theory]
    [InlineData(1, 254, 254)]
    [InlineData(1, 100, 100)]
    [InlineData(100, 200, 101)]
    [InlineData(1, 1, 1)]
    public void AddressRange_ShouldCalculateCorrectCount(int start, int end, int expectedCount)
    {
        // Arrange
        var options = new ScanOptions
        {
            StartAddress = start,
            EndAddress = end
        };

        // Act
        var count = options.EndAddress - options.StartAddress + 1;

        // Assert
        count.Should().Be(expectedCount);
    }
}
