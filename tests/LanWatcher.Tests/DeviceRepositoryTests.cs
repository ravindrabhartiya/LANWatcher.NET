using FluentAssertions;
using LanWatcher.Models;
using LanWatcher.Services;

namespace LanWatcher.Tests;

public class DeviceRepositoryTests
{
    [Fact]
    public void NewRepository_ShouldBeEmpty()
    {
        // Arrange & Act
        var repository = new DeviceRepository();

        // Assert
        repository.GetAllDevices().Should().BeEmpty();
    }

    [Fact]
    public void AddOrUpdateDevice_ShouldAddNewDevice()
    {
        // Arrange
        var repository = new DeviceRepository();
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
        var repository = new DeviceRepository();
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
        var repository = new DeviceRepository();

        // Act & Assert
        repository.GetDevice("192.168.1.999").Should().BeNull();
    }

    [Fact]
    public void ClearDevices_ShouldRemoveAllDevices()
    {
        // Arrange
        var repository = new DeviceRepository();
        repository.AddOrUpdateDevice(new NetworkDevice { IpAddress = "192.168.1.1" });
        repository.AddOrUpdateDevice(new NetworkDevice { IpAddress = "192.168.1.2" });
        repository.AddOrUpdateDevice(new NetworkDevice { IpAddress = "192.168.1.3" });

        // Act
        repository.ClearDevices();

        // Assert
        repository.GetAllDevices().Should().BeEmpty();
    }

    [Fact]
    public void UpdateDevices_ShouldReplaceAllDevices()
    {
        // Arrange
        var repository = new DeviceRepository();
        repository.AddOrUpdateDevice(new NetworkDevice { IpAddress = "192.168.1.1" });
        
        var newDevices = new List<NetworkDevice>
        {
            new() { IpAddress = "192.168.1.10" },
            new() { IpAddress = "192.168.1.20" }
        };

        // Act
        repository.UpdateDevices(newDevices);

        // Assert
        repository.GetAllDevices().Should().HaveCount(2);
        repository.GetDevice("192.168.1.1").Should().BeNull();
        repository.GetDevice("192.168.1.10").Should().NotBeNull();
        repository.GetDevice("192.168.1.20").Should().NotBeNull();
    }

    [Fact]
    public void GetAllDevices_ShouldReturnDevicesSortedByIpAddress()
    {
        // Arrange
        var repository = new DeviceRepository();
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
        var repository = new DeviceRepository();
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
        var repository = new DeviceRepository();
        repository.AddOrUpdateDevice(new NetworkDevice { IpAddress = "192.168.1.1" });
        var eventFired = false;
        repository.OnDevicesChanged += (s, e) => eventFired = true;

        // Act
        repository.ClearDevices();

        // Assert
        eventFired.Should().BeTrue();
    }
}
