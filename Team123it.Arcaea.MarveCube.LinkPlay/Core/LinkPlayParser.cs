using Team123it.Arcaea.MarveCube.LinkPlay.Models;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core;

public static class LPParser
{
    public static LPRequest.Req01TryGiveHost ParseClientPack01(this byte[] data)
    {
        return new LPRequest.Req01TryGiveHost()
        {
            Prefix = data[..4],
            Token = BitConverter.ToUInt64(data.AsSpan()[4..12]),
            Counter = BitConverter.ToUInt32(data.AsSpan()[12..16]),
            ClientTime = BitConverter.ToUInt64(data.AsSpan()[16..24]),
            PlayerId = BitConverter.ToUInt64(data.AsSpan()[24..32])            
        };
    }


    public static LPRequest.Req02TrySelectSong ParseClientPack02(this byte[] data)
    {
        return new LPRequest.Req02TrySelectSong()
        {
            Prefix = data[..4],
            Token = BitConverter.ToUInt64(data.AsSpan()[4..12]),
            Counter = BitConverter.ToUInt32(data.AsSpan()[12..16]),
            ClientTime = BitConverter.ToUInt64(data.AsSpan()[16..24]),
            SongIdxWithDiff = BitConverter.ToInt16(data.AsSpan()[24..26])
        };
    }
        
    public static LPRequest.Req04TryKickPlayer ParseClientPack04(this byte[] data)
    {
        return new LPRequest.Req04TryKickPlayer()
        {
            Prefix = data[..4],
            Token = BitConverter.ToUInt64(data.AsSpan()[4..12]),
            Counter = BitConverter.ToUInt32(data.AsSpan()[12..16]),
            ClientTime = BitConverter.ToUInt64(data.AsSpan()[16..24]),
            PlayerId = BitConverter.ToUInt64(data.AsSpan()[24..32])            
        };
    }
        
    public static LPRequest.Req06ReturnToLobby ParseClientPack06(this byte[] data)
    {
        return new LPRequest.Req06ReturnToLobby()
        {
            Prefix = data[..4],
            Token = BitConverter.ToUInt64(data.AsSpan()[4..12]),
            Counter = BitConverter.ToUInt32(data.AsSpan()[12..16]),
            ClientTime = BitConverter.ToUInt64(data.AsSpan()[16..24]),
        };
    }
        
    public static LPRequest.Req07UnlocksUpdate ParseClientPack07(this byte[] data)
    {
        return new LPRequest.Req07UnlocksUpdate()
        {
            Prefix = data[..4],
            Token = BitConverter.ToUInt64(data.AsSpan()[4..12]),
            Counter = BitConverter.ToUInt32(data.AsSpan()[12..16]),
            ClientTime = BitConverter.ToUInt64(data.AsSpan()[16..24]),
            SongMap = data[24..536]
        };
    }

    /// <summary>
    /// Host transfer from the client, return ClientPack08
    /// </summary>
    public static LPRequest.Req08RoundRobinEnable ParseClientPack08(this byte[] data)
    {
        return new LPRequest.Req08RoundRobinEnable()
        {
            Prefix = data[..4],
            Token = BitConverter.ToUInt64(data.AsSpan()[4..12]),
            Counter = BitConverter.ToUInt32(data.AsSpan()[12..16]),
            ClientTime = BitConverter.ToUInt64(data.AsSpan()[16..24]),
            RobinEnabled = BitConverter.ToBoolean(data.AsSpan()[24..])
        };
    }
    
    public static LPRequest.Req09Ping ParseClientPack09(this byte[] data)
    {
        return new LPRequest.Req09Ping()
        {
            Prefix = data[..4],
            Token = BitConverter.ToUInt64(data.AsSpan()[4..12]),
            Counter = BitConverter.ToUInt32(data.AsSpan()[12..16]),
            ClientTime = BitConverter.ToUInt64(data.AsSpan()[16..24]),
            Score = BitConverter.ToUInt32(data.AsSpan()[24..28]),
            SongTime = BitConverter.ToUInt32(data.AsSpan()[28..32]),
            State = (PlayerStates)data[32],
            Difficulty = (Difficulties)data[33],
            ClearType = (ClearTypes)data[34],
            DownloadProgress = data[35],
            Character = data[36],
            CharacterUncapped = BitConverter.ToBoolean(data[36..37])
        };
    }

    public static LPRequest.Req0ALeaveRoom ParseClientPack0A(this byte[] data)
    {
        return new LPRequest.Req0ALeaveRoom()
        {
            Prefix = data[..4],
            Token = BitConverter.ToUInt64(data.AsSpan()[4..12]),
            Counter = BitConverter.ToUInt32(data.AsSpan()[12..16]),
        };
    }

    public static LPRequest.Req0BSongSuggestion ParseClientPack0B(this byte[] data)
    {
        return new LPRequest.Req0BSongSuggestion()
        {
            Prefix = data[..4],
            Token = BitConverter.ToUInt64(data.AsSpan()[4..12]),
            Counter = BitConverter.ToUInt32(data.AsSpan()[12..16]),
            SongIdxWithDiff = BitConverter.ToInt16(data.AsSpan()[16..18])
        };
    }
}