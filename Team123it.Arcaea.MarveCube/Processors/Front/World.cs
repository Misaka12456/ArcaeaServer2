using Newtonsoft.Json.Linq;
using Team123it.Arcaea.MarveCube.Core;
using World2 = Team123it.Arcaea.MarveCube.Processors.Background.World;

namespace Team123it.Arcaea.MarveCube.Processors.Front
{
	/// <summary>
	/// 世界(World)模式相关API。<br />
	/// 对应API前缀:/years/19/world/
	/// </summary>
	public class World
	{
		/// <summary>
		/// 获取世界模式完整信息。
		/// </summary>
		/// <param name="userid">玩家的用户id。</param>
		/// <returns>获取到的世界模式信息数据 <see cref="JObject"/> 。</returns>
		/// <exception cref="ArcaeaAPIException" />
		public static JObject GetAllWorldInfo(uint userid)
		{
			var r = new JObject();
			var info = new PlayerInfo(userid, out bool isExists);
			if (!isExists) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.UserNotExist); //用户不存在
			if (info.Banned.Value) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountHasBeenBlocked);
			//用户账号被冻结
			r.Add("current_map", info.CurrentMap);
			r.Add("user_id", userid);
			r.Add("maps", World2.GetAllMaps(userid, out bool success));
			if (!success) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
			return r;
		}

		/// <summary>
		/// 获取玩家指定地图的数据。
		/// </summary>
		/// <param name="userid">玩家的用户id。</param>
		/// <param name="mapid">地图id。</param>
		/// <returns>获取到的地图数据 <see cref="JObject"/> 。</returns>
		/// <exception cref="ArcaeaAPIException" />
		public static JObject GetUserMapInfo(uint userid,string mapid)
		{
			var r = World2.GetUserMap(userid, mapid, out bool success);
			if (!success) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
			return r;
		}

		/// <summary>
		/// 获取玩家的指定地图的完整信息。
		/// </summary>
		/// <param name="userid">玩家的用户id。</param>
		/// <param name="mapid">地图id。</param>
		/// <returns>获取到的地图完整信息 <see cref="JObject"/> 。</returns>
		/// <exception cref="ArcaeaAPIException" />
		public static JObject GetUserSingleMap(uint userid,string mapid)
		{
			var r = new JObject();
			var info = new PlayerInfo(userid, out bool isExists);
			if (!isExists) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.UserNotExist); //用户不存在
			if (info.Banned.Value) throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountHasBeenBlocked);
			//玩家账号已被冻结
			info.CurrentMap = mapid;
			r.Add("user_id", userid);
			r.Add("current_map", mapid);
			var userMapDetails = World2.GetUserMap(userid, mapid, out _);
			var userMap = World2.GetMap(mapid, out _);
			userMap.Remove("curr_position");
			userMap.Remove("curr_capture");
			userMap.Add("curr_position", userMapDetails.TryGetValue("curr_position", out var curr_position) ? (int)curr_position! : 0);
			userMap.Add("curr_capture", userMapDetails.TryGetValue("curr_capture", out var curr_capture) ? (double)curr_capture! : 0.0);
			r.Add("maps", new JArray()
			{
				{ userMap }
			});
			return r;
		}
	}
}
