namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public class LinkPlayConstructor
    {
        public static LinkPlayConstructor CreateInstance()
        {
            return new LinkPlayConstructor();
        }

        public static byte[] Command0C(Room room)
        {
            var returnedBytes = new List<byte>();
            var packPrefix = new byte[] {0x06, 0x16, 0x0C, 0x09};
            returnedBytes.AddRange(packPrefix);
            returnedBytes.AddRange(BytesHelper.Ulong2Bytes(room.RoomId));
            returnedBytes.AddRange(BytesHelper.Uint2Bytes(room.CommandQueueLength));
            returnedBytes.AddRange(BytesHelper.Ulong2Bytes(room.Players[0].LastTimestamp));
            returnedBytes.AddRange(BytesHelper.Int2Bytes((int)room.RoomState)[..1]);
            returnedBytes.AddRange(BytesHelper.Uint2Bytes(room.CountDown));
            returnedBytes.AddRange(BytesHelper.Ulong2Bytes(room.Timestamp));
            return returnedBytes.ToArray();
        }

        public static byte[] Command12(Room room, uint playerIndex)
        {
            var returnedBytes = new List<byte>();
            var player = room.Players[playerIndex];
            var packPrefix = new byte[] {0x06, 0x16, 0x12, 0x09};
            returnedBytes.AddRange(packPrefix);
            returnedBytes.AddRange(BytesHelper.Ulong2Bytes(room.RoomId));
            returnedBytes.AddRange(BytesHelper.Uint2Bytes(room.CommandQueueLength));
            returnedBytes.AddRange(BytesHelper.Ulong2Bytes(player.LastTimestamp));
            returnedBytes.AddRange(BitConverter.GetBytes(playerIndex)[..1]);
            returnedBytes.AddRange(BytesHelper.Ulong2Bytes(player.PlayerId));
            returnedBytes.AddRange(BytesHelper.Int2Bytes(player.CharacterId)[..1]);
            returnedBytes.AddRange(BytesHelper.Uint2Bytes(player.IsCharacterUncapped)[..1]);
            returnedBytes.AddRange(BytesHelper.Int2Bytes((int)player.Difficulty)[..1]);
            returnedBytes.AddRange(BytesHelper.Uint2Bytes(player.Score));
            returnedBytes.AddRange(BytesHelper.Uint2Bytes(player.Timer));
            returnedBytes.AddRange(BytesHelper.Int2Bytes((int)player.ClearType)[..1]);
            returnedBytes.AddRange(BytesHelper.Uint2Bytes((uint)player.PlayerState)[..1]);
            returnedBytes.AddRange(BytesHelper.Int2Bytes(player.DownloadPercent)[..1]);
            returnedBytes.AddRange(BytesHelper.Int2Bytes(player.OnlineState)[..1]);
            return returnedBytes.ToArray();
        }

        public static byte[] Command13(Room room)
        {
            var returnedBytes = new List<byte>();
            var packPrefix = new byte[] {0x06, 0x16, 0x13, 0x09};
            returnedBytes.AddRange(packPrefix);
            returnedBytes.AddRange(BytesHelper.Ulong2Bytes(room.RoomId));
            returnedBytes.AddRange(BytesHelper.Uint2Bytes(room.CommandQueueLength));
            returnedBytes.AddRange(BytesHelper.Ulong2Bytes(room.Players[0].LastTimestamp));
            returnedBytes.AddRange(BytesHelper.Ulong2Bytes(room.HostId));
            returnedBytes.AddRange(BytesHelper.Int2Bytes((int)room.RoomState)[..1]);
            returnedBytes.AddRange(BytesHelper.Uint2Bytes(room.CountDown));
            returnedBytes.AddRange(BytesHelper.Ulong2Bytes(room.Timestamp));
            returnedBytes.AddRange(BytesHelper.Ushort2Bytes(room.SongIdx));
            returnedBytes.AddRange(BytesHelper.Ushort2Bytes(room.Interval));
            returnedBytes.AddRange(BytesHelper.Ulong2Bytes(room.Times)[..7]);
            returnedBytes.AddRange(room.GetPlayerLastScore());
            returnedBytes.AddRange(BytesHelper.Ushort2Bytes(room.LastSongIdx));
            returnedBytes.AddRange(BytesHelper.Int2Bytes(room.RoundSwitch)[..1]);
            return returnedBytes.ToArray();
        }

        public static byte[] Command15(Room room)
        {
            var returnedBytes = new List<byte>();
            var packPrefix = new byte[] {0x06, 0x16, 0x15, 0x09};
            returnedBytes.AddRange(packPrefix);
            returnedBytes.AddRange(BytesHelper.Ulong2Bytes(room.RoomId));
            returnedBytes.AddRange(BytesHelper.Uint2Bytes(room.CommandQueueLength));
            returnedBytes.AddRange(room.GetPlayerInfo());
            returnedBytes.AddRange(room.SongUnlock);
            returnedBytes.AddRange(BytesHelper.Ulong2Bytes(room.HostId));
            returnedBytes.AddRange(BytesHelper.Int2Bytes((int)room.RoomState)[..1]);
            returnedBytes.AddRange(BytesHelper.Uint2Bytes(room.CountDown));
            returnedBytes.AddRange(BytesHelper.Ulong2Bytes(room.Timestamp));
            returnedBytes.AddRange(BytesHelper.Ushort2Bytes(room.SongIdx));
            returnedBytes.AddRange(BytesHelper.Ushort2Bytes(room.Interval));
            returnedBytes.AddRange(BytesHelper.Ulong2Bytes(room.Times)[..7]);
            returnedBytes.AddRange(room.GetPlayerLastScore());
            returnedBytes.AddRange(BytesHelper.Ushort2Bytes(room.LastSongIdx));
            returnedBytes.AddRange(BytesHelper.Int2Bytes(room.RoundSwitch)[..1]);
            return returnedBytes.ToArray();
        }
    }
}