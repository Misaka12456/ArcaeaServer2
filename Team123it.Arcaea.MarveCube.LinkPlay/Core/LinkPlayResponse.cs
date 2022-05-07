using Team123it.Arcaea.MarveCube.LinkPlay.Models;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public static class LinkPlayResponse
    {
        public static byte[] Resp0CPing(Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x0c, 0x09}); // [0, 4)
            returnedBytes.AddRange(BitConverter.GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(BitConverter.GetBytes(room.Counter)); // [12, 16)
            returnedBytes.AddRange(BitConverter.GetBytes(room.ClientTime)); // [16, 24)
            returnedBytes.AddRange(BitConverter.GetBytes((uint) room.RoomState)); // [24]
            returnedBytes.AddRange(BitConverter.GetBytes(room.CountDown)); // [25, 29)
            returnedBytes.AddRange(BitConverter.GetBytes(DateTime.Now.Ticks)); // [29, 37)
            return returnedBytes.ToArray();
        }

        public static byte[] Resp0DStateUpdate(Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x0d, 0x09}); // [0, 4)
            returnedBytes.AddRange(BitConverter.GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(BitConverter.GetBytes(room.Counter)); // [12, 16)
            returnedBytes.AddRange(BitConverter.GetBytes(room.ClientTime)); // [16, 24)

            returnedBytes.AddRange(BitConverter.GetBytes((uint) room.RoomState)); // [24]
            return returnedBytes.ToArray();
        }

        public static byte[] Resp0FSongSuggestion(Room room, int playerIndex, int songIndex, Difficulties difficulty)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x0f, 0x09}); // [0, 4)
            returnedBytes.AddRange(BitConverter.GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(BitConverter.GetBytes(room.Counter)); // [12, 16)

            returnedBytes.AddRange(BitConverter.GetBytes(room.Players[playerIndex].PlayerId)); // [16, 24)
            returnedBytes.AddRange(BitConverter.GetBytes((ushort) songIndex * 4 + (int) difficulty)); // [24, 26)
            return returnedBytes.ToArray();
        }

        public static byte[] Resp10HostTransfer(Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x10, 0x09}); // [0, 4)
            returnedBytes.AddRange(BitConverter.GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(BitConverter.GetBytes(room.Counter)); // [12, 16)
            returnedBytes.AddRange(BitConverter.GetBytes(room.ClientTime)); // [16, 24)

            returnedBytes.AddRange(BitConverter.GetBytes(room.HostId)); // [24, 32)
            return returnedBytes.ToArray();
        }

        public static byte[] Resp11PlayerInfo(Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x11, 0x09}); // [0, 4)
            returnedBytes.AddRange(BitConverter.GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(BitConverter.GetBytes(room.Counter)); // [12, 16)
            returnedBytes.AddRange(BitConverter.GetBytes(room.ClientTime)); // [16, 24)
            foreach (var player in room.Players)
            {
                returnedBytes.AddRange(LinkPlayConstructor.PlayerInfoWithNameSchema(player)); // [24, 184)
            }

            return returnedBytes.ToArray();
        }

        public static byte[] Resp12PlayerUpdate(Room room, int playerIndex)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x12, 0x09}); // [0, 4)
            returnedBytes.AddRange(BitConverter.GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(BitConverter.GetBytes(room.Counter)); // [12, 16)
            returnedBytes.AddRange(BitConverter.GetBytes(room.ClientTime)); // [16, 24)
            returnedBytes.AddRange(BitConverter.GetBytes(playerIndex)[..1]); // [24]
            returnedBytes.AddRange(LinkPlayConstructor.PlayerInfoSchema(room.Players[playerIndex])); // [25, 47)
            return returnedBytes.ToArray();
        }

        public static byte[] Resp13PartRoomInfo(Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x13, 0x09}); // [0, 4)
            returnedBytes.AddRange(BitConverter.GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(BitConverter.GetBytes(room.Counter)); // [12, 16)
            returnedBytes.AddRange(BitConverter.GetBytes(room.ClientTime)); // [16, 24)

            returnedBytes.AddRange(LinkPlayConstructor.RoomInfoWithHostSchema(room)); // [24, 86)
            return returnedBytes.ToArray();
        }

        public static byte[] Resp14SongMapUpdate(Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x14, 0x09}); // [0, 4)
            returnedBytes.AddRange(BitConverter.GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(BitConverter.GetBytes(room.Counter)); // [12, 16)
            returnedBytes.AddRange(BitConverter.GetBytes(room.ClientTime)); // [16, 24)
            returnedBytes.AddRange(room.SongMap); // [24, 536)
            return returnedBytes.ToArray();
        }

        public static byte[] Resp15FullRoomInfo(Room room)
        {
            var returnedBytes = new List<byte>();
            returnedBytes.AddRange(new byte[] {0x06, 0x16, 0x15, 0x09}); // [0, 4)
            returnedBytes.AddRange(BitConverter.GetBytes(room.RoomId)); // [4, 12)
            returnedBytes.AddRange(BitConverter.GetBytes(room.Counter)); // [12, 16)
            foreach (var player in room.Players)
            {
                returnedBytes.AddRange(LinkPlayConstructor.PlayerInfoWithNameSchema(player)); // [16, 176)
            }
            
            returnedBytes.AddRange(room.SongMap); // [176, 688)
            returnedBytes.AddRange(LinkPlayConstructor.RoomInfoWithHostSchema(room)); // [688, 759)
            return returnedBytes.ToArray();
        }
    }
}