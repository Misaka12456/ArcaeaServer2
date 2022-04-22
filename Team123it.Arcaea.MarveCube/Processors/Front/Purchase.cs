using static Team123it.Arcaea.MarveCube.GlobalProperties;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Processors.Background;
using World2 = Team123it.Arcaea.MarveCube.Processors.Background.World;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Team123it.Arcaea.MarveCube.Processors.Front
{
	public class Purchase
	{
		public static JArray GetPurchaseData(ItemType type = ItemType.Pack)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				conn.Open();
				var cmd = conn.CreateCommand();
				switch (type)
				{
					case ItemType.Pack:
						{
							var packs = new JArray();
							cmd.CommandText = $"SELECT * FROM fixed_packs;";
							var rd = cmd.ExecuteReader();
							while (rd.Read())
							{
								if (rd.GetString("pid") == "base") continue;
								if (rd.GetInt32("price") == -1) continue;
								var singlePack = new JObject()
								{
									{"name",rd.GetString("pid") },
									{"price",rd.GetInt32("price") },
									{"orig_price",rd.GetInt32("original_price") }
								};
								if (!rd.IsDBNull(7)) // discount_from
								{
									singlePack.Add("discount_from", Convert.ToInt64((rd.GetDateTime("discount_from") - DateTime.UnixEpoch).TotalMilliseconds));
									singlePack.Add("discount_to", Convert.ToInt64((rd.GetDateTime("discount_to") - DateTime.UnixEpoch).TotalMilliseconds));
								}
								var items = new JArray
								{
									new JObject()
									{
										{"id",rd.GetString("is_available") },
										{"type","pack" },
										{"is_available",rd.GetBoolean("is_available") }
									}
								};
								if (rd.GetInt32("plus_character") != -1) //如果存在附加角色
								{
									items.Add(new JObject()
									{
										{"id",Player.GetCharacterNameId(rd.GetUInt32("plus_character"),out bool is_available,out _) },
										{"type","character" },
										{"is_available",is_available }
									});
								}
								singlePack.Add("items", items);
								packs.Add(singlePack);
							}
							rd.Close();
							return packs;
						}
					case ItemType.SingleSong:
						{
							var singleSongs = new JArray();
							var singleSongIds = new List<string>();
							cmd.CommandText = "SELECT * FROM fixed_purchases WHERE item_type='single';";
							var rd = cmd.ExecuteReader();
							while (rd.Read())
							{
								var singleSong = new JObject()
								{
									{ "name", rd.GetString("item_id") },
									{ "items", new JArray()
										{
											new JObject()
											{
												{ "type", rd.GetString("item_type") },
												{ "id", rd.GetString("item_id") },
												{ "is_available", rd.GetBoolean("is_available") },
												{ "amount", 1 }
											}
										}
									},
									{ "price", rd.GetInt32("price") },
									{ "orig_price", rd.GetInt32("original_price") }
								};
								if (!rd.IsDBNull(5)) // discount_from
								{
									singleSong.Add("discount_from", Convert.ToInt64((rd.GetDateTime("discount_from") - DateTime.UnixEpoch).TotalMilliseconds));
									singleSong.Add("discount_to", Convert.ToInt64((rd.GetDateTime("discount_to") - DateTime.UnixEpoch).TotalMilliseconds));
								}
								singleSongs.Add(singleSong);
							}
							rd.Close();
							return singleSongs;
						}
					default:
						throw new ArgumentException($"The value of argument 'type' is invalid: {type}", nameof(type));
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
				throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
			}
			finally
			{
				conn.Close();
			}
		}

		public static JObject PurchaseItem(PlayerInfo p, ItemType type, string itemId)
		{
			if (p.PurchasedItemsList == null) p.PurchasedItemsList = new JArray();
			bool isAlreadyHave = p.PurchasedItemsList.Any(jtoken => ((JObject)jtoken).Value<string>("id") == itemId);
			if (isAlreadyHave) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AlreadyHasItem);
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = type switch
				{
					ItemType.Pack => "SELECT 'pack' as item_type, pid as item_id, is_available, price, original_price, discount_from, discount_to FROM fixed_packs WHERE pid=?itemId;",
					_ => "SELECT * FROM fixed_purchases WHERE item_id=?itemId"
				};
				cmd.Parameters.Add(new MySqlParameter("?itemId", MySqlDbType.VarChar)
				{
					Value = itemId
				});
				var rd = cmd.ExecuteReader();
				if (rd.Read())
				{
					if (rd.GetBoolean("is_available"))
					{
						int price = rd.GetInt32("original_price");
						var now = DateTime.Now;
						if (!rd.IsDBNull(5) && (now >= rd.GetDateTime("discount_from")) && (now < rd.GetDateTime("discount_to")))
						{ // index[5]=`discount_from`
							price = rd.GetInt32("price");
						}
						if (price <= p.Ticket!.Value)
						{
							int remainMemories = p.Ticket!.Value - price;
							var purchasedList = p.PurchasedItemsList;
							purchasedList.Add(new JObject()
							{
								{ "type", rd.GetString("item_type") },
								{ "id", rd.GetString("item_id") },
								{ "amount", 1 }
							});
							rd.Close();
							cmd.Parameters.Clear();
							cmd.CommandText = $"UPDATE users SET ticket=?ticket, purchases=?purchaseList WHERE user_id={p.UserId!.Value}";
							cmd.Parameters.Add(new MySqlParameter("?ticket", MySqlDbType.Int32)
							{
								Value = remainMemories
							});
							cmd.Parameters.Add(new MySqlParameter("?purchaseList", MySqlDbType.LongText)
							{
								Value = purchasedList.ToString()
							});
							cmd.ExecuteNonQuery();
							var packs = GetUserPurchasedItemsList(p.UserId!.Value, ItemType.Pack);
							var singles = GetUserPurchasedItemsList(p.UserId!.Value, ItemType.SingleSong);
							var characters = JArray.FromObject(from @char in p.CharactersList
															   select @char.Value<uint>("character_id"));
							return new JObject()
							{
								{ "ticket", remainMemories },
								{ "packs", packs },
								{ "singles", singles },
								{ "characters", characters }
							};
						}
						else
						{
							throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.CannotGetThisItem);
						}
					}
					else
					{
						throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.CannotGetThisItem);
					}
				}
				else
				{
					throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.CannotGetThisItem);
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

		public static JArray GetUserPurchasedItemsList(uint userId, ItemType type = ItemType.Pack)
		{
			var p = new PlayerInfo(userId, out _);
			if (p.PurchasedItemsList != null)
			{
				var r = type switch
				{
					ItemType.Pack => JArray.FromObject(from item in p.PurchasedItemsList
													   where item.Value<string>("type") == "pack"
													   select item.Value<string>("id")),
					ItemType.SingleSong => JArray.FromObject(from item in p.PurchasedItemsList
															 where item.Value<string>("type") == "single"
															 select item.Value<string>("id")),
					_ => new JArray()
				};
				return r;
			}
			else // 将当前玩家的purchases项从NULL初始化为空Json数组字符串('[]')。
			{
				p.PurchasedItemsList = new JArray();
				using var conn = new MySqlConnection(DatabaseConnectURL);
				conn.Open();
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"UPDATE users SET purchases='{new JArray()}' WHERE user_id={p.UserId!.Value};";
				cmd.ExecuteNonQuery();
				conn.Close();
				return new JArray();
			}
		}

		public static JObject PurchaseStamina(PlayerInfo p, StaminaPurchaseType type)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				conn.Open();
				var cmd = conn.CreateCommand();
				switch (type)
				{
					case StaminaPurchaseType.Fragment:
						{
							cmd.CommandText = $"SELECT world_time_fullrecharged, overflow_staminas FROM users WHERE user_id={p.UserId!.Value};";
							var rd = cmd.ExecuteReader();
							rd.Read();
							var worldTimeFullRecharged = rd.GetDateTime("world_time_fullrecharged");
							int overflowStaminas = rd.GetInt32("overflow_staminas");
							rd.Close();
							int totalStaminas = (int)World2.CalculateCurrentStaminas(worldTimeFullRecharged, out _);
							if (totalStaminas == World2.FullStaminas)
							{
								overflowStaminas += 6;
							}
							else
							{
								if (World2.CalculateCurrentStaminas(worldTimeFullRecharged.AddMinutes(-6 * 30), out bool isOverflowStamina) == World2.FullStaminas)
								{
									worldTimeFullRecharged = worldTimeFullRecharged.AddMinutes(-6 * 30);
									totalStaminas = (int)World2.FullStaminas;
									if (isOverflowStamina)
									{
										int addOverflowStaminas = Convert.ToInt32(Math.Floor((DateTime.Now - worldTimeFullRecharged).TotalMinutes / 30));
										overflowStaminas += addOverflowStaminas;
									}
								}
								else
								{
									worldTimeFullRecharged = worldTimeFullRecharged.AddMinutes(-6 * 30);
									totalStaminas += 6;
								}
							}
							totalStaminas += overflowStaminas;
							cmd.CommandText = $"UPDATE users SET world_time_fullrecharged='{worldTimeFullRecharged:yyyy-M-d H:mm:ss}', overflow_staminas={overflowStaminas} WHERE user_id={p.UserId!.Value};";
							cmd.ExecuteNonQuery();
							var r = new JObject()
							{
								{ "user_id", p.UserId!.Value },
								{ "stamina", totalStaminas },
								{ "max_stamina_ts", Convert.ToInt64((worldTimeFullRecharged - DateTime.UnixEpoch).TotalMilliseconds) },
								{ "next_fragstam_ts", 0 }
							};
							return r;
						}
					case StaminaPurchaseType.Memory:
						{
							if (p.Ticket!.Value >= 50)
							{
								int totalMemories = p.Ticket!.Value - 50;
								cmd.CommandText = $"SELECT world_time_fullrecharged, overflow_staminas FROM users WHERE user_id={p.UserId!.Value};";
								var rd = cmd.ExecuteReader();
								rd.Read();
								var worldTimeFullRecharged = rd.GetDateTime("world_time_fullrecharged");
								int overflowStaminas = rd.GetInt32("overflow_staminas");
								rd.Close();
								int totalStaminas = (int)World2.CalculateCurrentStaminas(worldTimeFullRecharged, out _);
								if (totalStaminas == World2.FullStaminas)
								{
									overflowStaminas += 6;
								}
								else
								{
									if (World2.CalculateCurrentStaminas(worldTimeFullRecharged.AddMinutes(-6 * 30), out bool isOverflowStamina) == World2.FullStaminas)
									{
										worldTimeFullRecharged = worldTimeFullRecharged.AddMinutes(-6 * 30);
										totalStaminas = (int)World2.FullStaminas;
										if (isOverflowStamina)
										{
											int addOverflowStaminas = Convert.ToInt32(Math.Floor((DateTime.Now - worldTimeFullRecharged).TotalMinutes / 30));
											overflowStaminas += addOverflowStaminas;
										}
									}
									else
									{
										worldTimeFullRecharged = worldTimeFullRecharged.AddMinutes(-6 * 30);
										totalStaminas += 6;
									}
								}
								totalStaminas += overflowStaminas;
								cmd.CommandText = $"UPDATE users SET world_time_fullrecharged='{worldTimeFullRecharged:yyyy-M-d H:mm:ss}', overflow_staminas={overflowStaminas}," +
									$"ticket={totalMemories} WHERE user_id={p.UserId!.Value};";
								cmd.ExecuteNonQuery();
								var r = new JObject()
								{
									{ "user_id", p.UserId!.Value },
									{ "stamina", totalStaminas },
									{ "max_stamina_ts", Convert.ToInt64((worldTimeFullRecharged - DateTime.UnixEpoch).TotalMilliseconds) },
									{ "ticket", totalMemories }
								};
								return r;
							}
							else
							{
								throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.CannotGetThisItem);
							}
						}
					default:
						return new JObject();
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

		public static JObject Purchasex4StepBoost(PlayerInfo p)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				if (p.Ticket!.Value >= World2.ProgBoostMemories)
				{
					int remainedMemories = p.Ticket!.Value - World2.ProgBoostMemories;
					conn.Open();
					var cmd = conn.CreateCommand();
					cmd.CommandText = $"UPDATE users SET ticket={remainedMemories} WHERE user_id={p.UserId!.Value};";
					cmd.ExecuteNonQuery();
					return new JObject()
					{
						{ "ticket", remainedMemories }
					};
				}
				else
				{
					throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.CannotGetThisItem);
				}
			}
			catch (ArcaeaAPIException)
			{
				throw;
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
				throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
			}
			finally
			{
				conn.Close();
			}
		}
	}
}
