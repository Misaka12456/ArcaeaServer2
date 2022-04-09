using static Team123it.Arcaea.MarveCube.GlobalProperties;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;
using System.Text;
using Team123it.Arcaea.MarveCube.Core;
using static Team123it.Arcaea.MarveCube.Core.ArcaeaAPIException;
using Random = System.Enhance.Random;
using ToolBox.UserAgentParse;

namespace Team123it.Arcaea.MarveCube.Processors.Front
{
	/// <summary>
	/// 玩家帐号验证相关API。<br />
	/// 对应API前缀:/cutestscope/2/auth/
	/// </summary>
	public class Auth
	{
		/// <summary>
		/// [API]玩家登录.
		/// </summary>
		/// <param name="username">玩家帐号用户名/E-mail.</param>
		/// <param name="password">帐号密码.</param>
		/// <returns>Json数据.</returns>
		public static string Login(HttpRequest req,string username,string password)
		{
			var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				conn.Open();
				string passEncrypted = BitConverter.ToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-","").ToLower();
				var cmd = new MySqlCommand($"SELECT COUNT(*),user_id FROM users WHERE name='{username}' AND password='{passEncrypted}'", conn);
				var data = cmd.ExecuteReader();
				data.Read();
				int result = data.GetInt32(0);
				if (result == 1)
				{
					int userid = (int)data.GetValue(1);
					data.Close();
					cmd.CommandText = $"SELECT COUNT(*) FROM logins WHERE user_id={userid}";
					long result2 = (long)cmd.ExecuteScalar();
					if (result2 >= 1)
					{
						cmd.CommandText = $"DELETE FROM logins WHERE user_id={userid}";
						cmd.ExecuteNonQuery();
					}
					var ua = new UaUnit((string)req.Headers["User-Agent"]).Parse();
					string devName = (ua != null) ? ua.PhoneModelCode : string.Empty;
					string devId = req.Headers["DeviceId"];
					string token = Random.GenerateRandomString(15) + "=";
					cmd.CommandText = $"INSERT INTO logins (access_token,user_id,last_login_device,last_login_deviceId) VALUES (?token,?userid,?devName,?devId)";
					cmd.Parameters.Add(new MySqlParameter("?token", MySqlDbType.VarChar)
					{
						Value = token
					}); 
					cmd.Parameters.Add(new MySqlParameter("?userid", MySqlDbType.Int32)
					{
						Value = userid
					}); 
					cmd.Parameters.Add(new MySqlParameter("?devName", MySqlDbType.VarString)
					{
						Value = devName
					});
					cmd.Parameters.Add(new MySqlParameter("?devId", MySqlDbType.VarString)
					{
						Value = devId
					});
					cmd.ExecuteNonQuery();
					var response = new JObject()
					{
						{"success",true },
						{"access_token",token },
						{"token_type","Bearer" }
					};
					return response.ToString();
				}
				else
				{
					return new ArcaeaAPIException(APIExceptionType.UsernameOrPasswordInvalid);
				}
			}
			catch (ArcaeaAPIException)
			{
				throw;//return new ArcaeaAPIException(APIExceptionType.Other);
			}
			finally
			{
				conn.Close();
			}
		}
	}
}
