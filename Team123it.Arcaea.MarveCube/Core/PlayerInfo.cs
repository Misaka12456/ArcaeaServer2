#pragma warning disable IDE0032
#nullable enable
using static Team123it.Arcaea.MarveCube.GlobalProperties;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Team123it.Arcaea.MarveCube.Processors.Background;
using Team123it.Arcaea.MarveCube.Processors.Front;
using World = Team123it.Arcaea.MarveCube.Processors.Background.World;
using System.Linq;

namespace Team123it.Arcaea.MarveCube.Core
{
	/// <summary>
	/// 表示 Arcaea 玩家帐号信息。
	/// </summary>
	public class PlayerInfo
	{
		/// <summary>
		/// 玩家的用户id。
		/// </summary>
		/// <para>玩家不存在则会返回null。</para>
		public uint? UserId { get; private set; }

		/// <summary>
		/// 玩家的好友id(9位)。
		/// </summary>
		public string? UserCode { get; private set; }

		/// <summary>
		/// 玩家的昵称。
		/// </summary>
		public string? UserName { get; private set; }

		/// <summary>
		/// 获取或设置玩家当前游玩的世界模式地图id。
		/// <para>若玩家当前没有游玩任何一个地图,则获取本属性将返回 <see cref="string.Empty"/> ;<br />
		/// 若获取失败,则本属性返回 <see langword="null"/> 。</para>
		/// <para>若设置当前游玩的地图失败,则设置本属性将抛出 <see cref="ArcaeaAPIException"/> 异常。</para>
		/// </summary>
		/// <exception cref="ArcaeaAPIException" />
		public string? CurrentMap
		{
			get
			{
				try
				{
					using var conn = new MySqlConnection(DatabaseConnectURL);
					conn.Open();
					var cmd = conn.CreateCommand();
					cmd.CommandText = ($"SELECT COUNT(current_map),COUNT(*),current_map FROM users WHERE user_id={UserId}");
					var rd = cmd.ExecuteReader();
					rd.Read();
					if (rd.GetInt32(0) == 1) //玩家正在游玩一个地图
					{
						string mapId = rd.GetString(2);
						rd.Close();
						conn.Close();
						if (World.IsDuringEventTime(UserId!.Value, mapId)) //如果玩家正在游玩的地图在活动(Event)时间或该地图存在且永久开放
						{
							return mapId;
						}
						else
						{
							return string.Empty;
						}
					}
					else if (rd.GetInt32(1) == 1) //玩家存在但现在没有游玩任何一个地图
					{ 
						rd.Close();
						conn.Close();
						return string.Empty;
					}
					else //玩家不存在
					{
						rd.Close();
						conn.Close();
						return null;
					}
				}
				catch
				{
					return null;
				}
			}
			set
			{
				try
				{
					var currentMap = World.GetMap(!string.IsNullOrEmpty(value) ? value : string.Empty, out bool isExists);
					if (isExists)
					{
						using var conn = new MySqlConnection(DatabaseConnectURL);
						conn.Open();
						var cmd = conn.CreateCommand();
						cmd.CommandText = $"SELECT is_locked FROM user_world WHERE user_id={UserId!.Value} AND map_id='{value!}';";
						var rd = cmd.ExecuteReader();
						if (rd.Read())
						{
							if (rd.GetInt32(0) == 1)
							{
								rd.Close();
								cmd.CommandText = $"UPDATE user_world SET is_locked=0 WHERE user_id={UserId!.Value} AND map_id='{value!}';";
								cmd.ExecuteNonQuery();
							}
						}
						else
						{
							rd.Close();
							cmd.CommandText = $"INSERT INTO user_world (user_id,map_id,is_locked) VALUES ({UserId!.Value},'{value!}',0);";
							cmd.ExecuteNonQuery();
						}
						if (!rd.IsClosed) rd.Close();
						cmd.CommandText = ($"UPDATE users SET current_map='{value}' WHERE user_id={UserId}");
						if (cmd.ExecuteNonQuery() < 1)
						{
							conn.Close();
							throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
						}
						else
						{
							conn.Close();
						}
					}
					else
					{
						throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.CannotGetThisItem);
					}
				}
				catch(ArcaeaAPIException)
				{
					throw;
				}
				catch (Exception ex) //发生了未知异常
				{
					Console.WriteLine(ex.ToString());
					throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
			}
		}

