using System.Diagnostics.CodeAnalysis;
using System.Enhance;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json.Linq;
using Team123it.Arcaea.MarveCube.LinkPlay.Core;
using static Team123it.Arcaea.MarveCube.LinkPlay.Core.LinkPlayCrypto;
using static Team123it.Arcaea.MarveCube.LinkPlay.GlobalProperties;

namespace Team123it.Arcaea.MarveCube.LinkPlay
{
    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    public class Program
    {
        static Socket? _server;
        private static ConsoleWriter? _logWriter;
        
        public static void Main(string[] args)
        {
            Initialization(args);
        }

        private static void Initialization(string[] args)
        {
	        Console.Clear();
	        Console.WriteLine("Welcome to Arcaea Server 2 (123 Marvelous Cube) [LinkPlay Server].");
	        Console.WriteLine($"(C)Copyright 2015-{DateTime.Now.Year} 123 Open-Source Organization(Team123it). All rights reserved.");
	        Console.WriteLine();
	        Thread.Sleep(1000);
	        if (!File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
	        {
		        var data = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data"));
		        if (!data.Exists) data.Create();
		        uint port, redisPort;
		        string multiplayerServerUrl, redisUrl, redisPassword;
		        for (;;)
		        {
			        Console.WriteLine("请输入LinkPlay服务器的监听端口(需与主服务器保持同步), 并回车:");
			        if (ushort.TryParse(Console.ReadLine(), out var i))
			        {
				        port = i;
				        break;
			        }
			        Console.WriteLine("输入的监听端口无效, 请重新输入");
		        }

		        for (;;)
		        {
			        Console.WriteLine("请输入LinkPlay服务器的监听端口(需与主服务器保持同步), 并回车(例: \"127.0.0.1\"):");
			        var i = Console.ReadLine();
			        if (!string.IsNullOrWhiteSpace(i))
			        {
				        multiplayerServerUrl = i;
				        break;
			        }
			        Console.WriteLine("输入的服务器的IP无效, 请重新输入");
		        }
		        
		        for (;;)
		        {
			        Console.WriteLine("请输入Redis服务器的IP(需与主服务器保持同步), 并回车(例: \"127.0.0.1\"):");
			        var i = Console.ReadLine();
			        if (!string.IsNullOrWhiteSpace(i))
			        {
				        redisUrl = i;
				        break;
			        }
			        Console.WriteLine("输入的服务器的IP前缀无效, 请重新输入");
		        }

		        for (;;)
		        {
			        Console.WriteLine("请输入Redis服务器的监听端口(需与主服务器保持同步), 并回车:");
			        if (ushort.TryParse(Console.ReadLine(), out var i))
			        {
				        redisPort = i;
				        break;
			        }
			        Console.WriteLine("输入的监听端口无效, 请重新输入");
		        }
		        
		        for (;;)
		        {
			        Console.WriteLine("请输入Redis服务器的密码(需与主服务器保持同步), 并回车:");
			        var i = Console.ReadLine();
			        if (i is not null)
			        {
				        redisPassword = i;
				        break;
			        }
			        Console.WriteLine("输入的监听端口无效, 请重新输入");
		        }

		        Console.WriteLine("正在保存设置, 请稍后");
		        var config = new JObject
		        {
			        {
				        "settings", new JObject
				        {
					        {"isMaintaining", false}
				        }
			        },
			        {
				        "config", new JObject
				        {
					        {"multiplayerServerUrl", multiplayerServerUrl},
					        {"multiplayerServerPort", port},
					        {"redisServerUrl", redisUrl},
					        {"redisServerPort", redisPort},
					        {"redisServerPassword", redisPassword},
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
			        logFolder = !Directory.Exists(Path.Combine(AppContext.BaseDirectory, "data", "Logs")) 
				        ? Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "data", "Logs")) 
				        : new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data", "Logs"));

			        var newLog = string.Concat(DateTime.Now.ToString("yyyyMMddHHmmssfff"), ".log");
			        _logWriter = new ConsoleWriter {Tag = Path.Combine(AppContext.BaseDirectory, "data", "Logs", newLog)};
			        _logWriter.OnOutput += SaveLog;
			        Console.WriteLine($"[{DateTime.Now:yyyy-M-d H:mm:ss}] Information: Detected '--background' argument. All logs will output to file {Path.Combine(AppContext.BaseDirectory, "data", "Logs", newLog)}.");
			        Console.SetOut(_logWriter);
		        }
	        }
	        Console.CancelKeyPress += StopServer;
	        UdpBuilder();
	        StopServer(null, EventArgs.Empty);
        }
        
        private static void SaveLog(object? sender, TextEventArgs e)
        {
	        var logFile = ((ConsoleWriter)sender!).Tag;
	        File.AppendAllText(logFile, e.Text);
        }

        private static void UdpBuilder()
        {
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _server.Bind(new IPEndPoint(IPAddress.Parse(MultiplayerServerUrl), MultiplayerServerPort));//绑定端口号和IP
            Console.WriteLine("Server Initialized, Now starting to process UDP");
            ReceiveMsg();
        }

        /// <summary>
        /// 向特定ip的主机的端口发送数据报
        /// </summary>
        /// <param name="data">需要发出的数据包</param>
        /// <param name="token">获取token</param>
        /// <param name="endPoint">数据要到达的终止点，可在ReceiveMsg中获得( <see cref="EndPoint"/> )</param>
        public static async Task SendMsg(byte[] data, byte[] token, EndPoint endPoint)
        {
	        const SocketFlags flags = SocketFlags.None;
	        try
	        {
		        var encryptedData = await EncryptPack(token, data);
		        await _server?.SendToAsync(encryptedData, flags, endPoint)!;
	        }
	        catch (Exception e) { Console.WriteLine(e); }
        }
        /// <summary>
        /// 接收发送给本机ip对应端口号的数据报
        /// </summary>
        private static async void ReceiveMsg()
        {
	        const SocketFlags flags = SocketFlags.None;
	        try
	        {            
		        while (true)
		        {
			        EndPoint point = new IPEndPoint(IPAddress.Any, 0);//用来保存发送方的ip和端口号
			        var buffer = new byte[1024];
			        if (_server == null) continue;
			        var rawMessage = await _server.ReceiveFromAsync(buffer, flags, point);//接收数据报
			        var message = await DecryptPack(buffer[..rawMessage.ReceivedBytes]);
			        Console.WriteLine(point.ToString() + message);
			        await LinkPlayProcessor.ProcessPacket(message, point);
		        }
	        }
	        catch (Exception e) { Console.WriteLine(e); }
        }
        
        private static void StopServer(object? sender, EventArgs e)
        {
	        _logWriter?.Dispose();
	        Console.WriteLine("Api stopped. Press any key to exit program.");
	        Console.ReadKey(true);
	        Environment.Exit(0);
        }
    }
}