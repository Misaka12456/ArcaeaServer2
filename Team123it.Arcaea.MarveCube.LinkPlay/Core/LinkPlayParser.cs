using System.Net;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using static Team123it.Arcaea.MarveCube.LinkPlay.GlobalProperties;
using static Team123it.Arcaea.MarveCube.LinkPlay.RoomManager;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public class LinkPlayParser
    {
        public static void LinkPlayResp(byte[] data, EndPoint endPoint)
        {
            var returnBytes = new List<byte>();
            switch (data[2])
            {
                case 0x09:
                {
                    returnBytes.AddRange(Ping(data));
                    break;
                }
            }
            Program.SendMsg(returnBytes.ToArray(), data[4..12], endPoint);
        }
        
        private static byte[] Ping(byte[] data)
        {
            var roomId = FetchRoomIdByToken(data[4..12]);
            var mDatabaseConnectUrl = $"{RedisServerUrl}:{RedisServerPort},password={RedisServerPassword}";
            var conn = ConnectionMultiplexer.Connect(mDatabaseConnectUrl);
            var db = conn.GetDatabase();
            var redisRoom = JObject.Parse(db.StringGet($"Arcaea-LinkPlay-{roomId}"));
            if (FetchRoomById(roomId) is null)
            {
                var room = new Room
                {
                    RoomId = Convert.ToUInt64(roomId),
                    RoomCode = redisRoom.Value<string>("roomCode")
                };
                var player = new Player
                {
                    Token = BitConverter.ToUInt64(data[4..12]),
                    StartCommandCount = BitConverter.ToInt32(data[12..16]),
                    LastTimestamp = BitConverter.ToUInt64(data[16..24]),
                    Score = BitConverter.ToUInt32(data[24..28]),
                    Timer = BitConverter.ToUInt32(data[28..32]),
                    PlayerState = (PlayerStates) data[32],
                    Difficulty = (Difficulties) data[33],
                    ClearType = (ClearTypes) data[34],
                    DownloadPercent = data[35],
                    CharacterId = data[36],
                    IsCharacterUncapped = data[37]
                };
                room.Players = new[] {player};
                RegisterRoom(room, roomId);
                
                var returnBytes = LinkPlayConstructor.Command0C(room);
                return returnBytes;
            }
            else
            {
                var room = FetchRoomById(roomId)!.Value;
                var token = BitConverter.ToUInt64(data[4..12]).ToString();
                if (redisRoom.Value<JArray>("token")!.ToObject<List<string>>()!.Contains(token))
                {
                    var returnBytes = LinkPlayConstructor.Command0C(room);
                    return returnBytes;
                }
                else
                {
                    for (var i = 0; i < 4; ++i)
                    {
                        if(room.Players[i].PlayerId != 0)
                        {
                            room.Players[i] = new Player()
                            {
                                Token = BitConverter.ToUInt64(data[4..12]),
                                StartCommandCount = BitConverter.ToInt32(data[12..16]),
                                LastTimestamp = BitConverter.ToUInt64(data[16..24]),
                                Score = BitConverter.ToUInt32(data[24..28]),
                                Timer = BitConverter.ToUInt32(data[28..32]),
                                PlayerState = (PlayerStates) data[32],
                                Difficulty = (Difficulties) data[33],
                                ClearType = (ClearTypes) data[34],
                                DownloadPercent = data[35],
                                CharacterId = data[36],
                                IsCharacterUncapped = data[37]
                            };
                        }
                    }
                    UnRegisterRoom(roomId);
                    RegisterRoom(room, roomId);
                    var returnBytes = LinkPlayConstructor.Command0C(room);
                    return returnBytes;
                }
            }
        }
    }
}
