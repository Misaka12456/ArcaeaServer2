using System.Net;
using System.Text;
using Team123it.Arcaea.MarveCube.LinkPlay.Core;
using static Team123it.Arcaea.MarveCube.LinkPlay.Program;

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
        
        //Score Broadcast
        public uint LastScore;
        public uint LastSongTime;

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
            //Score Broadcast
            LastScore = 0;
            LastSongTime = 0;
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
        public long CountDown = -1;
        public long? CountDownStart = null;
        public ulong HostId;
        public ulong ClientTime;

        public short SongIdxWithDiff = -1;
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
            SongIdxWithDiff = -1;
            LastSong = -1;
            RoundRobin = false;
        }

        public long ServerTime => (long) (DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds * 1000;

        public byte[] GetResendPack(uint clientCounter)
        {
            return Counter - clientCounter > 0
                ? LinkPlayResponse.Resp15FullRoomInfo(this)
                : Array.Empty<byte>();
        }
        
        public bool IsAllOnline()
        {
            return (from player in Players where player.Token != 0 select player.OnlineState).All(state => state);
        }

        public bool IsAllState(PlayerStates desiredState, bool canOffline = false)
        {
            return (from player in Players where player.Token != 0 select player.PlayerState).All(state =>
                state == desiredState || (canOffline && state == desiredState));
        }
        
        private void UpdateCountDown()
        {
            if (CountDownStart is null) return;
            var d = ServerTime - CountDownStart.Value;
            CountDown -= d;
            CountDownStart += d;
        }

        public async Task<bool> UpdateState()
        {
            if (RoomState == RoomStates.Locked && IsAllState(PlayerStates.Downloading)) // 1->2
            {
                RoomState = RoomStates.Choosing;
                return true;
            }
            if (RoomState == RoomStates.Choosing && IsAllState(PlayerStates.Downloading)) // 2->1
            {
                RoomState = RoomStates.Locked;
                return true;
            }
            if (RoomState == RoomStates.NotReady && IsAllState(PlayerStates.Ready)) // 3->4
            {
                CountDown = 3999;
                RoomState = RoomStates.Countdown;
                CountDownStart = ServerTime;
                return true;
            }
            
            UpdateCountDown();
            if (RoomState is >= RoomStates.Countdown and <= RoomStates.Skill)
            {
                if (RoomState == RoomStates.Skill && CountDown < 0)
                {
                    // for (var i = 0; i < 4; i++) Players[i].ResetTimer
                    CountDown = -1;
                    CountDownStart = null;
                    RoomState = RoomStates.Playing;
                }
                if (RoomState == RoomStates.Countdown && CountDown < 0)
                {
                    ClearPrepareInfo();
                    await LinkPlayProcessor.Broadcast(LinkPlayResponse.Resp11PlayerInfo(this), this);

                    Counter += 9999;
                    RoomState = RoomStates.Syncing;
                    return true;
                }

                if (RoomState == RoomStates.Syncing && (CountDown < 0 || IsAllState(PlayerStates.Synced)))
                {
                    CountDown += 2999;
                    RoomState = RoomStates.Skill;
                    return true;
                }
            }
            if (RoomState == RoomStates.Playing && IsAllState(PlayerStates.GameEnd, true))
            {
                //MakeFinish()
                return true;
            }
            if (RoomState == RoomStates.GameEnd && IsAllState(PlayerStates.Choosing, true))
            {
                SongIdxWithDiff = -1;
                ClearPrepareInfo();
                if (RoundRobin)
                { 
                    //Robin
                }
                RoomState = RoomStates.Locked;
                return true;
            }
            return false;
        }

        public async Task UpdateUnlocks()
        {
            var oldSongMap = SongMap;
            var redisRoom = await LinkPlayRedisFetcher.FetchRoomById(RoomId);
            SongMap = LinkPlayCrypto.UnlocksAggregation(redisRoom.AllowSongs);
            if (oldSongMap != SongMap)
            {
                await LinkPlayProcessor.Broadcast(LinkPlayResponse.Resp14SongMapUpdate(this), this); Counter++;
            }
        }

        public async Task AlterState(RoomStates roomState)
        {
          var oldState = RoomState;
          if (roomState != oldState)
          {
              RoomState = roomState;
              await LinkPlayProcessor.Broadcast(LinkPlayResponse.Resp13PartRoomInfo(this), this); Counter++;
          }
        }

        public void ClearPrepareInfo()
        {
            for (var i = 0; i < Players.Length; i++)
            {
                Players[i].Score = 0;
                Players[i].ClearType = ClearTypes.None;
                Players[i].DownloadProgress = -1;
                Players[i].LastScore = Players[i].LastSongTime = 0;            
            }
        }

        public async Task<int> RemovePlayer(ulong token, ulong playerId)
        {
            var redisRoom = await LinkPlayRedisFetcher.FetchRoomById(RoomId);
            var playerIndex = redisRoom.PlayerId.IndexOf(playerId.ToString());
            if (playerIndex == -1) return -1;
            var removedEndPoint = Players[playerIndex].EndPoint;
            redisRoom.Token.RemoveAt(playerIndex);
            redisRoom.UserId.RemoveAt(playerIndex);
            redisRoom.AllowSongs.RemoveAt(playerIndex);
            redisRoom.PlayerId.Remove(playerId.ToString());
            Players[playerIndex] = new Player();
            await SendMsg(LinkPlayResponse.Resp12PlayerUpdate(this, playerIndex), BitConverter.GetBytes(token), removedEndPoint);
            await LinkPlayRedisFetcher.ReassignRedisRoom(redisRoom);
            return playerIndex;
        }
    }
}