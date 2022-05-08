// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Team123it.Arcaea.MarveCube.LinkPlay.Models
{
    public class ClientPack04
    {
        public byte[]? Prefix { get; set; } // [0, 4) {0x06, 0x16, 0x08, 0x09}
        public byte[]? Token { get; set; } // [4..12) Player.Token
        public uint Counter { get; set; } // [12..16)
        public ulong ClientTime { get; set; } // [16..24)
        public ulong PlayerId { get; set; } // [24..32)
    }
    
    public class ClientPack08
    {
        public byte[]? Prefix { get; set; } // [0, 4) {0x06, 0x16, 0x08, 0x09}
        public byte[]? Token { get; set; } // [4..12) Player.Token
        public uint Counter { get; set; } // [12..16)
        public ulong ClientTime { get; set; } // [16..24)
        public bool RobinEnabled { get; set; } // [24]
    }
    //<summary>
    // Packet structure for the ping packet.
    //</summary>
    public class ClientPack09
    {
        public byte[]? Prefix { get; set; } // [0, 4) {0x06, 0x16, 0x09, 0x09}
        public byte[]? Token { get; set; } // [4..12) Player.Token
        public uint Counter { get; set; } // [12..16)
        public ulong ClientTime { get; set; } // [16..24)
        public uint Score { get; set; } // [24..28)
        public uint SongTime { get; set; } // [28..32)
        public uint State { get; set; } // [32]

        public int Difficulty { get; set; } // [33]
        public uint ClearType { get; set; } // [34]
        public int DownloadProgress { get; set; } // [35]

        public int Character { get; set; } // [36]
        public bool CharacterUncapped { get; set; } // [37]
    }

    //<summary>
    // Packet structure for the leave room packet.
    //</summary>
    public class ClientPack0A
    {
        public byte[]? Prefix { get; set; } // [0, 4) {0x06, 0x16, 0x0A, 0x09}
        public byte[]? Token { get; set; } // [4..12) Player.Token
        public uint Counter { get; set; } // [12..16)
    }
    
    //<summary>
    // Packet structure for the leave room packet.
    //</summary>
    public class ClientPack0B
    {
        public byte[]? Prefix { get; set; } // [0, 4) {0x06, 0x16, 0x0A, 0x09}
        public byte[]? Token { get; set; } // [4..12) Player.Token
        public uint Counter { get; set; } // [12..16)
        public short SongIdx { get; set; } // [16..18)
    }
}