using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using static Team123it.Arcaea.MarveCube.LinkPlay.GlobalProperties;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public class LinkPlayRedisFetcher
    {
        public async Task<JObject>  FetchRoomIdByToken(ulong token)
        {
            var mDatabaseConnectUrl = $"{RedisServerUrl}:{RedisServerPort},password={RedisServerPassword}";
            var conn = await ConnectionMultiplexer.ConnectAsync(mDatabaseConnectUrl);
            var db = conn.GetDatabase();
            var roomId = JObject.Parse(db.StringGet($"Arcaea-LinkPlayToken-{token}"));
            await conn.CloseAsync();
            return roomId;
        }

        public async Task<JObject> FetchRoomById(ulong roomId)
        {
            var mDatabaseConnectUrl = $"{RedisServerUrl}:{RedisServerPort},password={RedisServerPassword}";
            var conn = await ConnectionMultiplexer.ConnectAsync(mDatabaseConnectUrl);
            var db = conn.GetDatabase();
            var room = JObject.Parse(db.StringGet($"Arcaea-LinkPlay-{roomId}"));
            await conn.CloseAsync();
            return room;
        }

        public async Task<ulong> FetchRoomIdByCode(string roomCode)
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
