using System.Net.Mime;
using System.Text;

using Crash.Server.Hubs;
using Crash.Server.Model;
using Crash.Server.Settings;

namespace Crash.Server
{
	internal class HtmlResult : IResult
	{
		private readonly string _html;

		public HtmlResult(string html)
		{
			_html = html;
		}

		public Task ExecuteAsync(HttpContext httpContext)
		{
			httpContext.Response.ContentType = MediaTypeNames.Text.Html;
			httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(_html);
			return httpContext.Response.WriteAsync(_html);
		}
	}

	internal static class ResultsExtensions
	{
		public static IResult Html(this IResultExtensions resultExtensions, string html)
		{
			ArgumentNullException.ThrowIfNull(resultExtensions);

			return new HtmlResult(html);
		}
	}

	// TODO : Improve logging
	public class Program
	{
		/// <summary>Creates an instance of the Crash WebApplication</summary>
		public static WebApplication CreateApplication(params string[] args)
		{
			var argHandler = new ArgumentHandler();

			argHandler.EnsureDefaults();
			argHandler.ParseArgs(args);
			if (argHandler.Exit)
			{
				return null;
			}

			var builder = WebApplication.CreateBuilder(args);
			var config = new ConfigHandler();

			builder.Services.AddSignalR()
				.AddHubOptions<CrashHub>(hubOptions =>
					config.Crash.SignalR.BuildCrashHubConfig(hubOptions))
				.AddJsonProtocol(jsonOptions =>
					config.Crash.SignalR.BuildJsonConfig(jsonOptions));

			builder.Services.AddDbContext<CrashContext>(options =>
				options.UseSqlite($"Data Source={argHandler.DatabaseFileName}"));

			// Do we need this?
			builder.WebHost.UseUrls(argHandler.URL);
			builder.Services.AddRazorPages();

			var app = builder.Build();

			app.MapHub<CrashHub>("/Crash");
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();
			app.MapRazorPages();

			app.MigrateDatabase<CrashContext>();
			return app;
		}

		public static void Main(string[] args)
		{
			// Display the number of command line arguments.
			Console.WriteLine(args.Length);
			var app = CreateApplication(args);
			app.Run();
		}
	}
}
