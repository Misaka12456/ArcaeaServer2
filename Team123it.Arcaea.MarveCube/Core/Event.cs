using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Team123it.Arcaea.MarveCube.Core
{
	/// <summary>
	/// 表示一个增值活动。
	/// </summary>
	public struct Event
	{
		/// <summary>
		/// 活动的名称。
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; }

		/// <summary>
		/// 活动是否为限时。
		/// </summary>
		[JsonProperty("isTimeLimited")]
		public bool IsTimeLimited { get; }

		/// <summary>
		/// 限时活动的开始日期,若非限时活动则为 <see langword="null" /> 。
		/// </summary>
		[JsonIgnore]
		public DateTime? StartTime
		{
			get
			{
				if (StartTimeStamp.HasValue)
				{
					return DateTime.UnixEpoch.AddSeconds(StartTimeStamp.Value);
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// 限时活动的结束日期,若非限时活动则为 <see langword="null" /> 。
		/// </summary>
		[JsonIgnore]
		public DateTime? EndTime
		{
			get
			{
				if (EndTimeStamp.HasValue)
				{
					return DateTime.UnixEpoch.AddSeconds(EndTimeStamp.Value);
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// 限时活动开始日期的时间戳,若非限时活动则为 <see langword="null" /> 。
		/// </summary>
		[JsonProperty("startTime")]
		private long? StartTimeStamp { get; }

		/// <summary>
		/// 限时活动结束日期的时间戳,若非限时活动则为 <see langword="null" /> 。
		/// </summary>
		[JsonProperty("endTime",NullValueHandling = NullValueHandling.Include)]
		private long? EndTimeStamp { get; }

		/// <summary>
		/// 活动的奖励数据数组。
		/// </summary>
		[JsonProperty("rewards")]
		public JArray Rewards { get; }

		/// <summary>
		/// 完成活动需满足的条件数据数组。
		/// </summary>
		[JsonProperty("conditions")]
		public JArray Conditions { get; }
	}
}
