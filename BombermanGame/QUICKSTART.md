# 🚀 Bomberman Multiplayer - Quick Start Guide

## ⚡ Hızlı Başlangıç (5 dakika)

### Windows Kullanıcıları

```batch
# 1. Kurulum
setup.bat

# 2. Oyunu Çalıştır
run.bat
```

### Linux/Mac Kullanıcıları

```bash
# 1. Çalıştırma izni ver
chmod +x setup.sh run.sh

# 2. Kurulum
./setup.sh

# 3. Oyunu Çalıştır
./run.sh
```

---

## 🎮 İlk Oyun

### 1. Hesap Oluştur
```
Main Menu → 2. Register
- Kullanıcı adı: hero
- Şifre: 123456
```

### 2. Giriş Yap
```
Main Menu → 1. Login
- Kullanıcı adı: hero
- Şifre: 123456
```

### 3. Oyunu Başlat
```
Game Menu → 1. Start Single Player Game
- Tema seç: Desert
- Başla!
```

---

## ⌨️ Kontroller

### Oyuncu 1
```
W / ↑   : Yukarı
S / ↓   : Aşağı
A / ←   : Sol
D / →   : Sağ
SPACE   : Bomba Koy
```

### Oyuncu 2 (İki Oyunculu)
```
I       : Yukarı
K       : Aşağı
J       : Sol
L       : Sağ
ENTER   : Bomba Koy
```

### Genel
```
ESC     : Menüye Dön
U       : Geri Al (Undo)
```

---

## 🎯 Oyun Mekaniği

### Temel Kurallar
1. Bombalar 3 saniye sonra patlar
2. Patlama 4 yöne yayılır
3. Düşmanlara değme!
4. Duvarları yıkarak power-up'lar bul

### Power-ups
- **B** = Bomba sayısı +1
- **P** = Bomba gücü +1
- **S** = Hız artışı +1

### Düşmanlar
- **E** = Statik (hareket etmez)
- **C** = Takipçi (basit AI)
- **A** = Akıllı (A* algoritması)

---

## 🌐 Multiplayer (Online)

### Host Olarak

1. Game Menu → 3. Multiplayer
2. 1. Host Game
3. IP adresini arkadaşına ver
4. Bekle...

### Client Olarak

1. Game Menu → 3. Multiplayer
2. 2. Join Game
3. Host'un IP'sini gir
4. Bağlan!

**IP Öğrenme:**
```bash
# Windows
ipconfig

# Linux/Mac
ifconfig
```

---

## ❓ Sorun Giderme

### "dotnet bulunamadı"
```bash
# .NET 7.0 SDK indirin
https://dotnet.microsoft.com/download
```

### "Database hatası"
```bash
# Database'i sıfırla
del bomberman.db  # Windows
rm bomberman.db   # Linux/Mac

# Tekrar çalıştır
dotnet run
```

### "Port zaten kullanımda" (Multiplayer)
```
- Farklı port deneyin (örn: 9998, 10000)
- Veya diğer programı kapatın
```

### "Bağlantı kurulamadı" (Multiplayer)
```
✓ Her iki bilgisayar aynı ağda mı?
✓ Firewall izin veriyor mu?
✓ IP adresi doğru mu?
✓ Host önce başladı mı?
```

---

## 📚 Daha Fazla Bilgi

- **README.md** - Genel bakış ve özellikler
- **DesignDocument.md** - Tasarım kalıpları açıklaması
- **UMLDiagrams.md** - UML diyagramları

---

## 🎓 Öğrenme İpuçları

### Yeni Başlayanlar
1. Tek oyunculu modla başlayın
2. Kontrolleri öğrenin
3. Power-up'ları toplayın
4. Düşman davranışlarını gözlemleyin

### İleri Seviye
1. İki oyunculu modda pratik yapın
2. Farklı temaları deneyin
3. Undo özelliğini kullanın
4. Online multiplayer oynayın

---

## 💡 İpuçları

### Strateji
- Köşelere sıkışma!
- Bomba patlamadan kaç
- Power-up'ları topla
- Düşman yollarını öğren

### Skoru Yükselt
- Duvar yık: +10
- Düşman öldür: +50
- Power-up topla: +25
- Hızlı bitir: Bonus!

---

## 🏆 İlk Hedefler

- [ ] İlk oyunu bitir
- [ ] 500+ skor yap
- [ ] 5 düşman öldür
- [ ] Tüm power-up'ları dene
- [ ] Arkadaşınla online oyna

---

**Oyunun tadını çıkar! 🎮💣**

Son Güncelleme: 18 Aralık 2025