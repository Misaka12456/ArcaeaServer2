#nullable enable
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Team123it.Arcaea.MarveCube.Core;
using static Team123it.Arcaea.MarveCube.GlobalProperties;
using PlayerInfo2 = Team123it.Arcaea.MarveCube.Core.PlayerInfo;

namespace Team123it.Arcaea.MarveCube.Processors.Front
{
	/// <summary>
	/// 查分Bot相关API。<br />
	/// 对应API前缀:/botarcapi/
	/// </summary>
	public static class Bot
	{
		/// <summary>
		/// 获取玩家的指定曲目及难度的最佳成绩数据。
		/// </summary>
		/// <param name="user">玩家的好友id或昵称。</param>
		/// <param name="songid">要查询的曲目id。</param>
		/// <param name="difficulty">要查询的曲目难度。</param>
		/// <param name="withsonginfo">是否需要包含SongInfo</param>
		/// <param name="withrecent">是否附有最近成绩数据</param>
		/// <returns>包含玩家基本个人信息及查询到的玩家最佳成绩数据的 <see cref="JObject"/> 类实例。</returns>
		/// <exception cref="BotAPIException" />
		public static JObject QueryPlayerBestScore(string user, string songid, SongDifficulty difficulty, bool withsonginfo, bool withrecent)
		{
			try
			{
				if (int.TryParse(user, out _) && user.Length != 9) throw new BotAPIException(BotAPIException.APIExceptionType.PlayerNotExist, null);
				var p = new PlayerInfo2(user, out bool isExists);
				var r = new JObject();
				if (!isExists)
				{
					p = PlayerInfo2.CreateFromUsername(user);
					if (p == null)
					{
						throw new BotAPIException(BotAPIException.APIExceptionType.PlayerNotExist, null);
					}
				}
				if (!p.Banned!.Value)
				{
					using var conn = new MySqlConnection(DatabaseConnectURL);
					conn.Open();
					var cmd = conn.CreateCommand();
					cmd.CommandText = $"SELECT COUNT(*) FROM fixed_songAlias WHERE alias LIKE concat('%',?alias,'%');";
					cmd.Parameters.Add(new MySqlParameter("?alias", songid));
					int equalCounts = Convert.ToInt32(cmd.ExecuteScalar()); //获取匹配songid(可能是别名)的曲目数量
					cmd.Parameters.Clear();
					string sid;
					if (equalCounts > 1)
					{
						conn.Close();
						throw new BotAPIException(BotAPIException.APIExceptionType.TooManySongsFromAlias, null);
					}
					else if (equalCounts == 1)
					{
						cmd.Parameters.Clear();
						cmd.CommandText = $"SELECT sid FROM fixed_songAlias WHERE alias LIKE concat('%',?alias,'%');";
						cmd.Parameters.Add(new MySqlParameter("?alias", songid));
						sid = (string) cmd.ExecuteScalar();
					}
					else
					{
						cmd.Parameters.Clear();
						cmd.CommandText = $"SELECT COUNT(*) FROM fixed_songs WHERE sid LIKE concat('%',?songid,'%') OR name_en LIKE concat('%',?name_en,'%') OR name_jp LIKE concat('%',?name_ja,'%');";
						cmd.Parameters.Add(new MySqlParameter("?songid", songid));
						cmd.Parameters.Add(new MySqlParameter("?name_en", songid));
						cmd.Parameters.Add(new MySqlParameter("?name_ja", songid));
						int equalCounts2 = Convert.ToInt32(cmd.ExecuteScalar());
						cmd.Parameters.Clear();
						if (equalCounts2 > 1)
						{
							conn.Close();
							throw new BotAPIException(BotAPIException.APIExceptionType.TooManySongsFromAlias, null);
						}
						else if (equalCounts2 == 1)
						{
							cmd.Parameters.Clear();
							cmd.CommandText = $"SELECT sid FROM fixed_songs WHERE sid LIKE concat('%',?songid,'%') OR name_en LIKE concat('%',?name_en,'%') OR name_jp LIKE concat('%',?name_ja,'%');";
							cmd.Parameters.Add(new MySqlParameter("?songid", songid));
							cmd.Parameters.Add(new MySqlParameter("?name_en", songid));
							cmd.Parameters.Add(new MySqlParameter("?name_ja", songid));
							sid = (string)cmd.ExecuteScalar();
						}
						else
						{
							conn.Close();
							throw new BotAPIException(BotAPIException.APIExceptionType.SongIsNotExist, null);
						}
					}
					cmd.Parameters.Clear();
					cmd.CommandText = $"SELECT COUNT(*),rating_pst,rating_prs,rating_ftr,rating_byd FROM fixed_songs WHERE sid=?songid";
					cmd.Parameters.Add(new MySqlParameter("?songid", sid));
					var rd = cmd.ExecuteReader();
					rd.Read();
					// 判断指定曲目及选择的难度是否存在
					if (rd.GetInt32(0) == 0) throw new BotAPIException(BotAPIException.APIExceptionType.SongIsNotExist, null);
					int rating_pst, rating_prs, rating_ftr, rating_byd, song_rating;
					rating_pst = rd.GetInt32(1);
					rating_prs = rd.GetInt32(2);
					rating_ftr = rd.GetInt32(3);
					rating_byd = rd.GetInt32(4);
					rd.Close();
					if (difficulty == SongDifficulty.Past && rating_pst == -1)
					{
						throw new BotAPIException(BotAPIException.APIExceptionType.DifficultyIsNotExist, null);
					}
					else
					{
						song_rating = rating_pst;
					}
					if (difficulty == SongDifficulty.Present && rating_prs == -1)
					{
						throw new BotAPIException(BotAPIException.APIExceptionType.DifficultyIsNotExist, null);
					}
					else
					{
						song_rating = rating_prs;
					}
					if (difficulty == SongDifficulty.Future && rating_ftr == -1)
					{
						throw new BotAPIException(BotAPIException.APIExceptionType.DifficultyIsNotExist, null);
					}
					else
					{
						song_rating = rating_ftr;
					}
					if (difficulty == SongDifficulty.Beyond && rating_byd == -1)
					{
						throw new BotAPIException(BotAPIException.APIExceptionType.DifficultyIsNotExist, null);
					}
					else
					{
						song_rating = rating_byd;
					}
					cmd.Parameters.Clear();
					cmd.CommandText = $"SELECT * FROM bests WHERE user_id={p.UserId!.Value} AND song_id=?songid AND difficulty=?difficulty;";
					cmd.Parameters.Add(new MySqlParameter("?songid", sid));
					cmd.Parameters.Add(new MySqlParameter("?difficulty", (int) difficulty));
					rd = cmd.ExecuteReader();
					if (rd.HasRows) //存在最好成绩
					{
						rd.Read();
						using var conn2 = new MySqlConnection(DatabaseConnectURL);
						conn2.Open();
						var cmd2 = conn2.CreateCommand();
						cmd2.CommandText = $"SELECT character_id FROM users WHERE user_id={p.UserId!.Value};";
						var record = new JObject()
						{
							{"song_id", sid},
							{"difficulty", (int) difficulty},
							{"score", rd.GetInt32(3)},
							{"shiny_perfect_count", rd.GetInt32(4)},
							{"perfect_count", rd.GetInt32(5)},
							{"near_count", rd.GetInt32(6)},
							{"miss_count", rd.GetInt32(7)},
							{"health", rd.GetInt32(8)},
							{"modifier", rd.GetInt32(9)},
							{"time_played", rd.GetInt64(10)},
							{"best_clear_type", rd.GetInt32(11)},
							{"clear_type", rd.GetInt32(12)},
							{"rating", rd.GetDecimal(13)}
						};
						rd.Close();
						conn2.Close();
						conn.Close();
						var playerInfo = PlayerInfo(user);
						r.Add("account_info", playerInfo.GetValue("account_info"));
						r.Add("record", record);
						if (withsonginfo)
						{
							var totalSongInfo = new JArray();
							var queriedSongInfo = SongInfo(record.Value<string>("song_id"));
							totalSongInfo.Add(queriedSongInfo);
							r.Add("songinfo", totalSongInfo);
						}
						if (withrecent)
						{
							var recentScore = playerInfo.GetValue("recent_score");
							r.Add("recent_score", recentScore);
							if (withsonginfo)
							{
								var queriedSongInfo = SongInfo(recentScore.Value<string>("song_id"));
								r.Add("recent_songinfo", queriedSongInfo);
							}
						}
						return r;
					}
					else
					{
						rd.Close();
						conn.Close();
						throw new BotAPIException(BotAPIException.APIExceptionType.PlayerNotPlayedThisDiff, null);
					}
				}
				else
				{
					throw new BotAPIException(BotAPIException.APIExceptionType.PlayerIsBlocked, null);
				}
			}
			catch (BotAPIException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				throw new BotAPIException(BotAPIException.APIExceptionType.Others, null);
			}
		}

