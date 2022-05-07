using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using Team123it.Arcaea.MarveCube.LinkPlay.Models;
using static Team123it.Arcaea.MarveCube.LinkPlay.GlobalProperties;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public static class LinkPlayRedisFetcher
    {
        public static async Task<LinkPlayToken> FetchRoomIdByToken(ulong token)
        {
            var mDatabaseConnectUrl = $"{RedisServerUrl}:{RedisServerPort},password={RedisServerPassword}";
            var conn = await ConnectionMultiplexer.ConnectAsync(mDatabaseConnectUrl);
            var db = conn.GetDatabase();
            var roomId = JsonConvert.DeserializeObject<LinkPlayToken>(db.StringGet($"Arcaea-LinkPlayToken-{token}"))!;
            await conn.CloseAsync();
            return roomId;
        }

        public static async Task<LinkPlayRoom> FetchRoomById(ulong roomId)
        {
            var mDatabaseConnectUrl = $"{RedisServerUrl}:{RedisServerPort},password={RedisServerPassword}";
            var conn = await ConnectionMultiplexer.ConnectAsync(mDatabaseConnectUrl);
            var db = conn.GetDatabase();
            var room = JsonConvert.DeserializeObject<LinkPlayRoom>(db.StringGet($"Arcaea-LinkPlay-{roomId}"))!;
            await conn.CloseAsync();
            return room;
        }

        public static async Task<ulong> FetchRoomIdByCode(string roomCode)
        {
            var mDatabaseConnectUrl = $"{RedisServerUrl}:{RedisServerPort},password={RedisServerPassword}";
            var conn = await ConnectionMultiplexer.ConnectAsync(mDatabaseConnectUrl);
            var db = conn.GetDatabase();
            var roomId = (ulong)db.StringGet($"Arcaea-LinkPlayWrapper-{roomCode}");
            await conn.CloseAsync();
            return roomId;
        }
    }
}
