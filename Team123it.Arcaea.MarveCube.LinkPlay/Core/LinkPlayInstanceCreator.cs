using System.Net;
using Team123it.Arcaea.MarveCube.LinkPlay.Models;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public static class LinkPlayInstanceCreator
    {
        public static async Task<(Room, int, bool)> PlayerCreator(Room room, ClientPack09 data, EndPoint endPoint)
        {
            var redisToken = await LinkPlayRedisFetcher.FetchRoomIdByToken(BitConverter.ToUInt64(data.Token));
            var redisRoom = await LinkPlayRedisFetcher.FetchRoomById(redisToken.RoomId);
            var playerCount = room.Players.Count(player => player.Token != 0);
            var tokenList = room.Players.Select(player => player.Token).ToList();
            var redisTokenCount = redisRoom.Token.Count;
            if (tokenList.Contains(BitConverter.ToUInt64(data.Token)) || redisTokenCount == playerCount)
            {
                var flag12 = false;
                var playerIndex = tokenList.IndexOf(BitConverter.ToUInt64(data.Token));
                var player = room.Players[playerIndex];
                if (player.Character != data.Character)
                {
                    flag12 = true;
                    room.Players[playerIndex].Character = data.Character;
                }
                if (player.CharacterUncapped != data.CharacterUncapped)
                {
                    flag12 = true;
                    room.Players[playerIndex].CharacterUncapped = data.CharacterUncapped;
                }
                if ((uint)player.PlayerState != data.State)
                {
                    flag12 = true;
                    room.Players[playerIndex].PlayerState = (PlayerStates) data.State;
                }
                if (player.Score != data.Score && room.RoomState == RoomStates.NotReady)
                {
                    flag12 = true;
                    room.Players[playerIndex].Score = data.Score;
                }
                if (player.DownloadProgress != data.DownloadProgress && room.RoomState == RoomStates.NotReady)
                {
                    flag12 = true;
                    room.Players[playerIndex].DownloadProgress = data.DownloadProgress;
                }
                if ((uint)player.Difficulty != data.Difficulty && room.RoomState == RoomStates.NotReady)
                {
                    flag12 = true;
                    room.Players[playerIndex].Difficulty = (Difficulties) data.Difficulty;
                }
                if ((uint)player.ClearType != data.ClearType && room.RoomState == RoomStates.NotReady)
                {
                    flag12 = true;
                    room.Players[playerIndex].ClearType = (ClearTypes) data.ClearType;
                }
                return (room, playerIndex, flag12);
            }
            
            var hostId = Convert.ToUInt64(redisRoom.PlayerId[0]);
            if (playerCount == 0)
            {
                var returnedRoom = new Room
                {
                    RoomId = redisRoom.RoomId,
                    SongMap = Convert.FromBase64String(redisRoom.AllowSongs[0]),
                    HostId = hostId,
                    ClientTime = data.ClientTime,
                };
                var player = new Player
                {
                    PlayerId = hostId,
                    Token = redisRoom.Token[0],
                    Score = data.Score,
                    DownloadProgress = data.DownloadProgress,
                    ClearType = (ClearTypes)data.ClearType,
                    Character = data.Character,
                    CharacterUncapped = data.CharacterUncapped,
                    EndPoint = endPoint,
                    Difficulty = (Difficulties)data.Difficulty,
                    SongMap = Convert.FromBase64String(redisRoom.AllowSongs[0])
                };
                player.SendUserName(redisToken.UserName);
                returnedRoom.Players.SetValue(player, 0);
                return (returnedRoom, 0, true);
            }
            else
            {
                var playerIndex = playerCount;
                var player = new Player
                {
                    PlayerId = Convert.ToUInt64(redisRoom.PlayerId[playerIndex]),
                    Token = redisRoom.Token[playerIndex],
                    Score = data.Score,
                    DownloadProgress = data.DownloadProgress,
                    ClearType = (ClearTypes)data.ClearType,
                    Character = data.Character,
                    CharacterUncapped = data.CharacterUncapped,
                    EndPoint = endPoint,
                    Difficulty = (Difficulties)data.Difficulty,
                    SongMap = Convert.FromBase64String(redisRoom.AllowSongs[playerIndex])
                };
                await room.UpdateUnlocks();
                player.SendUserName(redisToken.UserName);
                room.Players.SetValue(player, playerIndex);
                return (room, playerIndex, true);
            }
        }
    }
}