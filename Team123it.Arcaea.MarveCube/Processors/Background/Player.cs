#nullable enable
using static Team123it.Arcaea.MarveCube.GlobalProperties;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using Team123it.Arcaea.MarveCube.Core;

namespace Team123it.Arcaea.MarveCube.Processors.Background
{
	/// <summary>
	/// 提供适用于 <see cref="PlayerInfo"/> (玩家)类实例的常用 <see langword="static"/> 方法的类。无法继承此类。
	/// </summary>
	public sealed class Player
	{
		/// <summary>
		/// 刷新指定玩家的个人潜力值。
		/// </summary>
		/// <param name="userid">玩家的用户id(不是好友id)。</param>
		/// <returns>刷新后指定玩家的 <see cref="PlayerInfo"/> 实例及API/数据库版个人潜力值数值。</returns>
		public static KeyValuePair<PlayerInfo,int> RefreshPotential(uint userid,int score,ClearType clearType,int health)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = conn.CreateCommand();
			cmd.CommandText = $"SELECT user_rating FROM users WHERE user_id={userid};";
			int oldPlayerPotentialInt = Convert.ToInt32(cmd.ExecuteScalar());
			cmd.CommandText = $"SELECT rating FROM bests WHERE user_id={userid} AND rating > 0 ORDER BY rating DESC LIMIT 0,30;";
			// 取Best30
			var best30 = new List<double>();
			var rd = cmd.ExecuteReader();
			while (rd.Read()) //循环结果集中所有Rating行
			{
				best30.Add(rd.GetDouble(0)); //获取各个Rating
			}
			rd.Close();
			best30 = (from double best1 in best30 orderby best1 descending select best1).ToList();
			double newPlayerPotential = Math.Round(best30.Sum() / 30, 2, MidpointRounding.AwayFromZero);
			/** if (best30.Count == 30 && recent1 <= best29.Min()) //如果Recent1的单曲潜力值小于等于Best29中最后一个
			//{
			//	if (score < 9800000) //如果分数低于9800000
			//	{
			//		// 新个人潜力值 = (Best29 + Recent1) / 30 (四舍五入;保留两位小数)
			//		double newPlayerPotential = Math.Round((best29.Sum() + recent1) / 30, 2, MidpointRounding.AwayFromZero);
			//		uint newPlayerPotentialInt = (uint)(newPlayerPotential * 100);
			//		cmd.CommandText = $"UPDATE users SET user_rating={newPlayerPotentialInt} WHERE user_id={userid};";
			//		cmd.ExecuteNonQuery();
			//		conn.Close();
			//		var p = new PlayerInfo(userid,out _);
			//		return new KeyValuePair<PlayerInfo, int>(p, (int)newPlayerPotentialInt); //返回新个人潜力值
			//	} else //否则(EX保护)
			//	{
			//		conn.Close();
			//		var p = new PlayerInfo(userid,out _);
			//		return new KeyValuePair<PlayerInfo, int>(p, user_rating); //直接返回当前个人潜力值(不计算)
			//	}
			//} else //如果大于最后一个
			//{
			//	// 新个人潜力值 = (Best29 + Recent1) / 30 (四舍五入;保留两位小数)
			//	double newPlayerPotential = Math.Round((best29.Sum() + recent1) / 30, 2, MidpointRounding.AwayFromZero);
			//	uint newPlayerPotentialInt = (uint)(newPlayerPotential * 100);
			//	cmd.CommandText = $"UPDATE users SET user_rating={newPlayerPotentialInt} WHERE user_id={userid};";
			//	cmd.ExecuteNonQuery();
			//	conn.Close();
			//	var p = new PlayerInfo(userid,out _);
			//	return new KeyValuePair<PlayerInfo, int>(p, (int)newPlayerPotentialInt); //返回新个人潜力值
			//}
			*/
			int newPlayerPotentialInt = (int)Math.Ceiling(newPlayerPotential * 100);
			bool isPotentialPreserve = false;
			if (newPlayerPotentialInt < oldPlayerPotentialInt) //如果新的个人潜力值比旧的个人潜力值低
			{
				if (score >= 9800000) //如果分数>=980w(EX保护)
				{
					isPotentialPreserve = true;
				}
				else if (clearType == ClearType.TrackLost && health == -1) //如果为Hard回忆条下的Track Lost(困难回忆条TL保护)
				{
					isPotentialPreserve = true;
				}
			}
			if (!isPotentialPreserve) //如果新的个人潜力值(>=旧的个人潜力值)或(<旧的个人潜力值且Potential不受保护)
			{
				cmd.CommandText = $"UPDATE users SET user_rating={newPlayerPotentialInt} WHERE user_id={userid};";
				cmd.ExecuteNonQuery();
				conn.Close();
				return new KeyValuePair<PlayerInfo, int>(new PlayerInfo(userid, out _), newPlayerPotentialInt);
			}
			else
			{
				conn.Close();
				return new KeyValuePair<PlayerInfo, int>(new PlayerInfo(userid, out _), newPlayerPotentialInt);
			}
		}

