using static Team123it.Arcaea.MarveCube.GlobalProperties;
using Microsoft.AspNetCore.Mvc;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Processors.Front;
using System.Threading.Tasks;
using System.Enhance.Web.Json;
using Newtonsoft.Json.Linq;
using System;
using Team123it.Arcaea.MarveCube.Processors.Background;

namespace Team123it.Arcaea.MarveCube.Controllers
{
	/// <summary>
	/// [API Controller]玩家数据获取相关API控制器类。<br />
	/// 对应处理类: <see cref="Compose"/>
	/// </summary>
	[ApiController]
	[Route("years/19/compose")]
	public class ComposeController : ControllerBase
	{
		/// <summary>
		/// [API Action][POST]玩家信息获取。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <returns>Json字符串。</returns>
		[HttpGet("aggregate")]
		public async Task<JObjectResult> aggregate([FromHeader]string Authorization,[FromQuery]string calls)
		{
			return await Task.Run(() => {
				if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
				if (Request.IsObsoleteClientVer()) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.NeedUpdateClient);
				if (Authorization.Trim().ToLower().StartsWith("bearer"))
				{
					string token = Authorization.Split(' ')[1];
					if (Maintaining(out var players))
					{
						uint? userId = Tokens.GetUserIdByToken(token);
						if (!userId.HasValue || !players.Contains((int)userId.Value))
						{
							return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.ServerMaintaining);
						}
					}
					Console.WriteLine("calls=" + calls);
					if (string.IsNullOrWhiteSpace(calls))
					{
						return new JObjectResult(Compose.FullAggregate(token));
					}
					else
					{
						var callsObj = JArray.Parse(calls);
						var tinyCallsObj = new JObject()
						{
							{"endpoint","/user/me" },
							{"id",0 }
						};
						if ((callsObj.Count == 1 && ((JObject)callsObj[0]).ToString() == tinyCallsObj.ToString()))
						{
							return new JObjectResult(Compose.TinyAggregate(token));
						}
						else
						{
							return new JObjectResult(Compose.FullAggregate(token, calls));
						}
					}
				}
				else
				{
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
			});
		}
	}
}
