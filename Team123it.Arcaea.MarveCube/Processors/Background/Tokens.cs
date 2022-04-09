#nullable enable
using static Team123it.Arcaea.MarveCube.GlobalProperties;
using MySql.Data.MySqlClient;
using System;
using StackExchange.Redis;

namespace Team123it.Arcaea.MarveCube.Processors.Background
{
	/// <summary>
	/// 提供关于 Arcaea 客户端(用户) Token 处理的类。无法继承此类。
	/// </summary>
	public sealed class Tokens
	{
		/// <summary>
		/// 通过token获取对应的用户id(不是好友id)。
		/// </summary>
		/// <param name="token">要获取的用户id对应的此用户的token。</param>
		/// <returns>获取的用户id, 若不存在则为 <see langword="null" /> 。</returns>
		public static uint? GetUserIdByToken(string token)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = conn.CreateCommand();
			cmd.CommandText = $"SELECT COUNT(*),user_id FROM logins WHERE access_token=?token;";
			cmd.Parameters.Add(new MySqlParameter("?token", MySqlDbType.VarString)
			{
				Value = token
			});
			var rd = cmd.ExecuteReader();
			rd.Read();
			if (rd.GetInt32(0) == 1)
			{
				uint userid = (uint)rd.GetInt32(1);
				rd.Close();
				conn.Close();
				return userid;
			} else
			{
				rd.Close();
				conn.Close();
				return null;
			}
		}

		/// <summary>
		/// 通过token获取对应玩家的最近登入设备信息。
		/// </summary>
		/// <param name="token">要获取的最近登入设备信息对应的玩家的token。</param>
		/// <returns>成功返回(设备型号名,设备uuid);<br />
		/// 存在该玩家的登录信息但设备信息为空返回(<see cref="string.Empty"/>,<see cref="string.Empty"/>);<br />
		/// 不存在该玩家的登录信息返回 <see langword="null" /> 。</returns>
		public static (string,string)? GetDeviceInfoByToken(string token)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = conn.CreateCommand();
			cmd.CommandText = $"SELECT COUNT(*),COUNT(last_login_device),last_login_device,last_login_deviceId FROM logins WHERE access_token=?token;";
			cmd.Parameters.Add(new MySqlParameter("?token", MySqlDbType.VarString)
			{
				Value = token
			});
			var rd = cmd.ExecuteReader();
			rd.Read();
			if (rd.GetInt32(0) == 1)
			{
				if (rd.GetInt32(1) == 1)
				{
					string devName = rd.GetString(2);
					string devId = rd.GetString(3);
					rd.Close();
					conn.Close();
					return (devName, devId);
				}
				else
				{
					rd.Close();
					conn.Close();
					return (string.Empty, string.Empty);
				}
			}
			else
			{
				rd.Close();
				conn.Close();
				return null;
			}
		}

		/// <summary>
		/// 生成新的文件下载Token。
		/// </summary>
		/// <param name="userId">玩家账号的用户id。</param>
		/// <returns></returns>
		public static string? GenDownloadToken(int userId, string songid)
		{
			string token = Guid.NewGuid().ToString();
			var redisConn = ConnectionMultiplexer.Connect(MDatabaseConnectURL);
			try
			{
				if (redisConn.IsConnected)
				{
					var redisDB = redisConn.GetDatabase();
					string tokenValue = $"Arcaea-SongDownload-Token-{userId}-{songid}-{DateTime.Now:yyyyMMddHHmmssfff}";
					var expireTime = TimeSpan.FromHours(1.5);
					bool r = redisDB.StringSet(token, tokenValue, expireTime);
					redisConn.Close(true);
					if (r)
					{
						return token;
					}
					else
					{
						return null;
					}
				}
				else
				{
					return null;
				}
			}
			catch
			{
				return null;
			}
			finally
			{
				redisConn.Close();
			}
		}

		public static string? SetCustomDownloadToken(uint userId, string songId, string token)
		{
			var redisConn = ConnectionMultiplexer.Connect(MDatabaseConnectURL);
			try
			{
				if (redisConn.IsConnected)
				{
					var redisDB = redisConn.GetDatabase();
					string tokenValue = $"Arcaea-SongDownload-Token-{userId}-{songId}-{DateTime.Now:yyyyMMddHHmmssfff}";
					var expireTime = TimeSpan.FromHours(1.5);
					bool r = redisDB.StringSet(token, tokenValue, expireTime);
					redisConn.Close(true);
					if (r)
					{
						return token;
					}
					else
					{
						return null;
					}
				}
				else
				{
					return null;
				}
			}
			catch
			{
				return null;
			}
			finally
			{
				redisConn.Close();
			}
		}

		public static uint? GetUserIdByDownloadToken(string downloadToken, out string? downloadSongId)
		{
			var redisConn = ConnectionMultiplexer.Connect(MDatabaseConnectURL);
			try
			{
				if (redisConn.IsConnected)
				{
					var redisDB = redisConn.GetDatabase();
					var tokenValueWrapper = redisDB.StringGetWithExpiry(downloadToken);
					if (tokenValueWrapper.Value != RedisValue.Null)
					{
						string tokenValue = tokenValueWrapper.Value;
						if (tokenValue.StartsWith("Arcaea-SongDownload-Token-"))
						{
							uint userId = uint.Parse(tokenValue.Split('-')[3]);
							string songId = tokenValue.Split('-')[4];
							downloadSongId = songId;
							return userId;
						}
						else
						{
							downloadSongId = null;
							return null;
						}
					}
					else
					{
						downloadSongId = null;
						return null;
					}
				}
				else
				{
					downloadSongId = null;
					return null;
				}
			}
			catch
			{
				downloadSongId = null;
				return null;
			}
			finally
			{
				redisConn.Close();
			}
		}

		public static bool TryGetUserIdByDownloadToken(string downloadToken, out uint value, out string? downloadSongId)
		{
			var r = GetUserIdByDownloadToken(downloadToken, out string? downloadSongWrapper);
			if (r.HasValue)
			{
				value = r.Value;
				downloadSongId = downloadSongWrapper;
				return true;
			}
			else
			{
				value = 0;
				downloadSongId = null;
				return false;
			}
		}
	}
}
