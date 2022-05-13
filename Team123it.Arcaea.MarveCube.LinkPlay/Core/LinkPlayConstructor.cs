using Team123it.Arcaea.MarveCube.LinkPlay.Models;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public static class LinkPlayConstructor
    {
        public static byte[] PlayerInfoSchema(Player player)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(BitConverter.GetBytes(player.PlayerId)); // [0, 8)
            returnedBytes.Add((byte)player.Character); // [8]
            returnedBytes.AddRange(BitConverter.GetBytes(player.CharacterUncapped)); // [9]
            returnedBytes.Add((byte) player.Difficulty); // [10]
            returnedBytes.AddRange(BitConverter.GetBytes(player.Score)); // [11, 15)
            returnedBytes.AddRange(BitConverter.GetBytes(player.Timer)); // [15, 19)
            returnedBytes.Add((byte) player.ClearType); // [19]
            returnedBytes.Add((byte) player.PlayerState); // [20]
            returnedBytes.Add((byte)player.DownloadProgress); // [21]
            returnedBytes.AddRange(BitConverter.GetBytes(player.OnlineState)); // [22]
            return returnedBytes.ToArray();
        }

        public static byte[] PlayerInfoWithNameSchema(Player player)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(PlayerInfoSchema(player)); // [0, 23)
            returnedBytes.Add(0x00); // [23]
            returnedBytes.AddRange(player.Name); // [24, 40)
            return returnedBytes.ToArray();
        }

        public static byte[] PlayerScoreSchema(Player player)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.Add((byte)player.Character); // [0]
            returnedBytes.Add((byte)player.Difficulty); // [1]
            returnedBytes.AddRange(BitConverter.GetBytes(player.Score)); // [2, 6)
            returnedBytes.Add((byte)player.ClearType); // [6]
            returnedBytes.AddRange(BitConverter.GetBytes(player.PersonalBest)); // [7]
            returnedBytes.AddRange(BitConverter.GetBytes(player.Top)); // [8]
            return returnedBytes.ToArray();
        }

        public static byte[] RoomInfoSchema(Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.Add((byte)(uint)room.RoomState); // [0]
            returnedBytes.AddRange(BitConverter.GetBytes(room.CountDown)); // [1, 5)
            returnedBytes.AddRange(BitConverter.GetBytes((ulong)(DateTime.Now.Ticks*10))); // [5, 13)
            returnedBytes.AddRange(BitConverter.GetBytes(room.SongIdxWithDiff)); // [13, 15)
            returnedBytes.AddRange(BitConverter.GetBytes((ushort)1000)); // [15, 17)
            returnedBytes.AddRange(BitConverter.GetBytes((ulong)100)[..7]); // [17, 24)
            foreach (var roomPlayer in room.Players) returnedBytes.AddRange(PlayerScoreSchema(roomPlayer)); // [24, ...)
            returnedBytes.AddRange(BitConverter.GetBytes(room.LastSong));
            returnedBytes.AddRange(BitConverter.GetBytes(room.RoundRobin));
            return returnedBytes.ToArray();
        }

        public static byte[] RoomInfoWithHostSchema(Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(BitConverter.GetBytes(room.HostId)); // [..., ...+8)
            returnedBytes.AddRange(RoomInfoSchema(room)); // [0, ...)
            return returnedBytes.ToArray();
        }
    }
}