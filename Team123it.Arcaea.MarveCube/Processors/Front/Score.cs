using static Team123it.Arcaea.MarveCube.GlobalProperties;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;

using System.IO;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Processors.Background;
using World2 = Team123it.Arcaea.MarveCube.Processors.Background.World;
using static Team123it.Arcaea.MarveCube.Processors.Background.LeaderBoard;
using System.Text;
using System.Security.Cryptography;

namespace Team123it.Arcaea.MarveCube.Processors.Front
{
	/// <summary>
	/// 分数提交管理相关API。<br />
	/// 对应API前缀:/years/19/score/
	/// </summary>
	public static class Score
	{
		/// <summary>
		/// [API]提交分数。
		/// </summary>
		/// <param name="userid">玩家的用户id。</param>
		/// <param name="score">分数信息。</param>
		/// <param name="health">[扩展分数信息]游玩结束时的回忆度。</param>
		/// <param name="beyond_gauge">分数是否为Beyond挑战分数。</param>
		/// <param name="song_hash">Arcaea 客户端的曲目谱面文件的哈希值。</param>
		/// <param name="submission_hash">分数提交的哈希值。</param>
		/// <returns>Json数据。</returns>
		/// <exception cref="ArcaeaAPIException" />
		public static JObject ScorePost(uint userid,SingleScore score,int health, bool beyond_gauge, string song_hash,string submission_hash)
		{
			// TODO: 分数验证
			var info = new PlayerInfo(userid, out _);
			if (info.Banned.Value) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountHasBeenBlocked);
			// 玩家账号已被冻结
			if (!score.SongRating.HasValue)
			{
				var beforeR = new JObject()
				{
					{ "success",true },
					{ "value", new JObject()
						{
							{"user_rating",info.PotentialInt!.Value }
						}
					}
				};
				if (TryGetTotalScoreRank(info.UserId!.Value, out int? rank, TopRankLimit))
				{
					beforeR.Add("global_rank", rank!.Value);
				}
				return beforeR;
			}
			// 如果曲目信息不存在返回潜力值不变(KEEP)的数据
			var r = new JObject();
			#region "主提交模块"
			using var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = conn.CreateCommand();
			using var conn2 = new MySqlConnection(DatabaseConnectURL);
			conn2.Open();
			var cmd2 = conn2.CreateCommand();
			long rating = 0L;
			switch (score.Difficulty) //判断是否存在对应难度的定数
			{
				case SongDifficulty.Past: //若为Past
					cmd2.CommandText = $"SELECT rating_pst FROM fixed_songs WHERE sid='{score.SongId}';";
					rating = (int)cmd2.ExecuteScalar();
					conn2.Close();
					if (rating == -1) //难度不存在
					{
						SecurityManager.EditPlayerCreditPoint(userid, -1, $"Attempted to submit inexist difficulty score: {score.SongId}(Past)");
						throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountBlockWarning);
					}
					break;
				case SongDifficulty.Present: //若为Present
					cmd2.CommandText = $"SELECT rating_prs FROM fixed_songs WHERE sid='{score.SongId}';";
					rating = (int)cmd2.ExecuteScalar();
					conn2.Close();
					if (rating == -1) //难度不存在
					{
						SecurityManager.EditPlayerCreditPoint(userid, -1, $"Attempted to submit inexist difficulty score: {score.SongId}(Present)");
						throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountBlockWarning);
					}
					break;
				case SongDifficulty.Future: //若为Future
					cmd2.CommandText = $"SELECT rating_ftr FROM fixed_songs WHERE sid='{score.SongId}';";
					rating = (int)cmd2.ExecuteScalar();
					conn2.Close();
					if (rating == -1) //难度不存在
					{
						SecurityManager.EditPlayerCreditPoint(userid, -1, $"Attempted to submit inexist difficulty score: {score.SongId}(Future)");
						throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountBlockWarning);
					}
					break;
				case SongDifficulty.Beyond: //若为Beyond
					cmd2.CommandText = $"SELECT rating_byd FROM fixed_songs WHERE sid='{score.SongId}';";
					rating = (int)cmd2.ExecuteScalar();
					conn2.Close();
					if (rating == -1) //难度不存在
					{
						SecurityManager.EditPlayerCreditPoint(userid, -1, $"Attempted to submit inexist difficulty score: {score.SongId}(Beyond)");
						throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountBlockWarning);
					}
					break;
				default:
					conn2.Close();
					throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountBlockWarning);
			}
			DoScoreExtraChallenge(userid, score, song_hash, submission_hash);
			long playTimeStamp = (long)(score.PlayDate.Value - new DateTime(1970, 1, 1)).TotalSeconds;
			var s = score;
			int rewardMemories = Player.SetRewardMemories(s.Score!.Value, s.ClearType!.Value);
			cmd.CommandText = $"SELECT ticket FROM users WHERE user_id={userid}";
			int currentMemories = Convert.ToInt32(cmd.ExecuteScalar());
			currentMemories += rewardMemories;
			if (currentMemories > ushort.MaxValue)
			{
				currentMemories = ushort.MaxValue;
			}
			cmd.CommandText = $"UPDATE users SET song_id='{s.SongId}', " +
				$"difficulty={(int)s.Difficulty}, " +
				$"score={s.Score}, " +
				$"shiny_perfect_count={s.BigPureCount}, " +
				$"perfect_count={s.PureCount}, " +
				$"near_count={s.FarCount}, " +
				$"miss_count={s.LostCount}, " +
				$"health={health}, " +
				$"modifier=0, " +
				$"time_played={playTimeStamp}, " +
				$"clear_type={(int)s.ClearType}, " +
				$"rating={s.ScoreRating}, " +
				$"ticket={currentMemories} " +
				$"WHERE user_id={userid}";
			cmd.ExecuteNonQuery();
			cmd.CommandText = $"SELECT pakset FROM fixed_songs WHERE sid=?sid;";
			cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
			{
				Value = s.SongId
			});
			string pakset = (string)cmd.ExecuteScalar();
			string tableName = pakset.Trim().ToLower() switch
			{
				"unranked" => "bests_special",
				_ => "bests",
			};
			cmd.CommandText = $"SELECT COUNT(*),score,best_clear_type FROM {tableName} WHERE user_id={userid} AND song_id='{s.SongId}' AND difficulty={(int)s.Difficulty};";
			var rd = cmd.ExecuteReader();
			rd.Read();
			if (rd.GetInt32(0) == 1)
			{
				int historyScore = rd.GetInt32(1);
				var historyClearType = (ClearType)rd.GetInt32(2);
				rd.Close();
				bool isNewTypeHigher = false;
				bool isNewScoreHigher = false;
				if (s.ClearType.Value.CheckIsHigher(historyClearType)) //如果当前曲目完成类型高于历史最高完成类型
				{
					isNewTypeHigher = true;
					// 刷新历史最高曲目完成类型
				}
				if (s.Score.Value > historyScore) // 如果当前分数高于历史最高分数
				{
					isNewScoreHigher = true;
					// 刷新历史最高分数
				}
				if (isNewScoreHigher && isNewTypeHigher)
				{
					cmd.CommandText = $"UPDATE {tableName} SET score={s.Score.Value}," +
						$"shiny_perfect_count={s.BigPureCount.Value}," +
						$"perfect_count={s.PureCount.Value}," +
						$"near_count={s.FarCount.Value}," +
						$"miss_count={s.LostCount.Value}," +
						$"health={health}," +
						$"time_played={playTimeStamp}," +
						$"clear_type={(int)s.ClearType}," +
						$"rating={s.ScoreRating}," +
						$"best_clear_type={(int)s.ClearType}" +
						$" WHERE user_id={userid} AND song_id='{s.SongId}' AND difficulty={(int)s.Difficulty};";
					cmd.ExecuteNonQuery();
				}
				else if (isNewScoreHigher)
				{
					cmd.CommandText = $"UPDATE {tableName} SET score={s.Score.Value}," +
						$"shiny_perfect_count={s.BigPureCount.Value}," +
						$"perfect_count={s.PureCount.Value}," +
						$"near_count={s.FarCount.Value}," +
						$"miss_count={s.LostCount.Value}," +
						$"health={health}," +
						$"time_played={playTimeStamp}," +
						$"rating={s.ScoreRating}" +
						$" WHERE user_id={userid} AND song_id='{s.SongId}' AND difficulty={(int)s.Difficulty};";
					cmd.ExecuteNonQuery();
				}
				else if (isNewTypeHigher)
				{
					cmd.CommandText = $"UPDATE {tableName} SET best_clear_type={(int)s.ClearType}" +
						$" WHERE user_id={userid} AND song_id='{s.SongId}' AND difficulty={(int)s.Difficulty};";
					cmd.ExecuteNonQuery();
				}
			}
			else
			{
				rd.Close();
				cmd.CommandText = $"INSERT INTO {tableName} VALUES ({userid}," +
					$"'{s.SongId}'," +
					$"{(int)s.Difficulty}," +
					$"{s.Score}," +
					$"{s.BigPureCount.Value}," +
					$"{s.PureCount.Value}," +
					$"{s.FarCount.Value}," +
					$"{s.LostCount.Value}," +
					$"{health}," +
					$"0," +
					$"{playTimeStamp}," +
					$"{(int)s.ClearType}," +
					$"{(int)s.ClearType}," +
					$"{s.ScoreRating});";
				// 数据表 bests 列顺序:
				// 用户id,曲目id,曲目难度,分数,大Pure数,Pure数,Far数,Lost数,回忆度,0(固定0),游玩时间戳,最佳曲目完成类型,曲目完成类型,
				// 单曲潜力值(值类型:real)
				cmd.ExecuteNonQuery();
			}
			#endregion
			var newPlayerInfos = Player.RefreshPotential(userid,(int)s.Score!.Value,s.ClearType!.Value,health);
			cmd.CommandText = $"SELECT stamina_multiply,fragment_multiply,prog_boost_multiply FROM world_songplay WHERE " +
			   $"user_id={userid} AND song_id='{s.SongId}' AND difficulty={(int)s.Difficulty};";
			rd = cmd.ExecuteReader();
			if (rd.Read()) //如果玩家在游玩世界模式
			{
				bool isSpecial = tableName switch
				{
					_ => false
				};
				#region "世界(World)模式计算模块"
				int stamina_mtp = rd.GetInt32(0);
				int frag_mtp = rd.GetInt32(1);
				int prog_boost_mtp = (rd.GetInt32(2) != 0) ? 300 : 0;
				decimal climb_steps_mtp = stamina_mtp * (frag_mtp / 100) * ((prog_boost_mtp + 100) / 100); //游玩Step倍数
				decimal exp_steps_mtp = stamina_mtp * ((prog_boost_mtp + 100) / 100); //经验倍数
				rd.Close();
				cmd.CommandText = $"DELETE FROM world_songplay WHERE user_id={userid} AND song_id='{s.SongId}' AND difficulty={(int)s.Difficulty}";
				cmd.ExecuteNonQuery(); //移除占位数据
				cmd.CommandText = $"SELECT character_id,frag,prog,overdrive FROM user_chars WHERE user_id={userid}" +
					$" AND character_id = (SELECT character_id FROM users WHERE user_id={userid});";
				rd = cmd.ExecuteReader(); //读取玩家当前使用的角色的frag(对应FRAG值), prog(对应STEP值)和overdrive(对应OVER值)数据
				bool isExists = rd.Read();
				uint character_id = isExists ? (uint)rd.GetInt32(0) : 0; //角色id
				decimal frag = isExists ? rd.GetDecimal(1) : 50; //frag值
				decimal prog = isExists ? rd.GetDecimal(2) : 50; //step值
				decimal overDrive = isExists ? rd.GetDecimal(3) : 50; //over值
				rd.Close();
				cmd.CommandText = $"SELECT current_map,world_time_fullrecharged FROM users WHERE user_id={userid};";
				rd = cmd.ExecuteReader();
				rd.Read();
				string current_map = rd.GetString(0); //获取当前玩家正在完成的地图id
				uint staminas = World2.CalculateCurrentStaminas(rd.GetDateTime(1), out _);
				double specialScoreRating = (double)SingleScore.ComputeScoreRating(10.0f, score.Score!.Value);
				if (!beyond_gauge) //如果不是Beyond挑战
				{
					decimal base_step = (decimal)(2.5 + 2.45 * (isSpecial ? specialScoreRating : Math.Sqrt((double)s.ScoreRating.Value)));
					// 基础Step = 2.5 + 2.45 * √(单曲潜力值)
					decimal step = base_step * ((current_map.StartsWith("byd") ? overDrive : prog) / 50) * climb_steps_mtp; // Beyond梯子(不是Beyond绳子)使用over值而非step(prog)值
					// 游玩结果(最终Step) = 基础Step * (角色Step值 / 50) * 游玩Step倍数
					decimal exp = Math.Ceiling(step * 100 * exp_steps_mtp); //经验数
																			// 经验数 = 向上取整(最终Step * 100 * 经验倍数)
					rd.Close();
					cmd.CommandText = $"SELECT curr_position,curr_capture,is_locked FROM user_world WHERE user_id={userid} AND map_id='{current_map}';";
					rd = cmd.ExecuteReader();
					rd.Read();
					decimal beforeCapture = rd.GetDecimal(1);
					uint beforePosition = (uint)rd.GetInt32(0);
					int afterExp = info.UpgradeCharacterLevel((int)exp, out int level);
					bool isLocked = rd.GetBoolean(2);
					rd.Close();
					cmd.CommandText = $"SELECT character_id,frag,prog,overdrive FROM user_chars WHERE user_id={userid}" +
						$" AND character_id = (SELECT character_id FROM users WHERE user_id={userid});";
					rd = cmd.ExecuteReader(); //读取玩家当前使用的角色在获得经验后的frag(对应FRAG值), prog(对应STEP值)和overdrive(对应OVER值)数据
					rd.Read();
					prog = rd.GetDecimal("prog");
					frag = rd.GetDecimal("frag");
					overDrive = rd.GetDecimal("overdrive");
					step = base_step * (prog / 50) * climb_steps_mtp;
					// 游玩结果(最终Step) = 基础Step * (角色Step值 / 50) * 游玩Step倍数
					rd.Close();
					cmd.CommandText = $"SELECT * FROM users WHERE user_id={userid};";
					rd = cmd.ExecuteReader();
					rd.Read();
					var mapInfo = World2.FinishWorldPlay(userid, current_map, step, beforePosition, beforeCapture,
						out var rewards, out var steps, out uint afterPosition, out decimal afterCapture, out int stepsCount);
					var r_value = new JObject()
					{
						{"rewards",rewards },
						{"exp", afterExp },
						{"level",level },
						{"base_progress",base_step },
						{"progress",step },
						{"user_map", new JObject()
							{
								{"user_id",userid },
								{"curr_position",afterPosition },
								{"curr_capture",afterCapture },
								{"is_locked",isLocked },
								{"map_id",current_map },
								{"prev_capture",beforeCapture },
								{"prev_position",beforePosition },
								{"beyond_health",mapInfo.Value<JToken>("beyond_health") },
								{"step_count",stepsCount}
							}
						},
						{"char_stats",new JObject()
							{
								{"character_id", character_id},
								{"frag",frag },
								{"prog",prog },
								{"overdrive",overDrive }
							}
						},
						{"current_stamina",staminas },
						{"max_stamina_ts",  Convert.ToInt64((rd.GetDateTime(30) - DateTime.UnixEpoch).TotalMilliseconds) },
						{"user_rating",newPlayerInfos.Value }
					};
					rd.Close();
					if (stamina_mtp != 1)
						r_value.Add("stamina_multiply", stamina_mtp);
					if (frag_mtp != 100)
						r_value.Add("fragment_multiply", frag_mtp);
					if (prog_boost_mtp != 0)
					{
						r_value.Add("prog_boost_multiply", prog_boost_mtp);
					}
					if (afterPosition == (mapInfo.Value<int>("step_count") - 1) && mapInfo.Value<bool>("is_repeatable"))
					{
						afterPosition = 0;
					}
					cmd.CommandText = $"UPDATE user_world SET curr_position={afterPosition}, curr_capture={afterCapture}" +
						$" WHERE user_id={userid} AND map_id='{current_map}'";
					cmd.ExecuteNonQuery();
					conn.Close();
					r.Add("success", true);
					info.UpdateTotalScoreRank();
					if (TryGetTotalScoreRank(info.UserId!.Value, out int? rank, TopRankLimit))
					{
						r_value.Add("global_rank", rank!.Value);
					}
					r.Add("value", r_value);
				}
				else // Beyond地图结算
				{
					decimal scoreRating = isSpecial switch
					{
						true => (decimal)specialScoreRating,
						false => (decimal)Math.Sqrt((double)s.ScoreRating.Value)
					};
					decimal base_over = s.ClearType.Value != ClearType.TrackLost ? (75M / 28) : (25M / 28) + scoreRating * (7.0M / 8);
					// 基础Over(精彩程度) = (是否通过Beyond Challenge(没有TL)) ? 75/28 : 25/28 + √单曲潜力值 * (7/8)
					decimal over = base_over * (overDrive / 50) * climb_steps_mtp * World2.GetBeyondMapCharAffinity(current_map, character_id);
					// Beyond地图游玩结果(最终Over) = 精彩程度 * (角色Over值 / 50) * 相性契合倍数(如果角色与契合搭档一致) * 残片深化
					decimal exp = Math.Ceiling(over * 25 * exp_steps_mtp); //经验数
																		   // 经验数 = 向上取整(最终Over * 25 * 经验倍数)
					rd.Close();
					cmd.CommandText = $"SELECT curr_position,curr_capture,is_locked FROM user_world WHERE user_id={userid} AND map_id='{current_map}';";
					rd = cmd.ExecuteReader();
					rd.Read();
					decimal beforeCapture = rd.GetDecimal(1);
					uint beforePosition = (uint)rd.GetInt32(0);
					int afterExp = info.UpgradeCharacterLevel((int)exp, out int level);
					bool isLocked = rd.GetBoolean(2);
					rd.Close();
					cmd.CommandText = $"SELECT character_id,frag,prog,overdrive FROM user_chars WHERE user_id={userid}" +
						$" AND character_id = (SELECT character_id FROM users WHERE user_id={userid});";
					rd = cmd.ExecuteReader(); //读取玩家当前使用的角色在获得经验后的frag(对应FRAG值), prog(对应STEP值)和overdrive(对应OVER值)数据
					rd.Read();
					prog = rd.GetDecimal("prog");
					frag = rd.GetDecimal("frag");
					overDrive = rd.GetDecimal("overdrive");
					over = base_over * (overDrive / 50) * climb_steps_mtp * World2.GetBeyondMapCharAffinity(current_map, character_id);
					// Beyond地图游玩结果(最终Over) = 精彩程度 * (角色Over值 / 50) * 相性契合倍数(如果角色与契合搭档一致) * 残片深化
					rd.Close();
					cmd.CommandText = $"SELECT * FROM users WHERE user_id={userid};";
					rd = cmd.ExecuteReader();
					rd.Read();
					var mapInfo = World2.FinishWorldPlay(userid, current_map, over, beforePosition, beforeCapture,
						out var rewards, out var steps, out uint afterPosition, out decimal afterCapture, out int stepsCount);
					var r_value = new JObject()
					{
						{"rewards", rewards },
						{"exp", afterExp },
						{"level", level },
						{"base_progress", base_over },
						{"progress", over },
						{"user_map", new JObject()
							{
								{"user_id",userid },
								{"curr_position",afterPosition },
								{"curr_capture",afterCapture },
								{"is_locked",isLocked },
								{"map_id",current_map },
								{"prev_capture",beforeCapture },
								{"prev_position",beforePosition },
								{"beyond_health",mapInfo.Value<JToken>("beyond_health") },
								{"step_count",stepsCount}
							}
						},
						{"char_stats",new JObject()
							{
								{"character_id", character_id},
								{"frag",frag },
								{"prog",prog },
								{"overdrive",overDrive }
							}
						},
						{"current_stamina",staminas },
						{"max_stamina_ts",  Convert.ToInt64((rd.GetDateTime(30) - DateTime.UnixEpoch).TotalMilliseconds) },
						{"user_rating",newPlayerInfos.Value }
					};
					rd.Close();
					if (stamina_mtp != 1)
						r_value.Add("stamina_multiply", stamina_mtp);
					if (frag_mtp != 100)
						r_value.Add("fragment_multiply", frag_mtp);
					if (prog_boost_mtp != 0)
					{
						r_value.Add("prog_boost_multiply", prog_boost_mtp);
					}
					if (afterPosition == (mapInfo.Value<int>("step_count") - 1) && mapInfo.Value<bool>("is_repeatable"))
					{
						afterPosition = 0;
					}
					cmd.CommandText = $"UPDATE user_world SET curr_position={afterPosition}, curr_capture={afterCapture}" +
						$" WHERE user_id={userid} AND map_id='{current_map}'";
					cmd.ExecuteNonQuery();
					conn.Close();
					r.Add("success", true);
					info.UpdateTotalScoreRank();
					if (TryGetTotalScoreRank(info.UserId!.Value, out int? rank, TopRankLimit))
					{
						r_value.Add("global_rank", rank!.Value);
					}
					r.Add("value", r_value);
				}
				#endregion
			}
			else
			{
				var value = new JObject()
				{
					{"user_rating",newPlayerInfos.Value }
				};
				info.UpdateTotalScoreRank();
				if (TryGetTotalScoreRank(info.UserId!.Value, out int? rank, TopRankLimit))
				{
					value.Add("global_rank", rank!.Value);
				}
				r.Add("success", true);
				r.Add("value", value);
			}
			return r;
		}

		/// <summary>
		/// [API]获取排行榜数据。
		/// </summary>
		/// <param name="userid">玩家的用户id(非好友id)。<br />本参数在 type 参数为 <see cref="LeaderBoardType.Friend"/> 和 <see cref="LeaderBoardType.World"/> 下起作用, 其余类型不起作用(可设置为 <see langword="null" /> )。</param>
		/// <param name="songid">曲目id。</param>
		/// <param name="difficulty">曲目难度。</param>
		/// <param name="type">排行榜类型。</param>
		/// <returns>Json数据。</returns>
		/// <exception cref="ArcaeaAPIException" />
		public static JObject GetLeaderBoard(uint? userid,string songid,SongDifficulty difficulty,LeaderBoardType type)
		{
			var r = new JObject()
			{
				{"success",true }
			};
			var leaderboardData = GetSongLeaderBoard(songid, difficulty, type, userid, null, out bool isExists);
			if (!isExists)
			{
				r.Add("value", new JArray());
			}
			else
			{
				r.Add("value", leaderboardData);
			}
			return r;
		}

		/// <summary>
		/// 执行成绩附加条件(Score Extra Challenge)检查模块。
		/// </summary>
		/// <param name="userid">玩家的用户id(非好友id)。</param>
		/// <param name="score">玩家的分数。</param>
		/// <param name="song_hash">Arcaea 客户端的曲目谱面文件的哈希值。</param>
		/// <param name="submission_hash">分数提交的哈希值。</param>
		/// <exception cref="ArcaeaAPIException" />
		public static void DoScoreExtraChallenge(uint userid,SingleScore score, string song_hash,string submission_hash)
		{
			string chartFileName = string.Concat((int)score.Difficulty, ".aff");
			var chartFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, "data", "static", "Songs", score.SongId, chartFileName));
			if (chartFile.Exists)
			{
				string md5 = BitConverter.ToString(MD5.HashData(Encoding.UTF8.GetBytes(File.ReadAllText(chartFile.FullName, Encoding.UTF8)))).Replace("-", string.Empty).ToLower();
				if (song_hash != md5)
				{
					SecurityManager.EditPlayerCreditPoint(userid, -6, "Attempt to submit a score whose chart's hash check failed");
					throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountBlockWarning);
				}
			}
		}
	}
}
