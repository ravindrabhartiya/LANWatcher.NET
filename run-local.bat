@echo off
REM LanWatcher.NET - Local Run Script
REM Use this for full LAN scanning on Windows (Docker can't access the host network)

echo =======================================
echo   LanWatcher.NET - Native Run
echo   Full LAN scanning enabled
echo =======================================
echo.

cd /d "%~dp0"

REM Check if dotnet is available
where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK not found. Please install .NET 9 SDK.
    echo Download from: https://dotnet.microsoft.com/download/dotnet/9.0
    pause
    exit /b 1
)

echo Starting LanWatcher.NET...
echo.
echo Access the application at: http://localhost:5182
echo Press Ctrl+C to stop
echo.

dotnet run --urls "http://localhost:5182"
