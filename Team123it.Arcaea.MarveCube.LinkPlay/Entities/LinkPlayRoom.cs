using System.Net;
using Team123it.Arcaea.MarveCube.LinkPlay.Core;
using Team123it.Arcaea.MarveCube.LinkPlay.Models;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Entities
{
    public struct Room
    {
        public ulong RoomId;
        public List<Player> Players = new();
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
            Players = new List<Player>();
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

        public async Task Broadcast(byte[] data)
        {
            foreach (var player in Players)
            {
                await Program.SendMsg(data, BitConverter.GetBytes(player.Token), player.EndPoint);
            }
            Counter++;
        }
        
        public bool HostValidation(ulong token) // return true if host
        {
            var player = Players.FirstOrDefault(x => x.Token == token);
            return HostId == player.PlayerId;
        }

        public Player GetPlayer(ulong token, out int playerIndex)
        {
            var player = Players.FirstOrDefault(x => x.Token == token);
            playerIndex = Players.IndexOf(player);
            return player;
        }
        
        public void ReassignPlayer(Player player, int playerIndex) => Players[playerIndex] = player;

        public async Task<(EndPoint, ulong, int)> DestroyPlayer(ulong playerId)
        {
            var player = Players.FirstOrDefault(x => x.PlayerId == playerId);
            var playerIndex = Players.IndexOf(player);
            var tokenObject = await LPRedis.FetchRoomIdByToken(player.Token);
            var roomObject = await LPRedis.FetchRoomById(tokenObject.RoomId);
            
            roomObject.Token.RemoveAt(playerIndex);
            roomObject.AllowSongs.RemoveAt(playerIndex);
            roomObject.PlayerId.RemoveAt(playerIndex);
            roomObject.UserId.RemoveAt(playerIndex);
            Players.Remove(player);
            await roomObject.ReassignRedisRoom();
            return (player.EndPoint, player.Token, playerIndex);
        }

        public async Task RemovePlayer(ulong playerId)
        {
            if (RoomState == RoomStates.NotReady)
            {
                var (endPoint, token , playerIndex) = await DestroyPlayer(playerId);
                // TODO: Host replacing
                await Program.SendMsg(this.Resp11PlayerInfo(), BitConverter.GetBytes(token), endPoint);
                await LeavePrepareState();
            }
            else
            {
                var (_, _, playerIndex) = await DestroyPlayer(playerId);
                await Broadcast(this.Resp12PlayerUpdate(playerIndex, new Player()));
                // TODO: Host replacing
                await UpdateSongMap();
                await UpdateState();
            }
        }

        private void ClearPrepareInfo()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                
                player.Score = 0;
                player.ClearType = ClearTypes.None;
                player.DownloadProgress = -1;
                // player.difficulty = Difficulty.None; // Theoretically there should be a behavior like this, but 616 doesn't have
                player.LastScore = player.LastSongTime = 0;
                
                ReassignPlayer(player, i);
            }
        }
        
        public async Task LeavePrepareState()
        {
            ClearPrepareInfo();
            RoomState = RoomStates.Locked;
            SongIdxWithDiff = -1;

            await Broadcast(this.Resp11PlayerInfo());
            await Broadcast(this.Resp13PartRoomInfo());
        }

        public async Task UpdateSongMap()
        {
            var oldSongMap = SongMap;
            for (int i = 0; i < 512; i++) SongMap[i] = 0xff; // Set all to 0xff
            foreach (var player in Players)
            {
                for (int i = 0; i < 512; i++) SongMap[i] &= player.SongMap[i]; // AND with each player's song map
            }
            if (oldSongMap.SequenceEqual(SongMap)) return;
            await Broadcast(this.Resp14SongMapUpdate());
        }

        public async Task SetState(RoomStates state)
        {
            if (RoomState == state) return;
            RoomState = state;
            await Broadcast(this.Resp13PartRoomInfo());
        }

        public bool IsAllState(PlayerStates state, bool canOffline = false) => 
            Players.All(x => x.PlayerState == state && x.OnlineState || (!x.OnlineState && canOffline));

        private void UpdateCountDown()
        {
            if (CountDownStart == null) return;
            var d = (long)((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds - CountDownStart.Value);
            CountDown -= d;
            CountDownStart += d;
        }

        private async Task MakeFinish()
        {
            foreach (var player in Players)
            {
                //TODO: Kick Player when they are offline
            }

            LastSong = SongIdxWithDiff;
            var topScore = 0;
            for (int i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                
                player.LastDifficulty = player.Difficulty;
                player.LastScore = player.Score;
                player.LastCharacter = player.Character;
                player.LastClearType = player.ClearType;

                player.Top = false;
                topScore = (int)Math.Max(topScore, player.Score);
                ReassignPlayer(player, i);
            }
            Players.Where(x => x.Score == topScore).ToList().ForEach(x => x.Top = true);

            await SetState(RoomStates.GameEnd);
        }

        public async Task UpdateState()
        {
            if (RoomState == RoomStates.Locked && IsAllState(PlayerStates.Idle)) // 1 -> 2
            {
                await SetState(RoomStates.Idle);
            }
            if (RoomState == RoomStates.Idle && IsAllState(PlayerStates.Idle)) // 2 -> 1
            {
                await SetState(RoomStates.Locked);
            }
            if (RoomState == RoomStates.NotReady && IsAllState(PlayerStates.Ready)) // 3 -> 4
            {
                CountDown = 3999;
                await SetState(RoomStates.Countdown);
                CountDownStart = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds;
            }
            
            UpdateCountDown();
            if (RoomState is >= RoomStates.Countdown and <= RoomStates.Skill) // state 4, 5, 6 handling
            { 
                if (RoomState == RoomStates.Skill && CountDown < 0) // 6 -> 7
                {
                    CountDown = -1;
                    CountDownStart = null;
                    await SetState(RoomStates.Playing);
                }
                if (RoomState == RoomStates.Countdown && CountDown < 0) // 4 -> 5
                {
                    ClearPrepareInfo();
                    await Broadcast(this.Resp11PlayerInfo());

                    CountDown += 9999;
                    await SetState(RoomStates.Syncing);
                }
                if (RoomState == RoomStates.Syncing && (CountDown < 0 || IsAllState(PlayerStates.Synced))) // 5 -> 6
                { 
                    CountDown += 2999;
                    await SetState(RoomStates.Skill);
                }
                return;
            }
            if (RoomState == RoomStates.Playing && IsAllState(PlayerStates.GameEnd, true)) // 7 -> 8
            {
                await MakeFinish();
            }
            if (RoomState == RoomStates.GameEnd && IsAllState(PlayerStates.Idle, true)) // 8 -> 1
            {
                SongIdxWithDiff = -1;
                ClearPrepareInfo();
                if (RoundRobin)
                {
                    var hostId = HostId;
                    var player = Players.FirstOrDefault(x => x.PlayerId == hostId);
                    var playerIndex = Players.IndexOf(player);
                    HostId = Players[(playerIndex + 1) % Players.Count].PlayerId;
                }
                await SetState(RoomStates.Locked);
            }
        }
        
        public byte[] GetResendPack(uint clientCounter)
        {
            return Counter - clientCounter > 0
                ? this.Resp15FullRoomInfo()
                : Array.Empty<byte>();
        }

        public static long ServerTime => (long) (DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds * 1000;
    }
}