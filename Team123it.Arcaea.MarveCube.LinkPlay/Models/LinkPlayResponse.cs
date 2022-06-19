using Team123it.Arcaea.MarveCube.LinkPlay.Entities;
using static System.BitConverter;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public static class LinkPlayResponse
    {
        public static byte[] Resp0CPing(this Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x0c, 0x09}); // [0, 4)
            returnedBytes.AddRange(GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(GetBytes(room.Counter)); // [12, 16)
            returnedBytes.AddRange(GetBytes(room.ClientTime)); // [16, 24)
            returnedBytes.Add((byte) room.RoomState); // [24]
            returnedBytes.AddRange(GetBytes(room.CountDown)); // [25, 29)
            returnedBytes.AddRange(GetBytes(Room.ServerTime)); // [29, 37)
            return returnedBytes.ToArray();
        }

        public static byte[] Resp0DRaiseErrors(this Room room, int errorCode)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x0d, 0x09}); // [0, 4)
            returnedBytes.AddRange(GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(GetBytes(room.Counter)); // [12, 16)
            returnedBytes.AddRange(GetBytes(room.ClientTime)); // [16, 24)

            returnedBytes.AddRange(GetBytes(errorCode)[..1]); // [24]
            return returnedBytes.ToArray();
        }

        public static byte[] Resp0EScoreUpdates(this Room room, int playerIndex)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x0e, 0x09}); // [0, 4)
            returnedBytes.AddRange(GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(GetBytes(room.Counter)); // [12, 16)
            returnedBytes.AddRange(PlayerInfoSchema(room.Players[playerIndex])); // [16, 39)
            returnedBytes.AddRange(GetBytes(room.Players[playerIndex].LastScore)); // [39, 43)
            returnedBytes.AddRange(GetBytes(room.Players[playerIndex].LastSongTime));
            return returnedBytes.ToArray();
        }

        public static byte[] Resp0FSongSuggestion(this Room room, int playerIndex, short songIdxWithDiff)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x0f, 0x09}); // [0, 4)
            returnedBytes.AddRange(GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(GetBytes(room.Counter)); // [12, 16)

            returnedBytes.AddRange(GetBytes(room.Players[playerIndex].PlayerId)); // [16, 24)
            returnedBytes.AddRange(GetBytes(songIdxWithDiff)); // [24, 26)
            return returnedBytes.ToArray();
        }

        public static byte[] Resp10HostTransfer(this Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x10, 0x09}); // [0, 4)
            returnedBytes.AddRange(GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(GetBytes(room.Counter)); // [12, 16)
            returnedBytes.AddRange(GetBytes(room.ClientTime)); // [16, 24)
            
            returnedBytes.AddRange(GetBytes(room.HostId)); // [24, 32)
            return returnedBytes.ToArray();
        }

        public static byte[] Resp11PlayerInfo(this Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x11, 0x09}); // [0, 4)
            returnedBytes.AddRange(GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(GetBytes(room.Counter)); // [12, 16)
            returnedBytes.AddRange(GetBytes(room.ClientTime)); // [16, 24)
            foreach (var player in room.Players)
            {
                returnedBytes.AddRange(PlayerInfoWithNameSchema(player)); // [24, 184)
            }
            if (room.Players.Count >= 4) return returnedBytes.ToArray();
            for (var i = 0; i < 4 - room.Players.Count; i++)
            {
                returnedBytes.AddRange(PlayerInfoWithNameSchema(new Player()));
            }
            return returnedBytes.ToArray();
        }

        public static byte[] Resp12PlayerUpdate(this Room room, int playerIndex, Player? player = null)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x12, 0x09}); // [0, 4)
            returnedBytes.AddRange(GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(GetBytes(room.Counter)); // [12, 16)
            returnedBytes.AddRange(GetBytes(room.ClientTime)); // [16, 24)
            returnedBytes.AddRange(GetBytes(playerIndex)[..1]); // [24]
            returnedBytes.AddRange(player == null
                ? PlayerInfoSchema(room.Players[playerIndex])
                : PlayerInfoSchema(player.Value));
            return returnedBytes.ToArray();
        }

        public static byte[] Resp13PartRoomInfo(this Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x13, 0x09}); // [0, 4)
            returnedBytes.AddRange(GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(GetBytes(room.Counter)); // [12, 16)
            returnedBytes.AddRange(GetBytes(room.ClientTime)); // [16, 24)
            returnedBytes.AddRange(RoomInfoWithHostSchema(room)); // [24, 86)
            return returnedBytes.ToArray();
        }

        public static byte[] Resp14SongMapUpdate(this Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x14, 0x09}); // [0, 4)
            returnedBytes.AddRange(GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(GetBytes(room.Counter)); // [12, 16)
            returnedBytes.AddRange(GetBytes(room.ClientTime)); // [16, 24)
            returnedBytes.AddRange(room.SongMap); // [24, 536)
            return returnedBytes.ToArray();
        }

        public static byte[] Resp15FullRoomInfo(this Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x15, 0x09}); // [0, 4)
            returnedBytes.AddRange(GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(GetBytes(room.Counter)); // [12, 16)
            foreach (var player in room.Players)
            {
                returnedBytes.AddRange(PlayerInfoWithNameSchema(player)); // [16, 176)
            }
            if (room.Players.Count < 4)
            {
                for (var i = 0; i < 4 - room.Players.Count; i++)
                {
                    returnedBytes.AddRange(PlayerInfoWithNameSchema(new Player()));
                }
            }
            returnedBytes.AddRange(room.SongMap); // [176, 688)
            returnedBytes.AddRange(RoomInfoWithHostSchema(room)); // [688, 759)
            return returnedBytes.ToArray();
        }
        
        private static IEnumerable<byte> PlayerInfoSchema(Player player)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(GetBytes(player.PlayerId)); // [0, 8)
            returnedBytes.Add((byte)player.Character); // [8]
            returnedBytes.AddRange(GetBytes(player.CharacterUncapped)); // [9]
            returnedBytes.Add((byte)player.Difficulty); // [10]
            returnedBytes.AddRange(GetBytes(player.Score)); // [11, 15)
            returnedBytes.AddRange(GetBytes(player.Timer)); // [15, 19)
            returnedBytes.Add((byte)player.ClearType); // [19]
            returnedBytes.Add((byte)player.PlayerState); // [20]
            returnedBytes.Add((byte)player.DownloadProgress); // [21]
            returnedBytes.AddRange(GetBytes(player.OnlineState)); // [22]
            return returnedBytes.ToArray();
        }

        private static byte[] PlayerInfoWithNameSchema(Player player)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(PlayerInfoSchema(player)); // [0, 23)
            returnedBytes.Add(0x00); // [23]
            returnedBytes.AddRange(player.Name); // [24, 40)
            return returnedBytes.ToArray();
        }

        private static IEnumerable<byte> PlayerScoreSchema(Player player)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.Add((byte)player.Character); // [0]
            returnedBytes.Add((byte)player.Difficulty); // [1]
            returnedBytes.AddRange(GetBytes(player.Score)); // [2, 6)
            returnedBytes.Add((byte)player.ClearType); // [6]
            returnedBytes.AddRange(GetBytes(player.PersonalBest)); // [7]
            returnedBytes.AddRange(GetBytes(player.Top)); // [8]
            return returnedBytes.ToArray();
        }

        private static IEnumerable<byte> RoomInfoSchema(Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.Add((byte)room.RoomState); // [0]
            returnedBytes.AddRange(GetBytes(room.CountDown)); // [1, 5)
            returnedBytes.AddRange(GetBytes(Room.ServerTime)); // [5, 13)
            returnedBytes.AddRange(GetBytes(room.SongIdxWithDiff)); // [13, 15)
            returnedBytes.AddRange(GetBytes((ushort)1000)); // [15, 17)
            returnedBytes.AddRange(GetBytes((ulong)100)[..7]); // [17, 24)
            foreach (var roomPlayer in room.Players)
            {
                returnedBytes.AddRange(PlayerScoreSchema(roomPlayer)); // [24, ...)
            }
            if (room.Players.Count < 4)
            {
                for (var i = 0; i < 4 - room.Players.Count; i++)
                {
                    returnedBytes.AddRange(PlayerScoreSchema(new Player()));
                }
            }
            returnedBytes.AddRange(GetBytes(room.LastSong));
            returnedBytes.AddRange(GetBytes(room.RoundRobin));
            return returnedBytes.ToArray();
        }

        private static IEnumerable<byte> RoomInfoWithHostSchema(Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(GetBytes(room.HostId)); // [..., ...+8)
            returnedBytes.AddRange(RoomInfoSchema(room)); // [0, ...)
            return returnedBytes.ToArray();
        }
        
    }
}