using System.Reflection;
using static System.BitConverter;
#pragma warning disable CS8618

namespace Team123it.Arcaea.MarveCube.LinkPlay.Models
{
    public static class LPRequest
    {
        public static T Deserialize<T>(this byte[] data)
        {
            var returnedObject = Activator.CreateInstance<T>();
            var fields = typeof(T).GetFields();
            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<LPResponseAttribute>();
                if (attribute == null) continue;
                var bytes = data.AsSpan()[attribute.RangeStart..attribute.RangeEnd];
                switch (field)
                {
                    case var fieldInfo when fieldInfo.FieldType == typeof(bool):
                    {
                        field.SetValue(returnedObject, ToBoolean(data.AsSpan()[attribute.RangeStart..(attribute.RangeStart + 1)]));
                        break;
                    }
                    case var fieldInfo when fieldInfo.FieldType == typeof(short):       
                    {
                        field.SetValue(returnedObject, ToInt16(bytes));
                        break;
                    }
                    case var fieldInfo when fieldInfo.FieldType == typeof(int):       
                    {
                        field.SetValue(returnedObject, ToInt32(bytes));
                        break;
                    }
                    case var fieldInfo when fieldInfo.FieldType == typeof(uint):       
                    {
                        field.SetValue(returnedObject, ToUInt32(bytes));
                        break;
                    }
                    case var fieldInfo when fieldInfo.FieldType == typeof(ulong):       
                    {
                        field.SetValue(returnedObject, ToUInt64(bytes));
                        break;
                    }
                    case var fieldInfo when fieldInfo.FieldType == typeof(byte[]):
                    {
                        field.SetValue(returnedObject, data[attribute.RangeStart..attribute.RangeEnd]);
                        break;
                    }
                    case var fieldInfo when fieldInfo.FieldType == typeof(PlayerStates):
                    {
                        field.SetValue(returnedObject, (PlayerStates)data[attribute.RangeStart]);
                        break;
                    }
                    case var fieldInfo when fieldInfo.FieldType == typeof(ClearTypes):
                    {
                        field.SetValue(returnedObject, (ClearTypes)data[attribute.RangeStart]);
                        break;
                    }
                    case var fieldInfo when fieldInfo.FieldType == typeof(Difficulties):
                    {
                        field.SetValue(returnedObject, (Difficulties)data[attribute.RangeStart]);
                        break;
                    }
                }
            }
            return returnedObject;
        }

        public class Req01TryGiveHost
        {
            [LPResponse(0, 4)] public byte[] Prefix { get; set; } // [0, 4) {0x06, 0x16, 0x08, 0x09}
            [LPResponse(4, 12)] public ulong Token { get; set; } // [4..12) Player.Token
            [LPResponse(12, 16)] public uint Counter { get; set; } // [12..16)
            [LPResponse(16, 24)] public ulong ClientTime { get; set; } // [16..24)
        
            [LPResponse(24, 32)] public ulong PlayerId { get; set; } // [24..32)
        }

        public class Req02TrySelectSong
        {
            [LPResponse(0, 4)] public byte[] Prefix { get; set; } // [0, 4) {0x06, 0x16, 0x0A, 0x09}
            [LPResponse(4, 12)] public ulong Token { get; set; } // [4..12) Player.Token
            [LPResponse(12, 16)] public uint Counter { get; set; } // [12..16)
            [LPResponse(16, 24)] public ulong ClientTime { get; set; } // [16..24)
        
            [LPResponse(24, 26)] public short SongIdxWithDiff { get; set; } // [24..26)
        }

        public class Req03SongFinish
        {
            [LPResponse(0, 4)] public byte[] Prefix { get; set; } // [0, 4) {0x06, 0x16, 0x08, 0x09}
            [LPResponse(4, 12)] public ulong Token { get; set; } // [4..12) Player.Token
            [LPResponse(12, 16)] public uint Counter { get; set; } // [12..16)
            [LPResponse(16, 24)] public ulong ClientTime { get; set; } // [16..24)

            [LPResponse(24, 28)] public uint Score { get; set; } // [24, 28)
            [LPResponse(28)] public ClearTypes ClearType { get; set; } // [28]
            [LPResponse(29)] public Difficulties Difficulty { get; set; } // [29]

            [LPResponse(30)] public bool PersonalBest { get; set; } // [30]
        }

