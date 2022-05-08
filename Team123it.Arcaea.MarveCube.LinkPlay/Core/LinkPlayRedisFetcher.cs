using Newtonsoft.Json;
using StackExchange.Redis;
using Team123it.Arcaea.MarveCube.LinkPlay.Models;
using static Team123it.Arcaea.MarveCube.LinkPlay.GlobalProperties;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public static class LinkPlayRedisFetcher
    {
        private static readonly string MDatabaseConnectUrl = $"{RedisServerUrl}:{RedisServerPort},password={RedisServerPassword}";
        public static async Task<LinkPlayToken> FetchRoomIdByToken(ulong token)
        {
            var conn = await ConnectionMultiplexer.ConnectAsync(MDatabaseConnectUrl);
            var db = conn.GetDatabase();
            var roomId = JsonConvert.DeserializeObject<LinkPlayToken>(db.StringGet($"Arcaea-LinkPlayToken-{token}"))!;
            await conn.CloseAsync();
            return roomId;
        }

        public static async Task<LinkPlayRoom> FetchRoomById(ulong roomId)
        {
            var conn = await ConnectionMultiplexer.ConnectAsync(MDatabaseConnectUrl);
            var db = conn.GetDatabase();
            var room = JsonConvert.DeserializeObject<LinkPlayRoom>(db.StringGet($"Arcaea-LinkPlay-{roomId}"))!;
            await conn.CloseAsync();
            return room;
        }

        public static async Task<ulong> FetchRoomIdByCode(string roomCode)
        {
            var conn = await ConnectionMultiplexer.ConnectAsync(MDatabaseConnectUrl);
            var db = conn.GetDatabase();
            var roomId = (ulong)db.StringGet($"Arcaea-LinkPlayWrapper-{roomCode}");
            await conn.CloseAsync();
            return roomId;
        }

        public static async Task ReassignRedisRoom(LinkPlayRoom roomObject)
        {
            var conn = await ConnectionMultiplexer.ConnectAsync(MDatabaseConnectUrl);
            var db = conn.GetDatabase(); 
            await db.StringSetAsync($"Arcaea-LinkPlay-{roomObject.RoomId}", JsonConvert.SerializeObject(roomObject));
            await conn.CloseAsync();
        }
    }
}
