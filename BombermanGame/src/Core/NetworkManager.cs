// Core/NetworkManager.cs - DÜZELTİLMİŞ VERSİYON - Bağlantı sorunları giderildi
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BombermanGame.src.Models;
using BombermanGame.src.Utils;

namespace BombermanGame.src.Core
{
    /// <summary>
    /// Network yöneticisi - TCP tabanlı multiplayer desteği
    /// DÜZELTİLDİ: Timeout, hata kontrolü ve debug mesajları eklendi
    /// </summary>
    public class NetworkManager
    {
        private TcpListener? _server;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private bool _isServer;
        private bool _isConnected;
        private Thread? _listenerThread;
        private string _playerId;
        private int _connectionTimeout = 30000; // 30 saniye timeout

        public event Action<NetworkMessage>? OnMessageReceived;
        public event Action<string>? OnPlayerConnected;
        public event Action<string>? OnPlayerDisconnected;

        public bool IsConnected => _isConnected;
        public bool IsServer => _isServer;

        public NetworkManager()
        {
            _playerId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Sunucu olarak başlat - DÜZELTİLDİ
        /// </summary>
        public async Task StartServer(int port = 9999)
        {
            try
            {
                _isServer = true;

                // Önce portun kullanılabilir olduğundan emin ol
                _server = new TcpListener(IPAddress.Any, port);
                _server.Start(1); // Sadece 1 bağlantı bekle

                Console.WriteLine($"╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"║              SERVER BAŞLATILDI                               ║");
                Console.WriteLine($"╚══════════════════════════════════════════════════════════════╝\n");

                // Local IP'leri göster
                var localIPs = GetLocalIPAddresses();
                Console.WriteLine("📡 Kullanılabilir IP Adresleri:");
                foreach (var ip in localIPs)
                {
                    Console.WriteLine($"   • {ip}:{port}");
                }
                Console.WriteLine($"\n🔌 Port: {port}");
                Console.WriteLine($"👤 Server ID: {_playerId.Substring(0, 8)}...");
                Console.WriteLine($"\n⏳ Oyuncu bekleniyor...\n");

                // Timeout ile bağlantı kabul et
                var acceptTask = _server.AcceptTcpClientAsync();
                var timeoutTask = Task.Delay(_connectionTimeout);

                var completedTask = await Task.WhenAny(acceptTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    Console.WriteLine("❌ Timeout: Hiçbir oyuncu bağlanmadı (30 saniye)");
                    _server.Stop();
                    return;
                }

                _client = await acceptTask;
                _client.NoDelay = true; // Nagle algoritmasını devre dışı bırak (daha hızlı)
                _stream = _client.GetStream();
                _isConnected = true;

                var remoteEndPoint = _client.Client.RemoteEndPoint as IPEndPoint;
                Console.WriteLine($"\n✅ Oyuncu bağlandı!");
                Console.WriteLine($"   IP: {remoteEndPoint?.Address}");
                Console.WriteLine($"   Port: {remoteEndPoint?.Port}");

                // Dinleme thread'ini başlat
                _listenerThread = new Thread(ListenForMessages);
                _listenerThread.IsBackground = true;
                _listenerThread.Start();

                // Karşılama mesajı gönder
                var welcomeMsg = NetworkProtocol.CreateConnectMessage(_playerId, "Server");
                await SendMessageAsync(welcomeMsg);

                Console.WriteLine("📨 Karşılama mesajı gönderildi");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"\n❌ Socket Hatası:");
                Console.WriteLine($"   Kod: {ex.ErrorCode}");
                Console.WriteLine($"   Mesaj: {ex.Message}");

                if (ex.ErrorCode == 10048) // Port zaten kullanımda
                {
                    Console.WriteLine($"\n⚠️  Port {port} zaten kullanımda!");
                    Console.WriteLine("   Çözüm: Farklı bir port deneyin veya diğer programı kapatın.");
                }
                else if (ex.ErrorCode == 10013) // Erişim engellendi
                {
                    Console.WriteLine($"\n⚠️  Firewall tarafından engellendi!");
                    Console.WriteLine("   Çözüm: Windows Firewall'da uygulamaya izin verin.");
                }

                _isConnected = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Sunucu başlatma hatası: {ex.Message}");
                Console.WriteLine($"   Detay: {ex.GetType().Name}");
                _isConnected = false;
            }
        }

        /// <summary>
        /// İstemci olarak bağlan - DÜZELTİLDİ
        /// </summary>
        public async Task ConnectToServer(string host, int port = 9999)
        {
            try
            {
                _isServer = false;
                _client = new TcpClient();
                _client.NoDelay = true; // Nagle algoritmasını devre dışı bırak

                Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║              SUNUCUYA BAĞLANILIYOR                           ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");
                Console.WriteLine($"🎯 Hedef: {host}:{port}");
                Console.WriteLine($"👤 Client ID: {_playerId.Substring(0, 8)}...");
                Console.WriteLine($"\n⏳ Bağlanıyor...\n");

                // Timeout ile bağlan
                var connectTask = _client.ConnectAsync(host, port);
                var timeoutTask = Task.Delay(10000); // 10 saniye timeout

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    Console.WriteLine("❌ Timeout: Sunucuya bağlanılamadı (10 saniye)");
                    Console.WriteLine("\n🔍 Kontrol Listesi:");
                    Console.WriteLine("   1. IP adresi doğru mu?");
                    Console.WriteLine("   2. Sunucu çalışıyor mu?");
                    Console.WriteLine("   3. Firewall engelliyor mu?");
                    Console.WriteLine("   4. Aynı ağda mısınız?");
                    _client.Close();
                    return;
                }

                await connectTask; // Bağlantıyı tamamla

                _stream = _client.GetStream();
                _isConnected = true;

                Console.WriteLine("✅ Sunucuya bağlanıldı!");

                // Dinleme thread'ini başlat
                _listenerThread = new Thread(ListenForMessages);
                _listenerThread.IsBackground = true;
                _listenerThread.Start();

                // Bağlantı mesajı gönder
                var connectMsg = NetworkProtocol.CreateConnectMessage(_playerId, "Client");
                await SendMessageAsync(connectMsg);

                Console.WriteLine("📨 Bağlantı mesajı gönderildi");

                // Latency ölç
                Thread.Sleep(500); // Sunucunun hazır olması için bekle
                long latency = await MeasureLatencyAsync();
                if (latency > 0)
                {
                    Console.WriteLine($"🌐 Gecikme: {latency}ms");
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"\n❌ Bağlantı Hatası:");
                Console.WriteLine($"   Kod: {ex.ErrorCode}");
                Console.WriteLine($"   Mesaj: {ex.Message}");

                if (ex.ErrorCode == 10061) // Bağlantı reddedildi
                {
                    Console.WriteLine("\n⚠️  Sunucu bulunamadı veya erişilemedi!");
                    Console.WriteLine("   Muhtemel Sebepler:");
                    Console.WriteLine("   • Sunucu çalışmıyor");
                    Console.WriteLine("   • Yanlış IP adresi");
                    Console.WriteLine("   • Firewall engelliyor");
                }
                else if (ex.ErrorCode == 10060) // Bağlantı zaman aşımı
                {
                    Console.WriteLine("\n⚠️  Bağlantı zaman aşımına uğradı!");
                    Console.WriteLine("   • Ağ bağlantısını kontrol edin");
                    Console.WriteLine("   • Farklı ağdaysanız port forwarding gerekebilir");
                }
                else if (ex.ErrorCode == 11001) // Host bulunamadı
                {
                    Console.WriteLine("\n⚠️  Host bulunamadı!");
                    Console.WriteLine("   • IP adresini kontrol edin");
                }

                _isConnected = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Bağlantı hatası: {ex.Message}");
                Console.WriteLine($"   Tip: {ex.GetType().Name}");
                _isConnected = false;
            }
        }

