using static Team123it.Arcaea.MarveCube.GlobalProperties;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Enhance.Web.Json;
using System.Threading.Tasks;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Processors.Background;
using Newtonsoft.Json.Linq;
using Team123it.Arcaea.MarveCube.Processors.Front;

namespace Team123it.Arcaea.MarveCube.Controllers
{
	/// <summary>
	/// [API Controller]物品购买相关API控制器类。<br />
	/// 对应处理类: <see cref="Purchase"/>
	/// </summary>
	[ApiController]
	[Route("years/19/purchase")]
	public class PurchaseController : ControllerBase
	{
		/// <summary>
		/// [API Action][POST]使用兑换码兑换物品。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="code">指定的兑换码。</param>
		/// <returns>Json字符串。</returns>
		[HttpPost("me/redeem")]
		public async Task<JObjectResult> RedeemConvert([FromHeader] string Authorization,[FromForm]string code)
		{
			return await Task.Run(new Func<JObjectResult>(() =>
			{
				if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
				try
				{
					if (Authorization.Trim().ToLower().StartsWith("bearer"))
					{
						var now = DateTime.Now;
						uint? user_id = Tokens.GetUserIdByToken(Authorization.Split(" ")[1]);
						if (Maintaining(out var players))
						{
							if (!user_id.HasValue || !players.Contains((int)user_id.Value))
							{
								return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.ServerMaintaining);
							}
						}
						if (!user_id.HasValue) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.LoggedInAnotherDevice);
						if (code.ToLower() == "freefragments")
						{
							return new JObjectResult(new JObject()
							{
								{ "success", true },
								{ "value", new JObject()
									{
										{ "coupon", "fragment500" }
									} 
								}
							});
						}
						else
						{
							return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.InvalidRedeemCode);
						}
					}
					else
					{
						return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
					}
				}
				catch (ArcaeaAPIException ex)
				{
					return ex;
				}
				catch (Exception)
				{
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
			}));
		}

		/// <summary>
		/// [API Action][GET]获取单曲相关的购买信息。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <returns>Json字符串。</returns>
		[HttpGet("bundle/single")]
		public async Task<JObjectResult> FetchSingleSongPurchases([FromHeader]string Authorization)
		{
			return await Task.Run(new Func<JObjectResult>(() =>
			{
				if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
				try
				{
					if (Authorization.Trim().ToLower().StartsWith("bearer"))
					{
						uint? user_id = Tokens.GetUserIdByToken(Authorization.Split(" ")[1]);
						if (Maintaining(out var players))
						{
							if (!user_id.HasValue || !players.Contains((int)user_id.Value))
							{
								return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.ServerMaintaining);
							}
						}
						if (!user_id.HasValue) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.LoggedInAnotherDevice);
						return new JObjectResult(new JObject()
						{
							{ "success", true },
							{ "value", Purchase.GetPurchaseData(ItemType.SingleSong) }
						});
					}
					else
					{
						return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
					}
				}
				catch (ArcaeaAPIException ex)
				{
					return ex;
				}
				catch (Exception)
				{
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
			}));
		}

		/// <summary>
		/// [API Action][GET]获取曲包相关的购买信息。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <returns>Json字符串。</returns>
		[HttpGet("bundle/pack")]
		public async Task<JObjectResult> FetchSongPackPurchases([FromHeader] string Authorization)
		{
			return await Task.Run(new Func<JObjectResult>(() =>
			{
				if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
				try
				{
					if (Authorization.Trim().ToLower().StartsWith("bearer"))
					{
						uint? user_id = Tokens.GetUserIdByToken(Authorization.Split(" ")[1]);
						if (Maintaining(out var players))
						{
							if (!user_id.HasValue || !players.Contains((int)user_id.Value))
							{
								return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.ServerMaintaining);
							}
						}
						if (!user_id.HasValue) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.LoggedInAnotherDevice);
						return new JObjectResult(new JObject()
						{
							{ "success", true },
							{ "value", Purchase.GetPurchaseData(ItemType.Pack) }
						});
					}
					else
					{
						return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
					}
				}
				catch (ArcaeaAPIException ex)
				{
					return ex;
				}
				catch (Exception)
				{
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
			}));
		}

		/// <summary>
		/// [API Action][POST]购买物品。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="collection">包括完整HTTP Form请求表单数据的 <see cref="IFormCollection"/> 接口实例。</param>
		/// <returns>Json字符串。</returns>
		[HttpPost("me/pack")]
		public async Task<JObjectResult> PurchaseItems([FromHeader]string Authorization, [FromForm]IFormCollection collection)
		{
			return await Task.Run(new Func<JObjectResult>(() =>
			{
				if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
				try
				{
					if (Authorization.Trim().ToLower().StartsWith("bearer"))
					{
						uint? user_id = Tokens.GetUserIdByToken(Authorization.Split(" ")[1]);
						if (Maintaining(out var players))
						{
							if (!user_id.HasValue || !players.Contains((int)user_id.Value))
							{
								return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.ServerMaintaining);
							}
						}
						if (!user_id.HasValue) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.LoggedInAnotherDevice);
						var playerInfo = new PlayerInfo(user_id.Value, out _);
						var r = new JObject();
						if (collection.TryGetValue("pack_id", out var packIds))
						{
							r = Purchase.PurchaseItem(playerInfo, ItemType.Pack, packIds.ToString());
						}
						else if (collection.TryGetValue("single_id", out var singleIds))
						{
							r = Purchase.PurchaseItem(playerInfo, ItemType.SingleSong, singleIds.ToString());
						}
						r.Add("success", true);
						return new JObjectResult(r);
					}
					else
					{
						return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
					}
				}
				catch (ArcaeaAPIException ex)
				{
					return ex;
				}
				catch (Exception)
				{
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
			}));
		}

		[HttpPost("me/stamina/{purchaseType}")]
		public async Task<JObjectResult> PurchaseStamina([FromHeader]string Authorization, [FromRoute]string purchaseType)
		{
			return await Task.Run(new Func<JObjectResult>(() =>
			{
				if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
				try
				{
					if (Authorization.Trim().ToLower().StartsWith("bearer"))
					{
						uint? user_id = Tokens.GetUserIdByToken(Authorization.Split(" ")[1]);
						if (Maintaining(out var players))
						{
							if (!user_id.HasValue || !players.Contains((int)user_id.Value))
							{
								return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.ServerMaintaining);
							}
						}
						if (!user_id.HasValue) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.LoggedInAnotherDevice);
						var playerInfo = new PlayerInfo(user_id.Value, out _);
						var type = purchaseType switch
						{
							"fragment" => StaminaPurchaseType.Fragment,
							_ => throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.CannotGetThisItem)
						};
						var r = new JObject()
						{
							{ "success", true },
							{ "value", Purchase.PurchaseStamina(playerInfo, type) }
						};
						return new JObjectResult(r);
					}
					else
					{
						return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
					}
				}
				catch (ArcaeaAPIException ex)
				{
					return ex;
				}
				catch (Exception)
				{
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
			}));
		}

		[HttpPost("me/item")]
		public async Task<JObjectResult> PurchaseWorldHelper([FromHeader]string Authorization, [FromForm]string item_id)
		{
			return await Task.Run(new Func<JObjectResult>(() =>
			{
				if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
				try
				{
					if (Authorization.Trim().ToLower().StartsWith("bearer"))
					{
						uint? user_id = Tokens.GetUserIdByToken(Authorization.Split(" ")[1]);
						if (Maintaining(out var players))
						{
							if (!user_id.HasValue || !players.Contains((int)user_id.Value))
							{
								return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.ServerMaintaining);
							}
						}
						if (!user_id.HasValue) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.LoggedInAnotherDevice);
						var playerInfo = new PlayerInfo(user_id.Value, out _);
						var value = new JObject();
						switch (item_id)
						{
							case "stamina6": //记忆源点换6体力
								value = Purchase.PurchaseStamina(playerInfo, StaminaPurchaseType.Memory);
								break;
							case "prog_boost_300": //源韵强化(x4)
								value = Purchase.Purchasex4StepBoost(playerInfo);
								break;
								// throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.CannotGetThisItem);
						}
						var r = new JObject()
						{
							{ "success", true },
							{ "value", value }
						};
						return new JObjectResult(r);
					}
					else
					{
						return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
					}
				}
				catch (ArcaeaAPIException ex)
				{
					return ex;
				}
				catch (Exception)
				{
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
			}));
		}
	}
}
