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
            Console.WriteLine(BitConverter.ToString(packet[..4]));
            if (packet[2] == 0x04) await Command04Handler(packet);
            if (packet[2] == 0x08) await Command08Handler(packet);
            if (packet[2] == 0x09) await Command09Handler(packet, endPoint);
        }

        private static async Task Command04Handler(byte[] data)
        {
            var linkPlayToken = await LinkPlayRedisFetcher.FetchRoomIdByToken(BitConverter.ToUInt64(data[4..12]));
            var room = (Room) FetchRoomById(linkPlayToken.RoomId)!;
            var kickObject = LinkPlayParser.ParseClientPack04(data);
            await room.RemovePlayer(BitConverter.ToUInt64(kickObject.Token), kickObject.PlayerId);
            await Broadcast(LinkPlayResponse.Resp13PartRoomInfo(room), room); room.Counter++;
            ReassignRoom(room.RoomId, room);
        }
        
        private static async Task Command08Handler(byte[] data)
        {
            var linkPlayToken = await LinkPlayRedisFetcher.FetchRoomIdByToken(BitConverter.ToUInt64(data[4..12]));
            var room = (Room) FetchRoomById(linkPlayToken.RoomId)!;
            var robinObject = LinkPlayParser.ParseClientPack08(data);
            room.RoundRobin = robinObject.RobinEnabled;
            await Broadcast(LinkPlayResponse.Resp13PartRoomInfo(room), room); room.Counter++;
            ReassignRoom(room.RoomId, room);
        }
        
        private static async Task Command09Handler(byte[] data, EndPoint endPoint)
        {
            var room = new Room();
            var linkPlayToken = await LinkPlayRedisFetcher.FetchRoomIdByToken(BitConverter.ToUInt64(data[4..12]));
            if (FetchRoomById(linkPlayToken.RoomId) is not null) room = (Room) FetchRoomById(linkPlayToken.RoomId)!;
            var dataObject = LinkPlayParser.ParseClientPack09(data);
            var (newRoom, playerIndex) = await LinkPlayInstanceCreator.PlayerCreator(room, dataObject, endPoint);
            await SendMsg(LinkPlayResponse.Resp0CPing(newRoom), data[4..12], endPoint);
            if (dataObject.Counter > room.Counter) {}
            if (dataObject.Counter < room.Counter) await SendMsg(newRoom.GetResendPack(dataObject.Counter), dataObject.Token!, endPoint);
            if (newRoom.Players[playerIndex].OnlineState is false)
            {
                newRoom.Players[playerIndex].OnlineState = true;
                if (newRoom.Players.Count(player => player.Token != 0) > 1)
                {
                    await Broadcast(LinkPlayResponse.Resp11PlayerInfo(room),newRoom); newRoom.Counter++;
                    await Broadcast(LinkPlayResponse.Resp12PlayerUpdate(newRoom, playerIndex), newRoom); newRoom.Counter++;
                }
                await Broadcast(LinkPlayResponse.Resp13PartRoomInfo(newRoom), newRoom); newRoom.Counter++;
            }

            if (FetchRoomById(linkPlayToken.RoomId) is not null) ReassignRoom(newRoom.RoomId, newRoom);
            else RegisterRoom(newRoom, newRoom.RoomId);
        }

        private static async Task Broadcast(byte[] data, Room room)
        {
            foreach (var player in room.Players)
            {
                if (!player.EndPoint.Equals(new IPEndPoint(IPAddress.Any, 0)))
                {
                    await SendMsg(data, BitConverter.GetBytes(player.Token), player.EndPoint);
                }
            }
        }
    }
}