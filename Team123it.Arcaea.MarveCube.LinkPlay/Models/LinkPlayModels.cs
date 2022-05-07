using System.Net;
using System.Text;
using Team123it.Arcaea.MarveCube.LinkPlay.Core;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Models
{
    public struct Player
    {
        public byte[] Name = Encoding.ASCII.GetBytes("EmptyPlayer\x00\x00\x00\x00\x00");       // buf(16) (string)
        public uint PlayerId = 0;   // u32
        public ulong Token = 0;    // u64
        public uint UserId = 0;     // u32
        public byte[] SongMap = new byte[512];    // buf(512 <- state.common.songMapLen)

        public int Character  = -1;
        public bool CharacterUncapped = false;
        public Difficulties Difficulty = Difficulties.Empty;
        public uint Score = 0;
        public uint Timer = 0;
        public ClearTypes ClearType = ClearTypes.None;
        public PlayerStates PlayerState = PlayerStates.Choosing;
        public int DownloadProgress = 0;
        public bool OnlineState = false;

        public bool PersonalBest = false;
        public bool Top = false;

        public EndPoint EndPoint = new IPEndPoint(IPAddress.Any, 0);

        public void SendUserName(string setName)
        {
            var rawName = Encoding.ASCII.GetBytes(setName).ToList();
            for (var i = 0; i < 16 - rawName.Count; i++) rawName.Add(0x00);
            Name = rawName.ToArray();
        }
    }
    
    public struct Room
    {
        public ulong RoomId = 0;
        public Player[] Players = new Player[4];
        public byte[] SongMap = new byte[512];    // buf(512 <- state.common.songMapLen)

        public RoomStates RoomState = RoomStates.Locked;
        public uint Counter = 4;
        public int CountDown = -1;
        public uint HostId = 0;
        public ulong ClientTime = 0;

        public short SongIdx = -1;
        public short LastSong = -1;
        public bool RoundRobin = false;

        public byte[] GetResendPack(uint clientCounter)
        {
            return Counter - clientCounter > 0 
                ? LinkPlayResponse.Resp15FullRoomInfo(this) 
                : Array.Empty<byte>();
        }
    }
}

