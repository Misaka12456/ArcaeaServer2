using System.Net;
using System.Text;
using Team123it.Arcaea.MarveCube.LinkPlay.Models;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Entities
{
    public struct Player
    {
        public byte[] Name = Encoding.ASCII.GetBytes("EmptyPlayer\x00\x00\x00\x00\x00"); // buf(16) (string)
        public ulong PlayerId; // u32
        public ulong Token; // u64
        public uint UserId; // u32
        public byte[] SongMap = new byte[512]; // buf(512 <- state.common.songMapLen)

        public int Character = -1;
        public bool CharacterUncapped;
        public Difficulties Difficulty = Difficulties.Empty;
        public uint Score;
        public uint SongTime;
        public uint Timer;
        public ClearTypes ClearType = ClearTypes.None;
        public PlayerStates PlayerState = PlayerStates.Idle;
        public int DownloadProgress;
        public bool OnlineState;

        public bool PersonalBest;
        public bool Top;

        // Score Broadcast
        public uint LastScore;
        public uint LastSongTime;
        
        // Last Score
        public int? LastCharacter;
        public Difficulties? LastDifficulty;
        public uint? LastPlayedScore;
        public ClearTypes? LastClearType;

        public EndPoint EndPoint = new IPEndPoint(IPAddress.Any, 0);

        public Player()
        {
            Name = Encoding.ASCII.GetBytes("EmptyPlayer\x00\x00\x00\x00\x00");
            PlayerId = 0;
            Token = 0;
            UserId = 0;
            SongMap = new byte[512];
            Character = -1;
            CharacterUncapped = false;
            Difficulty = Difficulties.Empty;
            Score = 0;
            SongTime = 0;
            Timer = 0;
            ClearType = ClearTypes.None;
            PlayerState = PlayerStates.Idle;
            DownloadProgress = 0;
            OnlineState = false;
            PersonalBest = false;
            Top = false;
            EndPoint = new IPEndPoint(IPAddress.Any, 0);
            
            // Score Broadcast
            LastScore = 0;
            LastSongTime = 0;
            
            // Last Score
            LastCharacter = null;
            LastDifficulty = null;
            LastPlayedScore = null;
            LastClearType = null;
        }

        public void SendUserName(string setName)
        {
            var rawName = Encoding.ASCII.GetBytes(setName).ToList();
            while (rawName.Count != 16)
            {
                rawName.Add(0x00);
            }

            if (rawName.Count != 16)
            { 
                throw new Exception("Name is too short");
            }
            Name = rawName.ToArray();
        }
    }
}