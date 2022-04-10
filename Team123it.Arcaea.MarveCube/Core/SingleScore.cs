using static Team123it.Arcaea.MarveCube.GlobalProperties;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;

namespace Team123it.Arcaea.MarveCube.Core
{
	/// <summary>
	/// 表示单曲的成绩。
	/// </summary>
	public class SingleScore
	{
		/// <summary>
		/// 曲目id。
		/// </summary>
		public string SongId { get; }

		/// <summary>
		/// 曲目难度。		
		/// <para><see cref="SongDifficulty.Past"/> - Past难度</para>
		/// <para><see cref="SongDifficulty.Present"/> - Present难度</para>
		/// <para><see cref="SongDifficulty.Future"/> - Future难度</para>
		/// <para><see cref="SongDifficulty.Beyond"/> - Beyond难度</para>
		/// </summary>
		public SongDifficulty Difficulty { get; }

		/// <summary>
		/// 曲目定数。
		/// </summary>
		public double? SongRating { get; }

		/// <summary>
		/// 单曲潜力值(分数计算后实际获得的潜力值)
		/// </summary>
		public decimal? ScoreRating { get; }

		/// <summary>
		/// 分数。
		/// </summary>
		public uint? Score { get; }

		/// <summary>
		/// 大Pure数。
		/// <para>若初始化实例时没有设置曲目详情则此项值为 <see langword="null" /> 。</para>
		/// </summary>
		public uint? BigPureCount { get; }

		/// <summary>
		/// Pure数。
		/// <para>若初始化实例时没有设置曲目详情则此项值为 <see langword="null" /> 。</para>
		/// </summary>
		public uint? PureCount { get; }

		/// <summary>
		/// Far数。
		/// <para>若初始化实例时没有设置曲目详情则此项值为 <see langword="null" /> 。</para>
		/// </summary>
		public uint? FarCount { get; }

		/// <summary>
		/// Lost数。
		/// <para>若初始化实例时没有设置曲目详情则此项值为 <see langword="null" /> 。</para>
		/// </summary>
		public uint? LostCount { get; }

		/// <summary>
		/// 曲目游玩日期时间。
		/// <para>若初始化实例时没有设置曲目详情则此项值为 <see langword="null" /> 。</para>
		/// </summary>
		public DateTime? PlayDate { get; }

		/// <summary>
		/// 曲目完成类型。
		/// <para><see cref="ClearType.TrackLost"/> - [TL]Track Lost</para>
		/// <para><see cref="ClearType.EasyClear"/> - [EC]Track Complete(简单回忆条)</para>
		/// <para><see cref="ClearType.NormalClear"/> - [NC]Track Complete(普通回忆条)</para>
		/// <para><see cref="ClearType.HardClear"/> - [HC]Track Complete(困难回忆条)</para>
		/// <para><see cref="ClearType.FullRecall"/> - [FR]Full Recall</para>
		/// <para><see cref="ClearType.PureMemory"/> - [PM]Pure Memory</para>
		/// </summary>
		public ClearType? ClearType { get; }