		/// <summary>
		/// 玩家的最近游玩成绩。
		/// </summary>
		public JObject? RecentScore
		{
			get
			{
				using var conn = new MySqlConnection(DatabaseConnectURL);
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"SELECT COUNT(song_id),song_id,difficulty,score,shiny_perfect_count,perfect_count,near_count,miss_count,health,modifier,time_played,clear_type,rating FROM users WHERE user_id={UserId};";
				/* 0=COUNT(song_id)
				 * 1=song_id
				 * 2=difficulty
				 * 3=score
				 * 4=shiny_perfect_count
				 * 5=perfect_count
				 * 6=near_count
				 * 7=miss_count
				 * 8=health
				 * 9=modifier
				 * 10=time_played
				 * 11=clear_type
				 * 12=rating
				 */
				var rd = cmd.ExecuteReader();
				rd.Read();
				if (rd.GetInt32(0) == 1) //如果存在最近成绩
				{
					var recentScore = new SingleScore(rd.GetString(1), (SongDifficulty)rd.GetInt32(2),
						(uint)rd.GetInt32(3), (ClearType)rd.GetInt32(11), (uint)rd.GetInt32(4), (uint)rd.GetInt32(5), (uint)rd.GetInt32(6), (uint)rd.GetInt32(7), (ulong)rd.GetInt64(10) * 1000);
					int health = rd.GetInt32(8);
					decimal rating = rd.GetDecimal(12);
					var bestClearType = recentScore.ClearType!.Value;
					rd.Close();
					cmd.CommandText = $"SELECT best_clear_type FROM bests WHERE user_id={UserId} AND song_id='{recentScore.SongId}' AND difficulty={(int)recentScore.Difficulty} ";
					var rd2 = cmd.ExecuteReader();
					if (rd2.Read())
					{
						bestClearType = (ClearType)rd2.GetInt32(0);
						rd2.Close();
					}
					conn.Close();
					var r = new JObject()
					{
						{"song_id",recentScore.SongId },
						{"difficulty",(int)recentScore.Difficulty },
						{"score",recentScore.Score!.Value },
						{"shiny_perfect_count",recentScore.BigPureCount },
						{"perfect_count",recentScore.PureCount },
						{"near_count",recentScore.FarCount },
						{"miss_count",recentScore.LostCount },
						{"best_clear_type",(int)bestClearType },
						{"clear_type",(int)recentScore.ClearType },
						{"health",health },
						{"time_played",(long)Math.Floor((recentScore.PlayDate!.Value - new DateTime(1970,1,1)).TotalMilliseconds)},
						{"modifier",0 },
						{"rating",rating }
					};
					return r;
				} else
				{
					rd.Close();
					conn.Close();
					return null;
				}
			}
		}

		/// <summary>
		/// 玩家的好友列表。
		/// </summary>
		public JArray FriendsList
		{
			get
			{
				var r = new JArray();
				try
				{
					using var conn = new MySqlConnection(DatabaseConnectURL);
					conn.Open();
					var cmd = conn.CreateCommand();
					cmd.CommandText = $"SELECT user_id_other FROM friend WHERE user_id_me={UserId}";
					var rd = cmd.ExecuteReader();
					var friendsIds = new List<uint>();
					while (rd.Read())
					{
						friendsIds.Add((uint)rd.GetInt32(0));
					}
					rd.Close();
					foreach (uint friendId in friendsIds) //遍历玩家的所有好友的id
					{
						var singleR = new JObject();
						var friend = new PlayerInfo(friendId, out _);
						singleR.Add("user_id", friend.UserId);
						singleR.Add("name", friend.UserName);
						singleR.Add("recent_score", (friend.RecentScore != null) ? new JArray() { { friend.RecentScore } } : new JArray());
						singleR.Add("rating", friend.PotentialInt);
						cmd.CommandText = $"SELECT * FROM users WHERE user_id={friendId}";
						var rd2 = cmd.ExecuteReader();
						rd2.Read();
						singleR.Add("character", rd2.GetInt32(6));
						singleR.Add("join_date", rd2.GetInt64(4));
						singleR.Add("is_skill_sealed", rd2.GetInt32(7) == 1);
						singleR.Add("is_char_uncapped", rd2.GetInt32(8) == 1);
						singleR.Add("is_char_uncapped_override", rd2.GetInt32(9) == 1);
						rd2.Close();
						r.Add(singleR);
					}
					conn.Close();
					return r;
				}
				catch
				{
					return r;
				}
			}
		}

