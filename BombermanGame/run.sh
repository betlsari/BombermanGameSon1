#!/bin/bash

echo "╔══════════════════════════════════════════════════════════════╗"
echo "║        Bomberman Multiplayer - Starting Game                ║"
echo "╚══════════════════════════════════════════════════════════════╝"
echo ""

# .NET kontrolü
if ! command -v dotnet &> /dev/null
then
    echo "❌ .NET SDK bulunamadı!"
    echo "Lütfen setup.sh'yi çalıştırın."
    exit 1
fi

echo "🎮 Oyun başlatılıyor..."
echo ""

dotnet run

if [ $? -ne 0 ]; then
    echo ""
    echo "❌ Oyun başlatılırken hata oluştu!"
    exit 1
fi