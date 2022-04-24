using static Team123it.Arcaea.MarveCube.GlobalProperties;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System;
using System.Enhance.Web.Json;
using Team123it.Arcaea.MarveCube.Processors.Front;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Processors.Background;

namespace Team123it.Arcaea.MarveCube.Controllers
{
    /// <summary>
    /// [API Controller]Link Play多人游玩相关API控制器类。<br />
    /// 对应处理类: <see cref="Multiplayer"/>
    [Route("years/19/multiplayer")]
    [ApiController]
    public class MultiplayerController : ControllerBase
    {
		/// <summary>
		/// [API Action][POST]创建一个新的Link Play多人游戏房间。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="i">玩家的用户id。</param>
		/// <param name="wrapper">
		/// 包含Link Play客户端初始化数据的 <see cref="JObject"/> 类实例。
		/// <para>
		/// Link Play客户端初始化数据包括:
		/// <list type="bullet">
		/// <item>
		/// <see cref="int"/> protocolVersion - UDP下Link Play传输协议(616协议[数据明文开头十六进制为06 16])的版本号。<br />
		/// 截至Arcaea版本3.12.6该版本号值为<b>9</b>。
		/// </item>
		/// <item>
		/// <see cref="JArray"/> clientSongMap - 玩家的客户端式Link Play曲目解锁表(使用idx作为曲目的唯一标识符)。<br />
		/// (Link Play曲目解锁表类型说明详见 <seealso cref="Multiplayer.ConvertUnlocks(JObject)"/> )
		/// </item>
		/// </list>
		/// </para>
		/// </param>
		/// <returns>Json字符串。</returns>
		/// <exception cref="ArcaeaAPIException" />
		[HttpPost("me/room/create")]
        public async Task<JObjectResult> CreateRoom([FromHeader]string Authorization, [FromHeader]int i, [FromBody]JObject wrapper)
        {
            return await Task.Run(new Func<JObjectResult>(() =>
            {
				if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
				if (Authorization.ToLower().Trim().StartsWith("bearer"))
				{
					string token = Authorization.Split(" ")[1];
					uint? userid = Tokens.GetUserIdByToken(token); //获取token对应的用户id
					if (Maintaining(out var players))
					{
						if (!userid.HasValue || !players.Contains((int)userid.Value))
						{
							return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.ServerMaintaining);
						}
					}
					if (userid != null) //如果获取到了用户id
					{
						try
						{
							var result = Multiplayer.CreateRoom(i, wrapper.Value<JObject>("clientSongMap")!);
							var r = new JObject()
							{
								{ "success", true },
								{ "value", result }
							};
							return new JObjectResult(r);
						}
						catch (ArcaeaAPIException ex) //如果发生了异常
						{
							return ex; //直接返回对应异常的Json
						}
						catch (Exception) //发生了未知异常
						{
							return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
						}
					}
					else
					{
						return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.LoggedInAnotherDevice);
					}
				}
				else
				{
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
            }));
        }

		/// <summary>
		/// [API Action][POST]加入一个已有的Link Play多人游戏房间。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="roomCode">要加入的6位房间号。</param>
		/// <param name="i">玩家的用户id。</param>
		/// <param name="wrapper">
		/// 包含Link Play客户端初始化数据的 <see cref="JObject"/> 类实例。
		/// <para>
		/// Link Play客户端初始化数据包括:
		/// <list type="bullet">
		/// <item>
		/// <see cref="int"/> protocolVersion - UDP下Link Play传输协议(616协议[数据明文开头十六进制为06 16])的版本号。<br />
		/// 截至Arcaea版本3.12.6该版本号值为<b>9</b>。
		/// </item>
		/// <item>
		/// <see cref="JArray"/> clientSongMap - 玩家的客户端式Link Play曲目解锁表(使用idx作为曲目的唯一标识符)。<br />
		/// (Link Play曲目解锁表类型说明详见 <seealso cref="Multiplayer.ConvertUnlocks(JObject)"/> )
		/// </item>
		/// </list>
		/// </para>
		/// </param>
		/// <returns>Json字符串。</returns>
		/// <exception cref="ArcaeaAPIException" />
		[HttpPost("me/room/join/{roomCode}")]
        public async Task<JObjectResult> JoinRoom([FromHeader]string Authorization, [FromRoute]string roomCode, [FromHeader]int i, [FromBody]JObject wrapper)
        {
            return await Task.Run(new Func<JObjectResult>(() =>
            {
				if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
				if (Authorization.ToLower().Trim().StartsWith("bearer"))
				{
					string token = Authorization.Split(" ")[1];
					uint? userid = Tokens.GetUserIdByToken(token); //获取token对应的用户id
					if (Maintaining(out var players))
					{
						if (!userid.HasValue || !players.Contains((int)userid.Value))
						{
							return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.ServerMaintaining);
						}
					}
					if (userid != null) //如果获取到了用户id
					{
						try
						{
							var result = Multiplayer.JoinRoom(roomCode, i, wrapper.Value<JObject>("clientSongMap")!);
							var r = new JObject()
							{
								{ "success", true },
								{ "value", result }
							};
							return new JObjectResult(r);
						}
						catch (ArcaeaAPIException ex) //如果发生了异常
						{
							return ex; //直接返回对应异常的Json
						}
						catch (Exception) //发生了未知异常
						{
							return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
						}
					}
					else
					{
						return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.LoggedInAnotherDevice);
					}
				}
				else
				{
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
            }));
        }
    }
}