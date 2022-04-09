using static Team123it.Arcaea.MarveCube.GlobalProperties;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Processors.Background;
using Team123it.Arcaea.MarveCube.Processors.Front;
using World = Team123it.Arcaea.MarveCube.Processors.Front.World;
using System.Enhance.Web.Json;

namespace Team123it.Arcaea.MarveCube.Controllers
{
	/// <summary>
	/// [API Controller]World模式相关API控制器类。<br />
	/// 对应处理类: <see cref="User"/>
	/// </summary>
	[Route("years/19/world")]
	[ApiController]
	public class WorldController : ControllerBase
	{
		/// <summary>
		/// [API Action][GET]获取当前玩家的完整世界模式数据。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <returns>Json字符串。</returns>
		[HttpGet("map/me")]
		public Task<JObjectResult> myFullWorldInfo([FromHeader]string Authorization)
		{
			return Task.Run(new Func<JObjectResult>(() =>
			{
				if (Authorization.ToLower().StartsWith("bearer"))
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
							var result = World.GetAllWorldInfo(userid.Value);
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
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.UsernameOrPasswordInvalid);
				}
			}));
		}

		/// <summary>
		/// [API Action][GET]选择并进入指定地图。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="map_id">指定地图的id。</param>
		/// <returns>Json字符串。</returns>
		[HttpPost("map/me")]
		public Task<JObjectResult> entryWorldMap([FromHeader]string Authorization,[FromForm]string map_id)
		{
			return Task.Run(new Func<JObjectResult>(() =>
			{
				if (Authorization.ToLower().StartsWith("bearer"))
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
							var result = World.GetUserMapInfo(userid.Value, map_id);
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
						catch (Exception ex) //发生了未知异常
						{
							Console.WriteLine(ex.ToString());
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
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.UsernameOrPasswordInvalid);
				}
			}));
		}

		/// <summary>
		/// [API Action][GET]获取单个地图的完整信息。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="map_id">指定地图的id。</param>
		/// <returns>Json字符串。</returns>
		[HttpGet("map/me/{map_id}")]
		public Task<JObjectResult> getSingleMapInfo([FromHeader]string Authorization,string map_id)
		{
			return Task.Run(new Func<JObjectResult>(() =>
			{
				if (Authorization.ToLower().StartsWith("bearer"))
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
							var result = World.GetUserSingleMap(userid.Value, map_id);
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
						catch (Exception ex) //发生了未知异常
						{
							Console.WriteLine(ex.ToString());
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
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.UsernameOrPasswordInvalid);
				}
			}));
		}
	}
}
