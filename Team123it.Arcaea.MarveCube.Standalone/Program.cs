#nullable enable
using System.Enhance;
using System.Runtime.InteropServices;
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
