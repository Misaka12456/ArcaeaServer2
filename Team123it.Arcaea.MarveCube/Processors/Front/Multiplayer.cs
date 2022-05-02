using static Team123it.Arcaea.MarveCube.GlobalProperties;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System;
using MySql.Data.MySqlClient;

namespace Team123it.Arcaea.MarveCube.Processors.Front
{
	/// <summary>
	/// Link Play多人游玩相关API。<br />
	/// 对应API前缀:/years/19/multiplayer/
	/// </summary>
	public static class Multiplayer
	{
		/// <summary>
		/// 创建一个Link Play多人游戏房间。
		/// </summary>
		/// <param name="userId">创建房间的玩家的用户id。</param>
		/// <param name="clientSongMap">创建房间的玩家的客户端式Link Play曲目解锁表(使用idx作为曲目的唯一标识符)。</param>
		/// <returns>
		/// 包含服务器响应数据的 <see cref="JObject"/> 类实例。
		/// <para>
		/// 服务器响应数据包括:
		/// <list type="bullet">
		/// <item><see cref="long"/> roomId - 多人游戏房间的id</item>
		/// <item><see cref="string"/> roomCode - 多人游戏房间的房间号(其它玩家加入房间使用的唯一代码)</item>
		/// <item><see cref="string"/> key - 玩家(房主)加入房间使用的Key</item>
		/// <item><see cref="long"/> token - 玩家(房主)加入房间使用的token(与roomId值一致)</item>
		/// <item><see cref="string"/> playerId - 房间当中玩家(房主)的Link Play玩家编号(6位随机字符串)</item>
		/// <item><see cref="int"/> userId - 玩家(房主)账号的用户id</item>
		/// <item><see cref="string"/> endPoint - Link Play多人游玩模块的UDP服务器终结点(Endpoint)地址</item>
		/// <item><see cref="int"/> port - Link Play多人游玩模块的UDP服务器的连接端口</item>
		/// <item><see cref="string"/> orderedAllowedSongs - 包含该房间允许选择的曲目序号(idx)的Json数组( <see cref="JArray"/> )字符串经Base64编码后的服务端式Link Play曲目解锁表。</item>
		/// </list>
		/// </para>
		/// </returns>
		public static JObject CreateRoom(int userId, JObject clientSongMap)
		{
			using var conn = ConnectionMultiplexer.Connect(MDatabaseConnectURL);
			var random = new Random();
			var roomCode = RandomString(6);
			var roomId = random.NextInt64(1000000000000000000, 2000000000000000000);
			var defaultKey = new byte[] { 0x11, 0x45, 0x14, 0x19, 0x19, 0x19, 0x18, 0x00, 0x11, 0x45, 0x14, 0x19, 0x19, 0x19, 0x18, 0x00 };
			var playerId = random.Next(100000, 999999).ToString();
			var orderedAllowedSongs = ConvertUnlocks(clientSongMap);
			var db = conn.GetDatabase();
			var roomRedisData = new JObject()
			{
				{ "roomCode", roomCode },
				{ "roomId", roomId },
				{ "token", new JArray(roomId) },
				{ "key", defaultKey },
				{ "playerId", new JArray(playerId) },
				{ "userId", new JArray(userId) },
				{ "allowSongs", clientSongMap }
			};
			using var conn2 = new MySqlConnection(DatabaseConnectURL);
			conn2.Open();
			var cmd2 = conn2.CreateCommand();
			cmd2.CommandText = $"SELECT name FROM users WHERE user_id={userId};";
			var redisUserData = new JObject()
			{
				{"roomId", roomId},
				{"userName", (string)cmd2.ExecuteScalar()}
			};
			conn2.Close();
			db.StringSet($"Arcaea-LinkPlay-{roomId}", roomRedisData.ToString());
			db.StringSet($"Arcaea-LinkPlayWrapper-{roomCode}", roomId);
			db.StringSet($"Arcaea-LinkPlayToken-{roomId}", redisUserData.ToString());
			var r = new JObject()
			{
				{"roomCode", roomCode},
				{"roomId", roomId.ToString()},
				{"token", roomId.ToString()},
				{"key", Convert.ToBase64String(defaultKey)},
				{"playerId", playerId},
				{"userId", userId},
				{"endPoint", LinkplayEndpoint},
				{"port", LinkplayPort},
				{"orderedAllowedSongs", orderedAllowedSongs}
			};
			return r;
		}

