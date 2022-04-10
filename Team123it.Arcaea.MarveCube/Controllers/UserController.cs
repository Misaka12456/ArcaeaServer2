using static Team123it.Arcaea.MarveCube.GlobalProperties;
using User2 = Team123it.Arcaea.MarveCube.Processors.Front.User;
using File2 = System.IO.File;
using Path2 = System.IO.Path;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Processors.Background;
using Team123it.Arcaea.MarveCube.Processors.Front;
using System.Enhance.Web.Json;
using System.Web;
using System.Text;
using Newtonsoft.Json;

namespace Team123it.Arcaea.MarveCube.Controllers
{
	/// <summary>
	/// [API Controller]玩家用户数据相关API控制器类。<br />
	/// 对应处理类: <see cref="User"/>
	/// </summary>
	[Route("years/19/user")]
	[ApiController]
	public class UserController : ControllerBase
	{
		/// <summary>
		/// [API Action][POST]切换角色。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="character">要切换到的目标角色的id。</param>
		/// <param name="skill_sealed">角色技能是否被封印。(本参数值为字符串式布尔型,如 "false" )</param>
		/// <returns>Json字符串。</returns>
		[HttpPost("me/character")]
		public Task<JObjectResult> character([FromHeader]string Authorization,[FromForm] int character,[FromForm] string skill_sealed)
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
							bool isSkillSealed = skill_sealed != "false";
							var result = Processors.Front.User.ChangeCharacter(userid!.Value, (uint)character, isSkillSealed);
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
		/// [API Action][POST]新玩家注册。
		/// </summary>
		/// <param name="name">新玩家的昵称。</param>
		/// <param name="password">新玩家的密码。</param>
		/// <param name="email">新玩家的E-mail地址。</param>
		/// <returns>Json字符串。</returns>
		[HttpPost]
		public Task<JObjectResult> register([FromForm]string name,[FromForm]string password,[FromForm]string email)
		{
			return Task.Run(new Func<JObjectResult>(() =>
			{
				if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
				if (Maintaining(out _)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.ServerMaintaining);
				if (Request.IsObsoleteClientVer()) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.NeedUpdateClient);
				try
				{
					return new JObjectResult(Processors.Front.User.Register(name,password,email));
				}
				catch (ArcaeaAPIException ex)
				{
					return ex;
				}
				catch (Exception)
				{
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
			}));
		}

		[HttpGet("me/save")]
		public Task<JObjectResult> cloudFetch([FromHeader]string Authorization)
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
					if (userid.HasValue) //如果获取到了用户id
					{
						try
						{
							var syncData = Synchronization.FetchAll(userid.Value);
							var r = new JObject()
							{
								{"success",true },
								{"value", syncData }
							};
							Console.WriteLine(r.ToString());
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

		[HttpPost("me/save")]
		public Task<JObjectResult> cloudSave([FromHeader] string Authorization, [FromForm]IFormCollection form)
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
					if (userid.HasValue) //如果获取到了用户id
					{
						try
						{
							string scores_data = form["scores_data"];
							string clearlamps_data = form["clearlamps_data"];
							string clearedsongs_data = form["clearedsongs_data"];
							string unlocklist_data = form["unlocklist_data"];
							string story_data = form["story_data"];
							string installid_data = form["installid_data"];
							string devicemodelname_data = form["devicemodelname_data"];
							var scores = JObject.Parse(scores_data).Value<JArray>("");
							var clearLamps = JObject.Parse(clearlamps_data).Value<JArray>("");
							var clearedSongs = JObject.Parse(clearedsongs_data).Value<JArray>("");
							var unlockList = JObject.Parse(unlocklist_data).Value<JArray>("");
							var storyData = JObject.Parse(story_data).Value<JArray>("");
							string installId = JObject.Parse(installid_data).Value<string>("val");
							string deviceModelName = JObject.Parse(devicemodelname_data).Value<string>("val");
							bool syncStat = Synchronization.UploadAll(userid.Value, scores, clearLamps, clearedSongs, unlockList, storyData, installId, deviceModelName);
							if (syncStat)
							{
								var r = new JObject()
								{
									{"success", true },
									{
										"value", new JObject()
										{
											{ "user_id", userid.Value }
										}
									}
								};
								return new JObjectResult(r);
							}
							else
							{
								Console.WriteLine("Sync Failed, not occurred any exception(s)");
								return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
							}
						}
						catch (ArcaeaAPIException ex) //如果发生了异常
						{
							Console.WriteLine(ex.ToString());
							return ex; //直接返回对应异常的Json
						}
						catch (JsonException ex)
						{
							Console.WriteLine(ex.ToString());
							Console.WriteLine("HelpLink = " + ex.HelpLink);
							return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
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
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
			}));
		}

		/// <summary>
		/// [API Action][POST]调整玩家设置。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="type">设置的类型。
		/// <para>favorite_character - 星标搭档设置<br />
		/// is_hide_rating - 个人游玩潜力值隐藏/显示设置</para></param>
		/// <param name="value">设置的值。</param>
		/// <returns></returns>
		[HttpPost("me/setting/{type}")]
		public Task<JObjectResult> changeSettings([FromHeader]string Authorization,[FromRoute]string type,[FromForm]object value)
		{
			return Task.Run(new Func<JObjectResult>(() =>
			{
				Console.WriteLine(value);
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
							return new JObjectResult(User2.SetPlayerSettings(userid!.Value, type, value));
						}
						catch (ArcaeaAPIException ex) //如果发生了异常
						{
							return ex; //直接返回对应异常的Json
						}
						catch (Exception ex) //发生了未知异常
						{
							Console.WriteLine(ex.GetType() + ":" + ex.Message + "\n" + ex.StackTrace);
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
