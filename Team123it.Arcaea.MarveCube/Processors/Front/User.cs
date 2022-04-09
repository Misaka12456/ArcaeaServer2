using static Team123it.Arcaea.MarveCube.GlobalProperties;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;
using System.Text;
using Team123it.Arcaea.MarveCube.Core;
using Random = System.Enhance.Random;
using Team123it.Arcaea.MarveCube.Processors.Background;

namespace Team123it.Arcaea.MarveCube.Processors.Front
{
	/// <summary>
	/// 玩家用户数据相关API。<br />
	/// 对应API前缀:/cutestscope/2/user/
	/// </summary>
	public class User
	{
		/// <summary>
		/// [API]新玩家用户注册。
		/// </summary>
		/// <param name="name">新玩家的昵称。</param>
		/// <param name="password">新玩家的密码。</param>
		/// <param name="email">新玩家的E-mail。</param>
		/// <returns>Json数据。</returns>
		/// <exception cref="ArcaeaAPIException" />
		public static JObject Register(string name,string password,string email)
		{
			var r = new JObject();
			using var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = conn.CreateCommand();
			cmd.CommandText = $"SELECT COUNT(*) FROM users WHERE name='{name}'";
			bool isNameDuplicated = ((long)cmd.ExecuteScalar() == 1) ? true : false; //检查是否用户名重复
			if (isNameDuplicated)
			{
				conn.Close();
				throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.UsernameExists);
			}
			cmd.CommandText = $"SELECT COUNT(*) FROM users WHERE email='{email}'";
			bool isEmailDuplicated = ((long)cmd.ExecuteScalar() == 1) ? true : false; //检查是否E-mail重复
			if (isEmailDuplicated)
			{
				conn.Close();
				throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.EmailHasRegistered);
			}
			var tr = conn.BeginTransaction();
			string passSHA256 = BitConverter.ToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "").ToLower();
			byte[] save = new byte[1024];
			RandomNumberGenerator.Create().GetBytes(save,0,1024);
			string user_code = BitConverter.ToUInt32(save).ToString().Substring(0,8);
			while (user_code.Length < 9)
			{
				user_code = "0" + user_code;
			}
			cmd.CommandText = $"INSERT INTO users (user_code,name,email,password,join_date,favorite_character) VALUES " +
				$"('{user_code}','{name}','{email}','{passSHA256}',{(long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds},-1)";
			cmd.ExecuteNonQuery();
			cmd.CommandText = $"SELECT user_id FROM users WHERE name='{name}'";
			int user_id = (int)cmd.ExecuteScalar();
			cmd.CommandText = $"INSERT INTO user_chars (user_id,character_id,level,exp,level_exp,frag,prog,overdrive,skill_id) VALUES ({user_id},0,1,0,50,50,50,50,'gauge_easy');";
			cmd.ExecuteNonQuery();
			cmd.CommandText = $"INSERT INTO user_chars (user_id,character_id,level,exp,level_exp,frag,prog,overdrive) VALUES ({user_id},1,1,0,50,50,50,50);";
			cmd.ExecuteNonQuery();
			string token = Random.GenerateRandomString(15) + "=";
			cmd.CommandText = $"INSERT INTO logins (access_token,user_id) VALUES ('{token}',{user_id})";
			cmd.ExecuteNonQuery();
			tr.Commit();
			conn.Close();
			r.Add("success", true);
			r.Add("value", new JObject()
			{
				{"user_id",user_id },
				{"access_token",token }
			});
			return r;
		}

		/// <summary>
		/// [API]切换角色。
		/// </summary>
		/// <param name="userid">玩家的用户id。</param>
		/// <param name="charid">新角色id。</param>
		/// <param name="isSkillSealed">技能是否已被封印。</param>
		/// <exception cref="ArcaeaAPIException" />
		public static JObject ChangeCharacter(uint userid,uint charid,bool isSkillSealed)
		{
			int skill_sealed = isSkillSealed ? 1 : 0;
			var r = new JObject();
			var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = conn.CreateCommand();
			cmd.CommandText = $"UPDATE users SET character_id={charid},is_skill_sealed={skill_sealed} WHERE user_id={userid}";
			cmd.ExecuteNonQuery();
			conn.Close();
			r.Add("user_id", userid);
			r.Add("character", charid);
			return r;
		}

		/// <summary>
		/// [API]调整玩家的设置。
		/// </summary>
		/// <param name="userid">玩家的用户id(非好友id)。</param>
		/// <param name="type">设置的类型。
		/// <para>favorite_character - 星标搭档设置<br />
		/// is_hide_rating - 个人游玩潜力值隐藏/显示设置</para></param>
		/// <param name="value">设置的值。</param>
		/// <returns>玩家的微型Aggregate结果(TinyAggregate)的Json数据。</returns>
		public static JObject SetPlayerSettings(uint userid,string type,object value)
		{
			switch (type)
			{
				case "is_hide_rating":
					{
						bool hideRating = Convert.ToBoolean(value);
						using var conn = new MySqlConnection(DatabaseConnectURL);
						conn.Open();
						var cmd = conn.CreateCommand();
						cmd.CommandText = "UPDATE users SET is_hide_rating=?hideRating WHERE user_id=?uid;";
						cmd.Parameters.Add(new MySqlParameter("?uid", MySqlDbType.Int32)
						{
							Value = userid
						});
						cmd.Parameters.Add(new MySqlParameter("?hideRating", MySqlDbType.Int32)
						{
							Value = hideRating
						});
						cmd.ExecuteNonQuery();
					}
					var r = new JObject()
					{
						{ "value", new PlayerInfo(userid,out _).GetUserBaseInfoData() }
					};
					return r;
				case "favorite_character":
					{
						int favoriteChar = Convert.ToInt32(value);
						using var conn = new MySqlConnection(DatabaseConnectURL);
						conn.Open();
						var cmd = conn.CreateCommand();
						cmd.CommandText = "SELECT COUNT(character_id) FROM user_chars WHERE user_id=?uid AND character_id=?charId;";
						cmd.Parameters.Add(new MySqlParameter("?uid", MySqlDbType.Int32)
						{
							Value = userid
						});
						cmd.Parameters.Add(new MySqlParameter("?charId", MySqlDbType.Int32)
						{
							Value = favoriteChar
						});
						int isCharExists = Convert.ToInt32(cmd.ExecuteScalar());
						if (isCharExists == 1)
						{
							cmd.Parameters.Clear();
							cmd.CommandText = "UPDATE users SET favorite_character=?favCharId WHERE user_id=?uid;";
							cmd.Parameters.Add(new MySqlParameter("?uid", MySqlDbType.Int32)
							{
								Value = userid
							});
							cmd.Parameters.Add(new MySqlParameter("?favCharId", MySqlDbType.Int32)
							{
								Value = favoriteChar
							});
							cmd.ExecuteNonQuery();
							conn.Close();
						}
						else
						{
							conn.Close();
							SecurityManager.EditPlayerCreditPoint(userid, -6, $"Attempted to switch favorite character to the character that the player doesn't possess. Character Id:{favoriteChar}");
							throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountBlockWarning);
						}
					}
					var r2 = new JObject()
					{
						{ "value", new PlayerInfo(userid,out _).GetUserBaseInfoData() }
					};
					return r2;
				default:
					throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
			}
		}
	}
}
