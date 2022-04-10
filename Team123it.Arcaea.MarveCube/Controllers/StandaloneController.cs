using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Enhance.Web.Json;
using System.Threading.Tasks;
using Team123it.Arcaea.MarveCube.Core;

namespace Team123it.Arcaea.MarveCube.Controllers
{
	[Route("standalone")]
	[ApiController]
	public class StandaloneController : ControllerBase
	{
		[HttpGet("token")]
		public async Task<JObjectResult> GetCurrentStandaloneToken([FromQuery]string StandaloneKey)
		{
			return await Task.Run(() =>
			{
				if (!string.IsNullOrWhiteSpace(StandaloneKey))
				{
					if (StandaloneKey == StandaloneToken.Current.Key)
					{
						return new JObjectResult(new JObject()
						{
							{ "success", true },
							{ "value", StandaloneToken.Current.Token }
						});
					}
					else
					{
						return new JObjectResult(new JObject()
						{
							{ "success", false },
							{ "error_code", 403 }
						});
					}
				}
				else
				{
					return new JObjectResult(new JObject()
					{
						{ "success", false },
						{ "error_code", 401 }
					});
				}
			});
		}
	}
}
