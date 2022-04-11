#nullable enable
using static Team123it.Arcaea.MarveCube.GlobalProperties;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Enhance.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Web;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Processors.Background;

namespace Team123it.Arcaea.MarveCube.Processors.Front
{
	/// <summary>
	/// 数据提供相关API。<br />
	/// 对应API前缀:/years/19/serve/
	/// </summary>
	public static class Serve
	{
		private static readonly string[] SongFileList = { "0.aff", "1.aff", "2.aff", "3.aff", "0.ogg", "1.ogg", "2.ogg", "3.ogg", "base.ogg" };
		public static JObject GetDownloadAvailableSongs(uint userId, IEnumerable<string>? customSongIds = null, bool isUrlMode = true)
		{
			if (isUrlMode)
			{
				// StandaloneToken.ForceUpdateToken();
			}
			var r = new JObject();
			if (customSongIds == null)
			{
				var allSongsDirInfos = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data", "static", "Songs")).GetDirectories();
				foreach (var songDirInfo in allSongsDirInfos)
				{
					string songId = songDirInfo.Name;
					var songDetails = GetSingleSongDownloadDetails(userId, songId, isUrlMode);
					if (songDetails != null)
					{
						foreach (var songDetailsSingleton in songDetails)
						{
							r[songDetailsSingleton.Key] = songDetailsSingleton.Value;
						}
					}
				}
			}
			else
			{
				var selectedSongsDirInfos = from songDirInfo in new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data", "static", "Songs")).GetDirectories()
											where customSongIds.Contains(songDirInfo.Name)
											select songDirInfo;
				foreach (var songDirInfo in selectedSongsDirInfos)
				{
					string songId = songDirInfo.Name;
					var songDetails = GetSingleSongDownloadDetails(userId, songId, isUrlMode);
					foreach (var songDetailsSingleton in songDetails!)
					{
						r[songDetailsSingleton.Key] = songDetailsSingleton.Value;
					}
				}
			}
			return r;
		}

		private static JObject? GetSingleSongDownloadDetails(uint userId, string songId, bool isUrlMode = true)
		{
			using var conn = new MySqlConnection(DatabaseConnectURL);
			try
			{
				if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data", "static", "Songs", songId, "Preparing.stat"))
					|| !Directory.Exists(Path.Combine(AppContext.BaseDirectory, "data", "static", "Songs", songId)))
				{
					if (isUrlMode)
					{
						throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.CannotGetThisItem);
					}
					else
					{
						return null;
					}
				}
				conn.Open();
				var r = new JObject();
				var audio = new JObject();
				var charts = new JObject();
				var songDirInfo = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data", "static", "Songs", songId));
				var songFiles = from songFile in songDirInfo.GetFiles()
								where SongFileList.Contains(songFile.Name)
								select songFile;
				string token = RC4Helper.Encrypt($"{userId}-{songId}-{DateTime.Now:yyyyMMddHHmmssfff}", StandaloneToken.Current.Token);
				var cmd = conn.CreateCommand();
				cmd.Parameters.Add(new MySqlParameter("?sid", MySqlDbType.VarChar)
				{
					Value = songId
				});
				foreach (var songFile in songFiles)
				{
					cmd.CommandText = "SELECT checksum FROM fixed_songs_checksum WHERE sid=?sid AND filename=?filename;";
					cmd.Parameters.Add(new MySqlParameter("?filename", MySqlDbType.VarChar)
					{
						Value = songFile.Name
					});
					var checkSumR = cmd.ExecuteScalar();
					string checkSum;
					if (checkSumR != null)
					{
						checkSum = checkSumR.ToString()!;
					}
					else
					{
						checkSum = MD5Helper.MD5Encrypt(File.ReadAllBytes(songFile.FullName));
						cmd.CommandText = "INSERT INTO fixed_songs_checksum (`sid`,`filename`,`checksum`) VALUES (?sid,?filename,?checksum)";
						var checkSumParam = new MySqlParameter("?checksum", MySqlDbType.VarChar)
						{
							Value = checkSum
						};
						cmd.Parameters.Add(checkSumParam);
						cmd.ExecuteNonQuery();
						cmd.Parameters.Remove(checkSumParam);
					}
					string url = $"{RemoteDownloadURLPrefix}/song/download?sid={HttpUtility.UrlEncode(songId)}&file={songFile.Name}&token={HttpUtility.UrlEncode(token)}";
					if (songFile.Name == "base.ogg")
					{
						audio.Add("checksum", checkSum);
						if (isUrlMode)
						{
							audio.Add("url", url);
						}
						r.Add("audio", audio);
					}
					else
					{
						int chartNumber = int.Parse(songFile.Name.Split('.')[0]);
						var chart = new JObject()
						{
							{ "checksum", checkSum }
						};
						if (isUrlMode)
						{
							chart.Add("url", url);
						}
						charts.Add(chartNumber.ToString(), chart);
					}
					cmd.Parameters.RemoveAt("?filename");
				}
				r.Add("chart", charts);
				if (isUrlMode)
				{
					Tokens.SetCustomDownloadToken(userId, songId, token);
				}
				return new JObject()
				{
					{ songId, r }
				};
			}
			catch (ArcaeaAPIException ex)
			{
				Console.WriteLine(ex.ToString());
				throw ex;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				throw new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.Other);
			}
			finally
			{
				conn.Close();
			}
		}
	}
}
