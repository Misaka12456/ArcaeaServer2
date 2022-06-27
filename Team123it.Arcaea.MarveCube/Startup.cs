using System;
using System.Enhance.AspNetCore;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Team123it.Arcaea.MarveCube
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddTransient<DeChunkerMiddleware>();
			services.Configure<FormOptions>(options => options.BufferBody = true);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMiddleware<DeChunkerMiddleware>(); // Forcibly disable buffering(chunked) response

			app.UseRouting();

			app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });

			app.UseEndpoints((endpoints) =>
			{
				endpoints.MapControllers();
			});

			app.Run(async context =>
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"{DateTime.Now:[yyyy-M-d H:mm:ss]} Someone is trying to visit api without logining before.\n" +
					$"IP:{context.Connection.RemoteIpAddress}\n" +
					$"Visited Path:{context.Request.Path}");
				Console.ResetColor();
				await context.Response.WriteAsync("Sorry but this is not what you are waiting for...\n");
				await context.Response.WriteAsync($"Your IP:{context.Connection.RemoteIpAddress}\n");
				await context.Response.WriteAsync($"Current Path: {context.Request.Path}\n");
			});
		}

		// by Misaka12456 2022.4
	}
}
