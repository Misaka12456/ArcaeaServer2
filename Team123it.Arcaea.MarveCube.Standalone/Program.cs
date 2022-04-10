#nullable enable
using Newtonsoft.Json.Linq;
using System.Enhance;
using System.Runtime.InteropServices;
using System.Text;
using static Team123it.Arcaea.MarveCube.Standalone.GlobalProperties;

namespace Team123it.Arcaea.MarveCube.Standalone
{
	public class Program
	{
		private static ConsoleWriter? LogWriter;
		public static void Main(string[] args)
		{
			Initialization(args);
		}

		public static void Initialization(string[] args)
		{
			Console.Clear();
			Console.WriteLine("Welcome to Arcaea Server 2 (123 Marvelous Cube) [Standalone Version].");
			Console.WriteLine($"(C)Copyright 2015-{DateTime.Now.Year} 123 Open-Source Organization(Team123it). All rights reserved.");
			Console.WriteLine();
			Thread.Sleep(1000);
			if (!File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
			{
				var data = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data"));
				if (!data.Exists) data.Create();
				uint port;
				string mainServerURL,standaloneKey;
				for (; ; )
				{
					Console.WriteLine("请输入下载服务器的监听端口, 并回车:");
					if (ushort.TryParse(Console.ReadLine(), out ushort i))
					{
						port = i;
						break;
					}
					else
					{
						Console.WriteLine("输入的监听端口无效, 请重新输入");
						continue;
					}
				}
				for (; ; )
				{
					Console.WriteLine("请输入请求主服务器的URL前缀, 并回车(例: \"http://127.0.0.1\"):");
					string? i = Console.ReadLine();
					if (!string.IsNullOrWhiteSpace(i) && i.Trim().ToLower().StartsWith("http") && !i.Trim().ToLower().EndsWith('/'))
					{
						mainServerURL = i;
						break;
					}
					else
					{
						Console.WriteLine("输入的主服务器的URL前缀无效, 请重新输入");
						continue;
					}
				}
				for (; ; )
				{
					Console.WriteLine("请输入请求主服务器获取解密用的下载Token时使用的Key, 并回车:");
					string? i = Console.ReadLine();
					if (!string.IsNullOrWhiteSpace(i))
					{
						standaloneKey = i;
						break;
					}
					else
					{
						Console.WriteLine("输入的Key无效, 请重新输入");
						continue;
					}
				}
				Console.WriteLine("正在保存设置, 请稍后");
				var config = new JObject()
				{
					{
						"settings", new JObject()
						{
							{ "isMaintaining", false }
						}
					},
					{
						"config", new JObject()
						{
							{ "listenPort", port },
							{ "standaloneKey", standaloneKey },
							{ "mainServerURLPrefix", mainServerURL }
						}
					}
				};
				var dataDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data"));
				var songsDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data", "static", "Songs"));
				if (!dataDir.Exists) dataDir.Create();
				if (!songsDir.Exists) songsDir.Create();
				File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), config.ToString(), Encoding.UTF8);
				Console.WriteLine("请重启程序以使设置生效");
				Thread.Sleep(5000);
				Environment.Exit(0);
			}
			else
			{
				Console.WriteLine("Detected exist configuration and data store, now starting api...");
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && args.Contains("--background"))
				{
					DirectoryInfo logFolder;
					if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "data", "Logs")))
					{
						logFolder = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "data", "Logs"));
					}
					else
					{
						logFolder = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data", "Logs"));
					}
					var newLog = string.Concat(DateTime.Now.ToString("yyyyMMddHHmmssfff"), ".log");
					LogWriter = new ConsoleWriter()
					{
						Tag = Path.Combine(AppContext.BaseDirectory, "data", "Logs", newLog)
					};
					LogWriter.OnOutput += SaveLog;
					Console.WriteLine($"[{DateTime.Now:yyyy-M-d H:mm:ss}] Information: Detected '--background' argument. All logs will output to file {Path.Combine(AppContext.BaseDirectory, "data", "Logs", newLog)}.");
					Console.SetOut(LogWriter);
				}
			}
			Console.CancelKeyPress += StopServer;
			CreateHostBuilder(args).Build().Run();
			StopServer(null, new EventArgs());
		}

		private static void SaveLog(object? sender, TextEventArgs e)
		{
			string logFile = ((ConsoleWriter)sender!).Tag;
			File.AppendAllText(logFile, e.Text);
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			var config = new ConfigurationBuilder()
				.Build();
			return Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseConfiguration(config);
					webBuilder.UseStartup<Startup>();
					webBuilder.UseUrls($"http://*:{ListenPort}");
				});
		}

		public static void StopServer(object? sender, EventArgs e)
		{
			LogWriter?.Dispose();
			Console.WriteLine("Api stopped. Press any key to exit program.");
			Console.ReadKey(true);
			Environment.Exit(0);
		}
	}
}
