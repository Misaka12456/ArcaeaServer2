namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public class LPExceptions: Exception
    {
        public int ErrorCode { get; }
        
        public LPExceptions(int message) => ErrorCode = message;

        public static LPExceptions NotHost => new (3); // You are not the host

        public static LPExceptions CannotStart => new (5); // There is still player can not start
        
        public static LPExceptions NeedMorePlayers => new (6); // Need more players to start
        
        public static LPExceptions CannotPlaySong => new (7); // There is still player that has no unlocked song
    }
}