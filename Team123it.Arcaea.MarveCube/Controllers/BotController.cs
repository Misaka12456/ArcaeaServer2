#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Bots;
using Newtonsoft.Json.Linq;
using static Team123it.Arcaea.MarveCube.Core.BotAPIException;
using Team123it.Arcaea.MarveCube.Processors.Front;
using System.Enhance.Web.Json;

namespace Team123it.Arcaea.MarveCube.Controllers
{
	/// <summary>
	/// [API Controller]Arcaea查分Bot API相关控制器类。<br />
	/// 对应处理类: <see cref="Bot"/>
	/// </summary>
	[ApiController]
	[Route("botarcapi")]
	public class BotController : ControllerBase
	{
		[HttpGet("user")]
		public Task<JObjectResult> GetPlayerInfo([FromQuery] string? apikey,[FromQuery]string user)
		{
			return Task.Run(new Func<JObjectResult>(() =>
			{
				if (apikey == null) return new BotAPIException(APIExceptionType.InvalidApiKey, null);
				try
				{
					Background.CheckApiKey(apikey);
					var r = new JObject()
					{
						{"status",0 },
						{"content", Bot.GetPlayerInfo(user)}
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
			}));
		}

		[HttpGet("user/best")]
		public Task<JObjectResult> GetPlayerSongBest([FromQuery] string? apikey,[FromQuery]string user,[FromQuery]string songid,[FromQuery]int? difficulty, [FromQuery]bool withsonginfo, [FromQuery]bool withrecent)
		{
			return Task.Run(new Func<JObjectResult>(() => 
			{
				if (apikey == null) return new BotAPIException(APIExceptionType.InvalidApiKey, null);
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
						{"content",Bot.QueryPlayerBestScore(user,songid,(SongDifficulty)diff, withsonginfo, withrecent)}
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
			}));
		}

		[HttpGet("user/info")]
		public Task<JObjectResult> GetPlayerRecentScore([FromQuery] string? apikey,[FromQuery]string user, [FromQuery]bool withsonginfo)
		{
			return Task.Run(new Func<JObjectResult>(() => 
			{
				if (apikey == null) return new BotAPIException(APIExceptionType.InvalidApiKey, null);
				try
				{
					Background.CheckApiKey(apikey);
					var r = new JObject()
					{
						{"status",0 },
						{"content",Bot.QueryPlayerRecentScore(user, withsonginfo)}
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
			}));
		}

		[HttpGet("user/best30")]
		public Task<JObjectResult> GetPlayerBest30([FromQuery]string? apikey,[FromQuery]string user, [FromQuery]bool withsonginfo = false, [FromQuery]bool withrecent = false)
		{
			return Task.Run(new Func<JObjectResult>(() =>
			{
				if (apikey == null) return new BotAPIException(APIExceptionType.InvalidApiKey, null);
				try
				{
					Background.CheckApiKey(apikey);
					var r = new JObject()
					{
						{"status",0 },
						{"content",Bot.QueryPlayerBest30(user, withsonginfo, withrecent)}
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
			}));
		}

		[HttpGet("song/info")]
		public Task<JObjectResult> GetSongDetails([FromQuery]string? apikey, [FromQuery]string songid)
		{
			return Task.Run(new Func<JObjectResult>(() =>
			{
				if (apikey == null) return new BotAPIException(APIExceptionType.InvalidApiKey, null);
				try
				{
					Background.CheckApiKey(apikey);
					var r = new JObject()
					{
						{"status",0 },
						{"content", Bot.GetSongInfo(songid) }
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
			}));
		}
	}
}