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
				string? response = await HttpWebRequest.SendHttpRequestAsync("{您的Arcaea Server 2的起始域名(包括http/https)}/standalone/token?StandaloneKey={您在主服务器程序的Controllers/StandaloneController.cs里设置的StandaloneKey}");
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
