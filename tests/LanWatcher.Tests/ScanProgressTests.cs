using FluentAssertions;
using LanWatcher.Models;

namespace LanWatcher.Tests;

public class ScanProgressTests
{
    [Fact]
    public void NewScanProgress_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var progress = new ScanProgress();

        // Assert
        progress.TotalAddresses.Should().Be(0);
        progress.ScannedAddresses.Should().Be(0);
        progress.DevicesFound.Should().Be(0);
        progress.PortsScanned.Should().Be(0);
        progress.IsScanning.Should().BeFalse();
        progress.CurrentAction.Should().BeEmpty();
        progress.CurrentIp.Should().BeEmpty();
    }

    [Fact]
    public void ProgressPercentage_ShouldCalculateCorrectly()
    {
        // Arrange
        var progress = new ScanProgress
        {
            TotalAddresses = 100,
            ScannedAddresses = 50
        };

        // Act & Assert
        progress.ProgressPercentage.Should().Be(50.0);
    }

    [Fact]
    public void ProgressPercentage_ShouldReturnZero_WhenTotalIsZero()
    {
        // Arrange
        var progress = new ScanProgress
        {
            TotalAddresses = 0,
            ScannedAddresses = 0
        };

        // Act & Assert
        progress.ProgressPercentage.Should().Be(0);
    }

    [Fact]
    public void IncrementScanned_ShouldIncrementCounter()
    {
        // Arrange
        var progress = new ScanProgress();

        // Act
        progress.IncrementScanned();
        progress.IncrementScanned();
        progress.IncrementScanned();

        // Assert
        progress.ScannedAddresses.Should().Be(3);
    }

    [Fact]
    public void IncrementDevicesFound_ShouldIncrementCounter()
    {
        // Arrange
        var progress = new ScanProgress();

        // Act
        progress.IncrementDevicesFound();
        progress.IncrementDevicesFound();

        // Assert
        progress.DevicesFound.Should().Be(2);
    }

    [Fact]
    public void AddPortsScanned_ShouldAddToCounter()
    {
        // Arrange
        var progress = new ScanProgress();

        // Act
        progress.AddPortsScanned(50);
        progress.AddPortsScanned(30);

        // Assert
        progress.PortsScanned.Should().Be(80);
    }

    [Fact]
    public void ElapsedTime_ShouldReturnPositiveDuration_WhenScanning()
    {
        // Arrange
        var progress = new ScanProgress
        {
            StartTime = DateTime.Now.AddSeconds(-5)
        };

        // Act & Assert
        progress.ElapsedTime.TotalSeconds.Should().BeGreaterThanOrEqualTo(4);
    }
}
