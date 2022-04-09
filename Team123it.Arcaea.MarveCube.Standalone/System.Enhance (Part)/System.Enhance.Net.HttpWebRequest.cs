#nullable enable
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace System.Enhance.Net
{
	/// <summary>
	/// 提供适用于 <see cref="System.Net.HttpWebRequest"/> 类的增强方法。无法继承此类。
	/// </summary>
	public static class HttpWebRequest
	{
		/// <summary>
		/// 使用HTTP协议请求访问指定的URL并获取响应所返回的数据文本。
		/// </summary>
		/// <param name="url">必需。要访问的URL。</param>
		/// <param name="method">可选。请求的方法(GET/POST/PUT/DELETE)。默认为GET。</param>
		/// <param name="userAgent">可选。请求的User-Agent参数值。默认为 <see langword="null"/> 。</param>
		/// <param name="accept">可选。请求的Accept参数值。默认为 <see langword="null"/> 。</param>
		/// <param name="reqContentType">可选。请求的Content-Type参数值。默认为 <see langword="null"/> (视为application/x-www-form-urlencoded) 。</param>
		/// <param name="respEncoding">可选。要将响应的数据解码成字符串所使用的编码类型。 默认为 <see langword="null"/> (视为UTF-8编码)。</param>
		/// <param name="header">可选。请求的Header键值对集合。默认为 <see langword="null"/> 。</param>
		/// <param name="reqBody">可选。请求体(Body)的数据。仅在请求方法为POST/PUT下可用。默认为 <see langword="null"/> 。</param>
		/// <returns>成功返回响应所返回的数据, 失败抛出异常。</returns>
		/// <exception cref="HttpRequestException" />
		/// <exception cref="SocketException" />
		/// <exception cref="ArgumentException" />
		public static async Task<string> SendHttpRequestAsync(string url,string method = "GET",string? userAgent = null,
			string? accept = null, HttpContent? content = null,
			Encoding? respEncoding = null, Dictionary<string,string>? header = null)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(url))
				{
					throw new ArgumentException("请求访问的URL不能为空。");
				}
				else
				{
					var methodType = method switch
					{
						"GET" => HttpMethod.Get,
						"POST" => HttpMethod.Post,
						"PUT" => HttpMethod.Put,
						"DELETE" => HttpMethod.Delete,
						_ => throw new ArgumentException($"无效的请求类型值: '{method}' 。值必须是以下之一: POST, GET, PUT, DELETE", nameof(method))
					};
					var client = new HttpClient();
					if (userAgent != null) client.DefaultRequestHeaders.Add("User-Agent", userAgent);
					if (accept != null) client.DefaultRequestHeaders.Add("Accept", accept);
					if (header != null)
					{
						foreach (var keyValue in header!)
						{
							client.DefaultRequestHeaders.Add(keyValue.Key, keyValue.Value);
						}
					}
					using var reqMsg = new HttpRequestMessage(methodType, url);
					if (content != null)
					{
						reqMsg.Content = content;
					}
					else if (methodType == HttpMethod.Post && methodType == HttpMethod.Put)
					{
						reqMsg.Content = new FormUrlEncodedContent(new Dictionary<string, string>());
					}
					else
					{
						reqMsg.Content = null;
					}
					var resp = await client.SendAsync(reqMsg);
					if ((int)resp.StatusCode != 200)
					{
						throw new SocketException((int)resp.StatusCode);
					}
					else
					{
						var respReader = new StreamReader(resp.Content.ReadAsStreamAsync().Result, ((respEncoding != null) ? respEncoding! : Encoding.UTF8));
						string respDataStr = respReader.ReadToEnd();
						respReader.Close();
						return respDataStr;
					}
				}
			}
			catch (HttpRequestException)
			{
				throw;
			}
			catch (SocketException)
			{
				throw;
			}
			catch (IOException ex)
			{
				throw new HttpRequestException("Failed reading data from the response stream.", ex);
			}
			catch (ArgumentException)
			{
				throw;
			}
		}

		/// <summary>
		/// 使用HTTP协议请求访问指定的URL并获取响应所返回的数据文本。
		/// </summary>
		/// <param name="url">必需。要访问的URL。</param>
		/// <param name="method">可选。请求的方法(POST/PUT)。默认为POST。</param>
		/// <param name="userAgent">可选。请求的User-Agent参数值。默认为 <see langword="null"/> 。</param>
		/// <param name="accept">可选。请求的Accept参数值。默认为 <see langword="null"/> 。</param>
		/// <param name="reqContentType">可选。请求的Content-Type参数值。默认为 application/x-www-form-urlencoded 。</param>
		/// <param name="respEncoding">可选。要将响应的数据解码成字符串所使用的编码类型。 默认为 <see langword="null"/> (视为UTF-8编码)。</param>
		/// <param name="header">可选。请求的Header键值对集合。默认为 <see langword="null"/> 。</param>
		/// <param name="reqBodyForm">可选。请求体(Body)的键值对数据集合(Form格式的数据)。默认为 <see langword="null"/> 。</param>
		/// <returns>成功返回响应所返回的数据, 失败抛出异常。</returns>
		/// <exception cref="HttpRequestException" />
		/// <exception cref="SocketException" />
		/// <exception cref="ArgumentException" />
		public static async Task<string> SendHttpFormRequestAsync(string url, string method = "POST", string? userAgent = null,
			string? accept = null, string reqContentType = "application/x-www-form-urlencoded", Encoding? respEncoding = null,
			Dictionary<string, string>? header = null, Dictionary<string,string>? reqBodyForm = null)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(url))
				{
					throw new ArgumentException("请求访问的URL不能为空。");
				}
				else
				{
					var methodType = method switch
					{
						"POST" => HttpMethod.Post,
						"PUT" => HttpMethod.Put,
						_ => throw new ArgumentException($"无效的请求类型值: '{method}' 。值必须是以下之一: POST, PUT", nameof(method))
					};
					var client = new HttpClient();
					if (userAgent != null) client.DefaultRequestHeaders.Add("User-Agent", userAgent);
					if (accept != null) client.DefaultRequestHeaders.Add("Accept", accept);
					if (header != null)
					{
						foreach (var keyValue in header!)
						{
							client.DefaultRequestHeaders.Add(keyValue.Key, keyValue.Value);
						}
					}
					using var reqMsg = new HttpRequestMessage(methodType, url);
					if (reqBodyForm != null)
					{
						var encodedForm = reqBodyForm.Select(i => WebUtility.UrlEncode(i.Key) + "=" + WebUtility.UrlEncode(i.Value));
						reqMsg.Content = new StringContent(string.Join("&", encodedForm), null, reqContentType);
						// 解决方案来自 Stackoverflow "HttpClient: The uri string is too long"
						// 若直接使用UrlEncodedContent并使用url编码后超过640k的Dictionary作为初始化参数则会报错"The uri string is too long"
					}
					else
					{
						reqMsg.Content = null;
					}
					var resp = await client.SendAsync(reqMsg);
					if ((int)resp.StatusCode != 200)
					{
						throw new SocketException((int)resp.StatusCode);
					}
					else
					{
						var respReader = new StreamReader(resp.Content.ReadAsStreamAsync().Result, ((respEncoding != null) ? respEncoding! : Encoding.UTF8));
						string respDataStr = respReader.ReadToEnd();
						respReader.Close();
						return respDataStr;
					}
				}
			}
			catch (HttpRequestException)
			{
				throw;
			}
			catch (SocketException)
			{
				throw;
			}
			catch (IOException ex)
			{
				throw new HttpRequestException("Failed reading data from the response stream.", ex);
			}
			catch (ArgumentException)
			{
				throw;
			}
		}
	}
}
