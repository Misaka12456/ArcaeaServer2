#nullable enable
using System.ComponentModel;

namespace System.Enhance
{
	/// <summary>
	/// 提供适用于 <see cref="System.Collections"/> 类的增强方法的类。无法继承此类。
	/// </summary>
	public static class Collections
	{
		/// <summary>
		/// 获取枚举的 <see cref="DescriptionAttribute"/> 特性中的说明。
		/// </summary>
		/// <param name="instance">当前 <see cref="Enum"/> 枚举实例。</param>
		/// <returns>成功返回枚举的特性说明文本,失败返回 <see langword="null" /> 。</returns>
		public static string? GetDescription(this Enum instance)
		{
			var type = instance.GetType();
			var infos = type.GetMember(instance.ToString());
			if (infos != null && infos.Length > 0)
			{
				object[] attrs = infos[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
				if (attrs != null && attrs.Length > 0)
				{
					return ((DescriptionAttribute)attrs[0]).Description;
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}
	}
}
