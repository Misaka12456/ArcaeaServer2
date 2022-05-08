using Team123it.Arcaea.MarveCube.LinkPlay.Models;


namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public static class LinkPlayParser
    {
        public static ClientPack04 ParseClientPack04(byte[] data)
        {
            return new ClientPack04
            {
                Prefix = data[..4],
                Token = data[4..12],
                Counter = BitConverter.ToUInt32(data.AsSpan()[12..16]),
                ClientTime = BitConverter.ToUInt64(data.AsSpan()[16..24]),
                PlayerId = BitConverter.ToUInt64(data.AsSpan()[24..32])            
            };
        }

        //<summary>
        // Host transfer from the client, return ClientPack08
        //</summary>
        public static ClientPack08 ParseClientPack08(byte[] data)
        {
            return new ClientPack08
            {
                Prefix = data[..4],
                Token = data[4..12],
                Counter = BitConverter.ToUInt32(data.AsSpan()[12..16]),
                ClientTime = BitConverter.ToUInt64(data.AsSpan()[16..24]),
                RobinEnabled = BitConverter.ToBoolean(data.AsSpan()[24..])
            };
        }

        //<summary>
        // Ping from the client, return ClientPack09
        //</summary>
        public static ClientPack09 ParseClientPack09(byte[] data)
        {
            return new ClientPack09
            {
                Prefix = data[..4],
                Token = data[4..12],
                Counter = BitConverter.ToUInt32(data.AsSpan()[12..16]),
                ClientTime = BitConverter.ToUInt64(data.AsSpan()[16..24]),
                Score = BitConverter.ToUInt32(data.AsSpan()[24..28]),
                SongTime = BitConverter.ToUInt32(data.AsSpan()[28..32]),
                State = data[32],
                Difficulty = data[33],
                ClearType = data[34],
                DownloadProgress = data[35],
                Character = data[36],
                CharacterUncapped = BitConverter.ToBoolean(data[36..37])
            };
        }

        public static ClientPack0A ParseClientPack0A(byte[] data)
        {
            return new ClientPack0A
            {
                Prefix = data[..4],
                Token = data[4..12],
                Counter = BitConverter.ToUInt32(data.AsSpan()[12..16]),
            };
        }

        public static ClientPack0B ParseClientPack0B(byte[] data)
        {
            return new ClientPack0B
            {
                Prefix = data[..4],
                Token = data[4..12],
                Counter = BitConverter.ToUInt32(data.AsSpan()[12..16]),
                SongIdx = BitConverter.ToInt16(data.AsSpan()[16..18])
            };
        }
    }
}