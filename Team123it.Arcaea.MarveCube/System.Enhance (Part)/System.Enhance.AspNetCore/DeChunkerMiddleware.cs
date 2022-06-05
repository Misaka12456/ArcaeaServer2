using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System;
using System.IO.Compression;
using Microsoft.AspNetCore.Http.Features;

namespace System.Enhance.AspNetCore
{
	public class DeChunkerMiddleware : IMiddleware
	{
		public DeChunkerMiddleware()
		{

		}

		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			// Disable Transfer-Encoding:chunked, use automatically Content-Length instead
			var feature = context.Features.Get<IHttpResponseBodyFeature>();
			feature?.DisableBuffering();
			context.Response.Headers["Content-Encoding"] = "identity";
			// For NGINX Server, we set this to forcibly disable buffering response
			// (lowiro curl client doesn't support buffering/chunked response)
			// (https://github.com/Misaka12456/ArcaeaServer2/issues/11)
			context.Response.Headers["X-Accel-Buffering"] = "no";
			var originalBodyStream = context.Response.Body;
			using (var responseBody = new MemoryStream())
			{
				context.Response.Body = responseBody;
				long length = 0;
				await next(context);
				// If you want to read the body, uncomment these lines.
				context.Response.Body.Seek(0, SeekOrigin.Begin);
				var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
				length = context.Response.Body.Length;
				context.Response.Body.Seek(0, SeekOrigin.Begin);
				context.Response.Headers.ContentLength = length;
				await responseBody.CopyToAsync(originalBodyStream);
			}
		}
	}
}