		/// <summary>
		/// 获取玩家的最近游玩成绩数据。
		/// </summary>
		/// <param name="user">玩家的好友id或昵称。</param>
		/// <returns>包含玩家基本个人信息及玩家最近游玩成绩数据的 <see cref="JObject"/> 类实例。</returns>
		/// <exception cref="BotAPIException" />
		public static JObject QueryPlayerRecentScore(string user, bool withsonginfo)
		{
			try
			{
				if (int.TryParse(user, out _) && user.Length != 9) throw new BotAPIException(BotAPIException.APIExceptionType.PlayerNotExist, null);
				var p = new PlayerInfo2(user, out bool isExists);
				if (!isExists)
				{
					p = PlayerInfo2.CreateFromUsername(user);
					if (p == null)
					{
						throw new BotAPIException(BotAPIException.APIExceptionType.PlayerNotExist, null);
					}
				}
				if (!p.Banned!.Value)
				{
					if (p.RecentScore != null)
					{
						var r = new JObject();
						var accountInfo = PlayerInfo(user).GetValue("account_info");
						var recentScore = new JArray()
						{
							p.RecentScore!
						};
						r.Add("account_info", accountInfo);
						r.Add("recent_score", recentScore);
						using var conn = new MySqlConnection(DatabaseConnectURL);
						conn.Open();
						var cmd = conn.CreateCommand();
						cmd.CommandText = $"SELECT character_id FROM users WHERE user_id={p.UserId!.Value};";
						conn.Close();
						if (withsonginfo)
						{
							var totalSongInfo = new JArray();
							foreach (var score in recentScore)
							{
								var queriedSongInfo = SongInfo(score.Value<string>("song_id"));
								totalSongInfo.Add(queriedSongInfo);
							}
							r.Add("songinfo", totalSongInfo);
						}
						return r;
					}
					else
					{
						throw new BotAPIException(BotAPIException.APIExceptionType.RecentScoreIsEmpty, null);
					}
				}
				else
				{
					throw new BotAPIException(BotAPIException.APIExceptionType.PlayerIsBlocked, null);
				}
			}
			catch (BotAPIException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				throw new BotAPIException(BotAPIException.APIExceptionType.Others, null);
			}
		}

