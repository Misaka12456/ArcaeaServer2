using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace System.Enhance.AspNetCore
{
	public class RealIpFetcherMiddleware : IMiddleware
    {
        public RealIpFetcherMiddleware()
		{

		}

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var headers = context.Request.Headers;
            if (headers.ContainsKey("X-Forwarded-For"))
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse(headers["X-Forwarded-For"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries)[0]);
            }
            else if (headers.ContainsKey("X-Real-IP"))
			{
                context.Connection.RemoteIpAddress = IPAddress.Parse(headers["X-Real-IP"]);
			}
            return next(context);
        }
    }

	public class LargeDataProcessMiddleware : IMiddleware
	{
        public LargeDataProcessMiddleware()
		{

		}

		public Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
            var headers = context.Request.Headers;
            if (headers.ContainsKey("Transfer-Encoding"))
			{
                context.Response.Headers.Remove("Transfer-Encoding");
			}
            if (int.TryParse(context.Response.Headers["Content-Length"], out int contentLen) && (contentLen >= 1024))
			{
                context.Response.Headers.Remove("Content-Length");
                context.Response.Headers.Add("Transfer-Encoding", "Chunked");
            }
            return next(context);
		}
	}
}
