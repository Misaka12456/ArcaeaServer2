using Microsoft.AspNetCore.Http;
using static Team123it.Arcaea.MarveCube.GlobalProperties;
using StackExchange.Redis;
using System;

namespace Team123it.Arcaea.MarveCube.Core
{
	public static class QueryLimit
	{
		/// <summary>
		/// 对当前请求 <see cref="HttpRequest"/> 实例对应的Bot Apikey进行API访问频率检查。
		/// </summary>
		/// <param name="req">当前 <see cref="HttpRequest"/> 实例。</param>
		/// <returns>若当前请求 <see cref="HttpRequest"/> 实例对应的Bot Apikey已超过允许的QPS上限则返回 <see langword="false"/> , 否则返回 <see langword="true"/> 。</returns>
		public static bool DoQueryLimit(this HttpRequest req,string apikey)
		{
			if (MDatabaseConnectURL == null)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				string prefix = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
				Console.WriteLine($"[{prefix}][Error]Cannot connect to Memory Database (Redis Server). Had you set the database url in the config file?");
				return false;
			}
			using var conn = ConnectionMultiplexer.Connect(MDatabaseConnectURL);
			try
			{
				var db = conn.GetDatabase();
				var limit = db.StringGetWithExpiry(apikey);
				uint limitTimes = (limit.Value != RedisValue.Null) ? (uint)limit.Value : 0;
				if (limitTimes > MaxQueryTimesPerSecond)
				{
					return false;
				}
				else
				{
					limitTimes++;
					db.KeyDelete(apikey);
					db.SetAdd(apikey, limitTimes);
					db.KeyExpire(apikey, new TimeSpan(0, 0, 1));
					return true;
				}
			}
			catch
			{
				return false;
			}
			finally
			{
				conn.Close();
			}
		}
	}
}
