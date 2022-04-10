using static Team123it.Arcaea.MarveCube.Standalone.GlobalProperties;
using Newtonsoft.Json.Linq;
using System.Enhance.Net;

namespace Team123it.Arcaea.MarveCube.Standalone.Core
{
	public static class StandaloneTokenHelper
	{
		public static async Task<string> GetToken()
		{
			try
			{
				string? response = await HttpWebRequest.SendHttpRequestAsync($"{MainServerURLPrefix}/standalone/token?StandaloneKey={StandaloneKey}");
				if (!string.IsNullOrWhiteSpace(response))
				{
					var respData = JObject.Parse(response);
					if (respData.Value<bool>("success"))
					{
						string token = respData.Value<string>("value");
						return token;
					}
					else
					{
						return string.Empty;
					}
				}
				else
				{
					return string.Empty;
				}
			}
			catch (Exception ex)
			{
				await Task.FromException(ex);
				return string.Empty;
			}
		}
	}
}
