using static Team123it.Arcaea.MarveCube.GlobalProperties;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using Team123it.Arcaea.MarveCube.Core;

namespace Team123it.Arcaea.MarveCube.Processors.Front
{
	/// <summary>
	/// 玩家好友管理相关API。<br />
	/// 对应API前缀:/cutestscope/2/friend/
	/// </summary>
	public class Friend
	{

		/// <summary>
		/// [API]添加好友。
		/// </summary>
		/// <param name="userid">玩家的用户id。</param>
		/// <param name="friendCode">新好友玩家的好友id。</param>
		/// <exception cref="ArcaeaAPIException" />
		public static JObject AddFriend(uint userid,string friendCode)
		{
			var me = new PlayerInfo(userid, out _);
			if (me.UserCode == friendCode) //不能添加自己为好友
				throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.CannotAddSelfAsFriend);
			var friendInfo = new PlayerInfo(friendCode,out bool isExists);
			if (!isExists) //用户不存在
				throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.UserNotExist);
			using var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = conn.CreateCommand();
			cmd.CommandText = $"SELECT COUNT(*) FROM friend WHERE user_id_me={userid} AND user_id_other={friendInfo.UserId};";
			long result = (long)cmd.ExecuteScalar();
			if (result == 1) //如果已是好友
			{
				conn.Close();
				throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.UserIsAlreadyFriend);
			} else
			{
				cmd.CommandText = $"SELECT COUNT(*) FROM friend WHERE user_id_me={userid}";
				long friendCounts = (long)cmd.ExecuteScalar();
				if (friendCounts == long.Parse(ConfigurationManager.AppSettings["MaxFriendsCount"])) //如果好友列表已满
				{
					conn.Close();
					throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.FriendListIsFull);
				} else //否则(添加好友)
				{
					cmd.CommandText = $"INSERT INTO friend (user_id_me,user_id_other) VALUES ({userid},{friendInfo.UserId.Value})";
					cmd.ExecuteNonQuery();
					string updatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
					cmd.CommandText = $"SELECT join_date FROM users WHERE user_id={userid}";
					long join_date = (long)cmd.ExecuteNonQuery();
					string createdAt = new DateTime(1970, 1, 1).AddSeconds(join_date).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
					conn.Close();
					var r = new JObject()
					{
						{"user_id",userid },
						{"updatedAt",updatedAt },
						{"createdAt",createdAt },
						{"friends", me.FriendsList}
					};
					return r;
				}
			}
		}

		/// <summary>
		/// [API]删除好友。
		/// </summary>
		/// <param name="userid">玩家的用户id。</param>
		/// <param name="friendCode">要删除的好友对应玩家的好友id。</param>
		/// <exception cref="ArcaeaAPIException" />
		public static JObject DeleteFriend(uint userid,int friendid)
		{
			var me = new PlayerInfo(userid, out _);
			using var conn = new MySqlConnection(DatabaseConnectURL);
			conn.Open();
			var cmd = conn.CreateCommand();
			cmd.CommandText = $"DELETE FROM friend WHERE user_id_me={userid} AND user_id_other={friendid}";
			cmd.ExecuteNonQuery();
			string updatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
			cmd.CommandText = $"SELECT join_date FROM users WHERE user_id={userid}";
			long join_date = (long)cmd.ExecuteScalar();
			string createdAt = new DateTime(1970, 1, 1).AddSeconds(join_date).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
			conn.Close();
			var r = new JObject()
			{
				{"user_id",userid },
				{"updatedAt",updatedAt },
				{"createdAt",createdAt },
				{"friends", me.FriendsList}
			};
			return r;
		}
	}
}