		/// <summary>
		/// 使用曲目id, 曲目难度和单曲潜力值初始化 <see cref="SingleScore"/> 类的新实例。
		/// </summary>
		/// <param name="songId">
		/// 曲目id。
		/// </param>
		/// <param name="songDiff">
		/// 曲目难度。
		/// <para>若找不到对应曲目难度则 <see cref="SongRating"/> 和 <see cref="ScoreRating"/> 将均为 <see langword="null" /> 。</para>
		/// <para><see cref="SongDifficulty.Past"/> - Past难度</para>
		/// <para><see cref="SongDifficulty.Present"/> - Present难度</para>
		/// <para><see cref="SongDifficulty.Future"/> - Future难度</para>
		/// <para><see cref="SongDifficulty.Beyond"/> - Beyond难度</para>
		/// </param>
		/// <param name="scoreRating">单曲潜力值。
		/// </param>
		/// <exception cref="SongNotFoundException" />
		/// <exception cref="ArgumentException" />
		public SingleScore(string songId, SongDifficulty songDiff, decimal scoreRating)
		{
			Score = null;
			BigPureCount = null;
			PureCount = null;
			FarCount = null;
			LostCount = null;
			PlayDate = null;
			ClearType = null;
			SongId = songId;
			using var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = conn.CreateCommand();
			switch (songDiff)
			{
				case SongDifficulty.Past:
					#region "Past"
					Difficulty = songDiff;
					cmd.CommandText = $"SELECT COUNT(*),rating_pst FROM fixed_songs WHERE sid='{songId}'";
					var r_pst = cmd.ExecuteReader();
					r_pst.Read();
					if (r_pst.GetInt32(0) == 1)
					{
						double pst_rating = GetRealSongRatingFromDatabaseSongRating(r_pst.GetInt32(1));
						if (pst_rating != -1)
						{
							r_pst.Close();
							conn.Close();
							SongRating = pst_rating;
							ScoreRating = (decimal)scoreRating;
						}
						else
						{
							r_pst.Close();
							conn.Close();
							SongRating = null;
							ScoreRating = null;
						}
					}
					else
					{
						r_pst.Close();
						conn.Close();
						throw new SongNotFoundException(songId, null);
					}
					#endregion
					break;
				case SongDifficulty.Present:
					#region "Present"
					Difficulty = songDiff;
					cmd.CommandText = $"SELECT COUNT(*),rating_prs FROM fixed_songs WHERE sid='{songId}'";
					var r_prs = cmd.ExecuteReader();
					r_prs.Read();
					if (r_prs.GetInt32(0) == 1)
					{
						double prs_rating = GetRealSongRatingFromDatabaseSongRating(r_prs.GetInt32(1));
						if (prs_rating != -1)
						{
							r_prs.Close();
							conn.Close();
							SongRating = prs_rating;
							ScoreRating = (decimal)scoreRating;
						}
						else
						{
							r_prs.Close();
							conn.Close();
							SongRating = null;
							ScoreRating = null;
						}
					}
					else
					{
						r_prs.Close();
						conn.Close();
						throw new SongNotFoundException(songId, null);
					}
					#endregion
					break;
				case SongDifficulty.Future:
					#region "Future"
					Difficulty = songDiff;
					cmd.CommandText = $"SELECT COUNT(*),rating_ftr FROM fixed_songs WHERE sid='{songId}'";
					var r_ftr = cmd.ExecuteReader();
					r_ftr.Read();
					if (r_ftr.GetInt32(0) == 1)
					{
						double ftr_rating = GetRealSongRatingFromDatabaseSongRating(r_ftr.GetInt32(1));
						if (ftr_rating != -1)
						{
							r_ftr.Close();
							conn.Close();
							SongRating = ftr_rating;
							ScoreRating = (decimal)scoreRating;
						}
						else
						{
							r_ftr.Close();
							conn.Close();
							SongRating = null;
							ScoreRating = null;
						}
					}
					else
					{
						r_ftr.Close();
						conn.Close();
						throw new SongNotFoundException(songId, null);
					}
					#endregion
					break;
				case SongDifficulty.Beyond:
					Difficulty = songDiff;
					cmd.CommandText = $"SELECT COUNT(*),rating_byd FROM fixed_songs WHERE sid='{songId}'";
					var r_byd = cmd.ExecuteReader();
					r_byd.Read();
					if (r_byd.GetInt32(0) == 1)
					{
						double byd_rating = GetRealSongRatingFromDatabaseSongRating(r_byd.GetInt32(1));
						if (byd_rating != -1)
						{
							r_byd.Close();
							conn.Close();
							SongRating = byd_rating;
							ScoreRating = scoreRating;
						}
						else
						{
							r_byd.Close();
							conn.Close();
							SongRating = null;
							ScoreRating = null;
						}
					}
					else
					{
						r_byd.Close();
						conn.Close();
						throw new SongNotFoundException(songId, null);
					}
					break;
				default:
					throw new ArgumentException("不支持的 Team123it.Arcaea.MarveCube.Core.SongDifficulty 值。", "songDiff");
			}
		}

