using static Team123it.Arcaea.MarveCube.GlobalProperties;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Processors.Front;
using System.Enhance.Web.Json;

namespace Team123it.Arcaea.MarveCube.Controllers
{
	/// <summary>
	/// [API Controller]玩家账号验证相关API控制器类。<br />
	/// 对应处理类: <see cref="Auth"/>
	/// </summary>
	[ApiController]
	[Route("years/19/auth")]
	public class AuthController : ControllerBase
	{
		/// <summary>
		/// [API Action][POST]玩家登录。
		/// </summary>
		/// <param name="Authorization">基本验证(Basic Auth)参数。</param>
		/// <returns>Json数据。</returns>
		[HttpPost("login")]
		public Task<JObjectResult> login([FromHeader]string Authorization)
		{
			return Task.Run(new Func<JObjectResult>(() =>
			{
				if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
				if (Maintaining(out _)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.ServerMaintaining);
				if (Request.IsObsoleteClientVer()) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.NeedUpdateClient);
				if (Authorization.ToLower().StartsWith("basic"))
				{
					string raw = Authorization.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
					string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(raw));
					string username = decoded.Split(':')[0];
					string password = decoded.Split(':')[1];
					return new JObjectResult(Auth.Login(Request, username, password));
				}
				else
				{
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.UsernameOrPasswordInvalid);
				}
			}));
		}
	}
}
