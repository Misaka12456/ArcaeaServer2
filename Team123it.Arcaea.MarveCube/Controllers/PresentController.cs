using static Team123it.Arcaea.MarveCube.GlobalProperties;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Enhance.Web.Json;
using System.Threading.Tasks;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Processors.Background;
using Newtonsoft.Json.Linq;
using Team123it.Arcaea.MarveCube.Processors.Front;

namespace Team123it.Arcaea.MarveCube.Controllers
{
	/// <summary>
	/// [API Controller]礼物相关API控制器类。<br />
	/// 对应处理类: <see cref="Present"/>
	/// </summary>
	[Route("years/19/present")]
	[ApiController]
	public class PresentController : ControllerBase
	{
		/// <summary>
		/// [API Action][POST]接收礼物。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="presentId">要接收的礼物id。</param>
		/// <returns>Json字符串。</returns>
		[HttpPost("me/claim/{presentId}")]
		public async Task<JObjectResult> ClaimPresent([FromHeader]string Authorization, [FromRoute]string presentId)
		{
			return await Task.Run(new Func<JObjectResult>(() =>
			{
				if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
				try
				{
					if (Authorization.Trim().ToLower().StartsWith("bearer"))
					{
						uint? user_id = Tokens.GetUserIdByToken(Authorization.Split(" ")[1]);
						if (Maintaining(out var players))
						{
							if (!user_id.HasValue || !players.Contains((int)user_id.Value))
							{
								return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.ServerMaintaining);
							}
						}
						if (!user_id.HasValue) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.LoggedInAnotherDevice);
						var r = new JObject()
						{
							{ "success", true },
							{ "value", Present.ClaimPresent(user_id.Value, presentId) }
						};
						return new JObjectResult(r);
					}
					else
					{
						return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
					}
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
	}
}
