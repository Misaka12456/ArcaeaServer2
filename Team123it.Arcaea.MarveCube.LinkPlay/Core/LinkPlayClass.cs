using System.Text;


namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public static class BytesHelper
    {
        public static byte[] Ushort2Bytes(ushort integer) => BitConverter.GetBytes(integer);
        public static byte[] Int2Bytes(int integer) => BitConverter.GetBytes(integer);
        public static byte[] Uint2Bytes(uint integer) => BitConverter.GetBytes(integer);
        public static byte[] Long2Bytes(long integer) => BitConverter.GetBytes(integer);
        public static byte[] Ulong2Bytes(ulong integer) => BitConverter.GetBytes(integer);
        public static ushort Bytes2Ushort(byte[] bytes) => BitConverter.ToUInt16(bytes);
        public static int Bytes2Int(byte[] bytes) => BitConverter.ToInt32(bytes);
        public static uint Bytes2Uint(byte[] bytes) => BitConverter.ToUInt32(bytes);
        public static long Bytes2Long(byte[] bytes) => BitConverter.ToInt64(bytes);
        public static ulong Bytes2Ulong(byte[] bytes) => BitConverter.ToUInt64(bytes);
    }
    
    

    public enum Difficulties
    {
        Empty = 0xff,
        Past = 0,
        Present = 1,
        Future = 2,
        Beyond = 3
    }
    public enum ClearTypes
    {
        Empty = 0,
        TrackLost = 1,
        NormalClear = 2,
        FullRecall = 3, 
        PureMemory = 4,
        EasyClear = 5,
        HardClear = 6,
    }

    /// <summary>
    /// Room的状态 用于<see cref="Room.RoomState"/>
    /// </summary>
    /// <list type="bullet">
    /// <item>Locked - 房间锁定，在房间刚刚创建或者刚刚有人加入房间时短暂出现</item>
    /// <item>Choosing - 房间正在选择歌曲</item>
    /// <item>NotReady - 准备界面 还有人没准备好</item>
    /// <item>CountDown - 准备界面 所有人都准备好了 开始倒计时</item>
    /// <item>SyncLatency - 同步延迟(存疑)</item>
    /// <item>CountingDown - 倒计时中(存疑)</item>
    /// <item>Playing - 正在游玩</item>
    /// <item>GameEnd -  游戏结束结算, 关门或者所有人跑路</item>
    /// </list>
    public enum RoomStates
    {
        Locked = 1,
        Choosing = 2,  
        
        NotReady = 3,
        CountDown = 4,
        
        SyncLatency = 5,
        CountingDown = 6,
        
        Playing = 7,
        GameEnd = 8,
        
    }

    /// <summary>
    /// Player的状态 用于<see cref="Player.PlayerState"/>
    /// </summary>
    /// <item>Choosing - 正在选择歌曲</item>
    /// <item>Downloading - 正在下载歌曲</item>
    /// <item>NotReady - 准备界面 人没准备好</item>
    /// <item>Ready - 准备界面 人准备好了</item>
    /// <item>Syncing - 进入游戏 但是在显示技能前</item>
    /// <item>Async - 在Syncing Stage 1.5s 后出现</item>
    /// <item>Playing - 正在游玩</item>
    /// <item>GameEnd - 游戏结束结算, 关门或者跑路</item>
    public enum PlayerStates
    {
        Choosing = 1, 
        
        Downloading = 2, 
        NotReady = 3, 
        Ready = 4, 
        
        Syncing = 5, 
        Async = 6, 
        
        Playing = 7, 
        GameEnd = 8, 
    }
    
    public struct Player
    {
        public Player(int init) { }
        public ulong PlayerId { get; set; } = 0;
        public byte[] PlayerName { get; set; } = Encoding.ASCII.GetBytes("ArcaeaTest");
        public ulong Token { get; set; } = 0;

        public int CharacterId { get; set; } = 0xff;
        public int LastCharacterId { get; set; } = 0xff;
        public uint IsCharacterUncapped { get; set; } = 0;

        public Difficulties Difficulty { get; set; } = Difficulties.Empty;
        public Difficulties LastDifficulty { get; set; } = Difficulties.Empty;
        public uint Score { get; set; } = 0;
        public uint LastScore { get; set; } = 0;
        public uint Timer { get; set; } = 0;
        public uint LastTimer { get; set; } = 0;
        public ClearTypes ClearType { get; set; } = ClearTypes.Empty;
        public ClearTypes LastClearType { get; set; } = ClearTypes.Empty;
        public int BestScoreFlag { get; set; } = 0;
        public int BestPlayerFlag { get; set; } = 0;
        public int FinishFlag { get; set; } = 0;
    
        public PlayerStates PlayerState { get; set; } = PlayerStates.Choosing;
        public int DownloadPercent { get; set; } = 0;
        public int OnlineState { get; set; } = 0;
        
        public ulong LastTimestamp { get; set; } = 0;
        public int ExtraCommandQueue { get; set; } = 0;
        public byte[] SongUnlock{ get; set; } = new byte[512];
        
        public int StartCommandCount { get; set; } = 0;

        public void SetPlayerName(string playerName) { PlayerName = Encoding.UTF8.GetBytes(playerName)[..16]; }
    }

    public struct Room
    {
        public Room(int init) { }
        public ulong RoomId { get; set; } = 0;
        public string? RoomCode { get; set; } = "ARCAEA";
        
        public uint CountDown { get; set; } = 0xffffffff;
        public ulong Timestamp { get; set; } = 0;
        public RoomStates RoomState { get; set; } = RoomStates.Locked;
        public ushort SongIdx { get; set; } = 0xffff;
        public ushort LastSongIdx { get; set; } = 0xffff;
        
        public byte[] SongUnlock { get; set; } = new byte[512];
        
        public ulong HostId { get; set; } = 0;
        public Player[] Players { get; set; } = {new(), new(), new(), new()};
        public int PlayerCount { get; set; } = 0;
        
        public ushort Interval { get; set; } = 1000;
        public ulong Times { get; set; } = 100;
        public int RoundSwitch { get; set; } = 0;
        
        public List<byte> CommandQueue { get; set; } = new();
        public uint CommandQueueLength { get; set; } = 0;

        public byte[] GetPlayerInfo()
        {
            var returnedBytes = new List<byte>();
            foreach (var player in Players)
            {
                returnedBytes.AddRange(BytesHelper.Ulong2Bytes(player.PlayerId));
                returnedBytes.AddRange(BytesHelper.Int2Bytes(player.CharacterId)[..1]);
                returnedBytes.AddRange(BytesHelper.Uint2Bytes(player.IsCharacterUncapped)[..1]);
                returnedBytes.AddRange(BytesHelper.Int2Bytes((int) player.Difficulty)[..1]);
                returnedBytes.AddRange(BytesHelper.Uint2Bytes(player.Score));
                returnedBytes.AddRange(BytesHelper.Uint2Bytes(player.Timer));
                returnedBytes.AddRange(BytesHelper.Uint2Bytes((uint) player.ClearType)[..1]);
                returnedBytes.AddRange(BytesHelper.Uint2Bytes((uint) player.PlayerState)[..1]);
                returnedBytes.AddRange(BytesHelper.Int2Bytes(player.DownloadPercent)[..1]);
                returnedBytes.AddRange(BytesHelper.Int2Bytes(player.OnlineState)[..1]);
                returnedBytes.AddRange(BytesHelper.Int2Bytes(player.StartCommandCount)[..1]);
                returnedBytes.AddRange(player.PlayerName[..16]);
            }
            return returnedBytes.ToArray();
        }

        public byte[] GetPlayerLastScore()
        {
            if (LastSongIdx == 0xffff)
            {
                var emptyBytes = new List<byte>();
                for (var i = 0; i < 4; i++) emptyBytes.AddRange(new byte[]{0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00});
                return emptyBytes.ToArray();
            }

            var returnedBytes = new List<byte>();
            for (var i = 0; i < 4; ++i)
            {
                var player = Players[i];
                if (player.PlayerId != 0)
                {
                    returnedBytes.AddRange(BytesHelper.Int2Bytes(player.LastCharacterId)[..1]);
                    returnedBytes.AddRange(BytesHelper.Uint2Bytes((uint)player.LastDifficulty)[..1]);
                    returnedBytes.AddRange(BytesHelper.Uint2Bytes(player.LastScore));
                    returnedBytes.AddRange(BytesHelper.Uint2Bytes((uint)player.LastClearType)[..1]);
                    returnedBytes.AddRange(BytesHelper.Int2Bytes(player.BestScoreFlag)[..1]);
                    returnedBytes.AddRange(BytesHelper.Int2Bytes(player.BestPlayerFlag)[..1]);
                }
                else
                {
                    returnedBytes.AddRange(new byte[]{0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00});
                }
                
            }
            return returnedBytes.ToArray();
        }
    }
}