# LanWatcher.NET - Local Run Script (PowerShell)
# Use this for full LAN scanning on Windows (Docker can't access the host network)

Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "  LanWatcher.NET - Native Run" -ForegroundColor Cyan
Write-Host "  Full LAN scanning enabled" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

Set-Location $PSScriptRoot

# Check if dotnet is available
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: .NET SDK not found. Please install .NET 9 SDK." -ForegroundColor Red
    Write-Host "Download from: https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Starting LanWatcher.NET..." -ForegroundColor Green
Write-Host ""
Write-Host "Access the application at: " -NoNewline
Write-Host "http://localhost:5182" -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop"
Write-Host ""

dotnet run --urls "http://localhost:5182"
