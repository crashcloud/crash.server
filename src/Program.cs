﻿using System.Text.Json;
using System.Text.Json.Serialization;

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

			private readonly ArgumentHandler _argHandler;
			private readonly string[] _args;
			private WebApplication _app { get; set; }

			internal CrashServerCreator(params string[] args)
			{
				_args = args;
				
				_argHandler = new();
				_argHandler.EnsureDefaults();
				_argHandler.ParseArgs(args);
			}
			
			/// <summary>Creates an instance of the Crash WebApplication</summary>
			internal WebApplication? CreateApplication()
			{
				if (_argHandler.Exit)
				{
					return null;
				}

				if (_argHandler.ResetDB && File.Exists(_argHandler.DatabaseFileName))
				{
					File.Delete(_argHandler.DatabaseFileName);
				}

				var webBuilder = WebApplication.CreateBuilder(_args);
				var crashLogger = new CrashLoggerProvider();
				webBuilder.Logging.AddProvider(crashLogger);

				// Do we need this?
				webBuilder.WebHost.UseUrls(_argHandler.URL);
				webBuilder.Services.AddRazorPages();

				webBuilder.Services.AddDbContext<CrashContext>(options =>
					options.UseSqlite($"Data Source={_argHandler.DatabaseFileName}"));

				webBuilder.Services.AddSignalR()
					.AddHubOptions<CrashHub>(ConfigureCrashHubOptions)
					.AddJsonProtocol(ConfigureJsonOptions);

				_app = webBuilder.Build();

				if (_app.Environment.IsDevelopment())
				{
					_app.MapGet("/logging", () => string.Join("\n", crashLogger._logger.Messages));
					_app.MapGet("/config", () => _app.Configuration.AsEnumerable());
					_app.MapGet("/services", () =>
					{
						var crashHubOptionsService = _app.Services.GetService<IConfigureOptions<HubOptions<CrashHub>>>();
						var connectionHandler = _app.Services.GetService<HubConnectionHandler<CrashHub>>();
						;
					});
				}

				_app.UseHttpsRedirection();
				_app.UseStaticFiles();
				_app.UseRouting();
				_app.MapRazorPages();
				_app.MigrateDatabase<CrashContext>();
				_app.MapHub<CrashHub>("/Crash");
				
				
				return _app;
			}

			private void ConfigureJsonOptions(JsonHubProtocolOptions jsonOptions)
			{
				var crashConfig = _app.Configuration.GetRequiredSection("Crash");
				var signalRConfig = crashConfig.GetRequiredSection("SignalR");
				var jsonConfig = signalRConfig.GetRequiredSection("Json").Get<JsonSerializerOptions>();
				if (jsonConfig is null)
					return;
				
				jsonOptions.PayloadSerializerOptions = jsonConfig;
			}

			private void ConfigureCrashHubOptions(HubOptions<CrashHub> hubOptions)
			{
				var crashConfig = _app.Configuration.GetRequiredSection("Crash");
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

		public static void Main(string[] args)
		{
			var serverCreator = new CrashServerCreator(args);
			var app = serverCreator.CreateApplication();
			app?.Run();
		}
	}
}
