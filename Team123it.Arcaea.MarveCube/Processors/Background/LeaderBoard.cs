using System;
using static Team123it.Arcaea.MarveCube.GlobalProperties;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Team123it.Arcaea.MarveCube.Core;

namespace Team123it.Arcaea.MarveCube.Processors.Background
{
	/// <summary>
	/// 提供排行榜相关的 <see langword="static" /> 方法的类。无法继承此类。
	/// </summary>
	public static class LeaderBoard
	{
		/// <summary>
		/// 获取指定曲目及难度的排行榜数据。
		/// </summary>
		/// <param name="songid">曲目id。</param>
		/// <param name="difficulty">曲目难度。</param>
		/// <param name="type">要获取的排行榜类型。</param>
		/// <param name="userid">玩家的用户id,仅在排行榜类型为 <see cref="LeaderBoardType.Friend"/> 或 <see cref="LeaderBoardType.MyRank"/> 时可用(若排行榜类型为其它类型则可将该参数设置为 <see langword="null" /> 。 </param>
		/// <param name="limit">获取的成绩数量。若该参数值为 <see langword="null" /> 系统将以limit=20获取成绩。</param>
		/// <param name="isLeaderBoardExists">在当前方法返回时,若指定曲目及难度的排行榜存在(指定曲目及难度有效)且数据获取成功完成则本参数值为 <see langword="true" /> ,否则为 <see langword="false"/> 。</param>
		/// <returns>成功返回对应排行榜数据的 <see cref="JArray"/> 实例, 失败返回 <see cref="JArray"/> 的默认空实例。</returns>
		public static JArray GetSongLeaderBoard(string songid,SongDifficulty difficulty,LeaderBoardType type,uint? userid,uint? limit,out bool isLeaderBoardExists)
		{
			if (!limit.HasValue) limit = 20;
			var r = new JArray();
			try
			{
				if (difficulty != SongDifficulty.Beyond && difficulty != SongDifficulty.Future && difficulty != SongDifficulty.Present
					&& difficulty != SongDifficulty.Past)
				{
					isLeaderBoardExists = false;
					return r;
				} else
				{
					using var conn = new MySqlConnection(DatabaseConnectURL);
					conn.Open();
					var cmd = conn.CreateCommand();
					#region "难度/特殊排行榜曲目判断"
					switch (difficulty)
					{
						case SongDifficulty.Past: //Past
							cmd.CommandText = $"SELECT COUNT(*),rating_pst,pakset FROM fixed_songs WHERE sid=?sid;";
							break;
						case SongDifficulty.Present: //Present
							cmd.CommandText = $"SELECT COUNT(*),rating_prs,pakset FROM fixed_songs WHERE sid=?sid;";
							break;
						case SongDifficulty.Future: //Future
							cmd.CommandText = $"SELECT COUNT(*),rating_ftr,pakset FROM fixed_songs WHERE sid=?sid;";
							break;
						case SongDifficulty.Beyond: //Beyond
							cmd.CommandText = $"SELECT COUNT(*),rating_byd,pakset FROM fixed_songs WHERE sid=?sid;";
							break;
					}
					cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
					{
						Value = songid
					});
					#endregion
					var rd = cmd.ExecuteReader();
					rd.Read();
					if (rd.GetInt32(0) == 1 && rd.GetInt32(1) != -1) //如果存在对应曲目和对应难度
					{
						string tableName;
						switch (rd.GetString(2).Trim().ToLower())
						{
							case "unranked":
								tableName = "bests_special";
								break;
							default:
								tableName = "bests";
								break;
						}
						rd.Close();
						conn.Close();
						using var conn2 = new MySqlConnection(DatabaseConnectURL);
						conn2.Open();
						cmd = conn2.CreateCommand();
						switch (type)
						{
							case LeaderBoardType.World: //世界排行
								#region "世界排行"
								cmd.CommandText = $"SELECT user_id,score,time_played FROM {tableName} WHERE song_id=?sid AND difficulty={(int)difficulty} ORDER BY score DESC, time_played DESC LIMIT {limit}";
								rd = cmd.ExecuteReader();
								long rank = 0;
								while (rd.Read()) //遍历同一曲目同一难度的所有最佳成绩
								{
									rank++; //排名+1
									var rankScoreDetails = SingleScore.GetBestScoreJson((uint)rd.GetInt32(0), songid, difficulty,tableName,out _);
									// 获取成绩详情
									rankScoreDetails.Add("rank", rank);
									r.Add(rankScoreDetails);
								}
								rd.Close();
								conn2.Close();
								isLeaderBoardExists = true;
								return r;
								#endregion
							case LeaderBoardType.MyRank: //"我的"世界排行排名
								#region ""我的"世界排行排名"
								if (!userid.HasValue)
								{
									conn2.Close();
									isLeaderBoardExists = false;
									return r;
								}
								else
								{
									cmd.CommandText = $"SELECT COUNT(*) FROM {tableName} WHERE user_id={userid} AND song_id=?sid and difficulty={(int)difficulty}";
									if ((long)cmd.ExecuteScalar() == 1) //如果存在当前玩家对应曲目的最佳成绩
									{
										cmd.CommandText = $"SELECT COUNT(*) FROM {tableName} WHERE song_id=?sid AND difficulty={(int)difficulty} " +
											$"AND (score>(SELECT score FROM {tableName} WHERE user_id={userid} AND song_id=?sid AND difficulty={(int)difficulty}) " +
											$"AND time_played > (SELECT time_played FROM {tableName} WHERE user_id={userid} and song_id=?sid AND difficulty={(int)difficulty})" +
											$")";
										rd = cmd.ExecuteReader();
										rd.Read();
										long myRank = rd.GetInt64(0) + 1L; //所有高于当前玩家的其他玩家的最好成绩数量+1
										if (myRank <= 4) //排名前4
										{
											rd.Close();
											#region "世界模式排行"
											cmd.CommandText = $"SELECT user_id FROM {tableName} WHERE song_id=?sid AND difficulty={(int)difficulty} ORDER BY score DESC, time_played DESC LIMIT {limit.Value}";
											rd = cmd.ExecuteReader();
											long rank1 = 0;
											while (rd.Read()) //遍历同一曲目同一难度的所有最佳成绩
											{
												rank1++; //排名+1
												var rankScoreDetails = SingleScore.GetBestScoreJson((uint)rd.GetInt32(0), songid, difficulty,tableName,out _);
												// 获取成绩详情
												rankScoreDetails.Add("rank", rank1);
												r.Add(rankScoreDetails);
											}
											rd.Close();
											conn2.Close();
											isLeaderBoardExists = true;
											return r;
											#endregion
										} else if (myRank >= 5 && myRank <= 9983) //排名5-9983
										{
											rd.Close();
											cmd.CommandText = $"SELECT user_id,score,time_played FROM {tableName} WHERE song_id=?sid AND difficulty={(int)difficulty} ORDER BY score DESC,time_played DESC limit {limit.Value} offset {myRank - 5L};";
											rd = cmd.ExecuteReader();
											long rank1 = myRank - 5;
											while (rd.Read())
											{
												rank1++;
												var rankScoreDetails = SingleScore.GetBestScoreJson((uint)rd.GetInt32(0), songid, difficulty,tableName,out _);
												rankScoreDetails.Add("rank", rank1);
												r.Add(rankScoreDetails);
											}
											rd.Close();
											conn2.Close();
											isLeaderBoardExists = true;
											return r;
										} else if (myRank >= 9984 && myRank <= 9999) //排名9984-9999
										{
											rd.Close();
											cmd.CommandText = $"SELECT user_id FROM {tableName} WHERE song_id=?sid AND difficulty={(int)difficulty} ORDER BY score DESC,time_played DESC limit {limit.Value} offset {9998 - limit.Value};";
											rd = cmd.ExecuteReader();
											long rank1 = 9998 - limit.Value;
											while(rd.Read())
											{
												rank1++;
												var rankScoreDetails = SingleScore.GetBestScoreJson((uint)rd.GetInt32(0), songid, difficulty,tableName,out _);
												rankScoreDetails.Add("rank", rank1);
												r.Add(rankScoreDetails);
											}
											rd.Close();
											conn2.Close();
											isLeaderBoardExists = true;
											return r;
										} else //排名1w+
										{
											rd.Close();
											cmd.CommandText = $"SELECT user_id FROM {tableName} WHERE song_id=?sid AND difficulty={(int)difficulty} ORDER BY score DESC,time_played DESC limit {limit.Value} offset {9999 - limit.Value};";
											rd = cmd.ExecuteReader();
											long rank1 = 9999 - limit.Value;
											while (rd.Read())
											{
												rank1++;
												var rankScoreDetails = SingleScore.GetBestScoreJson((uint)rd.GetInt32(0), songid, difficulty,tableName,out _);
												rankScoreDetails.Add("rank", rank1);
												r.Add(rankScoreDetails);
											}
											rd.Close();
											conn2.Close();
											var myRankScoreDetails = SingleScore.GetBestScoreJson(userid.Value, songid, difficulty,tableName,out _);
											myRankScoreDetails.Add("rank", -1);
											r.Add(myRankScoreDetails);
											isLeaderBoardExists = true;
											return r;
										}
									}
									else //否则
									{
										conn2.Close();
										isLeaderBoardExists = false;
										return r;
									}
								}
								#endregion
							case LeaderBoardType.Friend: //好友排行
								#region "好友排行"
								if (!userid.HasValue)
								{
									conn2.Close();
									isLeaderBoardExists = false;
									return r;
								}
								else
								{
									cmd.CommandText = $"SELECT user_id,score,time_played FROM {tableName} WHERE user_id IN " +
										$"(SELECT {userid} UNION SELECT user_id_other FROM friend WHERE user_id_me={userid}) " +
										$"AND song_id=?sid AND difficulty={(int)difficulty} " +
										$"ORDER BY score DESC, time_played DESC limit {limit}";
									var rd1 = cmd.ExecuteReader();
									long rank2 = 0;
									while (rd1.Read())
									{
										rank2++;
										var friendSingleRankScore = SingleScore.GetBestScoreJson((uint)rd1.GetInt32(0), songid, difficulty,tableName,out _);
										friendSingleRankScore.Add("rank", rank2);
										r.Add(friendSingleRankScore);
									}
									rd1.Close();
									conn2.Close();
									isLeaderBoardExists = true;
									return r;
								}
							#endregion
							default: //占位用
								conn2.Close();
								isLeaderBoardExists = false;
								return r;
						}
					}
					else
					{
						rd.Close();
						conn.Close();
						isLeaderBoardExists = false;
						return r;
					}
				}
			}
			catch
			{
				isLeaderBoardExists = false;
				return r;
			}
		}