		/// <summary>
		/// 加入一个Link Play多人游戏房间。
		/// </summary>
		/// <param name="roomCode">要加入的房间的6位随机Code</param>
		/// <param name="userId">要加入房间的玩家的用户id。</param>
		/// <param name="clientSongMap">要加入房间的玩家的客户端式Link Play曲目解锁表(使用idx作为曲目的唯一标识符)。</param>
		/// <returns>
		/// 包含服务器响应数据的 <see cref="JObject"/> 类实例。
		/// <para>
		/// 服务器响应数据包括:
		/// <list type="bullet">
		/// <item><see cref="long"/> roomId - 多人游戏房间的id</item>
		/// <item><see cref="string"/> roomCode - 多人游戏房间的房间号(玩家加入房间使用的唯一代码)</item>
		/// <item><see cref="string"/> key - 玩家加入房间使用的Key</item>
		/// <item><see cref="long"/> token - 玩家加入房间使用的token(与roomId值一致)</item>
		/// <item><see cref="string"/> playerId - 房间当中玩家的Link Play玩家编号(6位随机字符串)</item>
		/// <item><see cref="int"/> userId - 玩家账号的用户id</item>
		/// <item><see cref="string"/> endPoint - Link Play多人游玩模块的UDP服务器终结点(Endpoint)地址</item>
		/// <item><see cref="int"/> port - Link Play多人游玩模块的UDP服务器的连接端口</item>
		/// <item><see cref="string"/> orderedAllowedSongs - 包含该房间允许选择的曲目的序号(idx)的Json数组( <see cref="JArray"/> )字符串经Base64编码后的服务端式Link Play曲目解锁表。</item>
		/// </list>
		/// </para>
		/// </returns>
		public static JObject JoinRoom(string roomCode, int userId, JObject clientSongMap)
		{
			var random = new Random();
			var defaultKey = new byte[] { 0x11, 0x45, 0x14, 0x19, 0x19, 0x19, 0x18, 0x00, 0x11, 0x45, 0x14, 0x19, 0x19, 0x19, 0x18, 0x00 };
			var playerId = random.Next(100000, 999999).ToString();
			using var conn = ConnectionMultiplexer.Connect(MDatabaseConnectURL);
			var db = conn.GetDatabase();
			var roomId = db.StringGet($"Arcaea-LinkPlayWrapper-{roomCode}");
			var token = random.NextInt64(1000000000000000000, 2000000000000000000).ToString();
			var roomRedisData = JObject.Parse(db.StringGet($"Arcaea-LinkPlay-{roomId}").ToString());
			var storedMap = roomRedisData.Value<JObject>("allowSongs");
			var finalMap = new JObject();
			foreach (var prop in clientSongMap.Properties())
			{
				var comp1 = clientSongMap.Value<JArray>(prop.Name);
				var comp2 = storedMap.Value<JArray>(prop.Name);
				var final = new JArray();
				for (var i = 0; i < 4; i++)
				{
					if ((bool)comp1[i] && (bool)comp2[i]) final.Add(true);
					else final.Add(false);
				}
				finalMap.Add(prop.Name, final);
			}
			var tokensList = roomRedisData.Value<JArray>("token");
			tokensList.Add(token);
			var playerIdsList = roomRedisData.Value<JArray>("playerId");
			playerIdsList.Add(playerId);
			var userIdsList = roomRedisData.Value<JArray>("userId");
			userIdsList.Add(userId);
			roomRedisData.Remove("playerId");
			roomRedisData.Remove("userId");
			roomRedisData.Remove("allowSongs");
			roomRedisData.Add("playerId", playerIdsList);
			roomRedisData.Add("userId", userIdsList);
			roomRedisData.Add("allowSongs", finalMap);
			using var conn2 = new MySqlConnection(DatabaseConnectURL);
			conn2.Open();
			var cmd2 = conn2.CreateCommand();
			cmd2.CommandText = $"SELECT name FROM users WHERE user_id={userId};";
			var redisUserData = new JObject()
			{
				{"roomId", (string)roomId},
				{"userName", (string)cmd2.ExecuteScalar()}
			};
			conn2.Close();
			db.StringSet($"Arcaea-LinkPlay-{roomCode}", roomRedisData.ToString());
			db.StringSet($"Arcaea-LinkPlayToken-{token}", redisUserData.ToString());
			var r = new JObject()
			{
				{"roomCode", roomCode},
				{"roomId", roomRedisData.Value<string>("roomId")},
				{"token", token},
				{"key", Convert.ToBase64String(defaultKey)},
				{"playerId", playerId},
				{"userId", userId},
				{"endPoint", LinkplayEndpoint},
				{"port", LinkplayPort},
				{"orderedAllowedSongs", ConvertUnlocks(finalMap)}
			};
			return r;
		}

		/// <summary>
		/// 将客户端式的Link Play曲目解锁表转换为服务端式的Link Play曲目解锁表。
		/// <para>
		/// <list type="bullet">
		/// <item>
		/// 客户端式:<br />
		/// 以 <code>{ "曲目序号(idx(<see cref="int"/>))": [ PST是否解锁(<see cref="bool"/>), PRS是否解锁(<see cref="bool"/>), FTR是否解锁(<see cref="bool"/>), BYD是否解锁(<see cref="bool"/>) ]}</code>作为单个Json键值对集合而成的Json对象 <see cref="JObject"/>。
		/// </item>
		/// <item>
		/// 服务端式:<br />
		/// Base64编码后的以每个曲目序号(idx(<see cref="int"/>))作为单个元素集合而成的Json数组 <see cref="JArray"/> 字符串。<br />
		/// 如:(示例为加密前的明文)<br />
		/// <code> [ 0, 1, 2, 3, 7, 9, 11, 12, 13, 14, 15, 17, 18, 19, 21, 24, 28, 29, ...... ]</code>
		/// </item>
		/// </list>
		/// </para>
		/// </summary>
		/// <param name="clientSongMap">要转换的客户端式的Link Play曲目解锁表。</param>
		/// <returns>转换结果。</returns>
		private static string ConvertUnlocks(JObject clientSongMap)
		{
			var mapDict = clientSongMap.ToObject<Dictionary<int, bool[]>>()!;
			var userUnlocks = new byte[512];
			foreach (var (key, value) in mapDict)
			{
				if (mapDict.ContainsKey(key))
				{
					for (var j = 0; j < value.Length; j++)
					{
						if (value[j]) userUnlocks[key / 2] += (byte)(1 << (j + 4 * (key % 2)));
					}
				}
			}
			return Convert.ToBase64String(userUnlocks);
		}

		/// <summary>
		/// 随机生成包含大写字母和数字的字符串。
		/// </summary>
		/// <param name="length">要生成的长度。</param>
		/// <returns>生成结果。</returns>
		private static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();
			return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
		}
	}
}
