using System.CommandLine;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

using Crash.Server.Cli;
using Crash.Server.Hubs;
using Crash.Server.Model;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace Crash.Server
{

	public class Program
	{

		internal class CrashServerCreator
		{

			private Arguments Handler { get; }
			private WebApplication App { get; set; }

			internal CrashServerCreator(Arguments args)
			{
				Handler = args;
			}

			private static bool Try(Action action, string errorMessage)
			{
				try
				{
					action();
					return true;
				}
				catch
				{
					Console.WriteLine(errorMessage);
					return false;
				}
			}

			/// <summary>Creates an instance of the Crash WebApplication</summary>
			internal bool TryCreateApplication(string[] args, out WebApplication? webApplication)
			{
				webApplication = null;
				if (Handler.Exit) return false;

				var fileInfo = new FileInfo(Handler.DatabasePath);
				if (fileInfo.Exists && !Try(fileInfo.Delete, $"Failed to delete the database file")) return false;
				if (!Try(fileInfo.Directory.Create, $"Failed to delete the database directory")) return false;

				var webBuilder = WebApplication.CreateBuilder(args);
				var crashLogger = new CrashLoggerProvider(Handler.LoggingLevel);
				webBuilder.Logging.AddProvider(crashLogger);

				// Do we need this?
				webBuilder.WebHost.UseUrls(Handler.URL);
				webBuilder.Services.AddRazorPages();

				webBuilder.Services.AddDbContext<CrashContext>(options =>
					options.UseSqlite($"Data Source={Handler.DatabasePath}"));

				webBuilder.Services.AddSignalR()
					.AddHubOptions<CrashHub>(ConfigureCrashHubOptions)
					.AddJsonProtocol(ConfigureJsonOptions);

				App = webBuilder.Build();

				if (App.Environment.IsDevelopment())
				{
					App.MapGet("/config", () => App.Configuration.AsEnumerable());
					App.MapGet("/services", () =>
					{
						var crashHubOptionsService = App.Services.GetService<IConfigureOptions<HubOptions<CrashHub>>>();
						var connectionHandler = App.Services.GetService<HubConnectionHandler<CrashHub>>();
					});
				}

				App.UseHttpsRedirection();
				App.UseStaticFiles();
				App.UseRouting();
				App.MapRazorPages();
				App.MigrateDatabase<CrashContext>();
				App.MapHub<CrashHub>("/Crash");

				webApplication = App;
				return true;
			}

			private void ConfigureJsonOptions(JsonHubProtocolOptions jsonOptions)
			{
				var crashConfig = App.Configuration.GetRequiredSection("Crash");
				var signalRConfig = crashConfig.GetRequiredSection("SignalR");
				var jsonConfig = signalRConfig.GetRequiredSection("Json").Get<JsonSerializerOptions>();
				if (jsonConfig is null)
					return;
				
				jsonOptions.PayloadSerializerOptions = jsonConfig;
			}

			private void ConfigureCrashHubOptions(HubOptions<CrashHub> hubOptions)
			{
				var crashConfig = App.Configuration.GetRequiredSection("Crash");
				var signalRConfig = crashConfig.GetRequiredSection("SignalR");
				var crashHubOptions = signalRConfig.GetRequiredSection("CrashHub").Get<HubOptions<CrashHub>>();
				
				// TODO : Is there a way to deserialize the Section TO the hubOptions object? Below feels redundant
				hubOptions.MaximumParallelInvocationsPerClient = crashHubOptions.MaximumParallelInvocationsPerClient;
				hubOptions.MaximumReceiveMessageSize = crashHubOptions.MaximumReceiveMessageSize;
				hubOptions.StreamBufferCapacity = crashHubOptions.StreamBufferCapacity;
				hubOptions.KeepAliveInterval = crashHubOptions.KeepAliveInterval;
				hubOptions.HandshakeTimeout = crashHubOptions.HandshakeTimeout;
				hubOptions.EnableDetailedErrors = crashHubOptions.EnableDetailedErrors;
				hubOptions.ClientTimeoutInterval = crashHubOptions.ClientTimeoutInterval;
			}
		}

		static async Task<int> Main(string[] args)
		{
			try
			{
				var validatedArgs = await Arguments.ParseArgs(args);
				if (validatedArgs.Exit) return 1;

				var serverCreator = new CrashServerCreator(validatedArgs);
				if (!serverCreator.TryCreateApplication(validatedArgs.Args, out var app)) return 1;

				if (validatedArgs.OpenBrowser)
				{
					var startInfo = new ProcessStartInfo
					{
						FileName = validatedArgs.URL,
						UseShellExecute = true,
						CreateNoWindow = true
					};
					Process.Start(startInfo);
				}

				await app?.RunAsync();

				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine("\n");
				Console.WriteLine($"Hmm. That was unexpected ...");
				Console.WriteLine($"Please review your args for misspellings or copy/paste errors.");

				var errorHelper = new ErrorHelper(args);
				if (errorHelper.TryCaptureException(ex, out var assistanceMessage))
				{
					Console.WriteLine(assistanceMessage);
				}

				Console.WriteLine("\n");
				return 1;
			}
		}

	}
}
