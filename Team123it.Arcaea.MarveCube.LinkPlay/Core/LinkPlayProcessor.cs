using System.Net;
using Team123it.Arcaea.MarveCube.LinkPlay.Entities;
using Team123it.Arcaea.MarveCube.LinkPlay.Models;

namespace Team123it.Arcaea.MarveCube.LinkPlay.Core
{
    public class LinkPlayProcessor
    {
        private ulong _roomId;
    
        private Room _room;

        private EndPoint _endPoint;
    
        private readonly byte[] _message;
    
        public LinkPlayProcessor(byte[] data, EndPoint endpoint)
        {
            var token = LPRedis.FetchRoomIdByToken(BitConverter.ToUInt64(data.AsSpan()[4..12])).Result;
            _roomId = token.RoomId;
            _room = RoomManager.FetchRoomById(_roomId).GetValueOrDefault();
            _endPoint = endpoint;
            _message = data;
        }
    
        public async Task ProcessPacket()
        {
            switch (_message[2])
            {
                case 0x01:
                {
                    await Process01TryGiveHost();
                    break;
                }
                case 0x02:
                {
                    await Process02TrySelectSong();
                    break;
                }
                case 0x03:
                {
                    await Process03SongFinish();
                    break;
                }
                case 0x04:
                {
                    await Process04TryKickPlayer();
                    break;
                }
                case 0x06:
                {
                    await Process06ReturnToLobby();
                    break;
                }
                case 0x07:
                {
                    await Process07UnlocksUpdate();
                    break;
                }
                case 0x08:
                {
                    await Process08RoundRobin();
                    break;
                }
                case 0x09:
                {
                    await Process09Ping();
                    break;
                }
                case 0x0A:
                {
                    await Process0ALeaveRoom();
                    break;
                }
                case 0x0B:
                {
                    await Process0BSongSuggestion();
                    break;
                }
            }
        }

        private async Task Process01TryGiveHost()
        {
            var packet = _message.Deserialize<LPRequest.Req01TryGiveHost>();
            try
            {
                if (!_room.HostValidation(packet.Token)) throw LPExceptions.NotHost;
                if (!_room.Players.Any(x => x.PlayerId == packet.PlayerId)) throw new LPExceptions(1); // 给了不存在的人
                if (!_room.GetPlayer(packet.Token, out _).OnlineState) throw new LPExceptions(2); // 给了不在线的人
                if (_room.RoomState > RoomStates.Idle) throw new LPExceptions(4); // 开始了就不能给了
                
                _room.HostId = packet.PlayerId;

                await _room.Broadcast(_room.Resp10HostTransfer());
                _room.ReassignRoom();
            }
            catch (LPExceptions e)
            {
                await Program.SendMsg(_room.Resp0DRaiseErrors(e.ErrorCode), BitConverter.GetBytes(packet.Token), _endPoint);
            }
        }

        private async Task Process02TrySelectSong()
        {
            var packet = _message.Deserialize<LPRequest.Req02TrySelectSong>();
            try
            {
                if (!_room.HostValidation(packet.Token)) throw LPExceptions.NotHost;
                if (_room.RoomState != RoomStates.Idle) throw LPExceptions.CannotStart;
                if (_room.Players.Count < 2) throw LPExceptions.NeedMorePlayers;

                _room.RoomState = RoomStates.NotReady;
                _room.SongIdxWithDiff = packet.SongIdxWithDiff;

                await _room.Broadcast(_room.Resp11PlayerInfo());
                await _room.Broadcast(_room.Resp13PartRoomInfo());
                _room.ReassignRoom();
            }
            catch (LPExceptions e)
            {
                await Program.SendMsg(_room.Resp0DRaiseErrors(e.ErrorCode), BitConverter.GetBytes(packet.Token), _endPoint);
            }

        }

        private async Task Process03SongFinish()
        {
            var packet = _message.Deserialize<LPRequest.Req03SongFinish>();
            var player = _room.GetPlayer(packet.Token, out var playerIndex);
            try
            {
                if (player.PlayerState != PlayerStates.Playing) throw new LPExceptions(1);
                
                player.PlayerState = PlayerStates.GameEnd;
                player.Score = packet.Score;
                player.ClearType = packet.ClearType;
                player.Difficulty = packet.Difficulty;
                player.PersonalBest = packet.PersonalBest;

                await _room.Broadcast(_room.Resp12PlayerUpdate(playerIndex));
                _room.ReassignPlayer(player, playerIndex);
                _room.ReassignRoom();
            }
            catch (LPExceptions e)
            {
                await Program.SendMsg(_room.Resp0DRaiseErrors(e.ErrorCode), BitConverter.GetBytes(packet.Token), _endPoint);
            }
        }

