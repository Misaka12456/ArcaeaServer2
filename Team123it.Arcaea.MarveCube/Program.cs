#nullable enable
using static Team123it.Arcaea.MarveCube.GlobalProperties;
using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Team123it.Arcaea.MarveCube
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Initialization(args);
		}

		public static void Initialization(string[] args)
		{
			Console.Clear();
			Console.WriteLine("Welcome to Arcaea Server 2(123 MarveCube Public Version).");
			Console.WriteLine($"(C)Copyright 2015-{DateTime.Now.Year} 123 Open-Source Organization(Team123it). All rights reserved.");
			Console.WriteLine();
			Thread.Sleep(1000);
			Console.WriteLine("Please wait while system detecting the configurating state...");
			if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory,"data"))
				 || (!File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json"))))
			{
				Console.WriteLine("Detected the very first start,  starting initialization...");
				FirstStart.FirstStart.FastInitialize();
				Console.WriteLine("Config initialized, now starting api...");
				CreateHostBuilder(args).Build().Run();
			}
			else
			{
				Console.WriteLine("Detected exist configuration and data store, now starting api...");
				CreateHostBuilder(args).Build().Run();
			}
			Console.WriteLine("Api stopped. Press any key to exit program.");
			Console.ReadKey(true);
			Environment.Exit(0);
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			//X509Certificate2? x509ca = null;
			//if (HttpsCertificatePassword != null)
			//{
			//	x509ca = new X509Certificate2(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "data", "https.pfx")), HttpsCertificatePassword);
			//}
			var config = new ConfigurationBuilder()
				.Build();
			return Host.CreateDefaultBuilder(args)
				.ConfigureLogging(logBuilder =>
				{
					logBuilder.SetMinimumLevel(LogLevel.Trace);
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseConfiguration(config);
					webBuilder.UseStartup<Startup>();
					webBuilder.UseUrls($"http://*:{ListenPort}");
				})
				.UseEnvironment("Development");
		}
	}
}