		[Obsolete("新版个人潜力值计算不再使用Best30+Recent10算法。请改用 RefreshPotential() 方法。\r\n调用本方法将会始终返回 default 。")]
		/// <summary>
		/// 刷新指定玩家的个人潜力值。
		/// </summary>
		/// <param name="userid">玩家的用户id(不是好友id)。</param>
		/// <param name="newSongId">玩家的新游玩曲目id。</param>
		/// <param name="newSongDiff">玩家的新游玩曲目难度。</param>
		/// <param name="newScore">玩家的新游玩曲目成绩。</param>
		/// <param name="newScoreRating">玩家的新游玩单曲潜力值。</param>
		/// <returns>刷新后指定玩家的 <see cref="PlayerInfo"/> 实例及API/数据库版个人潜力值数值。</returns>
		public static KeyValuePair<PlayerInfo,int> BestRecentRefreshPotential(uint userid,string newSongId,SongDifficulty newSongDiff,uint newScore,decimal newScoreRating)
		{
			return default;
			/*
			PlayerInfo info = new PlayerInfo(userid);
			var r10 = info.Recent10;
			if (r10.Count < 10)
			{
				r10.Add(new SingleScore(newSongId, newSongDiff, newScoreRating));
			}
			else
			{
				foreach (var singleRecent in r10)
				{
					if (newSongId == singleRecent.SongId && newSongDiff == singleRecent.Difficulty && (decimal)singleRecent.SongRating >= newScoreRating)
					{ //如果Recent10中存在同曲目同难度且单曲潜力值比新成绩的单曲潜力值高的数据
						if (newScore < 9800000) //如果分数小于980w
						{
							r10.Remove(singleRecent);
							r10.Add(new SingleScore(newSongId, newSongDiff, newScoreRating));
							break;
						}
					}
					else if (singleRecent == r10.Last() && r10.Last().ScoreRating > newScoreRating) //如果当前是Recent10最新的一个数据且这个数据的单曲潜力值比新成绩的单曲潜力值高
					{
						if (newScore < 9800000) //如果分数小于980w
						{
							r10.Remove(singleRecent);
							r10.Add(new SingleScore(newSongId, newSongDiff, newScoreRating));
							break;
						}
					}
				}
			}
			info.Recent10 = r10; //刷新Recent10
			if (info.Best30.Count < 30)
			{
				var b30 = info.Best30;
				b30.Add(new SingleScore(newSongId, newSongDiff, newScoreRating)); //添加新成绩数据
				info.Best30 = b30;
			}
			else if (newScoreRating <= info.Best30.Last().ScoreRating) //如果新成绩潜力值高于Best30最后一位潜力值
			{
				var b30 = info.Best30;
				b30.RemoveAt(b30.Count - 1); //移除Best30最后一个数据
				b30.Add(new SingleScore(newSongId, newSongDiff, newScoreRating)); //添加新成绩数据
				info.Best30 = b30;
			}
			else
			{
				var b30 = info.Best30;
				b30.Add(new SingleScore(newSongId, newSongDiff, newScoreRating)); //添加新成绩数据
				info.Best30 = b30;
			}
			info.BestRecentRefreshPotential(); //刷新潜力值(Best30会自动排序)
			return new KeyValuePair<PlayerInfo, int>(info, (int)info.PotentialInt);
			*/
		}

		[Obsolete("新版个人潜力值计算不再需要使用Best30+Recent10算法手动计算。 系统将自动取Best29+Recent1的均值作为新个人潜力值。")]
		/// <summary>
		/// 通过指定的Best30数据和Recent10数据计算玩家的个人潜力值。
		/// </summary>
		/// <param name="Best30">指定的Best30数据。</param>
		/// <param name="Recent10">指定的Recent10数据。</param>
		/// <returns>计算后的玩家的个人潜力值(API/数据库版格式)。</returns>
		public static uint CalculatePotentialInt(List<SingleScore> Best30,List<SingleScore> Recent10)
		{
			decimal totalAvg = 0;
			foreach (var SingleBest in Best30)
			{
				if (SingleBest.ScoreRating.HasValue)
				{
					totalAvg += SingleBest.ScoreRating.Value;
				}
			}
			foreach (var SingleRecent in Recent10)
			{
				if (SingleRecent.Score.HasValue)
				{
					// totalAvg += SingleRecent.ScoreRating.Value;
				}
			}
			decimal result_ptt = totalAvg / 40m;
			return (uint)(Math.Round(result_ptt, 2) * 100);
		}

