using static Team123it.Arcaea.MarveCube.GlobalProperties;
using Newtonsoft.Json.Linq;
using System.Linq;
using Team123it.Arcaea.MarveCube.Core;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Team123it.Arcaea.MarveCube.Processors.Front
{
	public class Present
	{
		/// <summary>
		/// 接收指定的礼物。
		/// </summary>
		/// <param name="userId">玩家的用户id。</param>
		/// <param name="presentId">要接收的礼物id。</param>
		/// <returns>包含接收结果信息的 <see cref="JObject"/> 类实例。</returns>
		/// <exception cref="ArcaeaAPIException" />
		public static JObject ClaimPresent(uint userId, string presentId)
		{
			var p = new PlayerInfo(userId, out _);
			if (p.Banned!.Value)
			{
				throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountHasBeenBlocked);
			}
			if (p.ClaimedPresentsList != null && p.ClaimedPresentsList.Any(claimedPresent => ((string)claimedPresent) == presentId))
			{
				throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AlreadyHasItem);
			}
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				var items = (from present in FetchAvailablePresents(p)
							 where present.Value<string>("present_id") == presentId
							 select present.Value<JArray>("items")).First();
				conn.Open();
				var cmd = conn.CreateCommand();
				var claimedPresents = p.ClaimedPresentsList ?? new JArray();
				foreach (JObject item in items)
				{
					switch (item.Value<string>("type"))
					{
						case "memory":
							int amount = item.Value<int>("amount");
							int totalMemories = p.Ticket!.Value + amount;
							cmd.CommandText = $"UPDATE users SET ticket={totalMemories} WHERE user_id={p.UserId!.Value};";
							cmd.ExecuteNonQuery();
							break;
					}
				}
				claimedPresents.Add(presentId);
				cmd.CommandText = $"UPDATE users SET claimed_presents=?claimedPresents WHERE user_id={p.UserId!.Value};";
				cmd.Parameters.Add(new MySqlParameter("?claimedPresents", MySqlDbType.Text)
				{
					Value = claimedPresents.ToString()
				});
				cmd.ExecuteNonQuery();
				p.RefreshData();
				var r = new JObject()
				{
					{ "user", p.GetUserBaseInfoData() },
					{ "items", items },
					{ "reward_char_stats", new JArray() }
				};
				return r;
			}
			catch (ArcaeaAPIException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
			}
			finally
			{
				conn.Close();
			}
		}

		/// <summary>
		/// 获取指定玩家可接收的礼物列表。
		/// </summary>
		/// <param name="info">玩家对应的 <see cref="PlayerInfo"/> 类实例。</param>
		/// <param name="language">要获取的礼物信息使用的语言。默认为简体中文(zh-Hans)。</param>
		/// <returns>包括可接受礼物列表的 <see cref="JArray"/> 类实例。</returns>
		public static JArray FetchAvailablePresents(PlayerInfo info, string language = "zh-Hans")
		{
			var r = new JArray();
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"SELECT * FROM fixed_presents;";
				var rd = cmd.ExecuteReader();
				while (rd.Read())
				{
					if ((info.ClaimedPresentsList != null) && info.ClaimedPresentsList.ToObject<List<string>>()!.Contains(rd.GetString("present_id")))
					{
						continue;
					}
					var singlePresent = new JObject()
					{
						{ "expire_ts", Convert.ToInt64((rd.GetDateTime("expire_time") - DateTime.UnixEpoch).TotalMilliseconds) },
						{ "is_claimed", (info.ClaimedPresentsList != null) && info.ClaimedPresentsList.ToObject<List<string>>()!.Contains(rd.GetString("present_id")) },
						{ "description", rd.GetString($"description_{language}") },
						{ "present_id", rd.GetString("present_id") },
						{ "items", JArray.Parse(rd.GetString("items")) }
					};
					Console.WriteLine(singlePresent.Value<long>("expire_ts"));
					r.Add(singlePresent);
				}
				rd.Close();
				return r;
			}
			catch (ArcaeaAPIException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return new JArray();
			}
			finally
			{
				conn.Close();
			}
		}
	}
}