		/// <summary>
		/// 玩家的角色列表。
		/// </summary>
		public JArray CharactersList
		{
			get
			{
				var r = new JArray();
				try 
				{
					using var conn = new MySqlConnection(DatabaseConnectURL);
					conn.Open();
					using var conn2 = new MySqlConnection(DatabaseConnectURL);
					conn2.Open();
					var cmd = conn.CreateCommand();
					cmd.CommandText = $"SELECT * FROM user_chars WHERE user_id={UserId}";
					var rd = cmd.ExecuteReader();
					while (rd.Read())
					{
						var cmd2 = conn2.CreateCommand();
						cmd2.CommandText = $"SELECT character_nameid FROM fixed_characters where character_id={rd.GetInt32(1)}";
						var rd2 = cmd2.ExecuteReader();
						if (rd2.Read())
						{
							// rd:用户的角色数据(除角色名外完整数据/可变)
							// rd2:角色数据(仅角色名/固定)
							var charData = new JObject()
							{
								{"character_id",rd.GetInt32(1) },
								{"name",rd2.GetString(0) },
								{"level",rd.GetInt32(2) },
								{"exp",rd.GetInt32(3)},
								{"level_exp",rd.GetInt32(4)},
								{"frag",rd.GetInt32(5) },
								{"prog",rd.GetInt32(6) },
								{"overdrive",rd.GetInt32(7) },
								{"skill_id",rd.IsDBNull(8) ? string.Empty : rd.GetString(8) },
								{"skill_unlock_level",rd.GetInt32(9) },
								{"skill_requires_uncap",rd.GetBoolean(10) },
								{"skill_id_uncap",rd.IsDBNull(11) ? string.Empty : rd.GetString(11) },
								{"char_type",rd.GetInt32(12) },
								{"uncap_cores",new JArray() },
								{"is_uncapped",rd.GetBoolean(13) },
								{"is_uncapped_override",rd.GetBoolean(14) }
							};
							if (charData.Value<int>("character_id") == 21) // 硬编码: 包含语音的搭档[莲(Ren)]
							{
								charData.Add("voice",JArray.FromObject(new[] { 0, 1, 2, 3, 100, 1000, 1001 }));
							}
							r.Add(charData);
						}
						rd2.Close();
						
					}
					rd.Close();
					conn2.Close();
					conn.Close();
					return r;
				}
				catch
				{
					return r;
				}
			}
		}

		/// <summary>
		/// 玩家的个人潜力值。
		/// </summary>
		public double? Potential
		{
			get
			{
				if (PotentialInt == null)
				{
					return null;
				} else
				{
					return Math.Round((double)(PotentialInt.Value / 100),2);
				}
			}
		}

		/// <summary>
		/// 玩家的个人信用点数(共12点)。
		/// </summary>
		public int? CreditPoint
		{
			get
			{
				try
				{
					if (UserId.HasValue)
					{
						var conn = new MySqlConnection(DatabaseConnectURL);
						conn.Open();
						var cmd = conn.CreateCommand();
						cmd.CommandText = $"SELECT credit_point FROM users WHERE user_id={UserId.Value};";
						int r = Convert.ToInt32(cmd.ExecuteScalar());
						conn.Close();
						return r;
					}
					else
					{
						return null;
					}
				}
				catch
				{
					return null;
				}
			}
		}

		public JArray? PurchasedItemsList { get; set; }

		public JArray? ClaimedPresentsList { get; set; }

		/// <summary>
		/// 玩家的个人潜力值(API/数据库版)。
		/// <para>常规潜力值格式: 潜力值 12.76 实际 12.76(浮点数)</para>
		/// <para>API/数据库版潜力值格式: 潜力值 12.76 实际 1276(整数)</para>
		/// </summary>
		public uint? PotentialInt { get=>_potentialint; }
		private uint? _potentialint;

		/// <summary>
		/// 玩家的记忆源点数。
		/// </summary>
		public int? Ticket
		{
			get
			{
				return _ticket;
			}
			set
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
				else
				{
					_ticket = value;
				}
			}
		}
		private int? _ticket;