		/// <summary>
		/// 获取指定数字id(character_id)对应的角色的名称id(character_nameid)。
		/// </summary>
		/// <param name="charId">角色的数字id。</param>
		/// <param name="isCharAvailable">在当前方法返回时,若角色可用(is_available)则本参数值为 <see langword="true"/> , 否则为 <see langword="false"/> 。</param>
		/// <param name="success">在当前方法返回时,若名称获取成功则本参数值为 <see langword="true"/> , 否则为 <see langword="false"/> 。</param>
		/// <returns>成功返回角色名称id, 失败返回 <see cref="string.Empty"/> 。</returns>
		public static string GetCharacterNameId(uint charId,out bool isCharAvailable,out bool success)
		{
			try
			{
				using var conn = new MySqlConnection(DatabaseConnectURL);
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"SELECT character_nameid,is_available FROM fixed_characters WHERE character_id={charId}";
				var rd = cmd.ExecuteReader();
				if (rd.Read())
				{
					string nameid = rd.GetString(0);
					isCharAvailable = rd.GetBoolean(1);
					rd.Close();
					conn.Close();
					success = true;
					return nameid;
				}
				else
				{
					rd.Close();
					conn.Close();
					isCharAvailable = false;
					success = false;
					return string.Empty;
				}
			}
			catch
			{
				isCharAvailable = false;
				success = false;
				return string.Empty;
			}
		}

		/// <summary>
		/// 获取玩家指定曲目指定难度的成绩。
		/// </summary>
		/// <param name="userid">玩家的用户id(非好友id)。</param>
		/// <param name="sid">曲目的sid。</param>
		/// <param name="difficulty">曲目的难度id(0=Past 1=Present 2=Future 3=Beyond),默认为2(Future)。</param>
		/// <returns>成功返回 <see cref="SingleScore"/> 实例,失败返回 <see langword="null" /> 。</returns>
		public static SingleScore? GetPlayerSongScore(uint userid,string sid,int difficulty = 2)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = "SELECT * FROM bests WHERE user_id=?uid AND song_id=?sid AND difficulty=?diff;";
				cmd.Parameters.Add(new MySqlParameter("?uid", MySqlDbType.Int32)
				{
					Value = userid
				});
				cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
				{
					Value = sid
				});
				cmd.Parameters.Add(new MySqlParameter("?uid", MySqlDbType.Int32)
				{
					Value = difficulty
				});
				var rd = cmd.ExecuteReader();
				if (rd.Read())
				{
					var score = new SingleScore(sid, (SongDifficulty)difficulty, rd.GetUInt32(3), (ClearType)rd.GetInt32(12), rd.GetUInt32(4),
						rd.GetUInt32(5), rd.GetUInt32(6), rd.GetUInt32(7), rd.GetUInt32(10) * 1000UL);
					rd.Close();
					conn.Close();
					return score;
				}
				else
				{
					rd.Close();
					conn.Close();
					return null;
				}
			}
			catch
			{
				conn.Close();
				return null;
			}
		}

		public static int SetRewardMemories(uint score, ClearType clearType)
		{
			int rewardMemories = 0;
			var random = new Random();
			switch (clearType)
			{
				case ClearType.PureMemory:
					rewardMemories = random.Next(200, 251);
					break;
				case ClearType.FullRecall:
					rewardMemories = random.Next(175, 200);
					break;
				case ClearType.EasyClear:
				case ClearType.NormalClear:
				case ClearType.HardClear:
					if (score >= 9900000)
					{
						rewardMemories = random.Next(125, 175);
					}
					else if (score >= 9800000)
					{
						rewardMemories = random.Next(100, 125);
					}
					else if (score >= 5600000)
					{
						rewardMemories = Convert.ToInt32((score - 5600000) / 75000M);
					}
					else
					{
						rewardMemories = 0;
					}
					break;
				case ClearType.TrackLost:
					if (score >= 5600000)
					{
						rewardMemories = Convert.ToInt32((score - 5600000) / 250000M);
					}
					else
					{
						rewardMemories = 0;
					}
					break;
			}
			return rewardMemories;
		}
	}
}
