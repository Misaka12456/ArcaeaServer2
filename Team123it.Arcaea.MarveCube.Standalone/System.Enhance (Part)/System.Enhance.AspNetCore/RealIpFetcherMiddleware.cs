using System.Net;

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
}