		/// <summary>
		/// 玩家账号是否已被冻结。
		/// </summary>
		public bool? Banned { get; private set; }

		/// <summary>
		/// 玩家获得的世界模式曲目列表(不包括Beyond曲目)。
		/// </summary>
		public JArray WorldSongsList { get; set; }

		/// <summary>
		/// 使用玩家的用户id 初始化 <see cref="PlayerInfo"/> 类的新实例。
		/// </summary>
		/// <param name="userid">玩家的用户id(非好友id)。</param>
		/// <param name="isExists">在当前方法返回时, 若对应玩家存在则此参数值为 <see langword="true" /> , 否则为 <see langword="false"/> 。</param>
		public PlayerInfo(uint userid, out bool isExists)
		{
			UserId = userid;
			using var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = new MySqlCommand($"SELECT COUNT(*),user_code,name,user_rating,ticket,is_banned,purchases,claimed_presents,world_songs FROM users WHERE user_id={userid}",conn);
			// var tr = conn.BeginTransaction();
			var rd = cmd.ExecuteReader();
			rd.Read();
			if (rd.GetInt32(0) == 1) //如果玩家存在
			{
				UserCode = rd.GetString(1);
				UserName = rd.GetString(2);
				_potentialint = (uint)rd.GetInt32(3);
				_ticket = rd.GetInt32(4);
				Banned = rd.GetBoolean(5);
				PurchasedItemsList = (!rd.IsDBNull(6) && !string.IsNullOrWhiteSpace(rd.GetString(6))) ? JArray.Parse(rd.GetString(6)) : null;
				ClaimedPresentsList = (!rd.IsDBNull(7) && !string.IsNullOrWhiteSpace(rd.GetString(7))) ? JArray.Parse(rd.GetString(7)) : null;
				WorldSongsList = (!rd.IsDBNull(8) && !string.IsNullOrWhiteSpace(rd.GetString(8))) ? JArray.Parse(rd.GetString(8)) : new JArray();
				rd.Close();
				conn.Close();
				isExists = true;
			}
			else
			{
				rd.Close();
				conn.Close();
				UserCode = null;
				UserName = null;
				_potentialint = null;
				_ticket = null;
				Banned = null;
				PurchasedItemsList = null;
				ClaimedPresentsList = null;
				WorldSongsList = new JArray();
				isExists = false;
			}
		}

		/// <summary>
		/// 使用玩家的 好友id 初始化 <see cref="PlayerInfo"/> 类的新实例。
		/// </summary>
		/// <param name="usercode">玩家的9位好友id。</param>
		/// <param name="isExists">在当前方法返回时, 若对应玩家存在则此参数值为 <see langword="true" /> , 否则为 <see langword="false"/> 。</param>
		public PlayerInfo(string usercode, out bool isExists)
		{
			UserCode = usercode;
			var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = new MySqlCommand($"SELECT COUNT(*),user_id,name,user_rating,ticket,is_banned,purchases,claimed_presents,world_songs FROM users WHERE user_code=?usercode", conn);
			cmd.Parameters.Add(new MySqlParameter("?usercode", usercode));
			var rd = cmd.ExecuteReader();
			rd.Read();
			if (rd.GetInt32(0) == 1) //如果玩家存在
			{
				UserId = (uint)rd.GetInt32(1);
				UserName = rd.GetString(2);
				_potentialint = (uint)rd.GetInt32(3);
				_ticket = rd.GetInt32(4);
				Banned = rd.GetBoolean(5);
				PurchasedItemsList = (!rd.IsDBNull(6)) ? JArray.Parse(rd.GetString(6)) : new JArray();
				ClaimedPresentsList = (!rd.IsDBNull(7) && !string.IsNullOrWhiteSpace(rd.GetString(7))) ? JArray.Parse(rd.GetString(7)) : null;
				WorldSongsList = (!rd.IsDBNull(8) && !string.IsNullOrWhiteSpace(rd.GetString(8))) ? JArray.Parse(rd.GetString(8)) : new JArray();
				rd.Close();
				conn.Close();
				isExists = true;
			} else
			{
				rd.Close();
				conn.Close();
				UserId = null;
				UserName = null;
				PurchasedItemsList = new JArray();
				ClaimedPresentsList = null;
				_potentialint = null;
				_ticket = null;
				Banned = false;
				WorldSongsList = new JArray();
				isExists = false;
			}
		}