		/// <summary>
		/// 使用曲目id, 曲目难度和曲目完成类型初始化 <see cref="SingleScore"/> 类的新实例。
		/// </summary>
		/// <param name="songId">
		/// 曲目id。
		/// <para>若找不到id对应的曲目则会抛出 <see cref="SongNotFoundException"/> 异常。</para>
		/// </param>
		/// <param name="songDiff">
		/// 曲目难度。
		/// <para>若找不到对应曲目难度则 <see cref="SongRating"/> 和 <see cref="ScoreRating"/> 将均为 <see langword="null" /> 。</para>
		/// <para><see cref="SongDifficulty.Past"/> - Past难度</para>
		/// <para><see cref="SongDifficulty.Present"/> - Present难度</para>
		/// <para><see cref="SongDifficulty.Future"/> - Future难度</para>
		/// <para><see cref="SongDifficulty.Beyond"/> - Beyond难度</para>
		/// </param>
		/// <param name="score">曲目成绩。
		/// </param>
		/// <param name="clearType">
		/// 曲目完成类型。
		/// <para><see cref="ClearType.TrackLost"/> - [TL]Track Lost</para>
		/// <para><see cref="ClearType.EasyClear"/> - [EC]Track Complete(简单回忆条)</para>
		/// <para><see cref="ClearType.NormalClear"/> - [NC]Track Complete(普通回忆条)</para>
		/// <para><see cref="ClearType.HardClear"/> - [HC]Track Complete(困难回忆条)</para>
		/// <para><see cref="ClearType.FullRecall"/> - [FR]Full Recall</para>
		/// <para><see cref="ClearType.PureMemory"/> - [PM]Pure Memory</para>
		/// </param>
		/// <exception cref="SongNotFoundException" />
		/// <exception cref="ArgumentException" />
		public SingleScore(string songId, SongDifficulty songDiff, uint score,ClearType clearType)
		{
			BigPureCount = null;
			PureCount = null;
			FarCount = null;
			LostCount = null;
			PlayDate = null;
			SongId = songId;
			ClearType = clearType;
			using var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = conn.CreateCommand();
			switch (songDiff)
				{
					case SongDifficulty.Past:
						#region "Past"
						Difficulty = songDiff;
						cmd.CommandText = $"SELECT COUNT(*),rating_pst FROM fixed_songs WHERE sid='{songId}'";
						var r_pst = cmd.ExecuteReader();
						r_pst.Read();
						if (r_pst.GetInt32(0) == 1)
						{
							Score = score;
							double pst_rating = GetRealSongRatingFromDatabaseSongRating(r_pst.GetInt32(1));
							if (pst_rating != -1)
							{
								r_pst.Close();
								conn.Close();
								SongRating = pst_rating;
								ScoreRating = ComputeScoreRating(pst_rating, score);
							} else
							{
								r_pst.Close();
								conn.Close();
								SongRating = null;
								ScoreRating = null;
							}
						} else
						{
							r_pst.Close();
							conn.Close();
							throw new SongNotFoundException(songId,null);
						}
						#endregion
						break;
					case SongDifficulty.Present:
						#region "Present"
						Difficulty = songDiff;
						cmd.CommandText = $"SELECT COUNT(*),rating_prs FROM fixed_songs WHERE sid='{songId}'";
						var r_prs = cmd.ExecuteReader();
						r_prs.Read();
						if (r_prs.GetInt32(0) == 1)
						{
							Score = score;
							double prs_rating = GetRealSongRatingFromDatabaseSongRating(r_prs.GetInt32(1));
							if (prs_rating != -1)
							{
								r_prs.Close();
								conn.Close();
								SongRating = prs_rating;
								ScoreRating = ComputeScoreRating(prs_rating, score);
							}
							else
							{
								r_prs.Close();
								conn.Close();
								SongRating = null;
								ScoreRating = null;
							}
						}
						else
						{
							r_prs.Close();
							conn.Close();
							throw new SongNotFoundException(songId, null);
						}
						#endregion
						break;
					case SongDifficulty.Future:
						#region "Future"
						Difficulty = songDiff;
						cmd.CommandText = $"SELECT COUNT(*),rating_ftr FROM fixed_songs WHERE sid='{songId}'";
						var r_ftr = cmd.ExecuteReader();
						r_ftr.Read();
						if (r_ftr.GetInt32(0) == 1)
						{
							Score = score;
							double ftr_rating = GetRealSongRatingFromDatabaseSongRating(r_ftr.GetInt32(1));
							if (ftr_rating != -1)
							{
								r_ftr.Close();
								conn.Close();
								SongRating = ftr_rating;
								ScoreRating = ComputeScoreRating(ftr_rating, score);
							}
							else
							{
								r_ftr.Close();
								conn.Close();
								SongRating = null;
								ScoreRating = null;
							}
						}
						else
						{
							r_ftr.Close();
							conn.Close();
							throw new SongNotFoundException(songId, null);
						}
						#endregion
						break;
					case SongDifficulty.Beyond:
						Difficulty = songDiff;
						cmd.CommandText = $"SELECT COUNT(*),rating_byd FROM fixed_songs WHERE sid='{songId}'";
						var r_byd = cmd.ExecuteReader();
						r_byd.Read();
						if (r_byd.GetInt32(0) == 1)
						{
							Score = score;
							double byd_rating = GetRealSongRatingFromDatabaseSongRating(r_byd.GetInt32(1));
							if (byd_rating != -1)
							{
								r_byd.Close();
								conn.Close();
								SongRating = byd_rating;
								ScoreRating = ComputeScoreRating(byd_rating, score);
							}
							else
							{
								r_byd.Close();
								conn.Close();
								SongRating = null;
								ScoreRating = null;
							}
						}
						else
						{
							r_byd.Close();
							conn.Close();
							throw new SongNotFoundException(songId, null);
						}
						break;
					default:
						throw new ArgumentException("不支持的 Team123it.Arcaea.MarveCube.Core.SongDifficulty 值。", "songDiff");
				}
		}

