using System.Net;
using Team123it.Arcaea.MarveCube.LinkPlay.Models;
using static Team123it.Arcaea.MarveCube.LinkPlay.Program;
using static Team123it.Arcaea.MarveCube.LinkPlay.RoomManager;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public static class LinkPlayProcessor
    {
        public static async Task ProcessPacket(byte[] packet, EndPoint endPoint)
        {
            if (packet[..4] == new byte[] {0x06, 0x16, 0x09, 0x09}) await Command09Handler(packet, endPoint);
        }

        private static async Task Command09Handler(byte[] data, EndPoint endPoint)
        {
            var room = new Room();
            var linkPlayToken = await LinkPlayRedisFetcher.FetchRoomIdByToken(BitConverter.ToUInt64(data[4..12]));
            if (FetchRoomById(linkPlayToken.RoomId) is not null) room = (Room) FetchRoomById(linkPlayToken.RoomId)!;
            var dataObject = LinkPlayParser.ParseClientPack09(data);
            await SendMsg(LinkPlayResponse.Resp0CPing(room), data[4..12], endPoint);
            var (newRoom, playerIndex) = await LinkPlayInstanceCreator.PlayerCreator(room, dataObject, endPoint);
            if (newRoom.Players[playerIndex].OnlineState is false) newRoom.Players[playerIndex].OnlineState = true;
            if (dataObject.Counter > room.Counter) {}
            if (dataObject.Counter < room.Counter) await SendMsg(newRoom.GetResendPack(dataObject.Counter), dataObject.Token!, endPoint);
            if (room.Players.Length > 1)
            {
                await Broadcast(LinkPlayResponse.Resp12PlayerUpdate(newRoom, playerIndex), newRoom);
                newRoom.Counter++;
            }
            await Broadcast(LinkPlayResponse.Resp13PartRoomInfo(newRoom), newRoom);
            newRoom.Counter++;
            RegisterRoom(newRoom, newRoom.RoomId);
        }

        private static async Task Broadcast(byte[] data, Room room)
        {
            foreach (var player in room.Players) await SendMsg(data, BitConverter.GetBytes((ulong) 0), player.EndPoint);
        }
    }
}