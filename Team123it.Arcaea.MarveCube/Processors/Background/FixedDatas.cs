using static Team123it.Arcaea.MarveCube.GlobalProperties;
using Newtonsoft.Json.Linq;
using MySql.Data.MySqlClient;
using System.Linq;

namespace Team123it.Arcaea.MarveCube.Processors.Background
{
	public static class FixedDatas
	{
		/// <summary>
		/// 返回2.0版本更新前Arcaea所有存在Beyond难度的曲目的sid数组。
		/// </summary>
		/// <returns>以World模式Beyond曲目格式(sid + "3")命名的string数组。</returns>
		public static JArray GetAllBeyondSongIdsBeforeArcaeaVer20()
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = "SELECT sid FROM fixed_songs WHERE rating_byd!=-1 AND date<1643457600;"; // 1643457600: Arcaea 2.0.0版本更新的UNIX时间戳(UTC=2022.1.29 12:00/北京时间(GMT+8)=20:00)
				var rd = cmd.ExecuteReader();
				var bydSids = new JArray();
				while (rd.Read())
				{
					bydSids.Add(rd.GetString(0) + "3");
				}
				rd.Close();
				return bydSids;
			}
			catch
			{
				return new JArray();
			}
			finally
			{
				conn.Close();
			}
		}

		public static JArray GetAllPackIds()
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = "SELECT pid FROM fixed_packs;";
				var rd = cmd.ExecuteReader();
				var pids = new JArray();
				while (rd.Read())
				{
					pids.Add(rd.GetString(0));
				}
				rd.Close();
				return pids;
			}
			catch
			{
				return new JArray();
			}
			finally
			{
				conn.Close();
			}
		}

		/// <summary>
		/// 返回指定用户id对应的玩家所拥有的Beyond难度的曲目(2.0版本更新后)的sid数组。
		/// </summary>
		/// <param name="userId">玩家的用户id(非好友id)。</param>
		/// <returns>以World模式Beyond曲目格式(sid + "3")命名的string数组。</returns>
		public static JArray GetPlayerOwnBeyondSongIds(uint userId)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = "SELECT sid FROM user_bydunlocks WHERE user_id=?uid;";
				cmd.Parameters.Add(new MySqlParameter("?uid", MySqlDbType.Int32)
				{
					Value = userId
				});
				var rd = cmd.ExecuteReader();
				var bydSids = new JArray();
				while (rd.Read())
				{
					bydSids.Add(rd.GetString(0) + "3");
				}
				rd.Close();
				return bydSids;
			}
			catch
			{
				return new JArray();
			}
			finally
			{
				conn.Close();
			}
		}

		/// <summary>
		/// 返回指定用户id对应的玩家所拥有的所有Beyond难度的曲目(包括2.0版本更新前及更新后)的sid数组。
		/// </summary>
		/// <param name="userId">玩家的用户id(非好友id)。</param>
		/// <returns>以World模式Beyond曲目格式(sid + "3")命名的string数组。</returns>
		public static JArray GetPlayerAllOwnBeyondSongIds(uint userId)
		{
			var r1 = GetPlayerOwnBeyondSongIds(userId);
			var r2 = GetAllBeyondSongIdsBeforeArcaeaVer20();
			var r = new JArray(r1.Union(r2));
			return r;
		}
	}
}
