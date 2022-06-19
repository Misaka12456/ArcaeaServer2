namespace Team123it.Arcaea.MarveCube.LinkPlay.Models
{
    public enum PlayerStates 
    {
        Idle = 1, // 选歌

        Downloading = 2, // 正在下载
        NotReady = 3, // 在准备界面，自己没准备好
        Ready = 4, // 自己准备好了

        Syncing = 5,
        Synced = 6,

        Playing = 7, // 正在游玩
        GameEnd = 8, // 结算或者关门
    };

    public enum RoomStates 
    {
        Locked = 1, // 在有人 online 为 false 时进入此状态
        Idle = 2, // 选歌

        NotReady = 3, // 在准备界面，有人没准备好
        Countdown = 4, // 在准备界面，所有人都准备好了，进入倒计时

        Syncing = 5, // 同步延迟
        Skill = 6, // 游玩之前的技能展示

        Playing = 7, // 正在游玩
        GameEnd = 8, // 结算
        // 所有人要么离开要么关门也会进入状态 8
    };

    public enum Difficulties 
    {
        Empty = -1,
        Past = 0,
        Present = 1,
        Future = 2,
        Beyond = 3,
    };

    public enum ClearTypes 
    { // 与正常 ClearType 不同，0 代表不存在
        None = 0,
        TrackLost = 1,
        NormalComplete = 2,
        FullCombo = 3,
        PureMemory = 4,
        EasyClear = 5,
        HardClear = 6,
    };
}