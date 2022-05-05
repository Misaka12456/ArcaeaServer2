using Team123it.Arcaea.MarveCube.LinkPlay.Models;


namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public static class LinkPlayParser
    {
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
                CharacterUncapped = data[37]
            };
        }
    }
}