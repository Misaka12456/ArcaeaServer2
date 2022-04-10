#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Bots;
using Newtonsoft.Json.Linq;
using static Team123it.Arcaea.MarveCube.Core.BotAPIException;
using System.Text.RegularExpressions;
using Team123it.Arcaea.MarveCube.Processors.Front;
using System.Enhance.Web.Json;

namespace Team123it.Arcaea.MarveCube.Controllers
{
	/// <summary>
	/// [API Controller]Arcaea查分Bot API相关控制器类。<br />
	/// 对应处理类: <see cref="Bot"/>
	/// </summary>
	[ApiController]
	[Route("bot")]
	public class BotController : ControllerBase
	{
		[HttpGet("user")]
		public Task<JObjectResult> GetPlayerInfo([FromQuery] string? apikey,[FromQuery]string user)
		{
			return Task.Run(new Func<JObjectResult>(() =>
			{
				if (apikey == null) return new BotAPIException(APIExceptionType.InvalidApiKey, null);
				if (!Request.DoQueryLimit(apikey)) return new BotAPIException(APIExceptionType.QueryTooFrequently, null);
				if (IsSafeArgument(apikey) && IsSafeArgument(user))
				{
					try
					{
						Background.CheckApiKey(apikey);
						var r = new JObject()
						{
							{"status",0 },
							{"value", Bot.PlayerInfo(user)}
						};
						return new JObjectResult(r);
					}
					catch (BotAPIException ex)
					{
						return ex;
					}
					catch
					{
						return new BotAPIException(APIExceptionType.Others, null);
					}
				}
				else
				{
					var arguments = new Dictionary<string, string>();
					foreach (var arg in Request.Query)
					{
						arguments.Add(arg.Key, arg.Value);
					}
					return new BotAPIException(APIExceptionType.DangerousArguments, new KeyValuePair<string, Dictionary<string, string>>(apikey, arguments));
				}
			}));
		}

		[HttpGet("song/best")]
		public Task<JObjectResult> GetPlayerSongBest([FromQuery] string? apikey,[FromQuery]string user,[FromQuery]string songid,[FromQuery]int? difficulty)
		{
			return Task.Run(new Func<JObjectResult>(() => 
			{
				if (apikey == null) return new BotAPIException(APIExceptionType.InvalidApiKey, null);
				if (!Request.DoQueryLimit(apikey)) return new BotAPIException(APIExceptionType.QueryTooFrequently, null);
				if (Request.IsSafeArgument())
				{
					try
					{
						Background.CheckApiKey(apikey);
						int diff;
						if (!difficulty.HasValue) diff = 2;
						else if (difficulty!.Value != 0 && difficulty!.Value != 1 && difficulty!.Value != 2 && difficulty!.Value != 3) throw new BotAPIException(APIExceptionType.DifficultyIsNotExist, null);
						else diff = difficulty!.Value;
						var r = new JObject()
						{
							{"status",0 },
							{"value",Bot.QueryPlayerBestScore(user,songid,(SongDifficulty)diff)}
						};
						return new JObjectResult(r);
					}
					catch (BotAPIException ex)
					{
						return ex;
					}
					catch
					{
						return new BotAPIException(APIExceptionType.Others, null);
					}
				}
				else
				{
					var arguments = new Dictionary<string, string>();
					foreach (var arg in Request.Query)
					{
						arguments.Add(arg.Key, (string)arg.Value);
					}
					return new BotAPIException(APIExceptionType.DangerousArguments, new KeyValuePair<string, Dictionary<string, string>>(apikey, arguments));
				}
			}));
		}