        /// <summary>
        /// Mesaj gönder - DÜZELTİLDİ
        /// </summary>
        public async Task SendMessageAsync(NetworkMessage message)
        {
            if (_stream == null || !_isConnected)
            {
                Console.WriteLine("[NETWORK] ❌ Mesaj gönderilemedi: Bağlantı yok");
                return;
            }

            try
            {
                string json = NetworkProtocol.SerializeMessage(message);
                byte[] data = Encoding.UTF8.GetBytes(json + "\n");

                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();

                // Debug
                // Console.WriteLine($"[NETWORK] 📤 Gönderildi: {message.Type}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NETWORK] ❌ Gönderme hatası: {ex.Message}");
                Disconnect();
            }
        }

        /// <summary>
        /// Oyuncu hareketini gönder
        /// </summary>
        public async Task SendPlayerMove(Player player)
        {
            var msg = NetworkProtocol.CreateMoveMessage(player.Id.ToString(), player.Position);
            await SendMessageAsync(msg);
        }

        /// <summary>
        /// Bomba yerleştirme mesajı gönder
        /// </summary>
        public async Task SendPlaceBomb(Bomb bomb)
        {
            var msg = NetworkProtocol.CreatePlaceBombMessage(bomb.OwnerId.ToString(), bomb);
            await SendMessageAsync(msg);
        }

        /// <summary>
        /// Oyun durumunu gönder (sunucu için)
        /// </summary>
        public async Task SendGameState(GameManager gameManager)
        {
            if (!_isServer) return;

            var msg = NetworkProtocol.CreateGameStateMessage(gameManager);
            await SendMessageAsync(msg);
        }

        /// <summary>
        /// Ping gönder
        /// </summary>
        public async Task SendPing()
        {
            var msg = NetworkProtocol.CreatePingMessage(_playerId);
            await SendMessageAsync(msg);
        }

