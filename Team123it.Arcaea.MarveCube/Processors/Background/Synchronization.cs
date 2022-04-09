using static Team123it.Arcaea.MarveCube.GlobalProperties;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using Team123it.Arcaea.MarveCube.Core;
using System.IO;

namespace Team123it.Arcaea.MarveCube.Processors.Background
{
	/// <summary>
	/// 提供适用于玩家数据同步的常用 <see langword="static"/> 方法的类。无法继承此类。
	/// </summary>
	public sealed class Synchronization
	{
		public static bool UploadAll(uint userid,JArray scores,JArray clearLamps,JArray clearedSongs,JArray unlockList,JArray stories,string installId,string deviceModelName)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = "REPLACE INTO user_saves (`user_id`,`scores_data`,`clearlamps_data`,`clearedsongs_data`,`unlocklist_data`,`story_data`,`device_id`,`device_name`,`create_time`) " +
					"VALUES (?userId,?scoresData,?clearLampsData,?clearedSongsData,?unlockListData,?storyData,?deviceId,?deviceName,?createTime)";
				cmd.Parameters.Add(new MySqlParameter("?userId", MySqlDbType.Int32)
				{
					Value = userid
				});
				cmd.Parameters.Add(new MySqlParameter("?scoresData", MySqlDbType.LongText)
				{
					Value = scores.ToString()
				});
				cmd.Parameters.Add(new MySqlParameter("?clearLampsData", MySqlDbType.LongText)
				{
					Value = clearLamps.ToString()
				});
				cmd.Parameters.Add(new MySqlParameter("?clearedSongsData", MySqlDbType.LongText)
				{
					Value = clearedSongs.ToString()
				});
				cmd.Parameters.Add(new MySqlParameter("?unlockListData", MySqlDbType.LongText)
				{
					Value = unlockList.ToString()
				});
				cmd.Parameters.Add(new MySqlParameter("?storyData", MySqlDbType.LongText)
				{
					Value = stories.ToString()
				});
				cmd.Parameters.Add(new MySqlParameter("?deviceId", MySqlDbType.VarChar)
				{
					Value = installId
				});
				cmd.Parameters.Add(new MySqlParameter("?deviceName", MySqlDbType.VarChar)
				{
					Value = deviceModelName
				});
				cmd.Parameters.Add(new MySqlParameter("?createTime", MySqlDbType.DateTime)
				{
					Value = DateTime.Now
				});
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (ArcaeaAPIException)
			{
				return false;
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

		public static JObject FetchAll(uint userid)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"SELECT user_id, scores_data, clearlamps_data, clearedsongs_data, unlocklist_data, story_data, device_id, device_name, create_time " +
					$"FROM user_saves WHERE user_id={userid}";
				var rd = cmd.ExecuteReader();
				if (rd.Read())
				{
					var scoresData = !rd.IsDBNull(1) ? JArray.Parse(rd.GetString("scores_data")) : new JArray();
					var clearLampsData = !rd.IsDBNull(2) ? JArray.Parse(rd.GetString("clearlamps_data")) : new JArray();
					var clearedSongsData = !rd.IsDBNull(3) ? JArray.Parse(rd.GetString("clearedsongs_data")) : new JArray();
					var unlockListData = !rd.IsDBNull(4) ? JArray.Parse(rd.GetString("unlocklist_data")) : new JArray();
					var storyData = !rd.IsDBNull(5) ? JArray.Parse(rd.GetString("story_data")) : new JArray();
					string installId = !rd.IsDBNull(6) ? rd.GetString("device_id") : "DEFAULT-DEVICE-14956174-15963574";
					string deviceModelName = !rd.IsDBNull(7) ? rd.GetString("device_name") : "Default Device";
					long createTime = !rd.IsDBNull(8) ? Convert.ToInt64(Math.Round((rd.GetDateTime("create_time") - DateTime.UnixEpoch).TotalMilliseconds, 0, MidpointRounding.AwayFromZero)) : 0;
					rd.Close();
					var r = new JObject()
					{
						{ "user_id", userid },
						{
							"story", new JObject()
							{
								{ "", storyData },
							}
						},
						{
							"devicemodelname", new JObject()
							{
								{ "val", deviceModelName },
							}
						},
						{
							"installid", new JObject()
							{
								{ "val", installId }
							}
						},
						{
							"unlocklist", new JObject()
							{
								{ "", unlockListData }
							}
						},
						{
							"clearedsongs", new JObject()
							{
								{ "", clearedSongsData }
							}
						},
						{
							"clearlamps", new JObject()
							{
								{ "", clearLampsData }
							}
						},
						{
							"scores", new JObject()
							{
								{ "", scoresData }
							}
						},
						{
							"version", new JObject()
							{
								{ "val", 1 }
							}
						},
						{
							"createdAt", createTime
						}
					};
					return r;
				}
				else
				{
					rd.Close();
					var defaultR = new JObject()
					{
						{ "user_id", userid },
						{
							"story", new JObject()
							{
								{ "", JArray.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "defaultStoryData.json"), Encoding.UTF8)) }
							}
						},
						{
							"devicemodelname", new JObject()
							{
								{ "val", "Default Device" }
							}
						},
						{
							"installid", new JObject()
							{
								{ "val", "DEFAULT-DEVICE-14956174-15963574" }
							}
						},
						{
							"unlocklist", new JObject()
							{
								{ "", new JArray() }
							}
						},
						{
							"clearedsongs", new JObject()
							{
								{ "", new JArray() }
							}
						},
						{
							"clearlamps", new JObject()
							{
								{ "", new JArray() }
							}
						},
						{
							"scores", new JObject()
							{
								{ "", new JArray() }
							}
						},
						{
							"version", new JObject()
							{
								{ "val", 1 }
							}
						},
						{
							"createdAt", 0
						}
					};
					return defaultR;
				}
				// 解锁key(unlock_key)格式:
				// 曲目id|曲目难度id|特殊解锁id(|优先解锁曲目id|优先解锁曲目难度id)
				// 曲目难度id(0=Past 1=Present 2=Future 3=Beyond)
				// 特殊解锁id:无特殊解锁=0, 要求优先解锁其它曲目=3, 要求异象解锁=101
				// 括号中内容仅在特殊解锁id为3的情况下可用且必须存在
			}
			catch
			{
				throw;//throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
			}
			finally
			{
				conn.Close();
			}
		}

		private static JArray GetBlankUnlockList()
		{
			var unlocks = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "static", "unlocks.json"))).Value<JArray>("unlocks");
			var r = new JArray();
			foreach (JObject song in unlocks)
			{
				string sid = song.Value<string>("songId");
				int diff = song.Value<int>("ratingClass");
				var conditions = song.Value<JArray>("conditions");
				foreach (JObject condition in conditions)
				{
					var unlockKey = new StringBuilder();
					unlockKey.Append(sid).Append(@"|").Append(diff).Append(@"|");
					bool isUnlockNeedStore = true;
					switch (condition.Value<int>("type"))
					{
						case 0:
							// sid|2|0
							unlockKey.Append("0");
							break;
						case 3:
							// sid|2|3|preposition|2
							unlockKey.Append(@"3|").Append(condition.Value<string>("song_id")).Append(@"|").Append(condition.Value<int>("song_difficulty"));
							break;
						case 4:
							var subConditions = condition.Value<JArray>("conditions");
							string subConditionSid = string.Empty;
							int subConditionDiff = 0;
							foreach (JObject subCondition in subConditions)
							{
								subConditionSid = subCondition.Value<string>("song_id");
								subConditionDiff = subCondition.Value<int>("song_difficulty");
								break;
							}
							// sid|2|3|subcondition_preposition|2
							unlockKey.Append(@"3|").Append(subConditionSid).Append(@"|").Append(subConditionDiff);
							break;
						case 101:
							// sid|2|101
							unlockKey.Append("101");
							break;
						default:
							isUnlockNeedStore = false;
							break;
					}
					if (isUnlockNeedStore)
					{
						r.Add(new JObject()
						{
							{ "complete",0 },
							{ "unlock_key", unlockKey.ToString() }
						});
					}
				}
			}
			return r;
		}
	}
}
