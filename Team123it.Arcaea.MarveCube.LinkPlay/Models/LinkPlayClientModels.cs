// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Team123it.Arcaea.MarveCube.LinkPlay.Models
{
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
        public uint CharacterUncapped { get; set; } // [37]
    }
}