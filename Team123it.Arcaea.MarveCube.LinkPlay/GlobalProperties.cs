#nullable enable
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using Team123it.Arcaea.MarveCube.LinkPlay.Core;
using static Team123it.Arcaea.MarveCube.LinkPlay.GlobalProperties;

namespace Team123it.Arcaea.MarveCube.LinkPlay
{
	public static class RoomManager
	{
		private static Dictionary<string, Room> _rooms = new();
		public static void RegisterRoom(Room room, string roomId) { _rooms.Add(roomId, room); }
		public static void UnRegisterRoom(string roomId) { _rooms.Remove(roomId); }
		public static Room? FetchRoomById(string roomId)
		{
			if (_rooms.TryGetValue(roomId, out var room)) { return room; }
			else { return null; }
		}
		public static string FetchRoomIdByToken(byte[] data)
		{
			var mDatabaseConnectUrl = $"{RedisServerUrl}:{RedisServerPort},password={RedisServerPassword}";
			var conn = ConnectionMultiplexer.Connect(mDatabaseConnectUrl);
			var db = conn.GetDatabase();
			var roomData = JObject.Parse(db.StringGet($"Arcaea-LinkPlayToken-{BitConverter.ToUInt64(data)}"));
			var roomId = roomData.Value<string>("roomId");
			conn.Close();
			return roomId!;
		}
	}

	/// <summary>
	/// 提供适用于 <see cref="LinkPlay"/> 的全局属性的类。无法继承此类。
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
						if (settings.Value<JObject>("settings")!.Value<bool>("isMaintaining")) { return true; }
						else { return false; }
					}
					catch { return true; }
				}
				else { return true; }
			}
		}

		public static string MultiplayerServerUrl
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var config = settings.Value<JObject>("config");
						string prefix = config!.Value<string>("multiplayerServerUrl")!;
						return prefix;
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

		public static int MultiplayerServerPort
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var config = settings.Value<JObject>("config");
						int key = config!.Value<int>("multiplayerServerPort");
						return key;
					}
					catch
					{
						return 0;
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
		public static string RedisServerUrl
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var config = settings.Value<JObject>("config");
						return config!.Value<string>("redisServerUrl")!;
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

		public static int RedisServerPort
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var config = settings.Value<JObject>("config");
						return config!.Value<int>("redisServerPort");
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

		public static string RedisServerPassword
		{
			get
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "config.json")))
				{
					try
					{
						var settings = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "config.json"), Encoding.UTF8));
						var config = settings.Value<JObject>("config");
						return config!.Value<string>("redisServerPassword")!;
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