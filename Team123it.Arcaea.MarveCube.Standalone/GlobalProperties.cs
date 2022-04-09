#nullable enable
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Team123it.Arcaea.MarveCube.Standalone
{
	/// <summary>
	/// 提供适用于 <see cref="Standalone"/> 的全局属性的类。无法继承此类。
	/// </summary>
	public static class GlobalProperties
	{
		/// <summary>
		/// 获取当前服务器是否在维护中的标志。
		/// <para>注:为防止出现数据丢失或损坏或发生意外情况,获取过程中发生任何异常都将视为正在维护中。</para>
		/// </summary>
		public static bool Maintaining
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						if (settings.Value<JObject>("settings").Value<bool>("isMaintaining"))
						{
							return true;
						}
						else
						{
							return false;
						}
					}
					catch
					{
						return true;
					}
				}
				else
				{
					return true;
				}
			}
		}

		/// <summary>
		/// 获取HTTPS证书密码。
		/// <para>若证书密码设置项不存在则返回 <see langword="null"/> 。</para>
		/// </summary>
		/// <exception cref="FileNotFoundException" />
		public static string? HttpsCertificatePassword
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var config = settings.Value<JObject>("config");
						string pass = Encoding.UTF8.GetString(Convert.FromBase64String(config.Value<string>("httpsCerPass")));
						return pass;
					}
					catch 
					{
						return null;
					}
				}
				else
				{
					throw new FileNotFoundException($"找不到配置文件(config.json): {Path.Combine(AppContext.BaseDirectory, "data", "config.json")}");
				}
			}
		}

		/// <summary>
		/// 获取API的监听端口。
		/// </summary>
		/// <exception cref="FileNotFoundException" />
		/// <exception cref="JsonException" />
		public static int ListenPort
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var config = settings.Value<JObject>("config");
						return config.Value<int>("listenPort");
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
}
