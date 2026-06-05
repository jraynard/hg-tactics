using System;
using System.IO;

namespace HeavyGear.Net
{
    /// <summary>
    /// No-op transport used when no network session is active (single-player /
    /// before a session is created). Keeps game code from having to null-check
    /// everywhere during early porting phases.
    /// </summary>
    public sealed class NullTransport : ITransport
    {
        public static readonly NullTransport Instance = new NullTransport();

        public bool IsHost => true;
        public bool IsConnected => false;
        public int PlayerCount => 1;

        public event Action<int, string> PlayerJoined { add { } remove { } }
        public event Action<int> PlayerLeft { add { } remove { } }
        public event Action SessionEnded { add { } remove { } }
        public event Action<BinaryReader, int> PacketReceived { add { } remove { } }

        public void SendToAll(byte[] data, bool reliable = true) { }
        public void SendToHost(byte[] data, bool reliable = true) { }
        public void Update() { }
        public void StartGame() { }
        public void Dispose() { }
    }
}
