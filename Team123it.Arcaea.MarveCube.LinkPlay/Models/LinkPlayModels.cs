using System.Net;
using System.Text;
using Team123it.Arcaea.MarveCube.LinkPlay.Core;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Models
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
        public uint Timer;
        public ClearTypes ClearType = ClearTypes.None;
        public PlayerStates PlayerState = PlayerStates.Choosing;
        public int DownloadProgress;
        public bool OnlineState;

        public bool PersonalBest;
        public bool Top;

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
            Timer = 0;
            ClearType = ClearTypes.None;
            PlayerState = PlayerStates.Choosing;
            DownloadProgress = 0;
            OnlineState = false;
            PersonalBest = false;
            Top = false;
            EndPoint = new IPEndPoint(IPAddress.Any, 0);
        }

        public void SendUserName(string setName)
        {
            var rawName = Encoding.ASCII.GetBytes(setName).ToList();
            while (rawName.Count != 16)
            {
                rawName.Add(0x00);
            }

            if (rawName.Count != 16) throw new Exception("Name is too short");
            Name = rawName.ToArray();
        }
    }

    public struct Room
    {
        public ulong RoomId;
        public Player[] Players = {new(), new(), new(), new()};
        public byte[] SongMap = new byte[512]; // buf(512 <- state.common.songMapLen)

        public RoomStates RoomState = RoomStates.Locked;
        public uint Counter = 4;
        public int CountDown = -1;
        public ulong HostId;
        public ulong ClientTime;

        public short SongIdx = -1;
        public short LastSong = -1;
        public bool RoundRobin;

        public Room()
        {
            RoomId = 0;
            Players = new Player[] {new(), new(), new(), new()};
            SongMap = new byte[512];
            RoomState = RoomStates.Locked;
            Counter = 4;
            CountDown = -1;
            HostId = 0;
            ClientTime = 0;
            SongIdx = -1;
            LastSong = -1;
            RoundRobin = false;
        }

        public byte[] GetResendPack(uint clientCounter)
        {
            return Counter - clientCounter > 0
                ? LinkPlayResponse.Resp15FullRoomInfo(this)
                : Array.Empty<byte>();
        }

        public async Task RemovePlayer(ulong token, ulong playerId)
        {
            var redisToken = await LinkPlayRedisFetcher.FetchRoomIdByToken(token);
            var redisRoom = await LinkPlayRedisFetcher.FetchRoomById(redisToken.RoomId);
            redisRoom.Token.RemoveAt(redisRoom.PlayerId.IndexOf(playerId.ToString()));
            redisRoom.UserId.RemoveAt(redisRoom.PlayerId.IndexOf(playerId.ToString()));
            redisRoom.PlayerId.Remove(playerId.ToString());
            for (var i = 0; i < 4; i++) if (Players[i].PlayerId == playerId) Players[i] = new Player();
            await LinkPlayRedisFetcher.ReassignRedisRoom(redisRoom);
        }
    }
}