		/// <summary>
		/// 获取玩家的Best30数据。
		/// </summary>
		/// <param name="user">玩家的好友id或昵称。</param>
		/// <returns>包含玩家基本个人信息及Best30相关数据的 <see cref="JObject"/> 类实例。</returns>
		/// <exception cref="BotAPIException" />
		public static JObject QueryPlayerBest30(string user, bool withsonginfo, bool withrecent)
		{
			try
			{
				if (int.TryParse(user, out _) && user.Length != 9) throw new BotAPIException(BotAPIException.APIExceptionType.PlayerNotExist, null);
				var p = new PlayerInfo2(user, out bool isExists);
				if (!isExists)
				{
					p = PlayerInfo2.CreateFromUsername(user);
					if (p == null)
					{
						throw new BotAPIException(BotAPIException.APIExceptionType.PlayerNotExist, null);
					}
				}
				if (!p.Banned!.Value)
				{
					if (p.RecentScore != null)
					{
						var r = new JObject();
						var r_b30 = new JArray();
						var playerInfo = PlayerInfo(user);
						using var conn = new MySqlConnection(DatabaseConnectURL);
						conn.Open();
						var cmd = conn.CreateCommand();
						cmd.CommandText = $"SELECT song_id,difficulty,score,rating FROM bests WHERE user_id={p.UserId!.Value} AND rating > 0 ORDER BY rating DESC, score DESC LIMIT 30;";
						var rd = cmd.ExecuteReader();
						var Best30 = new Dictionary<KeyValuePair<string, int>, KeyValuePair<int, decimal>>();
						// Best30数据格式:
						// Key: KeyValuePair<曲名,难度id>
						// Value: KeyValuePair<分数,单曲潜力值>
						while (rd.Read())
						{
							Best30.Add(new KeyValuePair<string, int>(rd.GetString(0), rd.GetInt32(1)), new KeyValuePair<int, decimal>(rd.GetInt32(2), rd.GetDecimal(3)));
						}
						rd.Close();
						conn.Close();
						Best30 = Best30.OrderByDescending(singleBest => singleBest.Value.Value).ToDictionary(singleBest => singleBest.Key, singleBest => singleBest.Value);
						// 按照Best30->Value[曲目成绩信息]->Value[单曲潜力值]倒序排序Best30数据
						decimal b30_avg = 0M;
						int index = 0;
						foreach (var singleBest in Best30)
						{
							b30_avg += singleBest.Value.Value;
							var data = SingleScore.GetBestScoreJson(p.UserId!.Value, singleBest.Key.Key, (SongDifficulty) singleBest.Key.Value, "bests", out _);
							data.Remove("name");
							data.Remove("user_id");
							data.Remove("character");
							data.Remove("is_skill_sealed");
							data.Remove("is_char_uncapped");
							data.Add("rating", singleBest.Value.Value);
							r_b30.Add(data);
							index++;
						}
						// 计算Best30中所有单曲潜力值的平均值
						b30_avg /= 30;
						// 添加数据到母Object
						r.Add("best30_avg", b30_avg);
						r.Add("account_info", playerInfo.GetValue("account_info"));
						r.Add("best30_list", r_b30);
						if (withsonginfo)
						{
							var totalSongInfo = new JArray();
							foreach (var name in r_b30)
							{
								var queriedSongInfo = SongInfo(name.Value<string>("song_id"));
								totalSongInfo.Add(queriedSongInfo);
							}
							r.Add("best30_songinfo", totalSongInfo);
						}
						if (withrecent)
						{
							var recentScore = playerInfo.GetValue("recent_score");
							r.Add("recent_score", recentScore);
							if (withsonginfo)
							{
								var queriedSongInfo = SongInfo(recentScore.Value<string>("song_id"));
								r.Add("recent_songinfo", queriedSongInfo);
							}
						}
						return r;
					}
					else
					{
						throw new BotAPIException(BotAPIException.APIExceptionType.RecentScoreIsEmpty, null);
					}
				}
				else
				{
					throw new BotAPIException(BotAPIException.APIExceptionType.PlayerIsBlocked, null);
				}
			}
			catch (BotAPIException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				throw new BotAPIException(BotAPIException.APIExceptionType.Others, null);
			}
		}

