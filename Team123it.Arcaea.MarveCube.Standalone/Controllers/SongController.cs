using Microsoft.AspNetCore.Mvc;
using System.Enhance.Security.Cryptography;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Team123it.Arcaea.MarveCube.Standalone.Core;
using File2 = System.IO.File;

namespace Team123it.Arcaea.MarveCube.Standalone.Controllers
{
	[Route("song")]
	[ApiController]
	public class SongController : ControllerBase
	{
		private static readonly string[] Files = { "base.ogg", "3.ogg", "0.aff", "1.aff", "2.aff", "3.aff" };

		[HttpGet("download")]
		public async Task<IActionResult> DownloadSongData([FromQuery]string sid, [FromQuery]string file, [FromQuery]string token)
		{
			return await Task.Run(new Func<IActionResult>(() =>
			{
				try
				{
					if (!string.IsNullOrWhiteSpace(sid) && !string.IsNullOrWhiteSpace(file) && !string.IsNullOrWhiteSpace(token))
					{
						token = token.Replace(" ", string.Empty);
						string plainToken = RC4Helper.Decrypt(token, StandaloneTokenHelper.GetToken().Result);
						// plainToken格式: "{userId}-{songId}-{DateTime.Now:yyyyMMddHHmmssfff}"
						int userId = int.Parse(plainToken.Split('-')[0]);
						string songId = plainToken.Split('-')[1];
						var createTime = DateTime.ParseExact(plainToken.Split('-')[2], "yyyyMMddHHmmssfff", CultureInfo.CurrentCulture);
						if ((createTime - DateTime.Now).TotalHours < 1.5 && songId == sid)
						{
							if (Array.IndexOf(Files, file) != -1 && File2.Exists(Path.Combine(AppContext.BaseDirectory, "data", "static", "Songs", sid, file)))
							{
								byte[] data = File2.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "data", "static", "Songs", sid, file));
								Console.WriteLine($@"[{DateTime.Now:yyyy-M-d H:mm:ss.fff}][{HttpContext.Connection.RemoteIpAddress}][200 OK] Fetched file {sid}\{file}");
								return new FileContentResult(data, "application/octet-stream")
								{
									FileDownloadName = file
								};
							}
							else
							{
								Console.WriteLine($"[{DateTime.Now:yyyy-M-d H:mm:ss.fff}][{HttpContext.Connection.RemoteIpAddress}][404 Not Found] Cannot find song file \"{sid}/{file}\". Please check your spelling and try again.");
								return NotFound("Cannot find song file. Please check your spelling and try again.");
							}
						}
						else
						{
							Console.WriteLine($"[{DateTime.Now:yyyy-M-d H:mm:ss.fff}][{HttpContext.Connection.RemoteIpAddress}][401 Unauthorized] Download link has been expired. Please fetch a new download link from Project Arcaea Server.");
							return Unauthorized("Download link has been expired. Please fetch a new download link from Project Arcaea Server.");
						}
					}
					else
					{
						Console.WriteLine($"[{DateTime.Now:yyyy-M-d H:mm:ss.fff}][{HttpContext.Connection.RemoteIpAddress}][400 Bad Request] Bad request.");
						return BadRequest("Bad request.");
					}
				}
				catch (FormatException)
				{
					Console.WriteLine($"[{DateTime.Now:yyyy-M-d H:mm:ss.fff}][{HttpContext.Connection.RemoteIpAddress}][400 Bad Request] Invalid arguments.\n" +
						$"sid={sid}\n" +
						$"file={file}\n" +
						$"token={token}");
					return BadRequest("Invalid arguments.");
				}
				catch(Exception ex)
				{
					Console.WriteLine($"[{DateTime.Now:yyyy-M-d H:mm:ss.fff}][{HttpContext.Connection.RemoteIpAddress}][500 Internal Server Error] An unexpected error occurred.");
					Console.WriteLine(ex.ToString());
					return new ContentResult()
					{
						Content = "An unexpected error occurred. Please contact 123 Open-Source Organization(Team123it) to solve the problem.",
						ContentType = "text/html",
						StatusCode = StatusCodes.Status500InternalServerError
					};
				}
			}));
		}

		[HttpGet("test")]
		public async Task<IActionResult> TestFetchSongData([FromQuery]string sid, [FromQuery]string token)
		{
			return await Task.Run(new Func<IActionResult>(() =>
			{
				try
				{
					if (!string.IsNullOrWhiteSpace(sid) && !string.IsNullOrWhiteSpace(token))
					{
						if (token == StandaloneTokenHelper.GetToken().Result)
						{
							if (Directory.Exists(Path.Combine(AppContext.BaseDirectory, "data", "static", "Songs", sid)))
							{
								var folder = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data", "static", "Songs", sid));
								string[] fileExts = new[] { ".aff", ".ogg" };
								var files = new List<FileInfo>();
								foreach (string ext in fileExts)
								{
									files.AddRange(from file in folder.GetFiles(Path.Combine(AppContext.BaseDirectory, "data", "static", "Songs", sid))
												   where file.FullName.ToLower().EndsWith(ext)
												   select file);
								}
								var r = new StringBuilder("Project Arcaea Standalone API Song Data Test Result\n")
									 .Append("Song Id(sid):").AppendLine(sid);
								int fileCount = 0;
								foreach (var file in files)
								{
									r.Append("File Name: ").AppendLine(file.Name);
									r.Append("MD5 Checksum Result: ");
									byte[] md5RawHash = MD5.HashData(File2.ReadAllBytes(file.FullName));
									string md5Hash = BitConverter.ToString(md5RawHash).Replace("-", string.Empty).ToLower();
									r.AppendLine(md5Hash);
									fileCount++;
								}
								r.Append("Total File(s) Count: ").Append(fileCount);
								return Ok(r.ToString());
							}
							else
							{
								Console.WriteLine($"[{DateTime.Now:yyyy-M-d H:mm:ss.fff}][{HttpContext.Connection.RemoteIpAddress}][404 Not Found]Test failed: Cannot find data of the song '{sid}'");
								return NotFound($"Test failed: Cannot find data of the song '{sid}'.");
							}
						}
						else
						{
							Console.WriteLine($"[{DateTime.Now:yyyy-M-d H:mm:ss.fff}][{HttpContext.Connection.RemoteIpAddress}][403 Forbidden]Invalid test token: {token}.");
							return Forbid("Invalid token.");
						}
					}
					else
					{
						Console.WriteLine($"[{DateTime.Now:yyyy-M-d H:mm:ss.fff}][{HttpContext.Connection.RemoteIpAddress}][400 Bad Request] Bad request.");
						return BadRequest("Bad request.");
					}
				}
				catch (FormatException)
				{
					Console.WriteLine($"[{DateTime.Now:yyyy-M-d H:mm:ss.fff}][{HttpContext.Connection.RemoteIpAddress}][400 Bad Request] Invalid arguments.\n" +
						$"sid={sid}\n" +
						$"token={token}");
					return BadRequest("Invalid arguments.");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[{DateTime.Now:yyyy-M-d H:mm:ss.fff}][{HttpContext.Connection.RemoteIpAddress}][500 Internal Server Error] An unexpected error occurred.");
					Console.WriteLine(ex.ToString());
					return new ContentResult()
					{
						Content = "An unexpected error occurred. Please contact 123 Open-Source Organization(Team123it) to solve the problem.",
						ContentType = "text/html",
						StatusCode = StatusCodes.Status500InternalServerError
					};
				}
			}));
		}
	}
}
