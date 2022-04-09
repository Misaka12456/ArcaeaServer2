
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Enhance.Web.Json;

namespace Team123it.Arcaea.MarveCube.Core
{
	/// <summary>
	/// 表示Arcaea API返回的异常。
	/// <para>异常对应id请参考 <see cref="APIExceptionType"/> 枚举的注释。</para>
	/// </summary>
	public class ArcaeaAPIException : Exception
	{
		/// <summary>
		/// API异常的类型。
		/// </summary>
		public APIExceptionType Type { get; }

		/// <summary>
		/// 初始化 <see cref="ArcaeaAPIException"/> 类的新实例。
		/// </summary>
		/// <param name="type">异常类型。</param>
		public ArcaeaAPIException(APIExceptionType type) : base()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"{DateTime.Now:[yyyy-M-d H:mm:ss]}An unexpected error occurred.\n" +
				$"Error Base Type:{GetType()}\n" +
				$"Error Type: {(int)type}\n" +
				$"Stacktrace: {StackTrace}\n" +
				$"Message: {ToString()}");
			Console.ResetColor();
			Type = type;
		}

		/// <summary>
		/// 将当前 <see cref="ArcaeaAPIException"/> 实例转换为 Arcaea 客户端可读取的Json字符串。
		/// </summary>
		/// <param name="CurrentException">当前 <see cref="ArcaeaAPIException"/> 实例。</param>
		public static implicit operator string(ArcaeaAPIException CurrentException)
		{
			int error_code = (int)CurrentException.Type;
			var result = new JObject()
			{
				{"success",false },
				{"error_code",error_code }
			};
			if (error_code == 999)
			{
				// result.Add("details", CurrentException.Message + "\n" + CurrentException.StackTrace);
			}
			return result.ToString();
		}

		
		/// <summary>
		/// 将当前 <see cref="ArcaeaAPIException"/> 实例转换为异步WebAPI返回的 <see cref="JsonResult"/> 实例。<br />
		/// 注:实例内容即为 Arcaea 客户端可读取的Json字符串。
		/// </summary>
		/// <param name="CurrentException">当前 <see cref="ArcaeaAPIException"/> 实例。</param>
		[Obsolete("为提高处理效率,不再建议转换为 JsonResult 实例。请将API返回类型更改为 JObjectResult 或 Task<JObjectResult> (转换为 JObjectResult 实例)。")]
		public static implicit operator JsonResult(ArcaeaAPIException CurrentException)
		{
			int error_code = (int)CurrentException.Type;
			var result = new JObject()
			{
				{"success",false },
				{"error_code",error_code }
			};
			if (error_code == 999)
			{
				result.Add("details", CurrentException.Message + "\n" + CurrentException.StackTrace);
			}
			return new JsonResult(result.ToString());
		}

		/// <summary>
		/// 将当前 <see cref="ArcaeaAPIException"/> 实例转换为异步WebAPI返回的 <see cref="JObjectResult"/> 实例。<br />
		/// 注:实例内容即为 Arcaea 客户端可读取的 <see cref="JObject"/> 实例数据。
		/// </summary>
		/// <param name="CurrentException">当前 <see cref="ArcaeaAPIException"/> 实例。</param>
		public static implicit operator JObjectResult(ArcaeaAPIException CurrentException)
		{
			int error_code = (int)CurrentException.Type;
			var result = new JObject()
			{
				{"success",false },
				{"error_code",error_code }
			};
			if (error_code == 999)
			{
				result.Add("details", CurrentException.Message + "\n" + CurrentException.StackTrace);
			}
			return new JObjectResult(result);
		}

		/// <summary>
		/// 将当前 <see cref="ArcaeaAPIException"/> 实例转换为(Json)字符串。
		/// </summary>
		/// <returns>转换结果。</returns>
		public override string ToString()
		{
			int error_code = (int)Type;
			var result = new JObject()
			{
				{"success",false },
				{"error_code",error_code }
			};
			if (error_code == 999)
			{
				result.Add("details", Message + "\n" + StackTrace);
			}
			return result.ToString();
		}

		/// <summary>
		/// 表示 <see cref="ArcaeaAPIException"/> API异常的具体类型。
		/// </summary>
		public enum APIExceptionType
		{
			/// <summary>
			/// 账号已在其他设备上登录(通常为Token无效时)
			/// <para>游戏内提示: 您的账号已在其他设备上登录。请重启Arcaea。</para>
			/// </summary>
			LoggedInAnotherDevice = -4,
			/// <summary>
			/// 服务器正在维护。
			/// <para>游戏内提示: Arcaea的服务器正在维护中</para>
			/// </summary>
			ServerMaintaining = 2,
			/// <summary>
			/// 客户端需要更新。
			/// <para>游戏内提示: 请更新Arcaea到最新版本</para>
			/// </summary>
			NeedUpdateClient = 5,
			/// <summary>
			/// 新版本等待发布中。
			/// <para>游戏内提示: 你来的真早!/当前Arcaea版本正在准备发布中 请等待几分钟</para>
			/// </summary>
			PreparingForRelease = 9,
			/// <summary>
			/// 用户名已存在。
			/// <para>游戏内提示: 此用户名已被占用</para>
			/// </summary>
			UsernameExists = 101,
			/// <summary>
			/// 电子邮箱已注册。
			/// <para>游戏内提示: 此电子邮箱已被注册</para>
			/// </summary>
			EmailHasRegistered = 102,
			/// <summary>
			/// 已存在一个从当前设备创建的帐号。
			/// <para>游戏内提示: 已有一个账号由此设备创建</para>
			/// </summary>
			ExistOneAccountCreatedFromThisDevice = 103,
			/// <summary>
			/// 用户名或密码错误。
			/// <para>游戏内提示: 用户名或密码错误</para>
			/// </summary>
			UsernameOrPasswordInvalid = 104,
			/// <summary>
			/// 此账号24小时内已登录2台设备。
			/// <para>游戏内提示: 您的帐号已在24小时内登录2台设备。<br />请在使用这台新设备前等待 % 小时/天。</para>
			/// </summary>
			LogIn2DevicesIn24Hours = 105,
			/// <summary>
			/// 账户被冻结(封禁)。
			/// <para>游戏内提示: 该账户已被冻结</para>
			/// </summary>
			AccountHasBeenBlocked = 106,
			/// <summary>
			/// 体力不足。
			/// <para>游戏内提示: 您没有足够的体力</para>
			/// </summary>
			NotHaveEnoughStamina = 107,
			/// <summary>
			/// 活动已结束。
			/// <para>游戏内提示: 该活动已结束</para>
			/// </summary>
			EventEnded = 113,
			/// <summary>
			/// 成绩无法提交,因为活动已结束。
			/// <para>游戏内提示: 该活动已结束, 您的成绩不会提交。</para>
			/// </summary>
			NotSubmitScoreBecauseEventEnded = 114,
			/// <summary>
			/// 账号冻结警告。
			/// <para>游戏内提示: 检测到您正在使用修改版的Arcaea。<br />继续使用修改版的Arcaea将会导致您的帐号被封号。<br />这是最后一次警告。</para>
			/// </summary>
			AccountBlockWarning = 120,
			/// <summary>
			/// 账号被冻结。(第2个错误代码)
			/// <para>游戏内提示: 该账户已被冻结</para>
			/// </summary>
			AccountHasBeenBlocked2 = 121,
			/// <summary>
			/// 账号被暂时冻结。
			/// <para>游戏内提示: 您的帐号已被暂时冻结。 请前往 Arcaea 官网查看详情。</para>
			/// </summary>
			AccountHasBeenBlockedTemporarily = 122,
			/// <summary>
			/// 账号被限制。
			/// <para>游戏内提示: 您的帐号已被限制。 请前往 Arcaea 官网查看详情。</para>
			/// </summary>
			AccountHasBeenLimited = 123,
			/// <summary>
			/// 此功能的使用被限制。
			/// <para>游戏内提示: 非常抱歉您已被限制使用此功能<br />如果不知道发生了什么,请联系 arcaea@lowiro.com。</para>
			/// </summary>
			CurrentFunctionUseHasBeenLimited = 150,
			/// <summary>
			/// 该功能目前无法使用。
			/// <para>游戏内提示: 目前无法使用此功能</para>
			/// </summary>
			CannotUseFunctionNow = 151,
			/// <summary>
			/// 用户不存在。
			/// <para>游戏提示: 用户不存在</para>
			/// </summary>
			UserNotExist = 401,
			/// <summary>
			/// 无法连接至服务器。
			/// <para>游戏内提示: 无法连接至服务器</para>
			/// </summary>
			CannotConnectToServer = 403,
			/// <summary>
			/// 该物品目前无法获取。
			/// <para>游戏内提示: 此物品目前无法获取</para>
			/// </summary>
			CannotGetThisItem = 501,
			/// <summary>
			/// 该物品目前无法获取。(第2个错误代码)
			/// <para>游戏内提示: 此物品目前无法获取</para>
			/// </summary>
			CannotGetThisItem2 = 502,
			/// <summary>
			/// 无效的序列码(礼品码)。
			/// <para>游戏内提示: 无效的序列码</para>
			/// </summary>
			InvalidRedeemCode = 504,
			/// <summary>
			/// 序列码(礼品码)已被使用。
			/// <para>游戏内提示: 此序列码已被使用</para>
			/// </summary>
			RedeemCodeHasBeenUsed = 505,
			/// <summary>
			/// 已拥有此物品。
			/// <para>游戏内提示: 您已拥有了此物品</para>
			/// </summary>
			AlreadyHasItem = 506,
			/// <summary>
			/// 好友列表已满。
			/// <para>游戏内提示: 好友列表已满</para>
			/// </summary>
			FriendListIsFull = 601,
			/// <summary>
			/// 此用户已是好友。
			/// <para>游戏内提示: 此用户已是好友</para>
			/// </summary>
			UserIsAlreadyFriend = 602,
			/// <summary>
			/// 不能加自己为好友。
			/// <para>游戏内提示: 您不能加自己为好友</para>
			/// </summary>
			CannotAddSelfAsFriend = 604,
			/// <summary>
			/// 此帐号的设备数量达到上限。
			/// <para>游戏内提示: 设备数量达到上限</para>
			/// </summary>
			DeviceCountsLimitReached = 1001,
			/// <summary>
			/// 此设备已使用过此功能。
			/// <para>游戏内提示: 该设备已使用过本功能</para>
			/// </summary>
			DeviceHasAlreadyUsedFunction = 1002,
			/// <summary>
			/// 未知异常。
			/// <para>游戏内提示: 发生了未知错误</para>
			/// </summary>
			Other = 999
		}
	}
}
