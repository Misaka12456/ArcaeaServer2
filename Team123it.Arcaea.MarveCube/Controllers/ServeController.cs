using static Team123it.Arcaea.MarveCube.GlobalProperties;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Enhance.Web.Json;
using System.Threading.Tasks;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Processors.Background;
using System;
using Newtonsoft.Json.Linq;
using System.Linq;
using Team123it.Arcaea.MarveCube.Processors.Front;

namespace Team123it.Arcaea.MarveCube.Controllers
{
	/// <summary>
	/// [API Controller]数据下载相关API控制器类。<br />
	/// 对应处理类: <see cref="Serve"/>
	/// </summary>
	[Route("years/19/serve")]
	[ApiController]
	public class ServeController : ControllerBase
	{
		/// <summary>
		/// [API Action][GET]获取曲目下载的URL和校验值信息。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="sid">要下载的曲目的id(sid)。</param>
		/// <param name="url">是否在返回的信息中包含曲目下载的URL。</param>
		/// <returns>Json字符串。</returns>
		[HttpGet("download/me/song")]
		public async Task<JObjectResult> GetSongDownloadDetails([FromHeader]string Authorization, [FromQuery]IEnumerable<string> sid, [FromQuery]bool url)
		{
			return await Task.Run(() =>
			{
				try
				{
					if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
					if (Request.IsObsoleteClientVer()) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.NeedUpdateClient);
					if (Authorization.Trim().ToLower().StartsWith("bearer"))
					{
						string token = Authorization.Split(' ')[1];
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
							var info = new PlayerInfo(userid.Value, out _);
							if (info.Banned.Value) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountHasBeenBlocked);
							var r = new JObject()
							{
								{ "success", true },
								{ "value", Serve.GetDownloadAvailableSongs(userid.Value, sid.Any() ? sid : null, url) }
							};
							Console.WriteLine(r.ToString());
							return new JObjectResult(r);
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
				}
				catch (ArcaeaAPIException ex)
				{
					return ex;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
			});
		}
	}
}
