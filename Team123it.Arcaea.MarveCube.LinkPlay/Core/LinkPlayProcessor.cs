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
            if (packet[2] == 0x01) await Command01Handler(packet);
            if (packet[2] == 0x04) await Command04Handler(packet);
            if (packet[2] == 0x08) await Command08Handler(packet);
            if (packet[2] == 0x09) await Command09Handler(packet, endPoint);
            if (packet[2] == 0x0A) await Command0AHandler(packet);
            if (packet[2] == 0x0B) await Command0BHandler(packet);
        }

        private static async Task Command01Handler(byte[] data)
        {
            var redisToken = await LinkPlayRedisFetcher.FetchRoomIdByToken(BitConverter.ToUInt64(data[4..12]));
            var room = (Room) FetchRoomById(redisToken.RoomId)!;
            var hostObject = LinkPlayParser.ParseClientPack01(data);
            room.HostId = hostObject.PlayerId; room.ClientTime = hostObject.ClientTime;
            await Broadcast(LinkPlayResponse.Resp13PartRoomInfo(room), room); room.Counter++;
            ReassignRoom(room.RoomId, room);
        }

        private static async Task Command04Handler(byte[] data)
        {
            var redisToken = await LinkPlayRedisFetcher.FetchRoomIdByToken(BitConverter.ToUInt64(data.AsSpan()[4..12]));
            var room = (Room) FetchRoomById(redisToken.RoomId)!;
            var kickObject = LinkPlayParser.ParseClientPack04(data);
            var removeIndex = await room.RemovePlayer(BitConverter.ToUInt64(kickObject.Token), kickObject.PlayerId);
            if (removeIndex != -1)
            {
                await Broadcast(LinkPlayResponse.Resp12PlayerUpdate(room, removeIndex), room); room.Counter++;
                await Broadcast(LinkPlayResponse.Resp13PartRoomInfo(room), room); room.Counter++;
                await room.UpdateUnlocks();
                ReassignRoom(room.RoomId, room);            
            }
        }
        
        private static async Task Command08Handler(byte[] data)
        {
            var redisToken = await LinkPlayRedisFetcher.FetchRoomIdByToken(BitConverter.ToUInt64(data[4..12]));
            var room = (Room) FetchRoomById(redisToken.RoomId)!;
            var robinObject = LinkPlayParser.ParseClientPack08(data);
            room.RoundRobin = robinObject.RobinEnabled;
            await Broadcast(LinkPlayResponse.Resp13PartRoomInfo(room), room); room.Counter++;
            ReassignRoom(room.RoomId, room);
        }
        
        private static async Task Command09Handler(byte[] data, EndPoint endPoint)
        {
            var room = new Room();
            var redisToken = await LinkPlayRedisFetcher.FetchRoomIdByToken(BitConverter.ToUInt64(data.AsSpan()[4..12]));
            if (FetchRoomById(redisToken.RoomId) is not null) room = (Room) FetchRoomById(redisToken.RoomId)!;
            var dataObject = LinkPlayParser.ParseClientPack09(data);
            var (newRoom, playerIndex) = await LinkPlayInstanceCreator.PlayerCreator(room, dataObject, endPoint);
            await SendMsg(LinkPlayResponse.Resp0CPing(newRoom), data[4..12], endPoint);
            var redisTokenCount = (await LinkPlayRedisFetcher.FetchRoomById(redisToken.RoomId)).Token.Count;
            if (dataObject.Counter > room.Counter) {}
            if (dataObject.Counter < room.Counter) await SendMsg(newRoom.GetResendPack(dataObject.Counter), dataObject.Token!, endPoint);
            if (redisTokenCount > playerIndex && playerIndex >= 0)
            {
                if (newRoom.Players[playerIndex].OnlineState is false)
                {
                    newRoom.Players[playerIndex].OnlineState = true;
                    if (newRoom.Players.Count(player => player.Token != 0) > 1)
                    {
                        await Broadcast(LinkPlayResponse.Resp11PlayerInfo(room), newRoom);
                        newRoom.Counter++;
                        await Broadcast(LinkPlayResponse.Resp12PlayerUpdate(newRoom, playerIndex), newRoom);
                        newRoom.Counter++;
                        await newRoom.UpdateUnlocks();
                    }
                    await Broadcast(LinkPlayResponse.Resp13PartRoomInfo(newRoom), newRoom);
                    newRoom.Counter++;
                }
            }
            if (FetchRoomById(redisToken.RoomId) is not null) ReassignRoom(newRoom.RoomId, newRoom);
            else RegisterRoom(newRoom, newRoom.RoomId);
        }

        private static async Task Command0AHandler(byte[] data)
        {
            var redisToken = await LinkPlayRedisFetcher.FetchRoomIdByToken(BitConverter.ToUInt64(data.AsSpan()[4..12]));
            var redisRoom = await LinkPlayRedisFetcher.FetchRoomById(redisToken.RoomId);
            var room = (Room) FetchRoomById(redisToken.RoomId)!;
            var exitObject = LinkPlayParser.ParseClientPack0A(data);
            var removePlayerId = redisRoom.PlayerId[redisRoom.Token.IndexOf(BitConverter.ToUInt64(data.AsSpan()[4..12]))];
            var removeIndex = await room.RemovePlayer(BitConverter.ToUInt64(exitObject.Token), Convert.ToUInt64(removePlayerId));
            await Broadcast(LinkPlayResponse.Resp12PlayerUpdate(room, removeIndex), room); room.Counter++;
            await Broadcast(LinkPlayResponse.Resp13PartRoomInfo(room), room); room.Counter++;
            await room.UpdateUnlocks();
            ReassignRoom(room.RoomId, room);
        }

        private static async Task Command0BHandler(byte[] data)
        {
            var redisToken = await LinkPlayRedisFetcher.FetchRoomIdByToken(BitConverter.ToUInt64(data.AsSpan()[4..12]));
            var redisRoom = await LinkPlayRedisFetcher.FetchRoomById(redisToken.RoomId);
            var room = (Room) FetchRoomById(redisToken.RoomId)!;
            var recommendObject = LinkPlayParser.ParseClientPack0B(data);
            var playerIndex = redisRoom.Token.IndexOf(BitConverter.ToUInt64(recommendObject.Token));
            await Broadcast(LinkPlayResponse.Resp0FSongSuggestion(room, playerIndex, recommendObject.SongIdx), room); room.Counter++;
        }

        public static async Task Broadcast(byte[] data, Room room)
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