		/// <summary>
		/// 使用曲目id, 曲目难度, 曲目分数, 曲目完成类型和曲目游玩详情(大Pure数, Pure数, Far数, Lost数和游玩日期时间)初始化 <see cref="SingleScore"/> 类的新实例。
		/// </summary>
		/// <param name="songId">
		/// 曲目id。
		/// <para>若找不到id对应的曲目则会抛出 <see cref="SongNotFoundException"/> 异常。</para>
		/// </param>
		/// <param name="songDiff">
		/// 曲目难度。
		/// <para>若找不到对应曲目难度则 <see cref="SongRating"/> 和 <see cref="ScoreRating"/> 将均为 <see langword="null" /> 。</para>
		/// <para><see cref="SongDifficulty.Past"/> - Past难度</para>
		/// <para><see cref="SongDifficulty.Present"/> - Present难度</para>
		/// <para><see cref="SongDifficulty.Future"/> - Future难度</para>
		/// <para><see cref="SongDifficulty.Beyond"/> - Beyond难度</para>
		/// </param>
		/// <param name="score">曲目成绩。
		/// </param>
		/// <param name="clearType">
		/// 曲目完成类型。
		/// <para><see cref="ClearType.TrackLost"/> - [TL]Track Lost</para>
		/// <para><see cref="ClearType.EasyClear"/> - [EC]Track Complete(简单回忆条)</para>
		/// <para><see cref="ClearType.NormalClear"/> - [NC]Track Complete(普通回忆条)</para>
		/// <para><see cref="ClearType.HardClear"/> - [HC]Track Complete(困难回忆条)</para>
		/// <para><see cref="ClearType.FullRecall"/> - [FR]Full Recall</para>
		/// <para><see cref="ClearType.PureMemory"/> - [PM]Pure Memory</para>
		/// </param>
		/// <param name="bigPureCount">[曲目游玩详情]大Pure(精确Pure)数。</param>
		/// <param name="pureCount">[曲目游玩详情]Pure数。</param>
		/// <param name="farCount">[曲目游玩详情]Far数。</param>
		/// <param name="lostCount">[曲目游玩详情]Lost数。</param>
		/// <param name="playDate">[曲目游玩详情]游玩日期时间。</param>
		/// <exception cref="SongNotFoundException" />
		/// <exception cref="ArgumentException" />
		public SingleScore(string songId, SongDifficulty songDiff, uint score, ClearType clearType, uint bigPureCount,uint pureCount, uint farCount,uint lostCount,DateTime? playDate)
		{
			BigPureCount = bigPureCount;
			PureCount = pureCount;
			FarCount = farCount;
			LostCount = lostCount;
			PlayDate = playDate;
			SongId = songId;
			ClearType = clearType;
			using var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = conn.CreateCommand();
			switch (songDiff)
			{
				case SongDifficulty.Past:
					#region "Past"
					Difficulty = songDiff;
					cmd.CommandText = "SELECT COUNT(sid),rating_pst FROM fixed_songs WHERE sid=?sid";
					cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
					{
						Value = songId
					});
					var r_pst = cmd.ExecuteReader();
					r_pst.Read();
					if (r_pst.GetInt32(0) == 1)
					{
						Score = score;
						double pst_rating = GetRealSongRatingFromDatabaseSongRating(r_pst.GetInt32(1));
						if (pst_rating != -1)
						{
							r_pst.Close();
							conn.Close();
							SongRating = pst_rating;
							ScoreRating = ComputeScoreRating(pst_rating, score);
						}
						else
						{
							r_pst.Close();
							conn.Close();
							SongRating = null;
							ScoreRating = null;
						}
					}
					else
					{
						r_pst.Close();
						conn.Close();
						throw new SongNotFoundException(songId, null);
					}
					#endregion
					break;
				case SongDifficulty.Present:
					#region "Present"
					Difficulty = songDiff;
					cmd.CommandText = "SELECT COUNT(sid),rating_prs FROM fixed_songs WHERE sid=?sid";
					cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
					{
						Value = songId
					});
					var r_prs = cmd.ExecuteReader();
					r_prs.Read();
					if (r_prs.GetInt32(0) == 1)
					{
						Score = score;
						double prs_rating = GetRealSongRatingFromDatabaseSongRating(r_prs.GetInt32(1));
						if (prs_rating != -1)
						{
							r_prs.Close();
							conn.Close();
							SongRating = prs_rating;
							ScoreRating = ComputeScoreRating(prs_rating, score);
						}
						else
						{
							r_prs.Close();
							conn.Close();
							SongRating = null;
							ScoreRating = null;
						}
					}
					else
					{
						r_prs.Close();
						conn.Close();
						throw new SongNotFoundException(songId, null);
					}
					#endregion
					break;
				case SongDifficulty.Future:
					#region "Future"
					Difficulty = songDiff;
					cmd.CommandText = $"SELECT COUNT(sid),rating_ftr FROM fixed_songs WHERE sid=?sid";
					cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
					{
						Value = songId
					});
					var r_ftr = cmd.ExecuteReader();
					r_ftr.Read();
					if (r_ftr.GetInt32(0) == 1)
					{
						Score = score;
						double ftr_rating = GetRealSongRatingFromDatabaseSongRating(r_ftr.GetInt32(1));
						if (ftr_rating != -1)
						{
							r_ftr.Close();
							conn.Close();
							SongRating = ftr_rating;
							ScoreRating = ComputeScoreRating(ftr_rating, score);
						}
						else
						{
							r_ftr.Close();
							conn.Close();
							SongRating = null;
							ScoreRating = null;
						}
					}
					else
					{
						r_ftr.Close();
						conn.Close();
						throw new SongNotFoundException(songId, null);
					}
					#endregion
					break;
				case SongDifficulty.Beyond:
					#region "Beyond"
					Difficulty = songDiff;
					cmd.CommandText = $"SELECT COUNT(sid),rating_byd FROM fixed_songs WHERE sid=?sid";
					cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
					{
						Value = songId
					});
					var r_byd = cmd.ExecuteReader();
					r_byd.Read();
					if (r_byd.GetInt32(0) == 1)
					{
						Score = score;
						double byd_rating = GetRealSongRatingFromDatabaseSongRating(r_byd.GetInt32(1));
						if (byd_rating != -1)
						{
							r_byd.Close();
							conn.Close();
							SongRating = byd_rating;
							ScoreRating = ComputeScoreRating(byd_rating, score);
						}
						else
						{
							r_byd.Close();
							conn.Close();
							SongRating = null;
							ScoreRating = null;
						}
					}
					else
					{
						r_byd.Close();
						conn.Close();
						throw new SongNotFoundException(songId, null);
					}
					#endregion
					break;
				default:
					throw new ArgumentException("不支持的 Team123it.Arcaea.MarveCube.Core.SongDifficulty 值。", "songDiff");
			}
		}

		/// <summary>
		/// 使用曲目id, 曲目难度, 曲目完成类型和曲目游玩详情(曲目分数,大Pure数, Pure数, Far数, Lost数和游玩日期时间)初始化 <see cref="SingleScore"/> 类的新实例。
		/// </summary>
		/// <param name="songId">
		/// 曲目id。
		/// <para>若找不到id对应的曲目则会抛出 <see cref="SongNotFoundException"/> 异常。</para>
		/// </param>
		/// <param name="songDiff">
		/// 曲目难度。
		/// <para>若找不到对应曲目难度则 <see cref="SongRating"/> 和 <see cref="ScoreRating"/> 将均为 <see langword="null" /> 。</para>
		/// <para><see cref="SongDifficulty.Past"/> - Past难度</para>
		/// <para><see cref="SongDifficulty.Present"/> - Present难度</para>
		/// <para><see cref="SongDifficulty.Future"/> - Future难度</para>
		/// <para><see cref="SongDifficulty.Beyond"/> - Beyond难度</para>
		/// </param>
		/// <param name="score">曲目成绩。
		/// </param>
		/// <param name="clearType">
		/// 曲目完成类型。
		/// <para><see cref="ClearType.TrackLost"/> - [TL]Track Lost</para>
		/// <para><see cref="ClearType.EasyClear"/> - [EC]Track Complete(简单回忆条)</para>
		/// <para><see cref="ClearType.NormalClear"/> - [NC]Track Complete(普通回忆条)</para>
		/// <para><see cref="ClearType.HardClear"/> - [HC]Track Complete(困难回忆条)</para>
		/// <para><see cref="ClearType.FullRecall"/> - [FR]Full Recall</para>
		/// <para><see cref="ClearType.PureMemory"/> - [PM]Pure Memory</para>
		/// </param>
		/// <param name="bigPureCount">[曲目游玩详情]大Pure(精确Pure)数。</param>
		/// <param name="pureCount">[曲目游玩详情]Pure数。</param>
		/// <param name="farCount">[曲目游玩详情]Far数。</param>
		/// <param name="lostCount">[曲目游玩详情]Lost数。</param>
		/// <param name="playTimeStamp">[曲目游玩详情]游玩时间戳(UNIX时间戳,单位毫秒)。</param>
		/// <exception cref="SongNotFoundException" />
		/// <exception cref="ArgumentException" />
		public SingleScore(string songId, SongDifficulty songDiff, uint score, ClearType clearType, uint bigPureCount, uint pureCount, uint farCount, uint lostCount, ulong playTimeStamp)
		{
			BigPureCount = bigPureCount;
			PureCount = pureCount;
			FarCount = farCount;
			LostCount = lostCount;
			PlayDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(playTimeStamp);
			SongId = songId;
			ClearType = clearType;
			using var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = conn.CreateCommand();
			switch (songDiff)
			{
				case SongDifficulty.Past:
					#region "Past"
					Difficulty = songDiff;
					cmd.CommandText = $"SELECT COUNT(*),rating_pst FROM fixed_songs WHERE sid=?sid";
					cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
					{
						Value = songId
					});
					var r_pst = cmd.ExecuteReader();
					r_pst.Read();
					if (r_pst.GetInt32(0) == 1)
					{
						Score = score;
						double pst_rating = GetRealSongRatingFromDatabaseSongRating(r_pst.GetInt32(1));
						if (pst_rating != -1)
						{
							r_pst.Close();
							conn.Close();
							SongRating = pst_rating;
							ScoreRating = ComputeScoreRating(pst_rating, score);
						}
						else
						{
							r_pst.Close();
							conn.Close();
							SongRating = null;
							ScoreRating = null;
						}
					}
					else
					{
						r_pst.Close();
						conn.Close();
						throw new SongNotFoundException(songId, null);
					}
					#endregion
					break;
				case SongDifficulty.Present:
					#region "Present"
					Difficulty = songDiff;
					cmd.CommandText = $"SELECT COUNT(*),rating_prs FROM fixed_songs WHERE sid=?sid";
					cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
					{
						Value = songId
					});
					var r_prs = cmd.ExecuteReader();
					r_prs.Read();
					if (r_prs.GetInt32(0) == 1)
					{
						Score = score;
						double prs_rating = GetRealSongRatingFromDatabaseSongRating(r_prs.GetInt32(1));
						if (prs_rating != -1)
						{
							r_prs.Close();
							conn.Close();
							SongRating = prs_rating;
							ScoreRating = ComputeScoreRating(prs_rating, score);
						}
						else
						{
							r_prs.Close();
							conn.Close();
							SongRating = null;
							ScoreRating = null;
						}
					}
					else
					{
						r_prs.Close();
						conn.Close();
						throw new SongNotFoundException(songId, null);
					}
					#endregion
					break;
				case SongDifficulty.Future:
					#region "Future"
					Difficulty = songDiff;
					cmd.CommandText = $"SELECT COUNT(*),rating_ftr FROM fixed_songs WHERE sid=?sid";
					cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
					{
						Value = songId
					});
					var r_ftr = cmd.ExecuteReader();
					r_ftr.Read();
					if (r_ftr.GetInt32(0) == 1)
					{
						Score = score;
						double ftr_rating = GetRealSongRatingFromDatabaseSongRating(r_ftr.GetInt32(1));
						if (ftr_rating != -1)
						{
							r_ftr.Close();
							conn.Close();
							SongRating = ftr_rating;
							ScoreRating = ComputeScoreRating(ftr_rating, score);
						}
						else
						{
							r_ftr.Close();
							conn.Close();
							SongRating = null;
							ScoreRating = null;
						}
					}
					else
					{
						r_ftr.Close();
						conn.Close();
						throw new SongNotFoundException(songId, null);
					}
					#endregion
					break;
				case SongDifficulty.Beyond:
					#region "Beyond"
					Difficulty = songDiff;
					cmd.CommandText = $"SELECT COUNT(*),rating_byd FROM fixed_songs WHERE sid=?sid";
					cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
					{
						Value = songId
					});
					var r_byd = cmd.ExecuteReader();
					r_byd.Read();
					if (r_byd.GetInt32(0) == 1)
					{
						Score = score;
						double byd_rating = GetRealSongRatingFromDatabaseSongRating(r_byd.GetInt32(1));
						if (byd_rating != -1)
						{
							r_byd.Close();
							conn.Close();
							SongRating = byd_rating;
							ScoreRating = ComputeScoreRating(byd_rating, score);
						}
						else
						{
							r_byd.Close();
							conn.Close();
							SongRating = null;
							ScoreRating = null;
						}
					}
					else
					{
						r_byd.Close();
						conn.Close();
						throw new SongNotFoundException(songId, null);
					}
					#endregion
					break;
				default:
					throw new ArgumentException("不支持的 Team123it.Arcaea.MarveCube.Core.SongDifficulty 值。", "songDiff");
			}
		}

		/// <summary>
		/// 计算指定分数对应的单曲潜力值。
		/// </summary>
		/// <param name="songRating">对应单曲的难度定数。</param>
		/// <param name="score">要计算的分数。</param>
		/// <returns>计算后的单曲潜力值。</returns>
		public static decimal ComputeScoreRating(double songRating,uint score)
		{
			if (score >= 10000000) //高于1kw分
			{
				return (decimal)songRating + 2M;
			}
			else if (score == 9900000) //990w整(定数+1.5)
			{
				return (decimal)songRating + 1.5M;
			}
			else if (score == 9800000) //980w整(定数+1.0)
			{
				return (decimal)songRating + 1.0M;
			}
			else if (score == 9500000) //950w整(定数)
			{
				return (decimal)songRating;
			}
			else if (score <= (9500000 - songRating * 300000)) //低于通过3w分/0.1定数的计算方式从950w(单曲潜力值=定数)减到刚好使单曲潜力值为0.0时的分数
			{
				return 0M;
			}
			else
			{
				if (score < 9500000) //低于950w(-0.1/-3w)
				{
					decimal r = (decimal)songRating - ((9500000M - score) / 300000M);
					if (r < 0M) r = 0M;
					else r = Math.Round(r, 6);
					return r;
				}
				else if (score > 9500000 && score < 9800000) //高于950w低于980w(+0.1/+3w)
				{
					decimal r = (decimal)songRating + ((score - 9500000M) / 300000M);
					r = Math.Round(r, 6);
					return r;
				}
				else //高于980w低于1kw(+0.1/2w)
				{
					decimal r = (decimal)songRating + 1 + ((score - 9800000M) / 200000M);
					r = Math.Round(r, 6);
					return r;
				}
			}
		}
	

		/// <summary>
		/// 将数据库格式的曲目定数整数转换为实际曲目定数浮点数。
		/// <para>数据库格式: 定数 8.5 表示为 85(整数)</para>
		/// <para>实际格式: 定数 8.5 表示为 8.5(浮点数)</para>
		/// </summary>
		/// <param name="dbSongRating">要转换的数据库格式的曲目定数整数。</param>
		/// <returns>转换后的实际曲目定数浮点数。</returns>
		public static double GetRealSongRatingFromDatabaseSongRating(int dbSongRating)
		{
			if (dbSongRating == -1) return -1D;
			string str = dbSongRating.ToString();
			str = str.Insert(str.Length - 1, ".");
			double r = double.Parse(str);
			return r;
		}
	
		/// <summary>
		/// [Json]获取玩家的单曲最佳成绩。
		/// </summary>
		/// <param name="userid">玩家的用户id。</param>
		/// <param name="songId">曲目id。</param>
		/// <param name="difficulty">曲目难度。</param>
		/// <param name="tableName">
		/// 要查询的数据表名。
		/// <para>对于Ranked曲目,该项值为<b>bests</b>(默认最佳成绩数据表);<br />
		/// 对于Unranked曲目,该项值为<b>bests_special</b>(特殊最佳成绩数据表)</para>
		/// </param>
		/// <param name="isExists">在当前方法返回时, 若玩家存在对应单曲的最佳成绩则本参数值为 <see langword="true" /> , 否则为 <see langword="false" /> 。</param>
		/// <returns>生成的 <see cref="JObject"/> 实例。</returns>
		/// <exception cref="ArcaeaAPIException" />
		public static JObject GetBestScoreJson(uint userid,string songId,SongDifficulty difficulty,string tableName,out bool isExists)
		{
			var r = new JObject();
			try
			{
				using var conn = new MySqlConnection(DatabaseConnectURL);
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"SELECT * FROM {tableName} WHERE user_id={userid} AND song_id=?sid AND difficulty={(int)difficulty};";
				cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
				{
					Value = songId
				});
				var rd = cmd.ExecuteReader();
				if (rd.HasRows) //存在最好成绩
				{
					rd.Read();
					using var conn2 = new MySqlConnection(DatabaseConnectURL);
					conn2.Open();
					var cmd2 = conn2.CreateCommand();
					cmd2.CommandText = $"SELECT name, character_id, is_skill_sealed, is_char_uncapped, favorite_character FROM users WHERE user_id={userid};";
					var rd2 = cmd2.ExecuteReader();
					rd2.Read();
					r.Add("user_id", userid);
					r.Add("song_id", songId);
					r.Add("difficulty", (int)difficulty);
					r.Add("score", rd.GetInt32(3));
					r.Add("shiny_perfect_count", rd.GetInt32(4));
					r.Add("perfect_count", rd.GetInt32(5));
					r.Add("near_count", rd.GetInt32(6));
					r.Add("miss_count", rd.GetInt32(7));
					r.Add("health", rd.GetInt32(8));
					r.Add("modifier", rd.GetInt32(9));
					r.Add("time_played", rd.GetInt64(10));
					r.Add("best_clear_type", rd.GetInt32(11));
					r.Add("clear_type", rd.GetInt32(12));
					r.Add("name", rd2.GetString(0));
					if (rd2.GetInt32(4) != -1)
					{
						r.Add("character", rd2.GetInt32(4));
					}
					else
					{
						r.Add("character", rd2.GetInt32(1));
					}
					r.Add("is_skill_sealed", rd2.GetBoolean(2));
					r.Add("is_char_uncapped", rd2.GetBoolean(3));
					conn2.Close();
					conn.Close();
					isExists = true;
					return r;
				}
				else
				{
					conn.Close();
					isExists = false;
					return r;
				}
			}
			catch
			{
				throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
			}
		}
	}
}
