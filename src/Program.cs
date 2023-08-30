using Crash.Server.Hubs;
using Crash.Server.Model;
using Crash.Server.Settings;

namespace Crash.Server
{
	// TODO : Improve logging
	public class Program
	{
		/// <summary>Creates an instance of the Crash WebApplication</summary>
		public static WebApplication CreateApplication(string[] args)
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

			var app = builder.Build();

			app.MapGet("/", () => "Welcome to Crash!");
			app.MapHub<CrashHub>("/Crash");

#if DEBUG

			app.MapGet("/Debug", () =>
			{
				var scope = app.Services.CreateScope();
				var con = scope.ServiceProvider.GetService<CrashContext>();

				var aa = app.Urls;
				var bend = app.Environment;
				var config = app.Configuration;
				var life = app.Lifetime;

				var debugMessage = "Debugging is enabled!\n";
				debugMessage += $"| OS		| {Environment.OSVersion} \n";
				debugMessage += $"| DOTNET	| {Environment.Version} \n";
				debugMessage += $"| CPU #		| {Environment.ProcessorCount} \n";
				debugMessage += $"| Process	| {Environment.ProcessPath} \n";
				debugMessage += $"| Dir		| {Environment.CurrentDirectory} \n";
				debugMessage += $"| User		| {Environment.UserName} \n";
				debugMessage += $"| 64BitOS	| {Environment.Is64BitOperatingSystem} \n";
				debugMessage += $"| 64BitExe	| {Environment.Is64BitProcess} \n";
				debugMessage += $"| Services	| {app.Services} \n";
				debugMessage += $"| Environment	| {app.Environment.EnvironmentName} \n";
				debugMessage += $"| urls		| {string.Join(", \n| \t\t > ", app.Urls)} \n";
				debugMessage += $"| args		| {string.Join(", \n| \t\t > ", Environment.GetCommandLineArgs())} \n";
				debugMessage += $"| vars		| {string.Join(", \\n| \\t\\t > ", Environment.GetEnvironmentVariables())} \n";
				
				debugMessage += "|-----------------------------------------------------------------------------------------------------\n";

				debugMessage += $"| Users		| {con.Users.Count()}\n";
				debugMessage += $"| Changes	| {con.Changes.Count()}\n";
				debugMessage += $"| Latest	| {con.LatestChanges.Count()}\n";
				debugMessage += $"| Tracker	| {con.ChangeTracker}\n";

				return debugMessage;
			});


			app.MapGet($"/Debug/Changes", () => {
				var changeText = string.Empty;

				var scope = app.Services.CreateScope();
				var con = scope.ServiceProvider.GetService<CrashContext>();

				foreach (var change in con.Changes)
				{
					changeText += $"| Id		| {change.Id}\n";
					changeText += $"| UniqueId		| {change.UniqueId}\n";
					changeText += $"| Action		| {change.Action}\n";
					changeText += $"| Stamp		| {change.Stamp}\n";
					changeText += $"| Type		| {change.Type}\n";
					changeText += $"| Owner		| {change.Owner}\n";
					var payload = change.Payload ?? "";
					changeText += $"| Payload		| {payload[..Math.Min(payload.Length, 50)]}...\n";
					changeText += "---------------\n\n";
				}

				return changeText;
			});

			app.MapGet($"/Debug/Users", () => {
				var userText = string.Empty;

				var scope = app.Services.CreateScope();
				var con = scope.ServiceProvider.GetService<CrashContext>();

				foreach (var user in con.Users)
				{
					userText += $"| Name		| {user.Name}\n";
					userText += $"| Follows	| {user.Follows}\n";
					userText += "---------------\n\n";
				}

				return userText;
			});

			app.MapGet($"/Debug/Latest/", () => {
				var latestText = string.Empty;

				var scope = app.Services.CreateScope();
				var con = scope.ServiceProvider.GetService<CrashContext>();

				foreach (var change in con.LatestChanges.ToArray())
				{
					latestText += $"| Id		| {change.Id}\n";
					latestText += $"| Action		| {change.Action}\n";
					latestText += $"| Stamp		| {change.Stamp}\n";
					latestText += $"| Type		| {change.Type}\n";
					latestText += $"| Owner		| {change.Owner}\n";
					var payload = change.Payload ?? "";
					latestText += $"| Payload		| {payload[..Math.Min(payload.Length, 50)]}...\n";
					latestText += "---------------\n\n";
				}

				return latestText;
			});

#endif

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