		/// <summary>
		/// 获取指定范围内的总分数全服(Top)排名序号。
		/// </summary>
		/// <param name="userid">玩家的用户id。</param>
		/// <param name="topLimit">
		/// 要获取的Top范围(默认为Top50,即仅前50会返回实际排名,超过后返回 <see langword="null" /> )。<br />
		/// 若该值超过Lowiro官方的限制(Top200)则会抛出异常。</param>
		/// <returns>
		/// 玩家的排名。<br />
		/// 若玩家不在 <paramref name="topLimit"/> 所表示的Top范围内则返回 <see langword="null" /> 。
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException" />
		/// <exception cref="ArcaeaAPIException" />
		public static int? GetTotalScoreRank(uint userid, int topLimit = 50)
		{
			if (topLimit > 200)
			{
				throw new ArgumentOutOfRangeException(nameof(topLimit), $"Out of range. (Max Allow Value: 200, Current: {topLimit}");
			}
			using var conn = new MySqlConnection(DatabaseConnectURL);
			int? r = null;
			try
			{
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = "SELECT user_id, totalScore FROM users WHERE is_banned = 0 ORDER BY totalScore DESC;";
				var rd = cmd.ExecuteReader();
				long lastTotalScore = 0;
				int rank = 1;
				while (rd.Read())
				{
					if (rank <= topLimit) // 如果在topLimit范围之内
					{
						if (rd.GetInt32("user_id") == userid) break; // 找到你了, 玩家！(退出循环)
						if (lastTotalScore == rd.GetInt64("totalScore")) continue; // 上一个与当前的分数相同则排名也相同
						lastTotalScore = rd.GetInt64("totalScore"); // 更新"上一个"分数
						rank++;
					}
					else // 超过范围了！！
					{
						rank = -1;
						break;
					}
				}
				rd.Close();
				r = (rank != -1) ? rank : null; // 如果rank不是-1就返回实际排名 否则返回null(这里仅赋值并没有返回,返回在函数末尾)
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
			return r; // 实际在这里返回r
		}

		/// <summary>
		/// 获取指定范围内的总分数全服(Top)排名序号。
		/// </summary>
		/// <param name="userid">玩家的用户id。</param>
		/// <param name="topLimit">
		/// 要获取的Top范围(默认为Top50,即仅前50会返回实际排名,超过后返回 <see langword="null" /> )。<br />
		/// 若该值超过Lowiro官方的限制(Top200)则会抛出异常。</param>
		/// <param name="r">在当前方法返回时，若获取成功则值为玩家的排名，否则为 <see langword="null" /> 。此参数未经初始化即经传递。</param>
		/// <returns>
		/// 获取结果。<br />
		/// (失败=玩家不在 <paramref name="topLimit"/> 所表示的Top范围内)
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException" />
		/// <exception cref="ArcaeaAPIException" />
		public static bool TryGetTotalScoreRank(uint userid, out int? r, int topLimit = 50)
		{
			r = GetTotalScoreRank(userid, topLimit);
			if (r.HasValue)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool UpdateTotalScoreRank(this PlayerInfo info)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"UPDATE users SET totalScore = (IFNULL((SELECT SUM(score) FROM bests WHERE (difficulty = 2 OR difficulty = 3) AND user_id = ?uid),0)) WHERE user_id=?uid;";
				cmd.Parameters.Add(new MySqlParameter("?user_id", MySqlDbType.Int32)
				{
					Value = info.UserId!.Value
				});
				cmd.ExecuteNonQuery();
				return true;
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return false;
			}
			finally
			{
				conn.Close();
			}
		}

		/// <summary>
		/// 表示排行榜的类型。
		/// </summary>
		public enum LeaderBoardType
		{
			/// <summary>
			/// 世界排行。
			/// </summary>
			World = 0,
			/// <summary>
			/// "我的"世界排行排名。
			/// </summary>
			MyRank = 1,
			/// <summary>
			/// 好友排行。
			/// </summary>
			Friend = 2
		}
	}
}
