using System.Net;
using Team123it.Arcaea.MarveCube.LinkPlay.Models;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public static class LinkPlayInstanceCreator
    {
        public static async Task<(Room, int)> PlayerCreator(Room room, ClientPack09 data, EndPoint endPoint)
        {
            var playerCount = room.Players.Count(player => player.Token != 0);
            var tokenList = room.Players.Select(player => player.Token).ToList();
            if (tokenList.Contains(BitConverter.ToUInt64(data.Token))) return (room, tokenList.IndexOf(BitConverter.ToUInt64(data.Token)));
            
            var redisToken = await LinkPlayRedisFetcher.FetchRoomIdByToken(BitConverter.ToUInt64(data.Token));
            var redisRoom = await LinkPlayRedisFetcher.FetchRoomById(redisToken.RoomId);
            var hostId = Convert.ToUInt64(redisRoom.PlayerId[0]);
            if (playerCount == 0)
            {
                var returnedRoom = new Room
                {
                    RoomId = redisRoom.RoomId,
                    SongMap = LinkPlayCrypto.ConvertUnlocks(redisRoom.AllowSongs),
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
                };
                player.SendUserName(redisToken.UserName);
                returnedRoom.Players.SetValue(player, 0);
                return (returnedRoom, 0);
            }
            else
            {
                room.SongMap = LinkPlayCrypto.ConvertUnlocks(redisRoom.AllowSongs);
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
                };
                player.SendUserName(redisToken.UserName);
                room.Players.SetValue(player, playerIndex);
                return (room, playerIndex);
            }
        }
    }
}