using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable CS8618

namespace Team123it.Arcaea.MarveCube.LinkPlay.Models
{
    [Serializable]
    public class LinkPlayRoom
    {
        [JsonProperty("roomCode")] public string RoomCode { get; set; }
        [JsonProperty("roomId")] public ulong RoomId { get; set; }
        [JsonProperty("token")] public List<ulong> Token { get; set; }
        [JsonProperty("key")] public string Key { get; set; } = "EUUUGRkZGAARRRQZGRkYAA==";
        [JsonProperty("playerId")] public List<string> PlayerId { get; set; }
        [JsonProperty("userId")] public List<uint> UserId { get; set; }
        [JsonProperty("allowSongs")] public JObject AllowSongs { get; set; }
    }

    [Serializable]
    public class LinkPlayToken
    {
        [JsonProperty("roomId")] public ulong RoomId { get; set; }
        [JsonProperty("userName")] public string UserName { get; set; }
    }
}