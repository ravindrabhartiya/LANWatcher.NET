using LanWatcher.Models;

namespace LanWatcher.Services;

public interface IScanService
{
    ScanProgress Progress { get; }
    bool IsScanning { get; }
    List<NetworkDevice> Devices { get; }
    ScanOptions Options { get; set; }
    event EventHandler<ScanProgress>? OnProgressChanged;
    event EventHandler<NetworkDevice>? OnDeviceFound;
    event EventHandler<List<NetworkDevice>>? OnScanComplete;
    Task StartScanAsync();
    void StopScan();
    string GetLocalIpRange();
}

public class ScanService : IScanService
{
    private readonly INetworkScanner _scanner;
    private readonly IDeviceRepository _repository;
    private readonly ILogger<ScanService> _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    public ScanProgress Progress { get; private set; } = new();
    public bool IsScanning => Progress.IsScanning;
    public List<NetworkDevice> Devices => _repository.GetAllDevices();
    public ScanOptions Options { get; set; }

    public event EventHandler<ScanProgress>? OnProgressChanged;
    public event EventHandler<NetworkDevice>? OnDeviceFound;
    public event EventHandler<List<NetworkDevice>>? OnScanComplete;

    public ScanService(INetworkScanner scanner, IDeviceRepository repository, ILogger<ScanService> logger)
    {
        _scanner = scanner;
        _repository = repository;
        _logger = logger;

        Options = new ScanOptions
        {
            IpRange = _scanner.GetLocalIpRange()
        };

        _scanner.OnProgressChanged += (s, p) =>
        {
            Progress = p;
            OnProgressChanged?.Invoke(this, p);
        };

        _scanner.OnDeviceFound += (s, d) =>
        {
            _repository.AddOrUpdateDevice(d);
            OnDeviceFound?.Invoke(this, d);
        };
    }

    public string GetLocalIpRange()
    {
        return _scanner.GetLocalIpRange();
    }

    public async Task StartScanAsync()
    {
        if (IsScanning)
        {
            _logger.LogWarning("Scan already in progress");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _repository.ClearDevices();

        Progress = new ScanProgress
        {
            IsScanning = true,
            StartTime = DateTime.Now
        };
        OnProgressChanged?.Invoke(this, Progress);

        try
        {
            _logger.LogInformation("Starting network scan with options: {Options}", 
                System.Text.Json.JsonSerializer.Serialize(Options));

            var devices = await _scanner.ScanNetworkAsync(Options, _cancellationTokenSource.Token);
            _repository.UpdateDevices(devices);

            OnScanComplete?.Invoke(this, devices);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Scan was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during network scan");
        }
        finally
        {
            Progress.IsScanning = false;
            OnProgressChanged?.Invoke(this, Progress);
        }
    }

    public void StopScan()
    {
        _cancellationTokenSource?.Cancel();
        _logger.LogInformation("Scan stop requested");
    }
}
