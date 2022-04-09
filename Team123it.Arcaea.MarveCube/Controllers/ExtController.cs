#nullable enable
using System;
using System.Enhance.Web.Json;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Team123it.Arcaea.MarveCube.Core;
using Team123it.Arcaea.MarveCube.Processors.Background;
using static Team123it.Arcaea.MarveCube.GlobalProperties;
using File2 = System.IO.File;

namespace Team123it.Arcaea.MarveCube.Controllers
{
	[Route("years/19/ext")]
	[ApiController]
	public class ExtController : ControllerBase
	{
		private static string[] dbTables = {"bests","fixed_characters","fixed_packs","fixed_properties","fixed_songs","friend","logins","user_chars","user_world","users","user_saves","world_songplay"};
		[HttpGet("test")]
		public string DoFullTest([FromQuery]string? token)
		{
			if (string.IsNullOrWhiteSpace(token))
			{
				return @"Error: Invalid argument(s). Plase check your argument(s) before visit this page again.";
			}
			else
			{
				if (File2.Exists(Path.Combine(AppContext.BaseDirectory, "data", "testToken.txt")))
				{
					string gotToken = File2.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "testToken.txt"), Encoding.UTF8);
					if (token != gotToken)
					{
						return @"Error: Access denied: Incorrect test token.";
					}
					else
					{
						var test = new Thread(new ThreadStart(() =>
						{
							Thread.Sleep(5000);
							var start = DateTime.Now;
							Console.WriteLine("Welcome to 123 Marvelous Cube Test Module.");
							Console.Write("Please wait while we initializing the config...");
							string mysqlStr = DatabaseConnectURL;
							Console.WriteLine("Done.");
							Console.WriteLine("Database Connection String: {0}",DatabaseConnectURL);
							try
							{
								Console.WriteLine("Connecting to database...");
								var conn = new MySqlConnection(mysqlStr);
								conn.Open();
								Console.WriteLine("Database tables to test: {0}", string.Join(",", dbTables));
								Console.WriteLine("All tables tests will start in 5 seconds, console may be flushed by datas...");
								for (int i=4;i>0;i--)
								{
									Thread.Sleep(1000);
									Console.Write("All tables tests will start in {0} second", i);
									if (i > 1)
									{
										Console.Write("s\r\n");
									}
								}
								var cmd = conn.CreateCommand();
								foreach (string dbTable in dbTables)
								{
									string cmdStr = $"SELECT * FROM {dbTable}";
									cmd.CommandText = cmdStr;
									Console.ForegroundColor = ConsoleColor.Yellow;
									Console.WriteLine("Current command: {0}",cmdStr);
									Console.WriteLine("Executing...");
									var rd = cmd.ExecuteReader();
									Console.WriteLine("Result field count: {0}", rd.FieldCount);
									Console.ForegroundColor = ConsoleColor.White;
									while (rd.Read())
									{
										var result = new StringBuilder();
										for (int i = 0; i < rd.FieldCount; i++)
										{
											if (rd.IsDBNull(i))
											{
												result.Append("null");
											}
											else
											{
												result.Append(rd.GetValue(i));
											}
											if (i != (rd.FieldCount - 1))
											{
												result.Append(',');
											}
										}
										Console.WriteLine(result.ToString());
									}
									rd.Close();
								}
								Console.ResetColor();
								Console.WriteLine("Database tables test process finished.");
								conn.Close();
								Console.WriteLine("Connection closed.");
								File2.Delete(Path.Combine(AppContext.BaseDirectory, "data", "testToken.txt"));
								var end = DateTime.Now;
								var usedTime = end - start;
								Console.WriteLine("Test Successfully!!");
								Console.WriteLine("Used time:{0} s", usedTime.TotalSeconds);
							}
							catch(Exception ex)
							{
								var forceEnd = DateTime.Now;
								Console.WriteLine("Test failed!!");
								Console.WriteLine("Exception details:\r\nException Type:{0}\r\nException Message:{1}\r\nStacktrace:{2}", ex.GetType().ToString(), ex.Message, ex.StackTrace);
								Console.WriteLine("Test stopped unexpectedly.");
								Console.WriteLine("Used time:{0} s", (forceEnd - start).TotalSeconds);
							}
						}));
						test.Start();
						return "Welcome to 123 Marvelous Cube Test Module.\r\nWe are starting the test thread, please see the console for more details.\r\nHave a nice day!";
					}
				}
				else
				{
					return @"Information: Please create an text file which contains the test token on {AppContext.BaseDirectory}\data\testToken.txt and then visit {DomainName}/years/19/test?token={Your token} to start the test process.";
				}
			}
		}

		[HttpGet("song/album")]
		public Task<IActionResult> GetSongImage([FromQuery]string songid)
		{
			return Task.Run(new Func<IActionResult>(() =>
			{
				if (string.IsNullOrWhiteSpace(songid))
				{
					return new JObjectResult(new JObject() {
						{"success",false },
						{"msg","找不到曲目信息..." }
					});
				}
				else
				{
					if (File2.Exists(Path.Combine(AppContext.BaseDirectory,"data","albums",songid + ".png")) ||
						File2.Exists(Path.Combine(AppContext.BaseDirectory, "data", "albums", songid + ".jpg")))
					{
						string content_type,suffix;
						if (File2.Exists(Path.Combine(AppContext.BaseDirectory, "data", "albums", songid + ".png")))
						{
							content_type = "image/png";
							suffix = ".png";
						}
						else
						{
							content_type = "image/jpeg";
							suffix = ".jpg";
						}
						using var fs = new FileStream(Path.Combine(AppContext.BaseDirectory, "data", "albums", songid + suffix), FileMode.Open);
						var bytes = new byte[fs.Length];
						fs.Read(bytes, 0, bytes.Length);
						fs.Close();
						return new FileContentResult(bytes, content_type);
					}
					else
					{
						return new JObjectResult(new JObject()
						{
							{"success",false },
							{"msg","找不到对应的曲绘...可能是没这个曲目,也有可能是还没添加(x" }
						});
					}
				}
			}));
		}

		[HttpGet("song/download/{songId:required}/{fileName:required}")]
		public async Task<IActionResult> DownloadSongData([FromQuery]string token, [FromRoute]string songId, [FromRoute]string fileName)
		{
			return await Task.Run(new Func<IActionResult>(() =>
			{
				if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(songId) && !string.IsNullOrWhiteSpace(fileName))
				{
					if (Tokens.TryGetUserIdByDownloadToken(token, out uint userId, out string? downloadSongId))
					{
						var userInfo = new PlayerInfo(userId, out bool isExists);
						if (isExists)
						{
							if (!userInfo.Banned!.Value)
							{
								if (File2.Exists(Path.Combine(AppContext.BaseDirectory, "data", "static", "Songs", songId, fileName)))
								{
									string filePath = Path.Combine(AppContext.BaseDirectory, "data", "static", "Songs", songId, fileName);
									var file = new FileInfo(filePath);
									if (file.Directory!.Name == downloadSongId!)
									{
										byte[] data = File2.ReadAllBytes(filePath);
										var r = new FileContentResult(data, MediaTypeHeaderValue.Parse("application/octet-stream"))
										{
											FileDownloadName = file.Name
										};
										return r;
									}
									else
									{
										return new StatusCodeResult(StatusCodes.Status400BadRequest);
									}
								}
								else
								{
									return new StatusCodeResult(StatusCodes.Status404NotFound);
								}
							}
							else
							{
								return new ContentResult()
								{
									Content = new ArcaeaAPIException(ArcaeaAPIException.APIExceptionType.AccountHasBeenBlocked).ToString(),
									StatusCode = 403
								};
							}
						}
						else
						{
							return new StatusCodeResult(StatusCodes.Status403Forbidden);
						}
					}
					else
					{
						return new StatusCodeResult(StatusCodes.Status401Unauthorized);
					}
				}
				else
				{
					return new StatusCodeResult(StatusCodes.Status400BadRequest);
				}
			}));
		}
	}
}