		public static PlayerInfo? CreateFromUsername(string username)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = "SELECT user_id FROM users WHERE name=?name;";
				cmd.Parameters.Add(new MySqlParameter("?name", MySqlDbType.VarChar)
				{
					Value = username
				});
				var rd = cmd.ExecuteReader();
				if (rd.Read())
				{
					uint userId = rd.GetUInt32(0);
					rd.Close();
					var p = new PlayerInfo(userId, out bool isExists);
					if (isExists)
					{
						return p!;
					}
					else
					{
						return null;
					}
				}
				else
				{
					rd.Close();
					return null;
				}
			}
			catch(ArcaeaAPIException)
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
		/// 获取当前 <see cref="PlayerInfo"/> 实例对应的玩家的用户基础信息数据(即aggregate API返回的第一个数组(value=0)数据(/user/me))。
		/// </summary>
		///	<returns>成功返回数据 <see cref="JObject"/> 实例; 失败抛出异常。</returns>
		///	<exception cref="ArcaeaAPIException" />
		public JObject GetUserBaseInfoData()
		{
			try
			{
				// bool isBeyondChapterUnlocked = true;
				var packs = Purchase.GetUserPurchasedItemsList(UserId!.Value, ItemType.Pack);
				packs.Add("base");
				var r = new JObject() 
				{
					{"is_aprilfools", IsOverrideAprilFools || IsDuringAprilFools() }, //愚人节判断(4.1)
					{"curr_available_maps",new JArray()},
					{"character_stats",CharactersList },
					{"friends",FriendsList },
					{"is_locked_name_duplicate",false },
					{"prog_boost",0 },
					{"next_fragstam_ts",-1 },
					{"world_unlocks",new JArray() },
					{"cores",new JArray() },
					{"packs", packs },
					{"singles", Purchase.GetUserPurchasedItemsList(UserId!.Value, ItemType.SingleSong) },
					{"max_friend",50 },
					{"original_name",string.Empty },
					{"online_play_count", 10 },
					{"platform",new JArray(0)
						{
							{"ios"} 
						}
					},
					{"warning_ts",0 },
					{"offence_count",0 },
					{"ban_reason_ts",0 },
					{"ban_reason",0 }
				};
				if (LeaderBoard.TryGetTotalScoreRank(UserId!.Value, out int? rank, TopRankLimit))
				{
					r.Add("global_rank", rank!.Value);
				}
				var worldSongs = WorldSongsList;
				using var conn = new MySqlConnection(DatabaseConnectURL);
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"SELECT value FROM fixed_properties WHERE `key`='is_byd_chapter_unlocked';";
				Console.WriteLine(worldSongs.ToString());
				if (Convert.ToBoolean(Convert.ToInt32(cmd.ExecuteScalar())))
				{
					worldSongs.Merge(FixedDatas.GetPlayerAllOwnBeyondSongIds(UserId!.Value));
					Console.WriteLine(worldSongs.ToString());
				}
				r.Add("world_songs", worldSongs);
				cmd.CommandText = $"SELECT * FROM users WHERE user_id={UserId}";
				var rd = cmd.ExecuteReader();
				if (rd.Read()) //如果有玩家数据
				{
					var charactersList = CharactersList;
					var characters = new JArray();
					foreach (JObject character in CharactersList)
					{
						characters.Add(character.Value<int>("character_id"));
					}
					/* 玩家数据内容:
					 * 0=用户id 1=好友id 2=昵称 3=密码(SHA256加密) 4=用户加入服务器(创建用户)时的时间戳 5=个人潜力值(API/数据库格式)
					 * 6=正在使用的角色id 7=角色技能是否被封印 8=角色是否觉醒(is_char_uncapped)
					 * 9=角色的Over值功能是否已经解锁(is_char_uncapped_override)
					 * 10=是否不显示个人潜力值(is_hide_rating) 11=最近游玩曲目id 12=最近游玩曲目难度 13=最近游玩成绩
					 * 14=最近游玩大Pure数(shiny_perfect_count) 15=最近游玩Pure数(perfect_count) 16=最近游玩Far数(near_count)
					 * 17=最近游玩Lost数(miss_count) 18=最近游玩结束时的回忆度(health) 19=保留参数(modifier) 20=最近游玩时间戳(time_played)
					 * 21=最近游玩完成类型(clear_type) 22=最近游玩单曲潜力值(rating) 23=星标角色id(max_stamina_notification_enabled)
					 * 24=是否启用体力恢复提醒(max_stamina_notification_enabled) 25=当前游玩的World模式地图(current_map)
					 * 26=云端数据的来源设备id(cloud_device_id) 27=云端数据的来源设备名(cloud_device_name)
					 * 28=云端数据:曲目解锁数据(unlock_list) 29=记忆源点(ticket)
					 * 30=恢复到满体力后对应的时间戳(world_time_fullrecharged)(参考:Processor.Background.World.CalculateCurrentStaminas())
					 * 31=玩家账号是否被冻结(is_banned) 32=玩家的E-mail 33=玩家的信用点数(credit_point)
					 * 34=玩家信用点数变化原因记录(credit_edit_reasons)
					 */
					r.Add("settings", new JObject()
					{
						{"is_hide_rating",rd.GetBoolean(10) },
						{"max_stamina_notification_enabled",rd.GetBoolean(24) },
						{"favorite_character",rd.GetInt32(23) }
					});
					r.Add("user_id", UserId);
					r.Add("name", rd.GetString(2));
					r.Add("user_code", rd.GetString(1));
					r.Add("display_name", rd.GetString(2));
					r.Add("email", rd.GetString(32));
					r.Add("ticket", rd.GetInt64(29));
					r.Add("character", rd.GetInt32(6));
					r.Add("is_skill_sealed", rd.GetBoolean(7));
					r.Add("current_map", (!rd.IsDBNull(25) && World.IsDuringEventTime(UserId!.Value, rd.GetString(25))) ? rd.GetString(25) : string.Empty);
					r.Add("stamina", (rd.GetInt32("overflow_staminas") > 0) ? (World.FullStaminas + rd.GetInt32("overflow_staminas")) : World.CalculateCurrentStaminas(rd.IsDBNull(30) ? DateTime.Now : rd.GetDateTime(30), out _));
					r.Add("max_stamina_ts", (rd.GetInt32("overflow_staminas") > 0 || rd.IsDBNull(30)) ? 0 : Convert.ToInt64((rd.GetDateTime(30) - DateTime.UnixEpoch).TotalMilliseconds));
					r.Add("characters", characters);
					r.Add("warning_count", (12 - rd.GetInt32(33) <= 0) ? 0 : Convert.ToInt32((12M - rd.GetInt32(33)) / 3));
					var recent_score = new JArray();
					if (RecentScore != null)
					{
						recent_score.Add(RecentScore!);
					}
					r.Add("recent_score", recent_score);
					r.Add("rating", rd.GetInt32(5));
					r.Add("join_date", rd.GetInt64(4));
					bool isNullWorldTimeFullRecharged = rd.IsDBNull(30);
					bool isCurrentMapIsEventAndNotDuringTime = false;
					if (!rd.IsDBNull(25))
					{
						isCurrentMapIsEventAndNotDuringTime = !World.IsDuringEventTime(UserId!.Value, rd.GetString(25));
					}
					rd.Close();
					if (isNullWorldTimeFullRecharged)
					{
						cmd.CommandText = $"UPDATE users SET world_time_fullrecharged='{DateTime.Now:yyyy-M-d H:mm:ss.fff}' WHERE user_id={UserId!.Value};";
						cmd.ExecuteNonQuery();
					}
					if (isCurrentMapIsEventAndNotDuringTime)
					{
						cmd.CommandText = $"UPDATE users SET current_map=?currentMap WHERE user_id={UserId!.Value};";
						cmd.Parameters.Add(new MySqlParameter("?currentMap", MySqlDbType.VarChar)
						{
							Value = DBNull.Value
						});
						cmd.ExecuteNonQuery();
					}
					conn.Close();
					return r;
				}
				else //如果没有玩家数据
				{
					rd.Close();
					conn.Close();
					throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.UserNotExist);
				}
			}
			catch(ArcaeaAPIException)
			{
				throw;
			}
			catch (Exception)
			{
				throw;//throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
			}
		}

		/// <summary>
		/// 将当前 <see cref="PlayerInfo"/> 实例中的数据更新为数据库中对应的玩家的最新信息数据。
		/// </summary>
		/// <returns>
		/// 数据更新后的玩家信息对应的原 <see cref="PlayerInfo"/> 实例。
		/// </returns>
		/// <exception cref="ArcaeaAPIException" />
		/// <exception cref="NullReferenceException" />
		public PlayerInfo RefreshData()
		{
			if (!UserId.HasValue)
			{
				throw new NullReferenceException("Current instance is a empty instance of PlayerInfo (UserId is null).");
			}
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"SELECT COUNT(*),user_code,name,user_rating,ticket,is_banned,purchases,claimed_presents FROM users WHERE user_id={UserId}";
				var rd = cmd.ExecuteReader();
				rd.Read();
				if (rd.GetInt32(0) == 1) //如果玩家存在
				{
					UserCode = rd.GetString(1);
					UserName = rd.GetString(2);
					_potentialint = (uint)rd.GetInt32(3);
					_ticket = rd.GetInt32(4);
					Banned = rd.GetBoolean(5);
					PurchasedItemsList = (!rd.IsDBNull(6) && !string.IsNullOrWhiteSpace(rd.GetString(6))) ? JArray.Parse(rd.GetString(6)) : null;
					ClaimedPresentsList = (!rd.IsDBNull(7) && !string.IsNullOrWhiteSpace(rd.GetString(7))) ? JArray.Parse(rd.GetString(7)) : null;
					rd.Close();
					return this;
				}
				else
				{
					rd.Close();
					throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.UserNotExist);
				}
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
		/// 将当前使用中的角色升级.
		/// <para>本方法通常在World模式部分会被调用.</para>
		/// </summary>
		/// <param name="addedExp">获得的经验值.</param>
		/// <param name="level">升级完成后的角色等级.</param>
		/// <returns>升级完成后的角色总经验数.</returns>
		/// <exception cref="ArcaeaAPIException" />
		public int UpgradeCharacterLevel(int addedExp,out int level)
		{
			try
			{
				using var conn = new MySqlConnection(DatabaseConnectURL);
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"SELECT level,exp,level_exp FROM user_chars WHERE user_id={UserId} AND character_id=(SELECT character_id FROM users WHERE user_id={UserId});";
				var rd = cmd.ExecuteReader();
				if (rd.Read())
				{
					int beforeLevel = rd.GetInt32(0);
					int beforeExp = rd.GetInt32(1);
					int beforeLevelExp = rd.GetInt32(2);
					rd.Close();
					int afterExp = beforeExp + addedExp;
					if (afterExp < beforeLevelExp)
					{
						cmd.CommandText = $"UPDATE user_chars SET exp={afterExp} WHERE user_id={UserId} AND character_id=(SELECT character_id FROM users WHERE user_id={UserId});";
						cmd.ExecuteNonQuery();
						conn.Close();
						level = beforeLevel;
						return afterExp;
					} 
					else
					{
						cmd.CommandText = $"SELECT character_id FROM users WHERE user_id={UserId}";
						int character_id = (int)cmd.ExecuteScalar();
						using var conn2 = new MySqlConnection(DatabaseConnectURL);
						conn2.Open();
						var cmd2 = conn2.CreateCommand();
						cmd2.CommandText = $"SELECT maxLevel,level_exps,frag,prog,overdrive FROM fixed_characters WHERE character_id={character_id}";
						var rd2 = cmd2.ExecuteReader();
						rd2.Read();
						int maxLevel = rd2.GetInt32(0);
						var level_exps = JArray.Parse(rd2.GetString(1));
						var frag = JArray.Parse(rd2.GetString(2));
						var prog = JArray.Parse(rd2.GetString(3));
						var overdrive = JArray.Parse(rd2.GetString(4));
						rd2.Close();
						conn2.Close();
						int remainExp = afterExp;
						if (beforeLevel == maxLevel) //如果角色当前等级已是最高级
						{
							conn.Close();
							level = maxLevel;
							return (int)level_exps[maxLevel - 1];
						}
						else
						{
							int i = -1;
							for (i = beforeLevel; i < maxLevel; i++)
							{
								if (remainExp > (int)level_exps[i - 1]!) //如果剩余的经验值大于当前等级经验值
								{
									continue;
								}
								else
								{
									afterExp = remainExp;
									level = i;
									uint afterLevelExp = (uint)level_exps[i - 1]!;
									cmd.CommandText = $"UPDATE user_chars SET exp={afterExp},level={i},level_exp={afterLevelExp}," +
										$"frag={(uint)frag[i - 1]!}, prog={(uint)prog[i - 1]!}, overdrive={(uint)overdrive[i - 1]!}" +
										$" WHERE user_id={UserId} AND character_id=(SELECT character_id FROM users WHERE user_id={UserId});";
									cmd.ExecuteNonQuery();
									conn.Close();
									return afterExp;
								}
							}
							if (i == maxLevel)
							{
								cmd.CommandText = $"UPDATE user_chars SET exp={(uint)level_exps[maxLevel - 1]},level={maxLevel},level_exp={(uint)level_exps[maxLevel - 1]},frag={(uint)frag[maxLevel - 1]!}," +
									$" prog={(uint)prog[maxLevel - 1]!}, overdrive={(uint)overdrive[maxLevel - 1]!}" +
									$" WHERE user_id={UserId} AND character_id=(SELECT character_id FROM users WHERE user_id={UserId});";
								cmd.ExecuteNonQuery();
								conn.Close();
								level = maxLevel;
								return (int)level_exps[maxLevel - 1];
							}
							conn.Close();
							throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
						}
					}
				}
				else
				{
					rd.Close();
					int beforeLevel = 1;
					cmd.CommandText = $"SELECT character_id FROM users WHERE user_id={UserId}";
					int character_id = Convert.ToInt32((long)cmd.ExecuteScalar());
					using var conn2 = new MySqlConnection(DatabaseConnectURL);
					conn2.Open();
					var cmd2 = conn2.CreateCommand();
					cmd2.CommandText = $"SELECT maxLevel,level_exps,frag,prog,overdrive WHERE character_id={character_id}";
					var rd2 = cmd2.ExecuteReader();
					rd2.Read();
					int maxLevel = rd2.GetInt32(0);
					var level_exps = JArray.Parse(rd2.GetString(1));
					var frag = JArray.Parse(rd2.GetString(2));
					var prog = JArray.Parse(rd2.GetString(3));
					var overdrive = JArray.Parse(rd2.GetString(4));
					rd2.Close();
					conn2.Close();
					int remainExp = (int)addedExp;
					int i;
					for (i = beforeLevel; i < maxLevel; i++)
					{
						if (remainExp > (int)level_exps[i - 1]!) //如果剩余的经验值大于当前等级经验值
						{
							continue;
						}
						else
						{
							int afterExp = (int)remainExp;
							level = i;
							int afterLevelExp = (int)level_exps[i - 1]!;
							cmd.CommandText = $"UPDATE user_chars SET exp={afterExp},level={i},level_exp={afterLevelExp}," +
								$"frag={(uint)frag[i - 1]!}, prog={(uint)prog[i - 1]!}, overdrive={(uint)overdrive[i - 1]!}" +
								$" WHERE user_id={UserId} AND character_id=(SELECT character_id FROM users WHERE user_id={UserId});";
							cmd.ExecuteNonQuery();
							conn.Close();
							return afterExp;
						}
					}
					if (i == maxLevel)
					{
						cmd.CommandText = $"UPDATE user_chars SET exp={(uint)level_exps[maxLevel - 1]},level={maxLevel},level_exp={(uint)level_exps[maxLevel - 1]},frag={(uint)frag[maxLevel - 1]!}," +
							$" prog={(uint)prog[maxLevel - 1]!}, overdrive={(uint)overdrive[maxLevel - 1]!}" +
							$" WHERE user_id={UserId} AND character_id=(SELECT character_id FROM users WHERE user_id={UserId});";
						cmd.ExecuteNonQuery();
						conn.Close();
						level = maxLevel;
						return (int)level_exps[maxLevel - 1];
					}
					conn.Close();
					throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
			}
			catch (ArcaeaAPIException ex)
			{
				Console.WriteLine("Normal throw: " + ex.ToString());
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
			}
		}

		public static bool IsDuringAprilFools()
		{
			var now = DateTime.Now;
			return now >= AprilFoolsStartTime && now <= new DateTime(now.Year, 4, 2, 0, 0, 0);
		}
	}
}
