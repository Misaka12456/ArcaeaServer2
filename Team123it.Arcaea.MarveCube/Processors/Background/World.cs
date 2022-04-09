#nullable enable
using static Team123it.Arcaea.MarveCube.GlobalProperties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Team123it.Arcaea.MarveCube.Core;
using MySql.Data.MySqlClient;
using System.Linq;

namespace Team123it.Arcaea.MarveCube.Processors.Background
{

	/// <summary>
	/// 提供世界模式(World模式)相关的 <see langword="static" /> 方法的类。无法继承此类。
	/// </summary>
	public class World
	{
		/// <summary>
		/// 满体力数。
		/// </summary>
		public const int FullStaminas = 12;

		/// <summary>
		/// 每次游玩消耗的体力数。
		/// </summary>
		public const int StaminasPerCost = 2;

		/// <summary>
		/// 每次游玩Beyond地图消耗的体力数。
		/// </summary>
		public const int BeyondStaminasPerCost = 3;

		/// <summary>
		/// 每次源韵强化(x4)消耗的记忆源点数。
		/// </summary>
		public const int ProgBoostMemories = 50;

		/// <summary>
		/// 获取所有地图数据。
		/// </summary>
		/// <param name="userid">玩家的用户id。</param>
		/// <param name="success">在当前方法返回时, 若成功获取数据则本参数值为 <see langword="true" /> , 否则为 <see langword="false"/> 。</param>
		/// <returns>成功返回所有地图的 <see cref="JArray"/> 实例, 失败返回 <see cref="JArray"/> 的默认空实例。</returns>
		public static JArray GetAllMaps(uint userid,out bool success)
		{
			var r = new JArray();
			try
			{
				using var conn = new MySqlConnection(DatabaseConnectURL);
				conn.Open();
				var mapNames = GetAllMapsName(); //获取所有地图id
				bool isEventMapTesting = IsWorldEventMapTesting(out var testPlayers) && testPlayers.Contains((int)userid);
				if (mapNames != null) //如果世界模式地图集合不为空
				{
					foreach (string mapName in mapNames)
					{
						var info = GetMap(mapName,out _);
						var steps = info.Value<JArray>("steps");
						info.Remove("steps");
						var rewards = new JArray();
						foreach (var step in steps) //遍历当前地图每个Step
						{
							var stepInfo = (JObject)step;
							if (stepInfo.TryGetValue("items",out _)) //如果当前Step存在奖励物品(Items)
							{
								rewards.Add(new JObject()
								{
									{"items",stepInfo["items"] },
									{"position",stepInfo["position"] }
								});
							}
						}
						if (isEventMapTesting && info.Value<long>("available_from") != -1)
						{
							var eventStartTime = DateTime.UnixEpoch.AddMilliseconds(info.Value<long>("available_from"));
							if (eventStartTime > DateTime.Now)
							{
								info.Remove("available_from");
								info.Add("available_from", 0); //将向测试玩家下发的限时地图信息里的地图起始时间戳改成0,从而使测试玩家能进入地图
							}
						}
						info.Remove("rewards");
						info.Add("rewards", rewards);
						var cmd = conn.CreateCommand();
						cmd.CommandText = $"SELECT * FROM user_world WHERE map_id='{mapName}' AND user_id={userid};";
						var rd = cmd.ExecuteReader();
						info.Remove("curr_position");
						info.Remove("curr_capture");
						info.Remove("is_locked");
						if (rd.HasRows)
						{
							rd.Read();
							info.Add("curr_position", rd.GetInt32(2));
							info.Add("curr_capture", rd.GetDouble(3));
							info.Add("is_locked", rd.GetBoolean(4));
							rd.Close();
						}
						else
						{
							info.Add("curr_position", 0);
							info.Add("curr_capture", 0);
							info.Add("is_locked", true);
							rd.Close();
							cmd.CommandText = $"INSERT INTO user_world VALUES ({userid},'{mapName}',0,0,1)";
							cmd.ExecuteNonQuery();
						}
						r.Add(info);
					}
					conn.Close();
					success = true;
					return r;
				}
				else
				{
					conn.Close();
					success = false;
					return r;
				}
			}
			catch
			{
				success = false;
				return r;
			}
		}

