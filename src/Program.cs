using System.Text.Json;
using System.Text.Json.Serialization;

using Crash.Server.Hubs;
using Crash.Server.Model;

using Microsoft.AspNetCore.SignalR;

namespace Crash.Server
{
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

			if (argHandler.ResetDB && File.Exists(argHandler.DatabaseFileName))
			{
				File.Delete(argHandler.DatabaseFileName);
			}

			var webBuilder = WebApplication.CreateBuilder(args);
			var crashLogger = new CrashLoggerProvider();
			webBuilder.Logging.AddProvider(crashLogger);

			webBuilder.Services.AddSignalR()
				.AddHubOptions<CrashHub>(hubOptions =>
				{
					var crashConfig = webBuilder.Configuration.GetRequiredSection("Crash");
					var signalRConfig = crashConfig.GetRequiredSection("SignalR");
					hubOptions = signalRConfig.GetRequiredSection("CrashHub").Get<HubOptions<CrashHub>>();
				})
				.AddJsonProtocol(jsonOptions =>
				{
					var crashConfig = webBuilder.Configuration.GetRequiredSection("Crash");
					var signalRConfig = crashConfig.GetRequiredSection("SignalR");
					jsonOptions.PayloadSerializerOptions =
						signalRConfig.GetRequiredSection("Json").Get<JsonSerializerOptions>();
				});

			webBuilder.Services.AddDbContext<CrashContext>(options =>
				options.UseSqlite($"Data Source={argHandler.DatabaseFileName}"));
			
			// Do we need this?
			webBuilder.WebHost.UseUrls(argHandler.URL);
			webBuilder.Services.AddRazorPages();

			var app = webBuilder.Build();

			app.MapHub<CrashHub>("/Crash");

			if (app.Environment.IsDevelopment())
			{
				app.MapGet("/logging", () => string.Join("\n", crashLogger.Logger.Messages));
				app.MapGet("/settings", () => app.Configuration.ToString());
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();
			app.MapRazorPages();

			app.MigrateDatabase<CrashContext>();
			return app;
		}

		public static void Main(string[] args)
		{
			var app = CreateApplication(args);
			app.Run();
		}
	}
}
