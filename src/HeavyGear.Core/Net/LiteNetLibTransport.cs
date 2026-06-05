using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HeavyGear.Net
{
    /// <summary>
    /// LAN transport backed by LiteNetLib 1.3.x UDP.
    ///
    /// Host  : LiteNetLibTransport.CreateHost(port)
    /// Client: LiteNetLibTransport.Connect(address, port)
    ///
    /// Discovery: call DiscoverLAN(port) on a background thread; it fires
    /// SessionDiscovered for each host that responds within DiscoveryTimeoutMs ms.
    /// </summary>
    public sealed class LiteNetLibTransport : ITransport, INetEventListener
    {
        // ── Constants ─────────────────────────────────────────────────────
        public const int DefaultPort = 7777;
        public const int DiscoveryTimeoutMs = 1500;
        private const string ConnectionKey = "HeavyGear";

        // ── State ─────────────────────────────────────────────────────────
        private readonly NetManager _manager;
        private readonly bool _isHost;
        private bool _gameStarted;
        private bool _disposed;

        // ── ITransport ────────────────────────────────────────────────────
        public bool IsHost => _isHost;
        public bool IsConnected => _manager.ConnectedPeersCount > 0;
        public int PlayerCount => _isHost ? _manager.ConnectedPeersCount + 1 : 2;

        public event Action<int, string> PlayerJoined;
        public event Action<int> PlayerLeft;
        public event Action SessionEnded;
        public event Action<BinaryReader, int> PacketReceived;

        // ── Discovery ─────────────────────────────────────────────────────
        /// <summary>Fired on any thread when a LAN host is found during DiscoverLAN.</summary>
        public static event Action<IPEndPoint, string> SessionDiscovered;

        // ── Factory ───────────────────────────────────────────────────────
        /// <summary>Start hosting on the given port.</summary>
        public static LiteNetLibTransport CreateHost(int port = DefaultPort)
        {
            var t = new LiteNetLibTransport(isHost: true);
            t._manager.Start(port);
            return t;
        }

        /// <summary>Connect to a specific host as a client.</summary>
        public static LiteNetLibTransport Connect(string address, int port = DefaultPort)
        {
            var t = new LiteNetLibTransport(isHost: false);
            t._manager.Start();
            t._manager.Connect(address, port, ConnectionKey);
            return t;
        }

        /// <summary>
        /// Broadcast a UDP discovery request and collect responses for DiscoveryTimeoutMs ms.
        /// Fires SessionDiscovered for each host found. Safe to call on a background thread.
        /// </summary>
        public static void DiscoverLAN(int port = DefaultPort)
        {
            var listener = new EventBasedNetListener();
            var probe = new NetManager(listener) { UnconnectedMessagesEnabled = true };
            probe.Start();

            listener.NetworkReceiveUnconnectedEvent += (ep, reader, type) =>
            {
                if (type != UnconnectedMessageType.BasicMessage) return;
                string name = reader.AvailableBytes > 0 ? reader.GetString() : ep.ToString();
                reader.Recycle();
                SessionDiscovered?.Invoke(ep, name);
            };

            try
            {
                var writer = new NetDataWriter();
                writer.Put("discover");
                probe.SendUnconnectedMessage(writer, new IPEndPoint(IPAddress.Broadcast, port));

                int elapsed = 0;
                while (elapsed < DiscoveryTimeoutMs)
                {
                    probe.PollEvents();
                    Thread.Sleep(50);
                    elapsed += 50;
                }
            }
            finally
            {
                probe.Stop();
            }
        }

        // ── Constructor ───────────────────────────────────────────────────
        private LiteNetLibTransport(bool isHost)
        {
            _isHost = isHost;
            _manager = new NetManager(this) { UnconnectedMessagesEnabled = true };
        }

        // ── ITransport ────────────────────────────────────────────────────
        public void Update() => _manager?.PollEvents();

        public void SendToAll(byte[] data, bool reliable = true)
        {
            var method = reliable ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable;
            _manager.SendToAll(data, method);
        }

        public void SendToHost(byte[] data, bool reliable = true)
        {
            var method = reliable ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable;
            _manager.FirstPeer?.Send(data, method);
        }

        public void StartGame()
        {
            if (!_isHost || _gameStarted) return;
            _gameStarted = true;
            var writer = new NetDataWriter();
            writer.Put((byte)0xFF);
            _manager.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _manager?.Stop();
        }

        // ── INetEventListener ─────────────────────────────────────────────
        public void OnPeerConnected(NetPeer peer)
        {
            int index = _manager.ConnectedPeersCount; // 1-based for remotes
            PlayerJoined?.Invoke(index, peer.Id.ToString());
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
        {
            PlayerLeft?.Invoke(peer.Id);
            if (_manager.ConnectedPeersCount == 0 && !_isHost)
                SessionEnded?.Invoke();
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            // Copy bytes before recycling the reader
            byte[] data = new byte[reader.AvailableBytes];
            reader.GetBytes(data, data.Length);
            reader.Recycle();

            // StartGame signal from host (0xFF)
            if (!_isHost && data.Length == 1 && data[0] == 0xFF)
            {
                _gameStarted = true;
                return;
            }

            int senderIndex = peer.Id;
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);
            PacketReceived?.Invoke(br, senderIndex);
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            if (_isHost)
                request.AcceptIfKey(ConnectionKey);
            else
                request.Reject();
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (!_isHost) return;
            if (messageType == UnconnectedMessageType.Broadcast && reader.AvailableBytes > 0)
            {
                string msg = reader.GetString();
                reader.Recycle();
                if (msg == "discover")
                {
                    var writer = new NetDataWriter();
                    writer.Put(System.Net.Dns.GetHostName());
                    _manager.SendUnconnectedMessage(writer, remoteEndPoint);
                }
            }
        }
    }
}
