using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Team123it.Arcaea.MarveCube.Core
{
	/// <summary>
	/// 表示一个适用于123 Marvelous Cube 独立模块(Standalone)项目的专用Token(独立项目Token)数据组。
	/// </summary>
	public struct StandaloneToken
	{
		/// <summary>
		/// 获取当前实时的 <see cref="StandaloneToken"/> 实例。
		/// </summary>
		public static StandaloneToken Current { get => GetCurrentToken(); }

		/// <summary>
		/// 当前 <see cref="StandaloneToken"/> 所表示的独立项目Token的原始字符串。
		/// </summary>
		public string Token { get; private set; }

		/// <summary>
		/// 下载服务器请求获取Token时使用的Key。
		/// </summary>
		public string Key { get; private set; }

		/// <summary>
		/// 获取当前实时的 <see cref="StandaloneToken"/> 实例。
		/// </summary>
		private static StandaloneToken GetCurrentToken()
		{
			if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
			{
				try
				{
					var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
					var config = settings.Value<JObject>("config");
					return new StandaloneToken() { Token = config.Value<string>("standaloneToken"), Key = config.Value<string>("standaloneKey") };
				}
				catch (Exception ex)
				{
					throw new JsonException($"配置文件 {Path.Combine(AppContext.BaseDirectory, "data", "config.json")} 读取失败: {ex.Message}");
				}
			}
			else
			{
				throw new FileNotFoundException($"找不到配置文件(config.json): {Path.Combine(AppContext.BaseDirectory, "data", "config.json")}");
			}
		}
	}
}
