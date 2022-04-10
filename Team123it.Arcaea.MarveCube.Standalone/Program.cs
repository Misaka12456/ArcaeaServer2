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
					Console.WriteLine("���������ط������ļ����˿�, ���س�:");
					if (ushort.TryParse(Console.ReadLine(), out ushort i))
					{
						port = i;
						break;
					}
					else
					{
						Console.WriteLine("����ļ����˿���Ч, ����������");
						continue;
					}
				}
				for (; ; )
				{
					Console.WriteLine("��������������������URLǰ׺, ���س�(��: \"http://127.0.0.1\"):");
					string? i = Console.ReadLine();
					if (!string.IsNullOrWhiteSpace(i) && i.Trim().ToLower().StartsWith("http") && !i.Trim().ToLower().EndsWith('/'))
					{
						mainServerURL = i;
						break;
					}
					else
					{
						Console.WriteLine("���������������URLǰ׺��Ч, ����������");
						continue;
					}
				}
				for (; ; )
				{
					Console.WriteLine("��������������������ȡ�����õ�����Tokenʱʹ�õ�Key, ���س�:");
					string? i = Console.ReadLine();
					if (!string.IsNullOrWhiteSpace(i))
					{
						standaloneKey = i;
						break;
					}
					else
					{
						Console.WriteLine("�����Key��Ч, ����������");
						continue;
					}
				}
				Console.WriteLine("���ڱ�������, ���Ժ�");
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
				Console.WriteLine("������������ʹ������Ч");
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
