using System.Net;
using System.Text;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Models
{
    public struct Player
    {
        public byte[] Name = Encoding.ASCII.GetBytes("EmptyPlayer\x00\x00\x00\x00\x00");       // buf(16) (string)
        public ulong PlayerId = 0;   // u32
        public uint UserId = 0;     // u32
        public ulong Token = 0;      // buf(8) (u64)
        public byte[] SongMap = new byte[512];    // buf(512 <- state.common.songMapLen)

        public int Character  = -1;
        public bool CharacterUncapped = false;
        public Difficulties Difficulty = Difficulties.Empty;
        public uint Score = 0;
        public uint Timer = 0;
        public ClearTypes ClearType = ClearTypes.None;
        public PlayerStates PlayerState = PlayerStates.Choosing;
        public uint DownloadProgress = 0;
        public bool OnlineState = false;

        public bool PersonalBest = false;
        public bool Top = false;

        public EndPoint EndPoint = new IPEndPoint(IPAddress.Any, 0);
    }
    
    public struct Room
    {
        public ulong RoomId = 0;
        public string RoomCode = "114514";
        public Player[] Players = {new() ,new() ,new() ,new() };
        public byte[] SongMap = new byte[512];    // buf(512 <- state.common.songMapLen)

        public RoomStates RoomState = RoomStates.Locked;
        public uint Counter = 0;
        public int CountDown = -1;
        public ulong HostId = 0;
        public ulong ClientTime = 0;

        public short SongIdx = -1;
        public short LastSong = -1;
        public bool RoundRobin = false;
    }
}

