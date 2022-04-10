using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Enhance;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using static Team123it.Arcaea.MarveCube.GlobalProperties;
using System.Text;
using System.Enhance.Web.Json;

namespace Team123it.Arcaea.MarveCube.Core
{
	/// <summary>
	/// 表示Arcaea Bot API返回的异常。
	/// <para>异常对应id请参考 <see cref="APIExceptionType"/> 枚举的注释。</para>
	/// </summary>
	public class BotAPIException : Exception
	{
		/// <summary>
		/// API异常的类型。
		/// </summary>
		public APIExceptionType Type { get; }

		/// <summary>
		/// API异常的简短说明。
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// 初始化 <see cref="BotAPIException"/> 类的新实例。
		/// </summary>
		/// <param name="type">异常类型。</param>
		public BotAPIException(APIExceptionType type,KeyValuePair<string,Dictionary<string,string>>? tag = null)
		{
			Type = type;
			Description = type.GetDescription()!;
			if (Type == APIExceptionType.DangerousArguments && ReportEmail != null)
			{ //如果是Dangerous Arguments而且设置了自动发送异常信息的邮箱
				using var msg = new MailMessage();
				var fromAddr = new MailAddress(ReportEmail!);
				msg.From = fromAddr;
				msg.To.Add(ReportDescEmail!);
				msg.Subject = "[Auto][WARNING][123 MarveCube] A bot api visitor is trying to visit the api with dangerous arguments";
				var body = new StringBuilder("A bot api visitor is trying to visit the api with dangerous arguments.<br />")
					.Append("If this is the vistor who mistyped the query string, ask s/he to correct the query string.<br >")
					.Append("If the visitor is unknown visitor and/or this keeps happening, it is recommended to ban the api key of the bot.<br />")
					.Append("Details:<br />")
					.Append("Time: ").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("<br />")
					.Append("Bot Api Key: ").Append(tag!.Value.Key).Append("<br />")
					.Append("Attempt query api url: ").Append(tag!.Value.Value["url"]).Append("<br />")
					.Append("Query strings:<br />");
				foreach(var queryStr in tag!.Value.Value)
				{
					if (queryStr.Key != "url")
					{
						body.Append(queryStr.Key).Append("=").Append(queryStr.Value).Append("<br />");
					}
				}
				body.Append($"<br />(C)Copyright 2015-{DateTime.Now.Year} 123 Open-Source Organization(Team123it). All rights reserved.");
				msg.Body = body.ToString();
				using var client = new SmtpClient(ReportEmailSmtp!.Split(':')[0], int.Parse(ReportEmailSmtp!.Split(':')[1]));
				client.Credentials = new NetworkCredential(ReportEmail!, ReportEmailPswd!);
				client.EnableSsl = true;
				client.Send(msg);
			}
		}

		/// <summary>
		/// 将当前 <see cref="BotAPIException"/> 实例转换为Json字符串。
		/// </summary>
		/// <param name="CurrentException">当前 <see cref="BotAPIException"/> 实例。</param>
		public static implicit operator string(BotAPIException CurrentException)
		{
			int error_code = (int)CurrentException.Type;
			string desc = CurrentException.Description;
			var result = new JObject()
			{
				{"status",error_code },
				{"message",desc }
			};
			return result.ToString();
		}

		/// <summary>
		/// 将当前 <see cref="BotAPIException"/> 实例转换为 <see cref="JObjectResult"/> 类实例。
		/// </summary>
		/// <param name="CurrentException"></param>
		public static implicit operator JObjectResult(BotAPIException CurrentException)
		{
			int error_code = (int)CurrentException.Type;
			string desc = CurrentException.Description;
			var result = new JObject()
			{
				{"status",error_code },
				{"message",desc }
			};
			return new JObjectResult(result);
		}

		/// <summary>
		/// 将当前 <see cref="BotAPIException"/> 实例转换为(Json)字符串。
		/// </summary>
		/// <returns>转换结果。</returns>
		public override string ToString()
		{
			int error_code = (int)Type;
			var result = new JObject()
			{
				{"status",error_code },
				{"message",Description }
			};
			return result.ToString();
		}

		/// <summary>
		/// 表示 <see cref="BotAPIException"/> API异常的具体类型。
		/// </summary>
		public enum APIExceptionType
		{
			/// <summary>
			/// 发生未知错误
			/// </summary>
			[Description("An unexpected error occurred. Please contact Lowiro.")]
			Others = -100,
			/// <summary>
			/// 服务器正在维护中
			/// </summary>
			[Description("Server is maintaining. Please wait patiently before finishing the maintain. If you have any problem or question, please contact Lowiro.")]
			ServerMaintaining = -101,
			/// <summary>
			/// 玩家不存在
			/// </summary>
			[Description("This player isn't exist.")]
			PlayerNotExist = -200,
			/// <summary>
			/// 玩家账号被封禁
			/// </summary>
			[Description("This player is blocked.")]
			PlayerIsBlocked = -201,
			/// <summary>
			/// 曲目不存在
			/// </summary>
			[Description("This song isn't exist.")]
			SongIsNotExist = -300,
			/// <summary>
			/// 该曲目不存在当前难度
			/// </summary>
			[Description("This difficulty of the song isn't exist.")]
			DifficultyIsNotExist = -301,
			/// <summary>
			/// 别名对应的曲目数量过多(>1)
			/// </summary>
			[Description("Too many songs matching this alias. Please give more accurate alias, or give the sid.")]
			TooManySongsFromAlias = -302,
			/// <summary>
			/// 玩家未游玩曲目的当前难度
			/// </summary>
			[Description("Player didn't play the difficulty of the song before.")]
			PlayerNotPlayedThisDiff = -303,
			/// <summary>
			/// 玩家的最近游玩成绩为空
			/// </summary>
			[Description("Player didn't play any song(s).")]
			RecentScoreIsEmpty = -304,
			/// <summary>
			/// 无效的ApiKey
			/// </summary>
			[Description("Invalid apikey. Please check your apikey before executing the visit to this api again.")]
			InvalidApiKey = -400,
			/// <summary>
			/// Bot账号被封禁
			/// </summary>
			[Description("Your bot account is blocked. Please contact Lowiro to get more details or to appeal misblock action.")]
			BotIsBlocked = -401,
			/// <summary>
			/// 查询频率(QPS)过高
			/// </summary>
			[Description("You queried too frequently, please slow down your query frequency, or you may be blocked.")]
			QueryTooFrequently = -402,
			/// <summary>
			/// 危险的参数(试图进行SQL注入攻击等)
			/// </summary>
			[Description("You tried to execute api with dangerous argument(s). Please do not execute with dangerous argument(s), or you may be blocked.")]
			DangerousArguments = -403
		}
	}
}