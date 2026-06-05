using System;
using System.IO;

namespace HeavyGear.Net
{
    /// <summary>
    /// Abstracts the network transport so game code is independent of the
    /// underlying library (LiteNetLib, local stub, etc.).
    /// </summary>
    public interface ITransport : IDisposable
    {
        bool IsHost { get; }
        bool IsConnected { get; }
        int PlayerCount { get; }

        /// <summary>Raised when a remote player joins. Args: (playerIndex, displayName)</summary>
        event Action<int, string> PlayerJoined;

        /// <summary>Raised when a player leaves. Arg: playerIndex</summary>
        event Action<int> PlayerLeft;

        /// <summary>Raised when the session ends.</summary>
        event Action SessionEnded;

        /// <summary>Raised when a packet arrives. Args: (reader, senderIndex)</summary>
        event Action<BinaryReader, int> PacketReceived;

        void SendToAll(byte[] data, bool reliable = true);
        void SendToHost(byte[] data, bool reliable = true);

        /// <summary>Poll for incoming packets; should be called once per frame.</summary>
        void Update();

        void StartGame();
    }
}
