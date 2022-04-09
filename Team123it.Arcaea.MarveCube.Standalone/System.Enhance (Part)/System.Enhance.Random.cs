using System.Security.Cryptography;

namespace System.Enhance
{
	/// <summary>
	/// 提供适用于 <see cref="System.Random"/> 类的增强方法的类。无法继承此类。
	/// </summary>
	public sealed class Random
	{
		/// <summary>
		/// 生成指定长度的随机字符串。
		/// </summary>
		/// <param name="digits">随机字符串的长度。</param>
		/// <returns>生成结果。</returns>
		public static string GenerateRandomString(int digits)
		{
			byte[] result = new byte[digits - 1];
			RandomNumberGenerator.Create().GetBytes(result, 0, digits - 1);
			string resultStr = Convert.ToBase64String(result).Substring(0, digits - 1);
			return resultStr;
		}
	}
}