		[HttpGet("user/recent")]
		public Task<JObjectResult> GetPlayerRecentScore([FromQuery] string? apikey,[FromQuery]string user)
		{
			return Task.Run(new Func<JObjectResult>(() => 
			{
				if (apikey == null) return new BotAPIException(APIExceptionType.InvalidApiKey, null);
				if (!Request.DoQueryLimit(apikey)) return new BotAPIException(APIExceptionType.QueryTooFrequently, null);
				if (Request.IsSafeArgument())
				{
					try
					{
						Background.CheckApiKey(apikey);
						var r = new JObject()
							{
								{"status",0 },
								{"value",Bot.QueryPlayerRecentScore(user)}
							};
						return new JObjectResult(r);
					}
					catch (BotAPIException ex)
					{
						return ex;
					}
					catch
					{
						return new BotAPIException(APIExceptionType.Others, null);
					}
				}
				else
				{
					var arguments = new Dictionary<string, string>();
					foreach (var arg in Request.Query)
					{
						arguments.Add(arg.Key, (string)arg.Value);
					}
					return new BotAPIException(APIExceptionType.DangerousArguments, new KeyValuePair<string, Dictionary<string, string>>(apikey, arguments));
				}
			}));
		}

		[HttpPost("user/best30")]
		public Task<JObjectResult> GetPlayerBest30([FromForm]string? apikey,[FromForm]string user)
		{
			return Task.Run(new Func<JObjectResult>(() =>
			{
				if (apikey == null) return new BotAPIException(APIExceptionType.InvalidApiKey, null);
				if (!Request.DoQueryLimit(apikey)) return new BotAPIException(APIExceptionType.QueryTooFrequently, null);
				if (Request.IsSafeArgument())
				{
					try
					{
						Background.CheckApiKey(apikey);
						var r = new JObject()
						{
							{"status",0 },
							{"value",Bot.QueryPlayerBest30(user)}
						};
						return new JObjectResult(r);
					}
					catch (BotAPIException ex)
					{
						return ex;
					}
					catch(Exception ex)
					{
						Console.WriteLine(ex.ToString());
						return new BotAPIException(APIExceptionType.Others, null);
					}
				}
				else
				{
					var arguments = new Dictionary<string, string>();
					foreach (var arg in Request.Query)
					{
						arguments.Add(arg.Key, (string)arg.Value);
					}
					return new BotAPIException(APIExceptionType.DangerousArguments, new KeyValuePair<string, Dictionary<string, string>>(apikey, arguments));
				}
			}));
		}

		[HttpGet("song")]
		public Task<JObjectResult> GetSongDetails([FromQuery]string? apikey, [FromQuery]string songid)
		{
			return Task.Run(new Func<JObjectResult>(() =>
			{
				if (apikey == null) return new BotAPIException(APIExceptionType.InvalidApiKey, null);
				if (!Request.DoQueryLimit(apikey)) return new BotAPIException(APIExceptionType.QueryTooFrequently, null);
				if (Request.IsSafeArgument())
				{
					try
					{
						Background.CheckApiKey(apikey);
						var r = new JObject()
						{
							{"status",0 },
							{"value", Bot.SongInfo(songid) }
						};
						return new JObjectResult(r);
					}
					catch (BotAPIException ex)
					{
						return ex;
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
						return new BotAPIException(APIExceptionType.Others, null);
					}
				}
				else
				{
					var arguments = new Dictionary<string, string>();
					foreach (var arg in Request.Query)
					{
						arguments.Add(arg.Key, (string)arg.Value);
					}
					return new BotAPIException(APIExceptionType.DangerousArguments, new KeyValuePair<string, Dictionary<string, string>>(apikey, arguments));
				}
			}));
		}
		public static bool IsSafeArgument(string arg)
		{
			if (arg.Contains(';') || arg.Contains('"') || arg.Contains('\'')
				|| arg.ToLower().Contains("insert") || arg.ToLower().Contains("update") 
				|| arg.ToLower().Contains("select") || arg.ToLower().Contains("delete") 
				|| arg.ToLower().Contains("chr") || arg.ToLower().Contains("mid")
				|| arg.ToLower().Contains("master") || arg.ToLower().Contains("truncate")
				|| arg.ToLower().Contains("char") || arg.ToLower().Contains("declare") 
				|| arg.ToLower().Contains("join") || arg.ToLower().Contains("and")
				|| arg.ToLower().Contains("exec") || arg.ToLower().Contains("drop"))
			{
				return false;
			}
			else
			{
				var regex = new Regex("[^0-9a-zA-Z]");
				return !regex.IsMatch(arg);
			}
		}
	}
}