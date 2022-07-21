using Newtonsoft.Json;

#pragma warning disable CS8618

namespace Team123it.Arcaea.MarveCube.LinkPlay.Models
{
    [Serializable]
    public class LPRoom
    {
        [JsonProperty("roomCode")] public string RoomCode { get; set; }
        [JsonProperty("roomId")] public ulong RoomId { get; set; }
        [JsonProperty("token")] public List<ulong> Token { get; set; }
        [JsonProperty("key")] public string Key { get; set; } = "EUUUGRkZGAARRRQZGRkYAA==";
        [JsonProperty("playerId")] public List<int> PlayerId { get; set; }
        [JsonProperty("userId")] public List<uint> UserId { get; set; }
        [JsonProperty("allowSongs")] public List<string> AllowSongs { get; set; }
    }

    [Serializable]
    public class LPToken
    {
        [JsonProperty("roomId")] public ulong RoomId { get; set; }
        [JsonProperty("userName")] public string UserName { get; set; }
    }
}