		/// <summary>
		/// 获取指定地图的完整数据。
		/// </summary>
		/// <param name="mapid">地图id。</param>
		/// <param name="isExists">在当前方法返回时, 若指定id对应的地图存在且成功获取完整数据则本参数值为 <see langword="true" /> , 否则为 <see langword="false"/> 。</param>
		/// <returns>成功返回当前地图的 <see cref="JObject"/> 实例, 失败返回 <see cref="JObject"/> 的默认空实例。</returns>
		public static JObject GetMap(string mapid,out bool isExists)
		{
			var r = new JObject();
			try
			{
				string currentMap = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "static", "WorldMap", $"{mapid}.json"),System.Text.Encoding.UTF8);
				r = JObject.Parse(currentMap);
				isExists = true;
				return r;
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
				isExists = false;
				return r;
			}
		}

		/// <summary>
		/// 获取玩家指定地图的数据。
		/// </summary>
		/// <param name="userid">玩家的用户id。</param>
		/// <param name="mapid">地图id。</param>
		/// <param name="success">在当前方法返回时, 若指定id对应的地图存在且成功获取完整数据则本参数值为 <see langword="true" /> , 否则为 <see langword="false"/> 。</param>
		/// <returns>成功返回地图的 <see cref="JObject"/> 实例, 失败返回 <see cref="JObject"/> 的默认空实例。</returns>
		public static JObject GetUserMap(uint userid,string mapid,out bool success)
		{
			var r = new JObject();
			try
			{
				using var conn = new MySqlConnection(DatabaseConnectURL);
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"SELECT * FROM user_world WHERE user_id={userid} AND map_id='{mapid}'";
				var rd = cmd.ExecuteReader();
				r.Add("user_id", userid);
				r.Add("map_id", mapid);
				if (rd.Read()) //如果玩家已有该地图数据
				{
					r.Add("curr_position", rd.GetInt32(2));
					r.Add("curr_capture", rd.GetDouble(3));
					r.Add("is_locked", rd.GetBoolean(4));
					rd.Close();
				}
				else //如果玩家没有该地图数据
				{
					rd.Close();
					cmd.CommandText = $"INSERT INTO user_world (user_id,map_id,is_locked) VALUES ({userid},'{mapid}',1);";
					cmd.ExecuteNonQuery();
					r.Add("curr_position", 0);
					r.Add("curr_capture", 0m);
					r.Add("is_locked", true);
				}
				conn.Close();
				success = true;
				return r;
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
				success = false;
				return r;
			}
		}

		/// <summary>
		/// 获取所有地图的名称列表。
		/// </summary>
		/// <returns>成功返回所有地图名称 <see cref="List"/> , 失败返回 <see langword="null" /> 。</returns>
		public static List<string>? GetAllMapsName()
		{
			try
			{
				var r = new List<string>();
				foreach (string mapFilePath in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "data", "static", "WorldMap")))
				{
					var mapFile = new FileInfo(mapFilePath);
					string mapId = mapFile.Name.Split(".json")[0];
					r.Add(mapId);
				}
				return r;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// 计算当前实际拥有的体力数量(通过恢复到满体力后对应的时间戳)
		/// <para>本方法计算使用的满体力数为常量 <see cref="FullStaminas" /> 的数值。</para>
		/// </summary>
		/// <param name="timeToFullRecharged">恢复到满体力后对应的时间戳(单位:秒)。
		/// <para>即 到达该时间戳时刚好恢复到满体力(12体力)状态。</para></param>
		/// <returns></returns>
		public static uint CalculateCurrentStaminas(DateTime fullRechargedTime, out bool isOverflowStamina)
		{
			var rechargingSpan = fullRechargedTime - DateTime.Now;
			Console.WriteLine("fullRechargedTime = " + fullRechargedTime);
			Console.WriteLine("rechargingSpan = " + rechargingSpan);
			if (rechargingSpan.Ticks <= 0) //如果相差时间小于等于0(已过恢复到满体力时的时间)
			{
				if (rechargingSpan.TotalMinutes <= -30)
				{
					isOverflowStamina = true;
				}
				else
				{
					isOverflowStamina = false;
				}
				return FullStaminas;
			}
			else
			{
				isOverflowStamina = false;
				if ((rechargingSpan.TotalMinutes / 30) < FullStaminas) //如果时间小于需要恢复所有体力所用的最小时间(0.5小时*满体力数)
				{
					uint r = (uint)Convert.ToInt32(FullStaminas - Math.Ceiling(rechargingSpan.TotalMinutes / 30));
					Console.WriteLine("rechargingSpan.TotalMinutes / 30 = " + (rechargingSpan.TotalMinutes / 30));
					// 当前体力数 = 满体力数 - 取高于当前小数的最小整数[分钟数/30]
					return r;
				}
				else //否则(当前体力为0)
				{
					return 0;
				}
			}
		}

		/// <summary>
		/// 将当前游玩成绩标记为世界(World)模式游玩成绩并开始游玩。
		/// </summary>
		/// <param name="userid">玩家的用户id。</param>
		/// <param name="songid">游玩的曲目id。</param>
		/// <param name="difficulty">游玩的曲目难度。</param>
		/// <param name="stamina_multiply">[可选][遗产章节/Play+专用]体力加成倍数。</param>
		/// <param name="fragment_multiply">[可选][遗产章节/Play+专用]残片加成倍数。</param>
		/// <param name="prog_boost_multiply">[可选][非遗产章节]源韵强化加成倍数。</param>
		/// <returns>游玩开始后剩余的体力数量.</returns>
		/// <exception cref="ArcaeaAPIException" />
		public static int StartWorldPlay(uint userid, string songid, SongDifficulty difficulty, int? stamina_multiply, int? fragment_multiply, int? prog_boost_multiply, out long fullRechargedTimeStamp)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = conn.CreateCommand();
			cmd.CommandText = $"SELECT world_time_fullrecharged,overflow_staminas,current_map FROM users WHERE user_id={userid};";
			var rd = cmd.ExecuteReader();
			rd.Read();
			var fullRechargedTime = rd.GetDateTime("world_time_fullrecharged");
			int overflowStaminas = rd.GetInt32("overflow_staminas");
			bool isBeyondChapter = (!rd.IsDBNull(2) && rd.GetString("current_map").ToLower().StartsWith("byd_"));
			rd.Close();
			int beforeStaminas = (int)CalculateCurrentStaminas(fullRechargedTime, out _) + overflowStaminas;
			if (beforeStaminas >= (isBeyondChapter ? BeyondStaminasPerCost : StaminasPerCost)) //如果体力充足
			{
				if (overflowStaminas > 0)
				{
					if (overflowStaminas >= (isBeyondChapter ? BeyondStaminasPerCost : StaminasPerCost))
					{
						overflowStaminas -= (isBeyondChapter ? BeyondStaminasPerCost : StaminasPerCost); // 直接用多余的体力 不动fullRechargedTime(正常体力完全恢复的时间戳)
					}
					else
					{
						overflowStaminas = 0;
						int afterOverflowCostStaminas = (isBeyondChapter ? BeyondStaminasPerCost : StaminasPerCost) - overflowStaminas;
						// 用完多余的体力后剩余需耗费的体力数(如当前体力为正常12体力+多余1体力的情况下,游玩需要2体力的地图, 则该值返回1(用了正常1体力和多余全部体力/结果为正常11体力+多余0体力)
						fullRechargedTime = DateTime.Now;
						fullRechargedTime = fullRechargedTime.AddMinutes(afterOverflowCostStaminas * 30);
						Console.WriteLine("fullRechargedTime = " + fullRechargedTime);
					}
				}
				else
				{
					if (fullRechargedTime <= DateTime.Now)
					{
						fullRechargedTime = DateTime.Now;
					}
					fullRechargedTime = fullRechargedTime.AddMinutes((isBeyondChapter ? BeyondStaminasPerCost : StaminasPerCost) * 30);
					Console.WriteLine("fullRechargedTime = " + fullRechargedTime);
				}
				long fullrechargedTimeStamp = Convert.ToInt32((fullRechargedTime - DateTime.UnixEpoch).TotalSeconds);
				Console.WriteLine("fullrechargedTimeStamp = " + fullrechargedTimeStamp);
				// 将时间戳+(每次游玩消耗体力数 * 30分钟)
				int stamina_mtp = 1;
				int frag_mtp = 100;
				int prog_boost_mtp = 0;
				if (stamina_multiply.HasValue) stamina_mtp = stamina_multiply.Value;
				if (fragment_multiply.HasValue) frag_mtp = fragment_multiply.Value;
				if (prog_boost_multiply.HasValue) prog_boost_mtp = prog_boost_multiply.Value;
				cmd.CommandText = $"DELETE FROM world_songplay WHERE user_id={userid} AND song_id='{songid}' AND difficulty={(int)difficulty};";
				cmd.ExecuteNonQuery(); //删除上次世界(World)模式游玩的占位数据(如果存在)
				cmd.CommandText = $"INSERT INTO world_songplay VALUES ({userid},'{songid}',{(int)difficulty},{stamina_mtp},{frag_mtp},{prog_boost_mtp});";
				cmd.ExecuteNonQuery(); //添加本次世界(World)模式游玩的占位数据
				cmd.CommandText = $"UPDATE users SET world_time_fullrecharged='{fullRechargedTime:yyyy-M-d H:mm:ss.fff}' , overflow_staminas={overflowStaminas} WHERE user_id={userid}";
				cmd.ExecuteNonQuery(); //更新时间戳
				conn.Close();
				int afterStaminas = beforeStaminas - (isBeyondChapter ? BeyondStaminasPerCost : StaminasPerCost); //消耗体力
				fullRechargedTimeStamp = fullrechargedTimeStamp;
				return afterStaminas;
			}
			else
			{
				conn.Close();
				throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.NotHaveEnoughStamina); //体力不足
			}
		}

		/// <summary>
		/// 结算世界(World)模式游玩成绩并完成游玩.
		/// </summary>
		/// <param name="userid">玩家的用户id.</param>
		/// <param name="mapid">地图id.</param>
		/// <param name="stepOrOver">玩家当前成绩的最终Step/Over值.</param>
		/// <param name="beforePosition">游玩前玩家所在的台阶序号(如50/100).</param>
		/// <param name="beforeCapture">游玩前玩家所在的台阶的单台阶进度(如7.3/20).</param>
		/// <param name="rewards">获得的奖励 <see cref="JArray"/> 数组.</param>
		/// <param name="steps">当前地图的未游玩部分台阶信息 <see cref="JArray"/> 数组.</param>
		/// <param name="afterPosition">结算后玩家所在的台阶序号.</param>
		/// <param name="afterCapture">结算后玩家所在的台阶的单台阶进度.</param>
		/// <param name="stepsCount">当前地图的总台阶数。</param>
		/// <returns>当前地图的完整信息.</returns>
		/// <exception cref="ArcaeaAPIException" />
		public static JObject FinishWorldPlay(uint userid, string mapid, decimal stepOrOver, uint beforePosition, decimal beforeCapture,
			out JArray? rewards, out JArray? steps, out uint afterPosition, out decimal afterCapture,out int stepsCount)
		{
			try
			{
				var info = GetMap(mapid, out bool isExists);
				if (!isExists || !IsDuringEventTime(userid, mapid))
				{
					throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.NotSubmitScoreBecauseEventEnded);
				}
				int step_count = info.Value<int>("step_count");
				stepsCount = step_count;
				var restricted_ids = new JArray[step_count]; //所有台阶的限制曲目数组列表(台阶有该项就不存在restricted_id项)
				string[] restricted_id = new string[step_count]; //所有台阶的限制单曲列表(台阶有该项就不存在restricted_ids项)
				string[] restricted_type = new string[step_count]; //所有台阶的限制类型列表
				decimal[] capture = new decimal[step_count]; //所有台阶的长度(0=这个台阶没有长度)(如20,15以及某616官方地图中的一堆2(2,2,2,2,2,2,2...))
				var items = new JArray[step_count]; //所有台阶的物品数组列表
				var step_types = new JArray[step_count]; //所有台阶的特殊台阶效果数组列表(因为一个台阶可以有好几个效果,所以此处对每一个台阶都是效果数组)
				int[] speed_limits = new int[step_count]; //所有台阶的限制最高的音符流速列表(0=不限速)
				int[] plus_staminas = new int[step_count]; //所有台阶的体力赠送数量列表(0=不赠送体力)
				var gotCharItemPosition = new List<int>();
				var gotBydSongItemPosition = new List<int>();
				int j = 0;
				foreach (JObject thisStep in info.Value<JArray>("steps")) //遍历所有台阶
				{
					capture[j] = thisStep.Value<decimal>("capture"); //添加台阶长度
					if (thisStep.TryGetValue("items", out var s_Items)) //如果本台阶存在物品	
					{
						items[j] = (JArray)s_Items;
						foreach (JObject item in items[j])
						{
							if (item.Value<string>("type") == "character")
							{
								gotCharItemPosition.Add(j);
							}
							else if (item.Value<string>("type") == "world_song" && (item.Value<string>("id").EndsWith('3') || mapid.ToLower().StartsWith("byd")))
							{
								gotBydSongItemPosition.Add(j);
							}
						}
					}
					else
					{
						items[j] = new JArray();
					}
					if (thisStep.TryGetValue("restrict_id", out var s_restrict_id)) //如果本台阶存在限制单曲
					{
						restricted_id[j] = (string)s_restrict_id!;
					}
					else
					{
						restricted_id[j] = string.Empty;
					}
					if (thisStep.TryGetValue("restricted_ids", out var s_restrict_ids)) //如果本台阶存在限制曲目数组
					{
						restricted_ids[j] = (JArray)s_restrict_ids;
					}
					else
					{
						restricted_ids[j] = new JArray();
					}
					if (thisStep.TryGetValue("restrict_type", out var s_restrict_type)) //如果本台阶存在限制类型
					{
						restricted_type[j] = (string)s_restrict_type!;
					}
					else
					{
						restricted_type[j] = string.Empty;
					}
					if (thisStep.TryGetValue("step_type", out var s_type)) //如果本台阶存在特殊台阶效果数组
					{
						step_types[j] = (JArray)s_type;
						if (((JArray)s_type).Contains("speedlimit")) //如果本台阶存在音符流速限制
						{
							int s_speedlimit = thisStep.Value<int>("speed_limit_value");
							speed_limits[j] = s_speedlimit;
						}
						if (((JArray)s_type).Contains("plusstamina")) //如果本台阶存在赠送的体力
						{
							int s_plus_stamina = thisStep.Value<int>("plus_stamina_value");
							plus_staminas[j] = s_plus_stamina;
						}
					}
					else
					{
						step_types[j] = new JArray();
					}
					j++;
				}
				if (!info.Value<bool>("is_beyond")) //如果不是Beyond地图
				{
					uint bp = beforePosition; //玩家所在的台阶序号(bp=Before Position)
					decimal bc = beforeCapture; //玩家所在台阶的单台阶进度(bc=Before Capture)
					decimal st = stepOrOver; //玩家应上升(爬)的长度(最终Step值)(st=STep)
					long tst = step_count; //地图的总台阶数(tst=Total Step Count)
					while (st > 0 && bp < tst) //当玩家的爬的长度>0(还未爬完)及玩家所在台阶序号小于地图的总台阶数时执行循环
					{ //(相当于循环每个台阶)
						decimal rc = (capture[(int)bp]!) - bc; //玩家当前台阶所剩爬的长度数量(rc=Remain Capture)
						// 当前台阶长度-当前台阶已完成的进度
						if (rc > st) //如果计算后所剩长度数量大于总爬的长度(最终当前台阶未爬完)
						{
							bc += st; //单台阶进度+爬的长度
							if (plus_staminas[bp] > 0)
							{
								using var conn = new MySqlConnection(DatabaseConnectURL);
								conn.Open();
								var cmd = conn.CreateCommand();
								cmd.CommandText = $"SELECT world_time_fullrecharged FROM users WHERE user_id={userid};";
								var fullRechargeTime = Convert.ToDateTime(cmd.ExecuteScalar());
								if (fullRechargeTime < DateTime.Now)
								{
									cmd.CommandText = $"SELECT overflow_staminas FROM users WHERE user_id={userid};";
									int overflowStaminas = Convert.ToInt32(cmd.ExecuteScalar());
									overflowStaminas += plus_staminas[bp];
									cmd.CommandText = $"UPDATE users SET overflow_staminas={overflowStaminas} WHERE user_id={userid};";
									cmd.ExecuteNonQuery();
								}
								else
								{
									fullRechargeTime = fullRechargeTime.AddMinutes(-(plus_staminas[bp] * 30));
									int overflowStaminas = Convert.ToInt32(Math.Floor((DateTime.Now - fullRechargeTime).TotalMinutes / 30));
									if (overflowStaminas < 0) overflowStaminas = 0;
									cmd.CommandText = $"UPDATE users SET world_time_fullrecharged={fullRechargeTime:yyyy-M-d H:mm:ss}, overflow_staminas={overflowStaminas} WHERE user_id={userid};";
									cmd.ExecuteNonQuery();
								}
								conn.Close();
							}
							break;
						}
						else //如果计算后所剩长度数量小于总爬的长度(当前台阶已爬完)
						{
							st -= rc; //总爬的长度-已爬的长度
							bc = 0; //单台阶进度归零(已爬到下一个台阶,下一次循环计算)
							bp++; //台阶编号+1
						}
					}
					if (bp >= tst) //如果计算完成后玩家所在的台阶序号大于等于整个地图的台阶数量(已完成该地图)
					{
						afterPosition = (uint)(tst - 1); //玩家所在台阶为顶层台阶序号(-1的原因是数组下标从0开始)
						afterCapture = 0; //玩家所在台阶的单台阶进度为0(顶层台阶固定长度为0,所以进度也为0)
						using var conn = new MySqlConnection(DatabaseConnectURL);
						conn.Open();
						var cmd = conn.CreateCommand();
						cmd.CommandText = $"UPDATE users SET current_map=?currentMap WHERE user_id={userid};"; // 将玩家当前游玩的地图id设置为null(玩家已经完成了地图 则不再选中此地图)
						cmd.Parameters.Add(new MySqlParameter("?currentMap", MySqlDbType.VarChar)
						{
							Value = DBNull.Value
						});
						cmd.ExecuteNonQuery();
						conn.Close();
					}
					else //如果当前地图未完成
					{
						afterPosition = bp; //爬完后玩家所在的新台阶序号(下afterCapture同理)
						afterCapture = bc;
					}
					j = 0;
					foreach (int charItemPosition in gotCharItemPosition)
					{
						if (afterPosition >= charItemPosition)
						{
							j = charItemPosition;
						}
						else
						{
							break;
						}
					}
					Console.WriteLine(j);
					int k = 0;
					foreach (int bydSongItemPosition in gotBydSongItemPosition)
					{
						if (afterPosition >= bydSongItemPosition)
						{
							k = bydSongItemPosition;
						}
						else
						{
							break;
						}
					}
					Console.WriteLine(k);
					#region "角色奖励获得结算"
					// if (items[j].Count > 0 && ((JObject)items[j][0]).TryGetValue("type",out JToken? gotCharId) && ((string)gotCharId! == "character"))
					// if (afterPosition == (uint)(tst - 1))
					if (items[j].Count > 0 && ((JObject)items[j][0]).TryGetValue("type", out var stepType) && ((string)stepType! == "character"))
					{ //如果经过了有角色奖励的台阶
						int char_id = int.Parse(((JObject)items[j][0]).Value<string>("id"));
						using var conn = new MySqlConnection(DatabaseConnectURL);
						conn.Open();
						var cmd = conn.CreateCommand();
						cmd.CommandText = $"SELECT level_exps,frag,prog,overdrive,skill_id,skill_unlock_level,skill_requires_uncap,char_type,is_uncapped,is_uncapped_override FROM fixed_characters WHERE character_id={char_id};";
						var rd = cmd.ExecuteReader();
						rd.Read();
						using var conn2 = new MySqlConnection(DatabaseConnectURL);
						conn2.Open();
						var cmd2 = conn2.CreateCommand();
						int level_exp = (int)JArray.Parse(rd.GetString(0))[0];
						int startFrag = (int)JArray.Parse(rd.GetString(1))[0];
						int startProg = (int)JArray.Parse(rd.GetString(2))[0];
						int startOverDrive = (int)JArray.Parse(rd.GetString(3))[0];
						cmd2.CommandText = "INSERT INTO user_chars " +
							"(`user_id`,`character_id`,`level`,`exp`,`level_exp`,`frag`,`prog`,`overdrive`,`skill_id`,`skill_unlock_level`,`skill_requires_uncap`,`char_type`,`is_uncapped`,`is_uncapped_override`) " +
							"VALUES " +
							$"(?userId,?charId,?level,?exp,?levelExp,?frag,?prog,?overdrive,?skillId,?skillUnlockLevel,?skillRequiresUncap,?charType,?isUncapped,?isUncappedOverride);";
						cmd2.Parameters.Add(new MySqlParameter("?userId", MySqlDbType.Int32)
						{
							Value = userid
						});
						cmd2.Parameters.Add(new MySqlParameter("?charId", MySqlDbType.Int32)
						{
							Value = char_id
						});
						cmd2.Parameters.Add(new MySqlParameter("?level", MySqlDbType.Int32)
						{
							Value = 1
						});
						cmd2.Parameters.Add(new MySqlParameter("?exp", MySqlDbType.Int32)
						{
							Value = 50
						});
						cmd2.Parameters.Add(new MySqlParameter("?levelExp", MySqlDbType.Int32)
						{
							Value = level_exp
						});
						cmd2.Parameters.Add(new MySqlParameter("?frag", MySqlDbType.Int32)
						{
							Value = startFrag
						});
						cmd2.Parameters.Add(new MySqlParameter("?prog", MySqlDbType.Int32)
						{
							Value = startProg
						});
						cmd2.Parameters.Add(new MySqlParameter("?overdrive", MySqlDbType.Int32)
						{
							Value = startOverDrive
						});
						cmd2.Parameters.Add(new MySqlParameter("?skillId", MySqlDbType.VarChar)
						{
							Value = rd.GetString("skill_id")
						});
						cmd2.Parameters.Add(new MySqlParameter("?skillUnlockLevel", MySqlDbType.Int32)
						{
							Value = rd.GetInt32("skill_unlock_level")
						});
						cmd2.Parameters.Add(new MySqlParameter("?skillRequiresUncap", MySqlDbType.Int32)
						{
							Value = rd.GetInt32("skill_requires_uncap")
						});
						cmd2.Parameters.Add(new MySqlParameter("?charType", MySqlDbType.Int32)
						{
							Value = rd.GetInt32("char_type")
						});
						cmd2.Parameters.Add(new MySqlParameter("?isUncapped", MySqlDbType.Int32)
						{
							Value = rd.GetInt32("is_uncapped")
						});
						cmd2.Parameters.Add(new MySqlParameter("?isUncappedOverride", MySqlDbType.Int32)
						{
							Value = rd.GetInt32("is_uncapped_override")
						});
						cmd2.ExecuteNonQuery();
						rd.Close();
						conn2.Close();
						conn.Close();
					}
					#endregion
					#region "Beyond曲目获得结算"
					// if (items[j].Count > 0 && ((JObject)items[j][0]).TryGetValue("type",out JToken? gotCharId) && ((string)gotCharId! == "character"))
					// if (afterPosition == (uint)(tst - 1))
					if (items[k].Count > 0 && ((JObject)items[k][0]).TryGetValue("type", out var stepType2) && ((string)stepType2! == "world_song")
						&& (((JObject)items[k][0]).Value<string>("id").EndsWith('3') || mapid.ToLower().StartsWith("byd")))
					{ //如果经过了有Beyond曲目奖励的台阶
						string sid = ((JObject)items[k][0]).Value<string>("id").TrimEnd('3');
						using var conn = new MySqlConnection(DatabaseConnectURL);
						conn.Open();
						var cmd = conn.CreateCommand();
						cmd.CommandText = "SELECT rating_byd FROM fixed_songs WHERE sid=?sid;";
						cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
						{
							Value = sid
						});
						decimal bydRating = Convert.ToDecimal(cmd.ExecuteScalar());
						if (bydRating != -1)
						{
							cmd.CommandText = "INSERT INTO user_bydunlocks VALUES (?uid,?sid);";
							cmd.Parameters.Add(new MySqlParameter("?uid", MySqlDbType.Int32)
							{
								Value = userid
							});
							cmd.ExecuteNonQuery();
						}
						conn.Close();
					}
					#endregion
				}
				else // Beyond地图结算
				{	
					uint bp = beforePosition; //玩家所在的台阶序号(bp=Before Position)
					decimal bc = beforeCapture; //玩家所在台阶的单台阶进度(bc=Before Capture)
					decimal st = stepOrOver; //玩家应上升(爬)的长度(最终Step值)(st=STep)
					long tst = step_count; //地图的总台阶数(tst=Total Step Count)
					while (st > 0 && bp < tst) //当玩家的爬的长度>0(还未爬完)及玩家所在台阶序号小于地图的总台阶数时执行循环
					{ //(相当于循环每个台阶)
						decimal rc = (capture[(int)bp]!) - bc; //玩家当前台阶所剩爬的长度数量(rc=Remain Capture)
															   // 当前台阶长度-当前台阶已完成的进度
						if (rc > st) //如果计算后所剩长度数量大于总爬的长度(最终当前台阶未爬完)
						{
							bc += st; //单台阶进度+爬的长度
							if (plus_staminas[bp] > 0)
							{
								using var conn = new MySqlConnection(DatabaseConnectURL);
								conn.Open();
								var cmd = conn.CreateCommand();
								cmd.CommandText = $"SELECT world_time_fullrecharged FROM users WHERE user_id={userid};";
								var fullRechargeTime = Convert.ToDateTime(cmd.ExecuteScalar());
								if (fullRechargeTime < DateTime.Now)
								{
									cmd.CommandText = $"SELECT overflow_staminas FROM users WHERE user_id={userid};";
									int overflowStaminas = Convert.ToInt32(cmd.ExecuteScalar());
									overflowStaminas += plus_staminas[bp];
									cmd.CommandText = $"UPDATE users SET overflow_staminas={overflowStaminas} WHERE user_id={userid};";
									cmd.ExecuteNonQuery();
								}
								else
								{
									fullRechargeTime = fullRechargeTime.AddMinutes(-(plus_staminas[bp] * 30));
									int overflowStaminas = Convert.ToInt32(Math.Floor((DateTime.Now - fullRechargeTime).TotalMinutes / 30));
									if (overflowStaminas < 0) overflowStaminas = 0;
									cmd.CommandText = $"UPDATE users SET world_time_fullrecharged={fullRechargeTime:yyyy-M-d H:mm:ss}, overflow_staminas={overflowStaminas} WHERE user_id={userid};";
									cmd.ExecuteNonQuery();
								}
								conn.Close();
							}
							break;
						}
						else //如果计算后所剩长度数量小于总爬的长度(当前台阶已爬完)
						{
							st -= rc; //总爬的长度-已爬的长度
							bc = 0; //单台阶进度归零(已爬到下一个台阶,下一次循环计算)
							bp++; //台阶编号+1
						}
					}
					if (bp >= tst) //如果计算完成后玩家所在的台阶序号大于等于整个地图的台阶数量(已完成该地图)
					{
						afterPosition = (uint)(tst - 1); //玩家所在台阶为顶层台阶序号(-1的原因是数组下标从0开始)
						afterCapture = 0; //玩家所在台阶的单台阶进度为0(顶层台阶固定长度为0,所以进度也为0)
						using var conn = new MySqlConnection(DatabaseConnectURL);
						conn.Open();
						var cmd = conn.CreateCommand();
						cmd.CommandText = $"UPDATE users SET current_map=?currentMap WHERE user_id={userid};"; // 将玩家当前游玩的地图id设置为null(玩家已经完成了地图 则不再选中此地图)
						cmd.Parameters.Add(new MySqlParameter("?currentMap", MySqlDbType.VarChar)
						{
							Value = DBNull.Value
						});
						cmd.ExecuteNonQuery();
						conn.Close();
					}
					else //如果当前地图未完成
					{
						afterPosition = bp; //爬完后玩家所在的新台阶序号(下afterCapture同理)
						afterCapture = bc;
					}
					j = 0;
					foreach (int charItemPosition in gotCharItemPosition)
					{
						if (afterPosition >= charItemPosition)
						{
							j = charItemPosition;
						}
						else
						{
							break;
						}
					}
					Console.WriteLine(j);
					#region "Beyond曲目获得结算"
					// if (items[j].Count > 0 && ((JObject)items[j][0]).TryGetValue("type",out JToken? gotCharId) && ((string)gotCharId! == "character"))
					// if (afterPosition == (uint)(tst - 1))
					if (items[j].Count > 0 && ((JObject)items[j][0]).TryGetValue("type", out var stepType) && ((string)stepType! == "world_song") 
						&& ((JObject)items[j][0]).Value<string>("id").EndsWith('3'))
					{ //如果经过了有Beyond曲目奖励的台阶
						string sid = ((JObject)items[j][0]).Value<string>("id").TrimEnd('3');
						using var conn = new MySqlConnection(DatabaseConnectURL);
						conn.Open();
						var cmd = conn.CreateCommand();
						cmd.CommandText = "SELECT rating_byd FROM fixed_songs WHERE sid=?sid;";
						cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
						{
							Value = sid
						});
						decimal bydRating = Convert.ToDecimal(cmd.ExecuteScalar());
						if (bydRating != -1)
						{
							cmd.CommandText = "INSERT INTO user_bydunlocks VALUES (?uid,?sid);";
							cmd.Parameters.Add(new MySqlParameter("?uid", MySqlDbType.Int32)
							{
								Value = userid
							});
							cmd.ExecuteNonQuery();
						}
						conn.Close();
					}
					#endregion
				}
				var r_rewards = new JArray(); //地图未游玩部分的奖励列表(r_rewards=Result Rewards,下r_steps同理)
				var r_steps = new JArray(); //地图未游玩部分的台阶列表
				for (uint i = afterPosition; i <= (stepsCount - 1); i++) //遍历地图未游玩部分的台阶序号
				{
					if (items[i]!.Count > 0) //如果当前台阶存在物品(奖励)
					{
						r_rewards.Add(new JObject()
						{
							{"position",i },
							{"items",items[i] }
						});
					}
					var thisStep = new JObject()
					{
						{"map_id",mapid },
						{"position",i },
						{"capture",capture[i] },
						{"reward_bundle",string.Empty },
						{"restrict_id",restricted_id[i] },
						{"restrict_ids",restricted_ids[i] },
						{"restrict_type",restricted_type[i] }
					};
					if (step_types[i]!.Count > 0) //如果当前台阶存在特殊台阶效果数组
					{
						thisStep.Add("step_type", step_types[i]);
					}
					if ((speed_limits[i]!) != 0) //如果当前台阶存在限速特效
					{ 
						thisStep.Add("speed_limit_value", speed_limits[i]!);
					};
					if ((plus_staminas[i]!) != 0) //如果当前台阶存在体力赠送特效
					{
						thisStep.Add("plus_stamina_value", plus_staminas[i]!);
					}
					r_steps.Add(thisStep);
				}
				rewards = r_rewards;
				steps = r_steps;
				return info;
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
		}

		public static bool IsDuringEventTime(uint userid,string mapid)
		{
			var map = GetMap(mapid, out bool isExists);
			if (isExists)
			{
				if (map.Value<long>("available_from") != -1)
				{
					if (IsWorldEventMapTesting(out var testPlayers) && testPlayers.Contains((int)userid))
					{
						return true;
					}
					else
					{
						var eventStartTime = DateTime.UnixEpoch.AddMilliseconds(map.Value<long>("available_from"));
						var eventStopTime = DateTime.UnixEpoch.AddMilliseconds(map.Value<long>("available_to"));
						var now = DateTime.Now.ToUniversalTime();
						if (now < eventStartTime || now > eventStopTime)
						{
							return false;
						}
						else
						{
							return true;
						}
					}
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		public static Dictionary<int, decimal> GetBeyondMapAffinityCharList(string mapid)
		{
			var map = GetMap(mapid, out bool isExists);
			if (isExists)
			{
				if (map.Value<bool>("is_beyond"))
				{
					var charAffs = map.Value<JArray>("character_affinity")!.ToObject<List<int>>()!;
					var affValues = map.Value<JArray>("affinity_multiplier")!.ToObject<List<decimal>>()!;
					var r = new Dictionary<int, decimal>();
					for (int i = 0; i < charAffs.Count; i++)
					{
						r.Add(charAffs[i], affValues[i]);
					}
					return r;
				}
				else
				{
					return new Dictionary<int, decimal>();
				}
			}
			else
			{
				return new Dictionary<int, decimal>();
			}
		}

		public static decimal GetBeyondMapCharAffinity(string mapid, uint charid)
		{
			var list = GetBeyondMapAffinityCharList(mapid);
			if (list.Any())
			{
				if (list.TryGetValue((int)charid, out decimal r))
				{
					return r;
				}
				else
				{
					return 1.0M;
				}
			}
			else
			{
				return 1.0M;
			}
		}
	}
}
