@echo off
chcp 65001 >nul
color 0A

echo ╔══════════════════════════════════════════════════════════════╗
echo ║        Bomberman Multiplayer - Starting Game                ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

REM .NET kontrolü
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ❌ .NET SDK bulunamadi!
    echo Lutfen setup.bat'i calistirin.
    pause
    exit /b 1
)

echo 🎮 Oyun baslatiliyor...
echo.

dotnet run

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ Oyun baslatilirken hata olustu!
    pause
    exit /b 1
)

pause