        private async Task Process04TryKickPlayer()
        {
            var packet = _message.Deserialize<LPRequest.Req04TryKickPlayer>();

            try
            {
                if (!_room.HostValidation(packet.Token)) throw LPExceptions.NotHost;
                if (!_room.Players.Any(x => x.PlayerId == packet.PlayerId)) throw new LPExceptions(2); // 踢了不存在的人

                await _room.RemovePlayer(packet.PlayerId);
                _room.ReassignRoom();
            }
            catch (LPExceptions e)
            {
                await Program.SendMsg(_room.Resp0DRaiseErrors(e.ErrorCode), BitConverter.GetBytes(packet.Token), _endPoint);
            }
        }

        private async Task Process06ReturnToLobby()
        {
            var packet = _message.Deserialize<LPRequest.Req06ReturnToLobby>();
            // TODO: Host Validation

            try
            {
                if (!_room.HostValidation(packet.Token)) throw LPExceptions.NotHost;
                if (_room.RoomState != RoomStates.NotReady) throw new LPExceptions(4);
                
                await _room.LeavePrepareState();
                _room.ReassignRoom();
            }
            catch (LPExceptions e)
            {
                await Program.SendMsg(_room.Resp0DRaiseErrors(e.ErrorCode), BitConverter.GetBytes(packet.Token), _endPoint);
            }
        }

        private async Task Process07UnlocksUpdate()
        {
            var packet = _message.Deserialize<LPRequest.Req07UnlocksUpdate>();
            var player = _room.GetPlayer(packet.Token, out var playerIndex);

            player.SongMap = packet.SongMap;

            await _room.UpdateSongMap();
            _room.ReassignPlayer(player, playerIndex);
            _room.ReassignRoom();
        }

        private async Task Process08RoundRobin()
        {
            var packet = _message.Deserialize<LPRequest.Req08RoundRobinEnable>();

            try
            {
                if (!_room.HostValidation(packet.Token)) throw LPExceptions.NotHost;
                _room.RoundRobin = packet.RobinEnabled;

                await _room.Broadcast(_room.Resp13PartRoomInfo());
                _room.ReassignRoom();
            }
            catch (LPExceptions e)
            {
                await Program.SendMsg(_room.Resp0DRaiseErrors(e.ErrorCode), BitConverter.GetBytes(packet.Token), _endPoint);
            }
        }

        private async Task Process09Ping()
        {
            var packet = _message.Deserialize<LPRequest.Req09Ping>();
            _room = RoomManager.FetchRoomById(_roomId).GetValueOrDefault();
            var (player, playerIndex) = await LinkPlayInstance.Handler(packet, _endPoint);
        
            if (packet.Counter > _room.Counter) return; // Not matter
        
            if (packet.Counter < _room.Counter) // Counter is smaller than current counter, so get resend packet
            {
                var sendPacket = _room.GetResendPack(packet.Counter);
                await Program.SendMsg(sendPacket, BitConverter.GetBytes(packet.Token), _endPoint);
                return;
            }
        
            await Program.SendMsg(_room.Resp0CPing(), BitConverter.GetBytes(packet.Token), _endPoint);
            // TODO: Interval Check
        
            if (!player.OnlineState) 
            {
                player.OnlineState = true;

                if (_room.Players.Count > 1) // 如果不止一个人，那么发一个 12 包
                {
                    await _room.Broadcast(_room.Resp12PlayerUpdate(playerIndex));
                }
            }
        
            if (_room.RoomState >= RoomStates.Playing && player.SongTime != packet.SongTime) // Score updating
            {
                player.LastScore = player.Score;
                player.LastSongTime = player.SongTime;
                player.SongTime = packet.SongTime;
                player.Score = packet.Score;

                await _room.Broadcast(_room.Resp0EScoreUpdates(playerIndex));
            }

            await _room.UpdateState();
            _room.ReassignRoom();
        }

        private async Task Process0ALeaveRoom()
        {
            var packet = _message.Deserialize<LPRequest.Req0ALeaveRoom>();
            var player = _room.GetPlayer(packet.Token, out _);
        
            await _room.RemovePlayer(player.PlayerId);
            _room.ReassignRoom();
        }

        private async Task Process0BSongSuggestion()
        {
            var packet = _message.Deserialize<LPRequest.Req0BSongSuggestion>();
            var _ = _room.GetPlayer(packet.Token, out var playerIndex);

            await _room.Broadcast(_room.Resp0FSongSuggestion(playerIndex, packet.SongIdxWithDiff));
            _room.ReassignRoom();
        }
    }
}