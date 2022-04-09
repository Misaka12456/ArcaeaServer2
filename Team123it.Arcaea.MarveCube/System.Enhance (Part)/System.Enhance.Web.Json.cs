#nullable enable
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace System.Enhance.Web.Json
{
	/// <summary>
	/// 表示一个 <see cref="JObject"/> 实例(Json数据)格式的 <see cref="ActionResult"/> 。
	/// </summary>
	public class JObjectResult : ActionResult
	{
		/// <summary>
		/// 使用指定的 <see cref="JObject"/> 实例初始化 <see cref="JObjectResult"/> 类的新实例。
		/// </summary>
		/// <param name="data">指定的 <see cref="JObject"/> 实例。</param>
		public JObjectResult(JObject data)
		{
			JsonData = data ?? throw new NullReferenceException("未将对象引用设置到对象的实例。\r\ndata 值为null。");
		}

		/// <summary>
		/// 使用指定的Json字符串初始化 <see cref="JObjectResult"/> 类的新实例。
		/// </summary>
		/// <exception cref="JsonReaderException" />
		public JObjectResult(string? jsonStr)
		{
			try
			{
				JsonData = (jsonStr == null) ? new JObject() : JObject.Parse(jsonStr);
			}
			catch(JsonReaderException ex)
			{
				throw new JsonReaderException(ex.Message, ex);
			}
		}
		public override void ExecuteResult(ActionContext context)
		{
			var resp = context.HttpContext.Response;
			resp.StatusCode = 200;
			resp.ContentType = "application/json";
			resp.WriteAsync(JsonData.ToString(Formatting.None));
		}

		/// <summary>
		/// 获取当前 <see cref="JObjectResult"/> 实例对应的 <see cref="JObject"/> 实例(Json数据)。
		/// </summary>
		public JObject JsonData { get; }

		/// <summary>
		/// 将当前 <see cref="JObjectResult"/> 实例对应的 Json 数据转换为Json字符串。
		/// </summary>
		/// <returns>转换后的Json字符串。</returns>
		public override string ToString()
		{
			return JsonData.ToString();
		}

		public string ToString(Formatting formatting,params JsonConverter[] converters)
		{
			return JsonData.ToString(formatting, converters);
		}
	}

	/// <summary>
	/// 表示一个 <see cref="JArray"/> 实例(Json数组数据)格式的 <see cref="ActionResult"/> 。
	/// </summary>
	public class JArrayResult : ActionResult
	{
		/// <summary>
		/// 获取当前 <see cref="JArrayResult"/> 实例对应的 <see cref="JArray"/> 实例(Json数组数据)。
		/// </summary>
		public JArray JsonData { get; }

		/// <summary>
		/// 使用指定的 <see cref="JArray"/> 实例初始化 <see cref="JArrayResult"/> 类的新实例。
		/// </summary>
		/// <param name="data">指定的 <see cref="JArray"/> 实例。</param>
		public JArrayResult(JArray data)
		{
			JsonData = data ?? throw new NullReferenceException("未将对象引用设置到对象的实例。\r\ndata 值为null。");
		}

		/// <summary>
		/// 使用指定的Json字符串初始化 <see cref="JArrayResult"/> 类的新实例。
		/// </summary>
		/// <exception cref="JsonReaderException" />
		public JArrayResult(string? jsonStr)
		{
			try
			{
				JsonData = (jsonStr == null) ? new JArray() : JArray.Parse(jsonStr);
			}
			catch (JsonReaderException ex)
			{
				throw new JsonReaderException(ex.Message, ex);
			}
		}
		public override void ExecuteResult(ActionContext context)
		{
			var resp = context.HttpContext.Response;
			resp.StatusCode = 200;
			resp.ContentType = "application/json";
			resp.WriteAsync(JsonData.ToString(Formatting.None));
		}

		/// <summary>
		/// 将当前 <see cref="JArrayResult"/> 实例对应的 Json 数据转换为Json字符串。
		/// </summary>
		/// <returns>转换后的Json字符串。</returns>
		public override string ToString()
		{
			return JsonData.ToString();
		}

		public string ToString(Formatting formatting, params JsonConverter[] converters)
		{
			return JsonData.ToString(formatting, converters);
		}
	}
}