        public class Req04TryKickPlayer
        {
            [LPResponse(0, 4)] public byte[] Prefix { get; set; } // [0, 4) {0x06, 0x16, 0x08, 0x09}
            [LPResponse(4, 12)] public ulong Token { get; set; } // [4..12) Player.Token
            [LPResponse(12, 16)] public uint Counter { get; set; } // [12..16)
            [LPResponse(16, 24)] public ulong ClientTime { get; set; } // [16..24)
        
            [LPResponse(24, 32)] public ulong PlayerId { get; set; } // [24..32)
        }

        public class Req06ReturnToLobby
        {
            [LPResponse(0, 4)] public byte[] Prefix { get; set; } // [0, 4) {0x06, 0x16, 0x08, 0x09}
            [LPResponse(4, 12)] public ulong Token { get; set; } // [4..12) Player.Token
            [LPResponse(12, 16)] public uint Counter { get; set; } // [12..16)
            [LPResponse(16, 24)] public ulong ClientTime { get; set; } // [16..24)
        }

        public class Req07UnlocksUpdate
        {
            [LPResponse(0, 4)] public byte[] Prefix { get; set; } // [0, 4) {0x06, 0x16, 0x08, 0x09}
            [LPResponse(4, 12)] public ulong Token { get; set; } // [4..12) Player.Token
            [LPResponse(12, 16)] public uint Counter { get; set; } // [12..16)
            [LPResponse(16, 24)] public ulong ClientTime { get; set; } // [16..24)
        
            [LPResponse(24, 536)] public byte[] SongMap { get; set; } // [24, 536)
        }

        public class Req08RoundRobinEnable
        {
            [LPResponse(0, 4)] public byte[] Prefix { get; set; } // [0, 4) {0x06, 0x16, 0x08, 0x09}
            [LPResponse(4, 12)] public ulong Token { get; set; } // [4..12) Player.Token
            [LPResponse(12, 16)] public uint Counter { get; set; } // [12..16)
            [LPResponse(16, 24)] public ulong ClientTime { get; set; } // [16..24)
        
            [LPResponse(24)] public bool RobinEnabled { get; set; } // [24]
        }

        public class Req09Ping
        {
            [LPResponse(0, 4)] public byte[] Prefix { get; set; } // [0, 4) {0x06, 0x16, 0x08, 0x09}
            [LPResponse(4, 12)] public ulong Token { get; set; } // [4..12) Player.Token
            [LPResponse(12, 16)] public uint Counter { get; set; } // [12..16)
            [LPResponse(16, 24)] public ulong ClientTime { get; set; } // [16..24)
        
            [LPResponse(24, 28)] public uint Score { get; set; } // [24, 28)
            [LPResponse(28, 32)] public uint SongTime { get; set; } // [28, 32)
            [LPResponse(32)] public PlayerStates State { get; set; } // [32]
        
            [LPResponse(33)] public Difficulties Difficulty { get; set; } // [33]
            [LPResponse(34)] public ClearTypes ClearType { get; set; } // [34]
            [LPResponse(35)] public int DownloadProgress { get; set; } // [35]
        
            [LPResponse(36)] public int Character { get; set; } // [36]
            [LPResponse(37)] public bool CharacterUncapped { get; set; } // [37]
        }

        public class Req0ALeaveRoom
        {
            [LPResponse(0, 4)] public byte[] Prefix { get; set; } // [0, 4) {0x06, 0x16, 0x08, 0x09}
            [LPResponse(4, 12)] public ulong Token { get; set; } // [4..12) Player.Token
            [LPResponse(12, 16)] public uint Counter { get; set; } // [12..16)
        }

        public class Req0BSongSuggestion
        {
            [LPResponse(0, 4)] public byte[] Prefix { get; set; } // [0, 4) {0x06, 0x16, 0x08, 0x09}
            [LPResponse(4, 12)] public ulong Token { get; set; } // [4..12) Player.Token
            [LPResponse(12, 16)] public uint Counter { get; set; } // [12..16) 
        
            [LPResponse(16, 18)] public short SongIdxWithDiff { get; set; } // [16..18)
        }
    

        [AttributeUsage(AttributeTargets.Property)]
        private class LPResponseAttribute: Attribute
        {
            public LPResponseAttribute(int rangeStart, int rangeEnd)
            {
                RangeStart = rangeStart;
                RangeEnd = rangeEnd;
            }
        
            public LPResponseAttribute(int rangeStart)
            {
                RangeStart = rangeStart;
            }
        
            public int RangeStart { get; }
            public int RangeEnd { get; }
        }
    }
}