		/// <summary>
		/// 查询玩家信息。
		/// </summary>
		/// <param name="userNameOrCode">玩家的9位好友id。</param>
		/// <returns>包含玩家完整的个人信息的 <see cref="JObject"/> 类实例。</returns>
		/// <exception cref="BotAPIException" />
		public static JObject PlayerInfo(string userNameOrCode)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				if (int.TryParse(userNameOrCode, out _) && userNameOrCode.Length != 9)
				{
					throw new BotAPIException(BotAPIException.APIExceptionType.PlayerNotExist, null);
				}
				JObject r;
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = "SELECT * FROM users WHERE user_code=?user OR name=?user";
				cmd.Parameters.Add(new MySqlParameter("?user", MySqlDbType.VarChar)
				{
					Value = userNameOrCode
				});
				var rd = cmd.ExecuteReader();
				if (rd.HasRows)
				{
					rd.Read();
					if (!rd.GetBoolean(31))
					{
						int userid = rd.GetInt32(0);
						r = new JObject();
						var accountInfo = new JObject()
						{
							{"code", rd.GetString(1)},
							{"name", rd.GetString(2)},
							{"user_id", userid},
							{"is_mutual", false},
							{"is_char_uncapped_override", rd.GetBoolean(9)},
							{"is_char_uncapped", rd.GetBoolean(8)},
							{"is_skill_sealed", rd.GetBoolean(7)},
							{"rating", rd.GetBoolean(10) ? -1 : rd.GetInt32(5)}, //id=10:是否隐藏个人潜力值
							{"join_date", rd.GetInt64(4)},
							{"character", rd.GetInt32(6)},

						};
						r.Add("account_info", accountInfo);
						rd.Close();
						cmd = conn.CreateCommand();
						cmd.CommandText = $"SELECT COUNT(song_id),song_id,difficulty,score,shiny_perfect_count,perfect_count,near_count,miss_count,health,modifier,time_played,clear_type,rating FROM users WHERE user_id={userid};";
						rd = cmd.ExecuteReader();
						rd.Read();
						JObject recentScoreJObj;
						if (rd.GetInt32(0) == 1) //如果存在最近成绩
						{
							var recentScore = new SingleScore(rd.GetString(1), (SongDifficulty)rd.GetInt32(2),
								(uint)rd.GetInt32(3), (ClearType)rd.GetInt32(11), (uint)rd.GetInt32(4),
								(uint)rd.GetInt32(5), (uint)rd.GetInt32(6), (uint)rd.GetInt32(7),
								(ulong)rd.GetInt64(10) * 1000);
							int health = rd.GetInt32(8);
							decimal rating = rd.GetDecimal(12);
							var bestClearType = recentScore.ClearType!.Value;
							rd.Close();
							cmd.CommandText = $"SELECT best_clear_type FROM bests WHERE user_id={userid} AND song_id='{recentScore.SongId}' AND difficulty={(int) recentScore.Difficulty} ";
							var rd2 = cmd.ExecuteReader();
							if (rd2.Read())
							{
								bestClearType = (ClearType)rd2.GetInt32(0);
								rd2.Close();
							}
							recentScoreJObj = new JObject()
							{
								{"song_id", recentScore.SongId},
								{"difficulty", (int)recentScore.Difficulty},
								{"score", recentScore.Score!.Value},
								{"shiny_perfect_count", recentScore.BigPureCount},
								{"perfect_count", recentScore.PureCount},
								{"near_count", recentScore.FarCount},
								{"miss_count", recentScore.LostCount},
								{"best_clear_type", (int)bestClearType},
								{"clear_type", (int)recentScore.ClearType},
								{"health", health},
								{"time_played", (long)Math.Floor((recentScore.PlayDate!.Value - new DateTime(1970, 1, 1)).TotalSeconds)},
								{"rating", rating}
							};
							r.Add("recent_score", recentScoreJObj);
						}
						else
						{
							rd.Close();
						}
						return r;
					}
					else
					{
						rd.Close();
						throw new BotAPIException(BotAPIException.APIExceptionType.PlayerIsBlocked, null);
					}
				}
				else
				{
					rd.Close();
					throw new BotAPIException(BotAPIException.APIExceptionType.PlayerNotExist, null);
				}
			}
			catch (BotAPIException)
			{
				throw;
			}
			catch (Exception)
			{
				throw new BotAPIException(BotAPIException.APIExceptionType.Others, null);
			}
			finally
			{
				conn.Close();
			}
		}

		/// <summary>
		/// 查询曲目信息。
		/// </summary>
		/// <param name="songid">曲目的id。</param>
		/// <returns>包含曲目完整信息的 <see cref="JObject"/> 类实例。</returns>
		/// <exception cref="BotAPIException" />
		public static JObject SongInfo(string songid)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = "SELECT * FROM fixed_songs WHERE sid=?sid OR (sid LIKE CONCAT('%',?sid,'%')) OR (name_en LIKE CONCAT('%',?sid,'%')) OR (name_jp LIKE CONCAT('%',?sid,'%'));";
				cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
				{
					Value = songid
				});
				var rd = cmd.ExecuteReader();
				if (rd.Read())
				{
					var r = new JObject
					{
						{"id", rd.GetString("sid")},
					};
					var title_localized = new JObject()
					{
						{"en", rd.GetString("name_en")}
					};
					if (!rd.IsDBNull(2) && !string.IsNullOrWhiteSpace(rd.GetString("name_jp")))
					{
						title_localized.Add("ja", rd.GetString("name_jp"));
					}
					r.Add("title_localized", title_localized);
					r.Add("artist", rd.GetString("artist"));
					r.Add("bpm", rd.GetString("bpm"));
					r.Add("bpm_base", rd.GetDecimal("bpm_base"));
					string pakset = rd.GetString("pakset");
					r.Add("set", pakset);
					r.Add("world_unlock", rd.GetBoolean("world_unlock"));
					r.Add("remote_dl", rd.GetBoolean("remote_download"));
					r.Add("side", rd.GetInt32("side"));
					r.Add("bg", rd.GetString("bg"));
					r.Add("time", rd.GetInt32("time"));
					r.Add("date", rd.GetInt32("date"));
					r.Add("version", rd.GetString("version"));
					var difficulties = new JArray();
					var diff_pst = new JObject()
					{
						{"ratingClass", 0}
					};
					var diff_prs = new JObject()
					{
						{"ratingClass", 1}
					};
					var diff_ftr = new JObject()
					{
						{"ratingClass", 2}
					};
					JObject? diff_byd = null;
					if (rd.GetInt32("difficulty_pst") != -1)
					{
						if (!rd.IsDBNull(20) && !string.IsNullOrEmpty(rd.GetString("chart_designer_pst")))
						{
							diff_pst.Add("chartDesigner", rd.GetString("chart_designer_pst").Replace("\\n", "\n"));
						}
						else
						{
							diff_pst.Add("chartDesigner", string.Empty);
						}
						if (!rd.IsDBNull(24) && !string.IsNullOrEmpty(rd.GetString("jacket_designer_pst")))
						{
							diff_pst.Add("jacketDesigner", rd.GetString("jacket_designer_pst").Replace("\\n", "\n"));
						}
						else
						{
							diff_pst.Add("jacketDesigner", string.Empty);
						}
						diff_pst.Add("jacketOverride", false);
						diff_pst.Add("realrating",rd.GetInt32("rating_pst"));
					}
					else
					{
						diff_pst = new JObject()
						{
							{ "ratingClass", 0 },
							{ "jacketDesigner", string.Empty },
							{ "chartDesigner", string.Empty },
							{ "jacketOverride", false },
							{ "realrating", -1 }
						};
					}
					if (rd.GetInt32("difficulty_prs") != -1)
					{
						if (!rd.IsDBNull(21) && !string.IsNullOrEmpty(rd.GetString("chart_designer_prs")))
						{
							diff_prs.Add("chartDesigner", rd.GetString("chart_designer_prs").Replace("\\n", "\n"));
						}
						else
						{
							diff_prs.Add("chartDesigner", string.Empty);
						}
						if (!rd.IsDBNull(25) && !string.IsNullOrEmpty(rd.GetString("jacket_designer_prs")))
						{
							diff_prs.Add("jacketDesigner", rd.GetString("jacket_designer_prs").Replace("\\n", "\n"));
						}
						else
						{
							diff_prs.Add("jacketDesigner", string.Empty);
						}
						diff_prs.Add("jacketOverride", false);
						diff_prs.Add("realrating",rd.GetInt32("rating_prs"));
					}
					else
					{
						diff_prs = new JObject()
						{
							{ "ratingClass", 1 },
							{ "jacketDesigner", string.Empty },
							{ "chartDesigner", string.Empty },
							{ "jacketOverride", false },
							{ "realrating", -1 }
						};
					}
					if (rd.GetInt32("difficulty_ftr") != -1)
					{
						if (!rd.IsDBNull(22) && !string.IsNullOrEmpty(rd.GetString("chart_designer_ftr")))
						{
							diff_ftr.Add("chartDesigner", rd.GetString("chart_designer_ftr").Replace("\\n", "\n"));
						}
						else
						{
							diff_ftr.Add("chartDesigner", string.Empty);
						}
						if (!rd.IsDBNull(26) && !string.IsNullOrEmpty(rd.GetString("jacket_designer_ftr")))
						{
							diff_ftr.Add("jacketDesigner", rd.GetString("jacket_designer_ftr").Replace("\\n", "\n"));
						}
						else
						{
							diff_ftr.Add("jacketDesigner", string.Empty);
						}
						diff_ftr.Add("jacketOverride", false);
						diff_ftr.Add("realrating", rd.GetInt32("rating_ftr"));
					}
					else
					{
						diff_ftr = new JObject()
						{
							{ "ratingClass", 2 },
							{ "jacketDesigner", string.Empty },
							{ "chartDesigner", string.Empty },
							{ "jacketOverride", false },
							{ "realrating", -1 }
						};
					}
					if (rd.GetInt32("difficulty_byd") != -1)
					{
						diff_byd = new JObject()
						{
							{"ratingClass", 3}
						};
						if (!rd.IsDBNull(23) && !string.IsNullOrEmpty(rd.GetString("chart_designer_byd")))
						{
							diff_byd.Add("chartDesigner", rd.GetString("chart_designer_byd").Replace("\\n", "\n"));
						}
						else
						{
							diff_byd.Add("chartDesigner", string.Empty);
						}
						if (!rd.IsDBNull(27) && !string.IsNullOrEmpty(rd.GetString("jacket_designer_byd")))
						{
							diff_byd.Add("jacketDesigner", rd.GetString("jacket_designer_byd").Replace("\\n", "\n"));
						}
						else
						{
							diff_byd.Add("jacketDesigner", string.Empty);
						}
						diff_byd.Add("jacketOvrride", false);
						diff_byd.Add("realrating", rd.GetInt32("rating_byd"));
					}
					difficulties.Add(diff_pst);
					difficulties.Add(diff_prs);
					difficulties.Add(diff_ftr);
					if (diff_byd != null)
					{
						difficulties.Add(diff_byd);
					}
					r.Add("difficulties", difficulties);
					rd.Close();
					return r;
				}
				else
				{
					rd.Close();
					throw new BotAPIException(BotAPIException.APIExceptionType.SongIsNotExist, null);
				}
			}
			catch (BotAPIException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				throw new BotAPIException(BotAPIException.APIExceptionType.Others, null);
			}
			finally
			{
				conn.Close();
			}
		}
	}
}