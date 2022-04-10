#nullable enable
using static Team123it.Arcaea.MarveCube.GlobalProperties;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Processors.Background;
using World2 = Team123it.Arcaea.MarveCube.Processors.Background.World;
using System.Linq;

namespace Team123it.Arcaea.MarveCube.Processors.Front
{
	/// <summary>
	/// 数据获取相关API。<br />
	/// 对应API前缀:/years/19/compose/
	/// </summary>
	public class Compose
	{
		/// <summary>
		/// [API][完整版]获取用户信息.
		/// </summary>
		/// <param name="token">来源token.</param>
		/// <returns>Json数据.</returns>
		/// <exception cref="ArcaeaAPIException" />
		public static string FullAggregate(string token, string calls = "")
		{
			try
			{
				uint? userIdCheck = Tokens.GetUserIdByToken(token);
				if (userIdCheck == null) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.LoggedInAnotherDevice);
				uint userId = userIdCheck.Value;
				var user = new PlayerInfo(userId, out bool isExists);
				if (!isExists) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.UserNotExist); //用户不存在
				if (user.Banned!.Value) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountHasBeenBlocked);
				//用户账号被冻结
				var r = new JObject()
				{
					{"success",true }
				};
				var r_value = new JArray();
				#region "value = 0:玩家数据 (定义参数:value0) | /user/me"
				var value0 = new JObject()
				{
					{"id",0 },
					{"value",user.GetUserBaseInfoData() }
				};
				#endregion
				#region "value = 1:曲包数据 (定义参数:value1,packs,singlePack,items,conn,cmd,rd) | /purchase/bundle/pack"
				var value1 = new JObject()
				{
					{"id", 1 },
					{"value", Purchase.GetPurchaseData() }
				};
				#endregion
				#region "value = 2:所有可下载的曲目元数据列表 (定义参数:value2|使用参数:conn,cmd,rd) | /serve/download/me/song?url=false"
				var value2 = new JObject()
				{
					{"id",2 },
					{"value", Serve.GetDownloadAvailableSongs(userId, null, false) }
				};
				#endregion
				#region "value = 3:服务器设置数据 (定义参数:value3,props,level_steps,level) | /game/info"
				var value3 = new JObject()
				{
					{"id",3 }
				};
				var props = new JObject()
				{
					{"stamina_recover_tick", 1800000 },
					{"core_exp",250 },
					{"curr_ts", Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds) } // 当前的时间(毫秒为单位)
				};
				using var conn = new MySqlConnection(DatabaseConnectURL);
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"SELECT * FROM fixed_properties";
				var rd = cmd.ExecuteReader();
				while (rd.Read())
				{
					switch (rd.GetString(0))
					{
						case "max_stamina": //世界(World)模式 - 满体力数
							props.Add("max_stamina", Convert.ToInt32(rd.GetString(1)));
							break;
						case "level_steps": //世界(World)模式 - 角色升级经验
							var level_steps = JArray.Parse(rd.GetString(1));
							int level = 0;
							var levels = new JArray();
							foreach (int currentLevelStep in level_steps)
							{
								level++;
								levels.Add(new JObject()
								{
									{"level",level },
									{"level_exp",currentLevelStep }
								});
							}
							props.Add("level_steps", levels);
							break;
						case "world_ranking_enabled": //是否启用初始曲包(base)世界排行榜
							props.Add("world_ranking_enabled", Convert.ToBoolean(Convert.ToInt32(rd.GetString(1))));
							break;
						case "is_byd_chapter_unlocked": //世界(World)模式 - Beyond章节是否已解封
							props.Add("is_byd_chapter_unlocked", Convert.ToBoolean(int.Parse(rd.GetString(1))));
							break;
					}
				}
				rd.Close();
				value3.Add("value", props);
				#endregion
				#region "value = 4: 礼物下发数据 (值类型:JArray) (定义参数:value4) | /present/me?lang=[语言id]"
				string langStr = "zh-Hans";
				if (!string.IsNullOrEmpty(calls))
				{
					var callsData = JArray.Parse(calls);
					langStr = (from call in callsData
							   where ((JObject)call).Value<int>("id") == 4
							   select ((JObject)call).Value<string>("endpoint").Split("?lang=")[1]).First();
				}
				var value4 = new JObject()
				{
					{"id",4 },
					{"value", Present.FetchAvailablePresents(user, langStr) }
				};
				#endregion
				#region "value = 5: 世界模式数据 (定义参数:value5) | /world/map/me"
				cmd.CommandText = $"SELECT current_map FROM users WHERE user_id={user!.UserId!.Value};";
				var currentMap = cmd.ExecuteScalar();
				var value5 = new JObject()
				{
					{"id",5 },
					{
						"value", new JObject()
						{
							{"current_map", (currentMap != null) ? Convert.ToString(currentMap) : string.Empty  },
							{"user_id",userId },
							{"maps", JArray.FromObject(World2.GetAllMaps(userId,out _).Where(data => data.Value<string>("map_id") == ((currentMap != null) ? Convert.ToString(currentMap) : string.Empty))) }
							// World2.GetAllMaps(userId,out _)
						}
					}
				};
				#endregion
				r_value.Add(value0);
				r_value.Add(value1);
				r_value.Add(value2);
				r_value.Add(value3);
				r_value.Add(value4);
				r_value.Add(value5);
				r.Add("value", r_value);
				return r.ToString();
			}
			catch (ArcaeaAPIException ex)
			{
				return ex;
			}
			//catch
			//{
			//	return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
			//}
		}

		public static string TinyAggregate(string token)
		{
			try
			{
				uint? userIdCheck = Tokens.GetUserIdByToken(token);
				if (userIdCheck == null) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.LoggedInAnotherDevice);
				uint userId = userIdCheck.Value;
				var user = new PlayerInfo(userId, out bool isExists);
				if (!isExists) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.UserNotExist); //用户不存在
				if (user.Banned!.Value) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountHasBeenBlocked);
				//用户账号被冻结
				var r = new JObject()
				{
					{"success",true }
				};
				var r_value = new JArray();
				#region "value = 0:玩家数据 (定义参数:value0)"
				var value0 = new JObject()
				{
					{"id",0 },
					{"value",user.GetUserBaseInfoData() }
				};
				#endregion
				r_value.Add(value0);
				r.Add("value", r_value);
				return r.ToString();
			}
			catch (ArcaeaAPIException ex)
			{
				return ex;
			}
		}
	}
}
