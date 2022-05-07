using System.Net;
using Team123it.Arcaea.MarveCube.LinkPlay.Models;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public static class LinkPlayInstanceCreator
    {
        public static async Task<(Room, int)> PlayerCreator(Room room, ClientPack09 data, EndPoint endPoint)
        {
            var tokenList = room.Players.Select(player => player.Token).ToList();
            if (tokenList.Contains(BitConverter.ToUInt64(data.Token))) return (room, tokenList.IndexOf(BitConverter.ToUInt64(data.Token)));
            
            var redisToken = await LinkPlayRedisFetcher.FetchRoomIdByToken(BitConverter.ToUInt64(data.Token));
            var redisRoom = await LinkPlayRedisFetcher.FetchRoomById(redisToken.RoomId);
            if (room.Players.Length == 0)
            {
                var returnedRoom = new Room()
                {
                    RoomId = redisRoom.RoomId,
                    SongMap = LinkPlayCrypto.ConvertUnlocks(redisRoom.AllowSongs),
                    HostId = redisRoom.PlayerId[0],
                    ClientTime = data.ClientTime,
                };
                var player = new Player()
                {
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
                returnedRoom.Players[0] = player;
                return (returnedRoom, 0);
            }
            else
            {
                room.SongMap = LinkPlayCrypto.ConvertUnlocks(redisRoom.AllowSongs);
                var player = new Player()
                {
                    Token = redisRoom.Token[0],
                    Score = data.Score,
                    DownloadProgress = data.DownloadProgress,
                    ClearType = (ClearTypes)data.ClearType,
                    Character = data.Character,
                    CharacterUncapped = data.CharacterUncapped,
                    EndPoint = endPoint,
                    Difficulty = (Difficulties)data.Difficulty,
                };
                var playerIndex = room.Players.Length - 1;
                player.SendUserName(redisToken.UserName);
                room.Players[playerIndex] = player;
                return (room, playerIndex);
            }
        }
    }
}