        /// <summary>
        /// Mesajları dinle (background thread) - DÜZELTİLDİ
        /// </summary>
        private void ListenForMessages()
        {
            byte[] buffer = new byte[8192];
            StringBuilder messageBuilder = new StringBuilder();

            Console.WriteLine("[NETWORK] 🎧 Dinleme başladı");

            while (_isConnected && _stream != null)
            {
                try
                {
                    // Stream'in okunabilir olduğundan emin ol
                    if (!_stream.CanRead)
                    {
                        Console.WriteLine("[NETWORK] ⚠️  Stream okunamıyor");
                        break;
                    }

                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("[NETWORK] ⚠️  Bağlantı karşı tarafça kapatıldı");
                        Disconnect();
                        break;
                    }

                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    messageBuilder.Append(data);

                    // Mesajları ayır (newline delimiter)
                    string[] messages = messageBuilder.ToString().Split('\n');

                    // Son parça tam mesaj değilse sakla
                    messageBuilder.Clear();
                    if (!data.EndsWith("\n"))
                    {
                        messageBuilder.Append(messages[messages.Length - 1]);
                    }

                    // Tam mesajları işle
                    for (int i = 0; i < messages.Length - 1; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(messages[i]))
                        {
                            ProcessMessage(messages[i]);
                        }
                    }
                }
                catch (System.IO.IOException ioEx)
                {
                    if (_isConnected)
                    {
                        Console.WriteLine($"[NETWORK] ❌ IO Hatası: {ioEx.Message}");
                        Disconnect();
                    }
                    break;
                }
                catch (Exception ex)
                {
                    if (_isConnected)
                    {
                        Console.WriteLine($"[NETWORK] ❌ Dinleme hatası: {ex.Message}");
                        Disconnect();
                    }
                    break;
                }
            }

            Console.WriteLine("[NETWORK] 🎧 Dinleme sona erdi");
        }

        /// <summary>
        /// Gelen mesajı işle
        /// </summary>
        private void ProcessMessage(string json)
        {
            try
            {
                var message = NetworkProtocol.DeserializeMessage(json);

                if (message == null || !NetworkProtocol.ValidateMessage(message))
                {
                    Console.WriteLine("[NETWORK] ⚠️  Geçersiz mesaj alındı");
                    return;
                }

                // Debug
                // Console.WriteLine($"[NETWORK] 📥 Alındı: {message.Type}");

                // Mesaj tipine göre işlem yap
                switch (message.Type)
                {
                    case MessageType.Connect:
                        Console.WriteLine($"[NETWORK] ✅ Oyuncu bağlandı: {message.SenderId.Substring(0, 8)}...");
                        OnPlayerConnected?.Invoke(message.SenderId);
                        break;

                    case MessageType.Disconnect:
                        Console.WriteLine($"[NETWORK] ❌ Oyuncu ayrıldı: {message.SenderId.Substring(0, 8)}...");
                        OnPlayerDisconnected?.Invoke(message.SenderId);
                        break;

                    case MessageType.Ping:
                        // Pong gönder
                        Task.Run(async () =>
                        {
                            var pong = NetworkProtocol.CreatePongMessage(_playerId);
                            await SendMessageAsync(pong);
                        });
                        break;

                    case MessageType.Pong:
                        // Ping yanıtı alındı
                        break;

                    default:
                        // Diğer mesajları event ile bildir
                        OnMessageReceived?.Invoke(message);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NETWORK] ❌ Mesaj işleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Bağlantıyı kes
        /// </summary>
        public void Disconnect()
        {
            if (!_isConnected) return;

            _isConnected = false;

            try
            {
                // Disconnect mesajı gönder
                var disconnectMsg = NetworkProtocol.CreateDisconnectMessage(_playerId);
                SendMessageAsync(disconnectMsg).Wait(1000);
            }
            catch { }

            // Kaynakları temizle
            try
            {
                _stream?.Close();
                _client?.Close();
                _server?.Stop();
            }
            catch { }

            Console.WriteLine("\n[NETWORK] 🔌 Bağlantı kesildi");
        }

        /// <summary>
        /// Bağlantı durumunu kontrol et
        /// </summary>
        public bool CheckConnection()
        {
            if (_client == null || !_client.Connected)
            {
                _isConnected = false;
                return false;
            }

            return _isConnected;
        }

        /// <summary>
        /// Latency ölç (ping-pong)
        /// </summary>
        public async Task<long> MeasureLatencyAsync()
        {
            var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var pingReceived = false;
            Action<NetworkMessage> handler = (msg) =>
            {
                if (msg.Type == MessageType.Pong)
                {
                    pingReceived = true;
                }
            };

            OnMessageReceived += handler;
            await SendPing();

            // Pong yanıtını bekle (max 5 saniye)
            var timeout = 5000;
            var elapsed = 0;
            while (!pingReceived && elapsed < timeout)
            {
                await Task.Delay(10);
                elapsed += 10;
            }

            OnMessageReceived -= handler;

            if (!pingReceived)
            {
                return -1; // Timeout
            }

            var endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return endTime - startTime;
        }

        /// <summary>
        /// Tüm local IP adreslerini al - DÜZELTİLDİ
        /// </summary>
        private List<string> GetLocalIPAddresses()
        {
            var ipList = new List<string>();

            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipList.Add(ip.ToString());
                    }
                }
            }
            catch
            {
                ipList.Add("127.0.0.1");
            }

            if (ipList.Count == 0)
            {
                ipList.Add("127.0.0.1");
            }

            return ipList;
        }
    }
}