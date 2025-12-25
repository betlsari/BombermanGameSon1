@echo off
chcp 65001 >nul
color 0A

echo ╔══════════════════════════════════════════════════════════════╗
echo ║        Bomberman Multiplayer - Kurulum Script'i             ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

REM .NET 6.0 kontrolü
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ❌ .NET 6.0 SDK bulunamadi!
    echo Lutfen https://dotnet.microsoft.com/download adresinden .NET 6.0 SDK'yi indirin.
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo ✅ .NET SDK bulundu: %DOTNET_VERSION%
echo.

REM Proje dosyası kontrolü
if not exist "BombermanGame.csproj" (
    echo ❌ BombermanGame.csproj bulunamadi!
    echo Lutfen script'i proje kok dizininde calistirin.
    pause
    exit /b 1
)

echo 📦 NuGet paketleri yukleniyor...
dotnet restore

if %ERRORLEVEL% NEQ 0 (
    echo ❌ Paket yukleme basarisiz!
    pause
    exit /b 1
)

echo ✅ Paketler basariyla yuklendi
echo.

echo 🔨 Proje derleniyor...
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo ❌ Derleme basarisiz!
    pause
    exit /b 1
)

echo ✅ Proje basariyla derlendi
echo.

echo ╔══════════════════════════════════════════════════════════════╗
echo ║              Kurulum Tamamlandi! 🎉                         ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.
echo Projeyi calistirmak icin:
echo   run.bat
echo.
pause