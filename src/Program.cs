using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;

using Crash.Server.Hubs;
using Crash.Server.Model;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace Crash.Server
{

	public class Program
	{

		private class CrashServerCreator
		{

			private Arguments Handler { get; }
			private WebApplication App { get; set; }

			internal CrashServerCreator(Arguments args)
			{
				Handler = args;
			}

			/// <summary>Creates an instance of the Crash WebApplication</summary>
			internal WebApplication? CreateApplication(params string[] args)
			{
				if (Handler.Exit)
				{
					return null;
				}

				if (Handler.ResetDB && File.Exists(Handler.DatabasePath))
				{
					File.Delete(Handler.DatabasePath);
				}

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
					App.MapGet("/logging", () => string.Join("\n", crashLogger._logger.Messages));
					App.MapGet("/config", () => App.Configuration.AsEnumerable());
					App.MapGet("/services", () =>
					{
						var crashHubOptionsService = App.Services.GetService<IConfigureOptions<HubOptions<CrashHub>>>();
						var connectionHandler = App.Services.GetService<HubConnectionHandler<CrashHub>>();
						;
					});
				}

				App.UseHttpsRedirection();
				App.UseStaticFiles();
				App.UseRouting();
				App.MapRazorPages();
				App.MigrateDatabase<CrashContext>();
				App.MapHub<CrashHub>("/Crash");


				return App;
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
			var validatedArgs = await Arguments.ParseArgs(args);
			if (validatedArgs.Exit) return 0;

			var serverCreator = new CrashServerCreator(validatedArgs);
			var app = serverCreator.CreateApplication(validatedArgs.Args);
			await app?.RunAsync();

			return 0;
		}

	}
}
