using Team123it.Arcaea.MarveCube.LinkPlay.Models;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public static class LinkPlayConstructor
    {
        public static byte[] PlayerInfoSchema(Player player)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(BitConverter.GetBytes(player.PlayerId)); // [0, 8)
            returnedBytes.AddRange(BitConverter.GetBytes(player.Character)[..1]); // [8]
            returnedBytes.AddRange(BitConverter.GetBytes(player.CharacterUncapped)); // [9]
            returnedBytes.AddRange(BitConverter.GetBytes((uint) player.Difficulty)[..1]); // [10]
            returnedBytes.AddRange(BitConverter.GetBytes(player.Score)); // [11, 15)
            returnedBytes.AddRange(BitConverter.GetBytes(player.Timer)); // [15, 19)
            returnedBytes.AddRange(BitConverter.GetBytes((uint) player.ClearType)); // [19]
            returnedBytes.AddRange(BitConverter.GetBytes((uint) player.PlayerState)); // [20]
            returnedBytes.AddRange(BitConverter.GetBytes(player.DownloadProgress)); // [21]
            return returnedBytes.ToArray();
        }

        public static byte[] PlayerInfoWithNameSchema(Player player)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(PlayerInfoSchema(player)); // [0, 22)
            returnedBytes.AddRange(BitConverter.GetBytes(player.OnlineState)); // [22]
            returnedBytes.AddRange(new byte[] {0x00}); // [23]
            returnedBytes.AddRange(player.Name); // [24, 40)
            return returnedBytes.ToArray();
        }

        public static byte[] PlayerScoreSchema(Player player)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(BitConverter.GetBytes(player.Character)[..1]); // [0]
            returnedBytes.AddRange(BitConverter.GetBytes((uint)player.Difficulty)[..1]); // [1]
            returnedBytes.AddRange(BitConverter.GetBytes(player.Score)); // [2, 6)
            returnedBytes.AddRange(BitConverter.GetBytes((uint)player.ClearType)); // [6]
            returnedBytes.AddRange(BitConverter.GetBytes(player.PersonalBest)); // [7]
            returnedBytes.AddRange(BitConverter.GetBytes(player.Top)); // [8]
            return returnedBytes.ToArray();
        }

        public static byte[] RoomInfoSchema(Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(BitConverter.GetBytes((uint)room.RoomState)); // [0]
            returnedBytes.AddRange(BitConverter.GetBytes(room.RoomId)); // [1, 5)
            returnedBytes.AddRange(BitConverter.GetBytes(DateTime.Now.Ticks)); // [5, 13)
            returnedBytes.AddRange(BitConverter.GetBytes(room.SongIdx)); // [13, 15)
            returnedBytes.AddRange(BitConverter.GetBytes((ushort)1000)); // [15, 17)
            returnedBytes.AddRange(BitConverter.GetBytes((long)100)[..7]); // [17, 24)
            foreach (var roomPlayer in room.Players) returnedBytes.AddRange(PlayerScoreSchema(roomPlayer)); // [24, ...)
            returnedBytes.AddRange(BitConverter.GetBytes(room.LastSong));
            returnedBytes.AddRange(BitConverter.GetBytes(room.RoundRobin));
            return returnedBytes.ToArray();
        }

        public static byte[] RoomInfoWithHostSchema(Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(RoomInfoSchema(room)); // [0, ...)
            returnedBytes.AddRange(BitConverter.GetBytes(room.HostId)); // [..., ...+8)
            return returnedBytes.ToArray();
        }
    }
}