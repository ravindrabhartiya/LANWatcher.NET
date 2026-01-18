using LanWatcher.Models;

namespace LanWatcher.Services;

/// <summary>
/// Background service that periodically refreshes known devices to keep their status up-to-date.
/// Runs every 30 seconds and checks devices sequentially to avoid network flooding.
/// </summary>
public class DeviceRefreshService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DeviceRefreshService> _logger;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(30);

    public DeviceRefreshService(IServiceScopeFactory scopeFactory, ILogger<DeviceRefreshService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Device refresh service started. Will refresh every {Interval} seconds", 
            _refreshInterval.TotalSeconds);

        // Wait a bit before starting the first refresh to let the app initialize
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RefreshDevicesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during background device refresh");
            }

            await Task.Delay(_refreshInterval, stoppingToken);
        }

        _logger.LogInformation("Device refresh service stopped");
    }

    private async Task RefreshDevicesAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
        var scanner = scope.ServiceProvider.GetRequiredService<INetworkScanner>();
        var scanService = scope.ServiceProvider.GetRequiredService<IScanService>();

        // Don't refresh if a manual scan is in progress
        if (scanService.IsScanning)
        {
            _logger.LogInformation("Skipping background refresh - manual scan in progress");
            return;
        }

        var devices = repository.GetAllDevices();
        if (!devices.Any())
        {
            _logger.LogInformation("No devices to refresh");
            return;
        }

        _logger.LogInformation("Starting background refresh of {Count} devices", devices.Count);

        var options = new ScanOptions
        {
            PingTimeout = 1000,
            PortTimeout = 300,
            ScanPorts = true,
            QuickScan = true // Use quick scan for background refresh
        };

        int onlineCount = 0;
        int offlineCount = 0;

        foreach (var device in devices)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            // Skip if a manual scan started
            if (scanService.IsScanning)
            {
                _logger.LogDebug("Aborting background refresh - manual scan started");
                return;
            }

            try
            {
                var refreshed = await scanner.ScanDeviceAsync(device.IpAddress, options, stoppingToken);

                if (refreshed.IsOnline)
                {
                    repository.AddOrUpdateDevice(refreshed);
                    onlineCount++;
                }
                else
                {
                    // Mark device as offline but preserve its data
                    device.IsOnline = false;
                    device.LastSeen = device.LastSeen; // Keep the last seen time
                    repository.AddOrUpdateDevice(device);
                    offlineCount++;
                }

                // Small delay between devices to be gentle on the network
                await Task.Delay(100, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh device {IpAddress}", device.IpAddress);
            }
        }

        _logger.LogInformation("Background refresh complete: {Online} online, {Offline} offline", 
            onlineCount, offlineCount);
    }
}
