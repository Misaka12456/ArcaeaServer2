#nullable enable
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Team123it.Arcaea.MarveCube
{
	/// <summary>
	/// 提供适用于 <see cref="MarveCube"/> 的全局属性的类。无法继承此类。
	/// </summary>
	public static class GlobalProperties
	{
		/// <summary>
		/// 获取当前服务器是否在维护中的标志。
		/// <para>注:为防止出现数据丢失或损坏或发生意外情况,获取过程中发生任何异常都将视为正在维护中。</para>
		/// </summary>
		public static bool Maintaining(out List<int> bypassPlayers)
		{
			if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
			{
				try
				{
					var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
					if (settings.Value<JObject>("settings").Value<bool>("isMaintaining"))
					{
						if (settings.TryGetValue("maintainBypassPlayers", out var maintainBypassPlayers))
						{
							bypassPlayers = ((JArray)maintainBypassPlayers!).ToObject<List<int>>()!;
						}
						else
						{
							bypassPlayers = new List<int>();
						}
						return true;
					}
					else
					{
						bypassPlayers = new List<int>();
						return false;
					}
				}
				catch
				{
					bypassPlayers = new List<int>();
					return true;
				}
			}
			else
			{
				bypassPlayers = new List<int>();
				return true;
			}
		}

		/// <summary>
		/// 获取当前Arcaea版本是否等待发布的标志。
		/// <para>若请求体当中有AppVersion(下称appVer)项且配置文件中有latestVersion(下称ltstVer)项，则当appVer>=ltstVer时返回实际的isPreparingForRelease状态, appVer&lt;ltstVer或ltstVer不存在时固定返回false(不是最新版本fallback到"正在维护中"的信息)<br />
		/// 若请求体当中无appVer则直接返回实际的isPreparingForRelease状态。</para>
		/// </summary>
		public static bool PreparingForRelease(HttpRequest req)
		{
			if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
			{
				try
				{
					var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8)).Value<JObject>("settings");
					bool isPreparingForRelease = settings.Value<bool>("isPreparingForRelease");
					if (isPreparingForRelease)
					{
						if (req.Headers["AppVersion"] != StringValues.Empty)
						{
							if (settings.TryGetValue("latestVersion", out var latestVersionToken))
							{
								var latestVersion = Version.Parse((string)latestVersionToken!);
								var appVersion = Version.Parse(req.Headers["AppVersion"]);
								if (appVersion >= latestVersion)
								{
									return true;
								}
								else
								{
									return false;
								}
							}
							else
							{
								return true;
							}
						}
						else
						{
							return true;
						}
					}
					else
					{
						return false;
					}
				}
				catch
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		public static bool IsWorldEventMapTesting(out List<int> testPlayers)
		{
			if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
			{
				try
				{
					var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8)).Value<JObject>("settings");
					bool isWorldEventMapTesting = settings.Value<bool>("isWorldEventMapTesting");
					if (isWorldEventMapTesting)
					{
						if (settings.TryGetValue("eventMapTestPlayers", out var eventMapTestPlayers))
						{
							testPlayers = ((JArray)eventMapTestPlayers!).ToObject<List<int>>()!;
						}
						else
						{
							testPlayers = new List<int>();
						}
						return true;
					}
					else
					{
						testPlayers = new List<int>();
						return false;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					testPlayers = new List<int>();
					return false;
				}
			}
			else
			{
				testPlayers = new List<int>();
				return false;
			}
		}

		/// <summary>
		/// 获取当前服务器是否覆盖系统时间日期而强制启用愚人节模块。
		/// </summary>
		public static bool IsOverrideAprilFools
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						if (settings.Value<JObject>("settings").Value<bool>("isOverrideAprilFools"))
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
						return false;
					}
				}
				else
				{
					return false;
				}
			}
		}
		public static DateTime AprilFoolsStartTime
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8)).Value<JObject>("settings");
						if (settings.TryGetValue("aprilFoolsStartTime", out var startTimeToken) && (long)startTimeToken != -1)
						{
							return DateTime.UnixEpoch.AddSeconds((long)startTimeToken);
						}
						else
						{
							return new DateTime(DateTime.Now.Year, 4, 1, 0, 0, 0);
						}
					}
					catch (Exception)
					{
						return new DateTime(DateTime.Now.Year, 4, 1, 0, 0, 0);
					}
				}
				else
				{
					return new DateTime(DateTime.Now.Year, 4, 1, 0, 0, 0);
				}
			}
		}

		/// <summary>
		/// 获取主数据库的连接URL。
		/// </summary>
		/// <exception cref="FileNotFoundException" />
		/// <exception cref="JsonException" />
		public static string DatabaseConnectURL
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var dbConnURL = new StringBuilder();
						var config = settings.Value<JObject>("config");
						string ip = config.Value<string>("dbIP");
						int port = config.TryGetValue("dbPort", out var tPort) ? (int)tPort! : 3306;
						string user = config.Value<string>("dbUser");
						string pass = Encoding.UTF8.GetString(Convert.FromBase64String(config.Value<string>("dbPass")));
						string name = config.TryGetValue("dbName", out var tName) ? (string)tName! : "arcaea";
						dbConnURL.Append("server=").Append(ip).Append(";port=").Append(port).Append(";user=").Append(user).Append(";password=")
							.Append(pass).Append(";database=").Append(name).Append(";charset=utf8");
						return dbConnURL.ToString();
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

		public static string GetDatabaseConnectNoDBNameURL(out string dbName)
		{
			if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
			{
				try
				{
					var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
					var dbConnURL = new StringBuilder();
					var config = settings.Value<JObject>("config");
					string ip = config.Value<string>("dbIP");
					int port = config.TryGetValue("dbPort", out var tPort) ? (int)tPort! : 3306;
					string user = config.Value<string>("dbUser");
					string pass = Encoding.UTF8.GetString(Convert.FromBase64String(config.Value<string>("dbPass")));
					dbName = config.TryGetValue("dbName", out var tName) ? (string)tName! : "arcaea";
					dbConnURL.Append("server=").Append(ip).Append(";port=").Append(port).Append(";user=").Append(user).Append(";password=")
										.Append(pass).Append(";charset=utf8");
					return dbConnURL.ToString();
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

		/// <summary>
		/// 获取全服总分排名(Top)的最大排名序号限制。 <br />
		/// 若值不存在则直接返回Lowiro默认的限制(200)。
		/// </summary>
		public static int TopRankLimit
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						if (settings.Value<JObject>("settings").TryGetValue("topRankLimit", out var topRankLimitToken))
						{
							return (int)topRankLimitToken;
						}
						else
						{
							return 200;
						}
					}
					catch
					{
						return 200;
					}
				}
				else
				{
					return 200;
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

		/// <summary>
		/// 获取客户端远程下载的URL前缀。
		/// </summary>
		/// <exception cref="FileNotFoundException" />
		/// <exception cref="JsonException" />
		public static string RemoteDownloadURLPrefix
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var config = settings.Value<JObject>("config");
						return config.Value<string>("remoteDlPrefix");
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

		/// <summary>
		/// 获取当前服务器所支持的 Arcaea 客户端的最低版本 <see cref="Version"/> 实例。
		/// </summary>
		/// <exception cref="FileNotFoundException" />
		/// <exception cref="JsonException" />
		public static Version MinSupportVersion
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						string minSupportVerStr = settings.Value<JObject>("settings").Value<string>("minSupportVer");
						var minSupportVer = Version.Parse(minSupportVerStr);
						return minSupportVer;
					}
					catch (FormatException)
					{
						return new Version(1, 0, 0);
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

		/// <summary>
		/// 判断当前请求对应的 Arcaea 客户端是否为过时的客户端(低于服务器所支持的最低版本)
		/// </summary>
		/// <param name="request">当前 <see cref="HttpRequest"/> 请求。</param>
		/// <returns>成功返回判断结果, 找不到 AppVersion 参数或失败固定返回 <see langword="true"/> 。</returns>
		public static bool IsObsoleteClientVer(this HttpRequest request)
		{
			try
			{
				string appVerStr = request.Headers["AppVersion"];
				if (appVerStr != string.Empty)
				{
					var appVer = Version.Parse(appVerStr);
					return (appVer < MinSupportVersion);
				}
				else
				{
					return true;
				}
			}
			catch
			{
				return true;
			}
		}

		/// <summary>
		/// 获取允许的每秒最大查询次数(API访问次数)(Query Per Second,QPS)。
		/// </summary>
		/// <exception cref="FileNotFoundException" />
		/// <exception cref="Exception" />
		public static uint MaxQueryTimesPerSecond
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var config = settings.Value<JObject>("config");
						return config.Value<uint>("maxQps");
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

		/// <summary>
		/// 获取内存数据库(Redis)的连接URL。
		/// </summary>
		/// <exception cref="FileNotFoundException" />
		/// <exception cref="JsonException" />
		public static string MDatabaseConnectURL
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var dbConnURL = new StringBuilder();
						var config = settings.Value<JObject>("config");
						string redisURL = config.TryGetValue("redisURL", out var redisURLToken) && !string.IsNullOrWhiteSpace((string?)redisURLToken) ? (string)redisURLToken! : "localhost";
						string mdbConnFullURL = $"{redisURL}:{config.Value<uint>("redisPort")}";
						if (config.TryGetValue("redisPswd", out var redisPswdToken) && !string.IsNullOrWhiteSpace((string?)redisPswdToken))
						{
							mdbConnFullURL += $",password={redisPswdToken}";
						}
						return mdbConnFullURL;
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

		/// <summary>
		/// 获取Link Play多人游玩模块的UDP服务器连接端口。
		/// </summary>
		public static int? LinkplayPort
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var dbConnURL = new StringBuilder();
						var config = settings.Value<JObject>("config");
						if (config!.TryGetValue("linkplayPort", out var LinkplayPort))
						{
							return (int?)LinkplayPort;
						}
						else
						{
							return null;
						}
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

		/// <summary>
		/// 获取Link Play多人游玩模块的UDP服务器终结点(Endpoint)地址。
		/// <para>
		/// 注:该项必须填UDP服务器的公网IPv4地址。
		/// </para>
		/// </summary>
		public static string? LinkplayEndpoint
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var dbConnURL = new StringBuilder();
						var config = settings.Value<JObject>("config");
						if (config!.TryGetValue("linkplayEndpoint", out var LinkplayEndpoint))
						{
							return (string?)LinkplayEndpoint;
						}
						else
						{
							return null;
						}
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

		/// <summary>
		/// 获取API运行异常自动提示邮件发送的目标邮箱地址。
		/// </summary>
		public static string? ReportDescEmail
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var dbConnURL = new StringBuilder();
						var config = settings.Value<JObject>("config");
						if (config.TryGetValue("reportDescEmail", out var rptDescEmail))
						{
							return (string)rptDescEmail!;
						}
						else
						{
							return null;
						}
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

		public static string? ReportEmail
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var dbConnURL = new StringBuilder();
						var config = settings.Value<JObject>("config");
						if (config.TryGetValue("reportEmail", out var rptEmail))
						{
							return (string)rptEmail!;
						}
						else
						{
							return null;
						}
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

		public static string? ReportEmailSmtp
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var dbConnURL = new StringBuilder();
						var config = settings.Value<JObject>("config");
						if (config.TryGetValue("reportEmailSmtp", out var rptSmtp))
						{
							return (string)rptSmtp!;
						}
						else
						{
							return null;
						}
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

		public static string? ReportEmailPswd
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var dbConnURL = new StringBuilder();
						var config = settings.Value<JObject>("config");
						if (config.TryGetValue("reportEmailPswd", out var rptPswd))
						{
							string pswd = Encoding.UTF8.GetString(Convert.FromBase64String((string)rptPswd!));
							return pswd;
						}
						else
						{
							return null;
						}
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
