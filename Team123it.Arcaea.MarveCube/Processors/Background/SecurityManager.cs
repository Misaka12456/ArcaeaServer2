using static Team123it.Arcaea.MarveCube.GlobalProperties;
using MySql.Data.MySqlClient;
using System;
using Newtonsoft.Json.Linq;

namespace Team123it.Arcaea.MarveCube.Processors.Background
{
	public static class SecurityManager
	{
		/// <summary>
		/// 修改玩家的信用点数。
		/// </summary>
		/// <param name="userid">玩家的用户id(非好友id)。</param>
		/// <param name="range">要修改的数量(可以为负数)。</param>
		public static void EditPlayerCreditPoint(uint userid,int range,string reason = "")
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT credit_point,name,credit_edit_reasons FROM users WHERE user_id=?uid;";
			cmd.Parameters.Add(new MySqlParameter("?uid", MySqlDbType.Int32)
			{
				Value = userid
			});
			var rd = cmd.ExecuteReader();
			rd.Read();
			int beforeCredit = rd.GetInt32(0);
			string username = rd.GetString(1);
			JArray reasons;
			if (rd.IsDBNull(2) || rd.GetString(2) == string.Empty)
			{
				reasons = new JArray();
			}
			else
			{
				reasons = JArray.Parse(rd.GetString(2));
			}
			rd.Close();
			int afterCredit = beforeCredit + range;
			bool isBanned = false;
			if (afterCredit < 0)
			{
				afterCredit = 0;
				isBanned = true;
			}
			else if (afterCredit == 0)
			{
				isBanned = true;
			}
			if (reason != "")
			{
				reasons.Add(new JObject()
				{
					{ "date", DateTime.Now.ToString("yyyy-M-d H:mm:ss") },
					{ "player", username + $"(id:{userid})" },
					{ "beforeCreditPoint", beforeCredit },
					{ "afterCreditPoint", afterCredit },
					{ "creditPointRange", range },
					{ "reason", reason }
				});
			}
			else
			{
				reasons.Add(new JObject()
				{
					{ "date", DateTime.Now.ToString("yyyy-M-d H:mm:ss") },
					{ "player", username + $"(id:{userid})" },
					{ "beforeCreditPoint", beforeCredit },
					{ "afterCreditPoint", afterCredit },
					{ "creditPointRange", range },
					{ "reason", "N/A" }
				});
			}
			cmd.Parameters.Clear();
			cmd.CommandText = "UPDATE users SET credit_point=?credit,credit_edit_reasons=?reasons,is_banned=?isBanned WHERE user_id=?uid;";
			cmd.Parameters.Add(new MySqlParameter("?credit", MySqlDbType.Int32)
			{
				Value = afterCredit
			});
			cmd.Parameters.Add(new MySqlParameter("?reasons", MySqlDbType.VarString)
			{
				Value = reasons.ToString()
			});
			cmd.Parameters.Add(new MySqlParameter("?uid", MySqlDbType.Int32)
			{
				Value = userid
			});
			cmd.Parameters.Add(new MySqlParameter("?isBanned", MySqlDbType.Int32)
			{
				Value = Convert.ToInt32(isBanned)
			});
			cmd.ExecuteNonQuery();
			conn.Close();
		}
	}
}
