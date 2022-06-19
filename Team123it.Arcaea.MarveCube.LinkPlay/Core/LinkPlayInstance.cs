using System.Net;
using System.Text;
using Team123it.Arcaea.MarveCube.LinkPlay.Entities;
using Team123it.Arcaea.MarveCube.LinkPlay.Models;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core;

public static class LinkPlayInstance
{
    public static async Task<(Player, int)> Handler(LPRequest.Req09Ping packet, EndPoint endPoint)
    {
        var redisToken = await LPRedis.FetchRoomIdByToken(packet.Token);
        var redisRoom = await LPRedis.FetchRoomById(redisToken.RoomId);

        if (RoomManager.FetchRoomById(redisToken.RoomId) is null) // Create a new room if it doesn't exist
        {
            var room = new Room
            {
                ClientTime = packet.ClientTime,
                Counter = 4,
                HostId = ulong.Parse(redisRoom.PlayerId[0]),
                SongMap = Convert.FromBase64String(redisRoom.AllowSongs[0])
            };
            var (player, playerIndex) = room.AddPlayer(packet, endPoint, redisToken);
            room.RegisterRoom(room.RoomId);
            return (player, playerIndex);
        }
        else
        {
            var room = RoomManager.FetchRoomById(redisToken.RoomId)!.Value;
            if (room.Players.Any(x => x.Token == packet.Token)) // If the player is already in the room
            {
                var player = room.GetPlayer(packet.Token, out var playerIndex);
                await room.DetectChanges(packet);
                return (player, playerIndex);
            }
            else
            {
                var (player, playerIndex) = room.AddPlayer(packet, endPoint, redisToken);
                room.ReassignRoom();
                return (player, playerIndex);
            }
        }
    }

    private static (Player, int) AddPlayer(this Room room, LPRequest.Req09Ping packet, EndPoint endPoint, LPToken token)
    {
        var player = new Player
        {
            Character = packet.Character,
            CharacterUncapped = packet.CharacterUncapped,
            Token = packet.Token,
            ClearType = packet.ClearType,
            Difficulty = packet.Difficulty,
            DownloadProgress = packet.DownloadProgress,
            EndPoint = endPoint,
            Name = Encoding.ASCII.GetBytes(token.UserName)
        };
        room.Players.Add(player);
        room.ReassignRoom();
        return (player, room.Players.Count - 1);
    }

    private static async Task DetectChanges(this Room room, LPRequest.Req09Ping packet)
    {
        bool flag12 = false;
        var player = room.GetPlayer(packet.Token, out var playerIndex);
        
        if (player.Character != packet.Character)
        {
            player.Character = packet.Character;
            flag12 = true;
        }
        if (player.CharacterUncapped != packet.CharacterUncapped)
        {
            player.CharacterUncapped = packet.CharacterUncapped;
            flag12 = true;
        }
        if (player.PlayerState != packet.State)
        {
            player.PlayerState = packet.State;
            flag12 = true;
        }
        if (player.Difficulty != packet.Difficulty)
        {
            player.Difficulty = packet.Difficulty;
            flag12 = true;
        }
        if (player.ClearType != packet.ClearType)
        {
            player.ClearType = packet.ClearType;
            flag12 = true;
        }
        if (player.DownloadProgress != packet.DownloadProgress)
        {
            player.DownloadProgress = packet.DownloadProgress;
            flag12 = true;
        }
        
        room.ReassignPlayer(player, playerIndex);
        if (flag12) await room.Broadcast(room.Resp12PlayerUpdate(playerIndex));
        room.ReassignRoom();
    }
}