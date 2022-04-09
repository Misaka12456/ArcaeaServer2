using static Team123it.Arcaea.MarveCube.GlobalProperties;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Processors.Background;
using Team123it.Arcaea.MarveCube.Processors.Front;
using World = Team123it.Arcaea.MarveCube.Processors.Background.World;
using static Team123it.Arcaea.MarveCube.Processors.Background.LeaderBoard;
using System.Threading.Tasks;
using System.Enhance.Web.Json;

namespace Team123it.Arcaea.MarveCube.Controllers
{
	/// <summary>
	/// [API Controller]玩家分数相关API控制器类。<br />
	/// 对应处理类: <see cref="Score"/>
	/// </summary>
	[ApiController]
	[Route("years/19/score")]
	public class ScoreController : ControllerBase
	{
		/// <summary>
		/// [API Action][POST]曲目游玩成绩提交。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="song_token">曲目提交Token参数。</param>
		/// <param name="song_hash">曲目文件(*.aff)的哈希值。</param>
		/// <param name="song_id">曲目id。</param>
		/// <param name="difficulty">曲目难度。</param>
		/// <param name="score">曲目成绩(数值)。</param>
		/// <param name="shiny_perfect_count">大Pure数。</param>
		/// <param name="perfect_count">Pure数。</param>
		/// <param name="near_count">Far数。</param>
		/// <param name="miss_count">Lost数。</param>
		/// <param name="health">游玩结束时的回忆度。</param>
		/// <param name="modifier">[未知,保留参数]</param>
		/// <param name="beyond_gauge">是否为Beyond挑战(世界模式/Beyond章节)。<br />
		/// 0=<see langword="false" /> 1=<see langword="true"/></param>
		/// <param name="clear_type">曲目完成类型。<br />
		/// 该参数值可对照 <see cref="ClearType"/> 枚举各元素值。</param>
		/// <param name="submission_hash">曲目提交哈希值。</param>
		/// <returns>Json字符串。</returns>
		[HttpPost("song")]
		public Task<JObjectResult> scorePost([FromHeader]string Authorization,[FromForm] string song_token, [FromForm] string song_hash,
			[FromForm] string song_id, [FromForm] uint difficulty, [FromForm] uint score, [FromForm] uint shiny_perfect_count,
			[FromForm] uint perfect_count, [FromForm] uint near_count, [FromForm] uint miss_count, [FromForm] int health,
			[FromForm] int modifier, [FromForm] int? beyond_gauge, [FromForm] uint clear_type, [FromForm] string submission_hash)
		{
			return Task.Run(new Func<JObjectResult>(() =>
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
						var info = new PlayerInfo(user_id.Value, out _);
						if (info.Banned.Value) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountHasBeenBlocked);
						// 玩家账号已被冻结
						SingleScore scoreInfo;
						try
						{
							scoreInfo = new SingleScore(song_id, (SongDifficulty)difficulty, score, (ClearType)clear_type,
										shiny_perfect_count, perfect_count, near_count, miss_count, now);
						}
						catch (SongNotFoundException)
						{
							return new JObjectResult(new JObject()
							{
								{ "success",true },
								{ "value", new JObject()
									{
										{"user_rating",info.PotentialInt!.Value }
									}
								}
							});
						}
						bool isBeyondGauge;
						if (beyond_gauge == null) isBeyondGauge = false;
						else if (beyond_gauge == 1) isBeyondGauge = true;
						else isBeyondGauge = false;
						return new JObjectResult(Score.ScorePost(user_id.Value, scoreInfo, health, isBeyondGauge, song_hash, submission_hash));
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
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
			}));
		}

		/// <summary>
		/// [API Action][GET]获取曲目排行榜(世界排行)。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="song_id">曲目id。</param>
		/// <param name="difficulty">曲目难度。</param>
		/// <returns>Json字符串。</returns>
		[HttpGet("song")]
		public Task<JObjectResult> getWorldLeaderBoard([FromHeader]string Authorization,[FromQuery]string song_id,[FromQuery]uint difficulty, int start, int limit)
		{
			return Task.Run(new Func<JObjectResult>(() =>
			{
				if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
				try
				{
					if (Authorization.Trim().ToLower().StartsWith("bearer"))
					{
						if (Maintaining(out var players))
						{
							var user_id = Tokens.GetUserIdByToken(Authorization.Replace("Bearer ", string.Empty));
							if (!user_id.HasValue || !players.Contains((int)user_id.Value))
							{
								return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.ServerMaintaining);
							}
						}
						return new JObjectResult(Score.GetLeaderBoard(null, song_id, (SongDifficulty)difficulty, LeaderBoardType.World));
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
		/// [API Action][GET]获取曲目排行榜("我的"世界排行排名)。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="song_id">曲目id。</param>
		/// <param name="difficulty">曲目难度。</param>
		/// <returns>Json字符串。</returns>
		[HttpGet("song/me")]
		public Task<JObjectResult> getMyRankLeaderBoard([FromHeader] string Authorization, [FromQuery] string song_id, [FromQuery] uint difficulty, int start, int limit)
		{
			return Task.Run(new Func<JObjectResult>(() =>
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
						return new JObjectResult(Score.GetLeaderBoard(user_id.Value, song_id, (SongDifficulty)difficulty, LeaderBoardType.MyRank));
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

		[HttpGet("song/friend")]
		/// <summary>
		/// [API Action][GET]获取曲目排行榜(好友排行)。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <param name="song_id">曲目id。</param>
		/// <param name="difficulty">曲目难度。</param>
		/// <returns>Json字符串。</returns>
		public Task<JObjectResult> getFriendLeaderBoard([FromHeader] string Authorization, [FromQuery] string song_id, [FromQuery] uint difficulty,int start,int limit)
		{
			return Task.Run(new Func<JObjectResult>(() =>
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
						return new JObjectResult(Score.GetLeaderBoard(user_id.Value, song_id, (SongDifficulty)difficulty, LeaderBoardType.Friend));
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
					throw;// return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
				}
			}));
		}

		/// <summary>
		/// [API Action][GET]获取当前玩家成绩提交的Token。
		/// </summary>
		/// <param name="Authorization">Bearer Token参数。</param>
		/// <returns>Json字符串。</returns>
		[HttpGet("token")]
		public Task<JObjectResult> songTokenCheck([FromHeader] string Authorization)
		{
			return Task.Run(new Func<JObjectResult>(() =>
			{
				if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
				if (Maintaining(out var players))
				{
					var user_id = Tokens.GetUserIdByToken(Authorization.Split(new[] { ' ' }, 2)[1]);
					if (!user_id.HasValue || !players.Contains((int)user_id.Value))
					{
						return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.ServerMaintaining);
					}
				}
				var r = new JObject()
				{
					{"success",true },
					{"value",new JObject()
					{
						{"token","1145141919810=" }
					} }
				};
				return new JObjectResult(r);
			}));
		}

		/// <summary>
		/// [API Action][GET]将当前游玩成绩标记为世界(World)模式游玩成绩并开始游玩。(+[Play+]体力加成 +[Play+]残片加成 +源韵强化加成)
		/// </summary>
		/// <param name="Authorization">Bearer Token参数.</param>
		/// <param name="song_id">曲目id.</param>
		/// <param name="difficulty">曲目难度.</param>
		/// <param name="stamina_multiply">体力加成倍数.</param>
		/// <param name="fragment_multiply">残片加成倍数.</param>
		/// <param name="prog_boost_multiply">源韵强化加成倍数.</param>
		/// <returns>Json字符串.</returns>
		[HttpGet("token/world")]
		public Task<JObjectResult> startWorldPlay([FromHeader] string Authorization, [FromQuery] string song_id, [FromQuery] int difficulty, [FromQuery] int? stamina_multiply, [FromQuery] int? fragment_multiply, [FromQuery] int? prog_boost_multiply)
		{
			return Task.Run(new Func<JObjectResult>(() =>
			{
				if (PreparingForRelease(HttpContext.Request)) return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.PreparingForRelease);
				try
				{
					if (Authorization.Trim().ToLower().StartsWith("bearer"))
					{
						uint? userid = (uint)Tokens.GetUserIdByToken(Authorization.Split(" ")[1]);
						if (Maintaining(out var players))
						{
							if (!userid.HasValue || !players.Contains((int)userid.Value))
							{
								return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.ServerMaintaining);
							}
						}
						if (userid != null) //如果获取到了用户id
						{
							try
							{
								int remainingStamina = World.StartWorldPlay(userid.Value, song_id, (SongDifficulty)difficulty,
									stamina_multiply, fragment_multiply, prog_boost_multiply, out long fullRechargedTimeStamp);
								int overflow_staminas = remainingStamina - 12;
								if (overflow_staminas < 0) overflow_staminas = 0;
								var r = new JObject()
								{
									{"success",true },
									{"value",new JObject()
										{
											{"stamina", remainingStamina },
											{"max_stamina_ts", fullRechargedTimeStamp * 1000 },
											{"token", "worldModeDefaultToken" }
										}
									}
								};
								return new JObjectResult(r);
							}
							catch (ArcaeaAPIException ex) //如果发生了异常
							{
								return ex; //直接返回对应异常的Json
							}
							catch (Exception ex) //发生了未知异常
							{
								Console.WriteLine(ex.ToString());
								return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
							}
						}
						else
						{
							return new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.LoggedInAnotherDevice);
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
	}
}
