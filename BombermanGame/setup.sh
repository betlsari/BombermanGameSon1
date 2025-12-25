#!/bin/bash

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║        Bomberman Multiplayer - Kurulum Script'i             ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""

# .NET 6.0 kurulu mu kontrol et
if ! command -v dotnet &> /dev/null
then
    echo "❌ .NET 6.0 SDK bulunamadı!"
    echo "Lütfen https://dotnet.microsoft.com/download adresinden .NET 6.0 SDK'yı indirin."
    exit 1
fi

echo "✅ .NET SDK bulundu: $(dotnet --version)"
echo ""

# Proje dizinine git
if [ ! -f "BombermanGame.csproj" ]; then
    echo "❌ BombermanGame.csproj bulunamadı!"
    echo "Lütfen script'i proje kök dizininde çalıştırın."
    exit 1
fi

echo "📦 NuGet paketleri yükleniyor..."
dotnet restore

if [ $? -ne 0 ]; then
    echo "❌ Paket yükleme başarısız!"
    exit 1
fi

echo "✅ Paketler başarıyla yüklendi"
echo ""

echo "🔨 Proje derleniyor..."
dotnet build

if [ $? -ne 0 ]; then
    echo "❌ Derleme başarısız!"
    exit 1
fi

echo "✅ Proje başarıyla derlendi"
echo ""

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║              Kurulum Tamamlandı! 🎉                         ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""
echo "Projeyi çalıştırmak için:"
echo "  $ dotnet run"
echo ""
echo "veya"
echo "  $ ./run.sh"
echo ""