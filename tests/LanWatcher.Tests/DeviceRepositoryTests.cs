using FluentAssertions;
using LanWatcher.Models;
using LanWatcher.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace LanWatcher.Tests;

public class DeviceRepositoryTests : IDisposable
{
    private readonly string _testFolder;

    public DeviceRepositoryTests()
    {
        // Create a unique folder for each test instance
        _testFolder = Path.Combine(Path.GetTempPath(), $"LanWatcherTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testFolder);
    }

    public void Dispose()
    {
        // Clean up test folder
        try
        {
            if (Directory.Exists(_testFolder))
            {
                Directory.Delete(_testFolder, true);
            }
        }
        catch { /* Ignore cleanup errors */ }
    }

    private DeviceRepository CreateRepository()
    {
        var logger = new Mock<ILogger<DeviceRepository>>();
        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.ContentRootPath).Returns(_testFolder);
        return new DeviceRepository(logger.Object, environment.Object);
    }

    [Fact]
    public void NewRepository_ShouldBeEmpty()
    {
        // Arrange & Act
        var repository = CreateRepository();

        // Assert
        repository.GetAllDevices().Should().BeEmpty();
    }

    [Fact]
    public void AddOrUpdateDevice_ShouldAddNewDevice()
    {
        // Arrange
        var repository = CreateRepository();
        var device = new NetworkDevice
        {
            IpAddress = "192.168.1.1",
            HostName = "router",
            IsOnline = true
        };

        // Act
        repository.AddOrUpdateDevice(device);

        // Assert
        repository.GetAllDevices().Should().HaveCount(1);
        repository.GetDevice("192.168.1.1").Should().NotBeNull();
        repository.GetDevice("192.168.1.1")!.HostName.Should().Be("router");
    }

    [Fact]
    public void AddOrUpdateDevice_ShouldUpdateExistingDevice()
    {
        // Arrange
        var repository = CreateRepository();
        var device1 = new NetworkDevice
        {
            IpAddress = "192.168.1.1",
            HostName = "router-old"
        };
        var device2 = new NetworkDevice
        {
            IpAddress = "192.168.1.1",
            HostName = "router-new"
        };

        // Act
        repository.AddOrUpdateDevice(device1);
        repository.AddOrUpdateDevice(device2);

        // Assert
        repository.GetAllDevices().Should().HaveCount(1);
        repository.GetDevice("192.168.1.1")!.HostName.Should().Be("router-new");
    }

    [Fact]
    public void GetDevice_ShouldReturnNull_ForNonexistentDevice()
    {
        // Arrange
        var repository = CreateRepository();

        // Act & Assert
        repository.GetDevice("192.168.1.999").Should().BeNull();
    }

    [Fact]
    public void ClearDevices_ShouldRemoveAllDevices()
    {
        // Arrange
        var repository = CreateRepository();
        repository.AddOrUpdateDevice(new NetworkDevice { IpAddress = "192.168.1.1" });
        repository.AddOrUpdateDevice(new NetworkDevice { IpAddress = "192.168.1.2" });
        repository.AddOrUpdateDevice(new NetworkDevice { IpAddress = "192.168.1.3" });

        // Act
        repository.ClearDevices();

        // Assert
        repository.GetAllDevices().Should().BeEmpty();
    }

    [Fact]
    public void UpdateDevices_ShouldMergeDevices()
    {
        // Arrange
        var repository = CreateRepository();
        repository.AddOrUpdateDevice(new NetworkDevice { IpAddress = "192.168.1.1" });
        
        var newDevices = new List<NetworkDevice>
        {
            new() { IpAddress = "192.168.1.10" },
            new() { IpAddress = "192.168.1.20" }
        };

        // Act
        repository.UpdateDevices(newDevices);

        // Assert
        repository.GetAllDevices().Should().HaveCount(3); // Merged, not replaced
        repository.GetDevice("192.168.1.1").Should().NotBeNull(); // Still exists
        repository.GetDevice("192.168.1.10").Should().NotBeNull();
        repository.GetDevice("192.168.1.20").Should().NotBeNull();
    }

    [Fact]
    public void GetAllDevices_ShouldReturnDevicesSortedByIpAddress()
    {
        // Arrange
        var repository = CreateRepository();
        repository.AddOrUpdateDevice(new NetworkDevice { IpAddress = "192.168.1.100" });
        repository.AddOrUpdateDevice(new NetworkDevice { IpAddress = "192.168.1.1" });
        repository.AddOrUpdateDevice(new NetworkDevice { IpAddress = "192.168.1.50" });

        // Act
        var devices = repository.GetAllDevices();

        // Assert
        devices.Should().HaveCount(3);
        devices[0].IpAddress.Should().Be("192.168.1.1");
        devices[1].IpAddress.Should().Be("192.168.1.50");
        devices[2].IpAddress.Should().Be("192.168.1.100");
    }

    [Fact]
    public void OnDevicesChanged_ShouldFireEvent_WhenDeviceAdded()
    {
        // Arrange
        var repository = CreateRepository();
        var eventFired = false;
        repository.OnDevicesChanged += (s, e) => eventFired = true;

        // Act
        repository.AddOrUpdateDevice(new NetworkDevice { IpAddress = "192.168.1.1" });

        // Assert
        eventFired.Should().BeTrue();
    }

    [Fact]
    public void OnDevicesChanged_ShouldFireEvent_WhenDevicesCleared()
    {
        // Arrange
        var repository = CreateRepository();
        repository.AddOrUpdateDevice(new NetworkDevice { IpAddress = "192.168.1.1" });
        var eventFired = false;
        repository.OnDevicesChanged += (s, e) => eventFired = true;

        // Act
        repository.ClearDevices();

        // Assert
        eventFired.Should().BeTrue();
    }
}
