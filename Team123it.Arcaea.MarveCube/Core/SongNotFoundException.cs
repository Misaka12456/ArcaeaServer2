using System;
using System.Security.Cryptography;
using System.Text;

namespace Team123it.Arcaea.MarveCube.Core
{
	/// <summary>
	/// 当找不到曲目信息时抛出的异常。
	/// </summary>
	public class SongNotFoundException : Exception
	{
		/// <summary>
		/// 无效曲目的曲目id。
		/// </summary>
		public string SongId { get; }

		/// <summary>
		/// 无效曲目的曲目难度。
		/// <para>若本 <see cref="SongNotFoundException"/> 实例未特指某个难度,则本属性值为 <see langword="null" /> 。</para>
		/// </summary>
		public SongDifficulty? Difficulty { get; }

		/// <summary>
		/// 解释说明当前异常的信息。
		/// </summary>
		public override string Message { get; }

		/// <summary>
		/// 初始化 <see cref="SongNotFoundException"/> 类的新实例。
		/// </summary>
		/// <param name="songId">无效曲目的曲目id。</param>
		/// <param name="songDiff">无效曲目的曲目难度。
		/// <para>若未特指某一难度,请将本参数置为 <see langword="null"/> 。</para></param>
		public SongNotFoundException(string songId,SongDifficulty? songDiff)
		{
			SongId = songId;
			Difficulty = songDiff;
			var b = new StringBuilder("找不到id为 ").Append(songId).Append(" ");
			string diffStr = string.Empty;
			if (songDiff != null) {
				switch (songDiff)
				{
					case SongDifficulty.Past:
						diffStr = "Past";
						break;
					case SongDifficulty.Present:
						diffStr = "Present";
						break;
					case SongDifficulty.Future:
						diffStr = "Future";
						break;
					case SongDifficulty.Beyond:
						diffStr = "Beyond";
						break;
				}
				b.Append("且难度为 ").Append(diffStr).Append(" ");
			}
			b.Append("的曲目信息。");
			Message = b.ToString();
		}

		/// <summary>
		/// 创建并返回表示当前异常实例的字符串。
		/// </summary>
		/// <returns>代表当前异常的字符串。</returns>
		public override string ToString()
		{
			var b = new StringBuilder(Message).Append("\r\n").Append((StackTrace != null) ? StackTrace : string.Empty);
			return b.ToString();
		}

		/// <summary>
		/// 判断指定的对象是否与当前 <see cref="SongNotFoundException"/> 实例相等。
		/// </summary>
		/// <param name="obj">要判断的对象。</param>
		/// <returns>判断结果。</returns>
		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			else
			{
				var instance = (SongNotFoundException)obj;
				if (SongId == instance.SongId)
				{
					if (Difficulty != null)
					{
						if (instance.Difficulty != null && Difficulty == instance.Difficulty) return true;
						else return false;
					}
					else
					{
						if (instance.Difficulty != null) return false;
						else return true;
					}
				}
			}

			return base.Equals(obj);
		}

		public static bool operator ==(SongNotFoundException left,SongNotFoundException right)
		{
			try
			{
				if (left.SongId == right.SongId)
				{
					if (left.Difficulty != null)
					{
						if (right.Difficulty != null && left.Difficulty == right.Difficulty) return true;
						else return false;
					}
					else
					{
						if (right.Difficulty != null) return false;
						else return true;
					}
				} else
				{
					return false;
				}
			}
			catch(NullReferenceException)
			{
				return false;
			}
		}

		public static bool operator !=(SongNotFoundException left,SongNotFoundException right)
		{
			return !(left == right);
		}

		/// <summary>
		/// 计算当前 <see cref="SongNotFoundException"/> 实例的哈希值。
		/// </summary>
		/// <returns>计算结果。</returns>
		public override int GetHashCode()
		{
			return int.Parse(Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(SongId))));
		}
	}
}
