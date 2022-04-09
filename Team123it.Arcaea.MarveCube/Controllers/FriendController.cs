using static Team123it.Arcaea.MarveCube.GlobalProperties;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Processors.Background;
using Team123it.Arcaea.MarveCube.Processors.Front;
using System.Enhance.Web.Json;

namespace Team123it.Arcaea.MarveCube.Controllers
{
	/// <summary>
	/// [API Controller]玩家好友管理相关API控制器类。<br />
	/// 对应处理类: <see cref="Friend"/>
	/// </summary>
	[Route("years/19/friend")]
	[ApiController]
	public class FriendController : ControllerBase
	{
		/// <summary>
		/// [API Action][POST]添加好友。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="friend_code">要添加的好友所对应的玩家的9位好友id。</param>
		/// <returns>Json字符串。</returns>
		[HttpPost("me/add")]
		public Task<JObjectResult> add([FromHeader] string Authorization,[FromForm]string friend_code)
		{
			return Task.Run(new Func<JObjectResult>(() =>
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
							var result = Friend.AddFriend(userid.Value, (string)friend_code);
							var r = new JObject()
						{
							{"success",true },
							{"value",result }
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
		/// {API Action][POST]删除好友。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="friend_id">要删除的好友所对应的玩家的用户id(非好友id)。</param>
		/// <returns>Json字符串。</returns>
		[HttpPost("me/delete")]
		public Task<JObjectResult> delete([FromHeader] string Authorization, [FromForm] int friend_id)
		{
			return Task.Run(new Func<JObjectResult>(() =>
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
							var result = Friend.DeleteFriend(userid.Value, friend_id);
							var r = new JObject()
						{
							{"success",true },
							{"value",result }
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
