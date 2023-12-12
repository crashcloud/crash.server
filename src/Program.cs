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
			var configuration = webBuilder.Configuration;

			
			webBuilder.Services.AddSignalR()
				.AddHubOptions<CrashHub>(hubOptions =>
				{
					hubOptions.MaximumReceiveMessageSize = 65535L;
					return;
					
					var crashHubConfig = configuration.GetSection("CrashHub");
					
					hubOptions.MaximumParallelInvocationsPerClient = crashHubConfig.GetValue<int>(nameof(HubOptions.MaximumParallelInvocationsPerClient));
					hubOptions.MaximumReceiveMessageSize = crashHubConfig.GetValue<long?>(nameof(HubOptions.MaximumReceiveMessageSize));
					hubOptions.StreamBufferCapacity = crashHubConfig.GetValue<int?>(nameof(HubOptions.StreamBufferCapacity));
					hubOptions.KeepAliveInterval = crashHubConfig.GetValue<TimeSpan?>(nameof(HubOptions.KeepAliveInterval));
					hubOptions.HandshakeTimeout = crashHubConfig.GetValue<TimeSpan?>(nameof(HubOptions.HandshakeTimeout));
					hubOptions.EnableDetailedErrors = crashHubConfig.GetValue<bool?>(nameof(HubOptions.EnableDetailedErrors));
				})
				.AddJsonProtocol(jsonOptions =>
				{
					var jsonConfig = configuration.GetSection("Json");
					
					var jOptions = jsonOptions.PayloadSerializerOptions;
					jOptions.AllowTrailingCommas = jsonConfig.GetValue<bool>(nameof(JsonSerializerOptions.AllowTrailingCommas));
					jOptions.DefaultIgnoreCondition = jsonConfig.GetValue<JsonIgnoreCondition>(nameof(JsonSerializerOptions.DefaultIgnoreCondition));
					jOptions.IgnoreReadOnlyFields = jsonConfig.GetValue<bool>(nameof(JsonSerializerOptions.IgnoreReadOnlyFields));
					jOptions.IgnoreReadOnlyProperties = jsonConfig.GetValue<bool>(nameof(JsonSerializerOptions.IgnoreReadOnlyProperties));
					jOptions.IncludeFields = jsonConfig.GetValue<bool>(nameof(JsonSerializerOptions.IncludeFields));
					jOptions.MaxDepth = jsonConfig.GetValue<int>(nameof(JsonSerializerOptions.MaxDepth));
					jOptions.PropertyNameCaseInsensitive = jsonConfig.GetValue<bool>(nameof(JsonSerializerOptions.PropertyNameCaseInsensitive));
					jOptions.WriteIndented = jsonConfig.GetValue<bool>(nameof(JsonSerializerOptions.WriteIndented));
					jOptions.ReadCommentHandling = jsonConfig.GetValue<JsonCommentHandling>(nameof(JsonSerializerOptions.ReadCommentHandling));
					jOptions.NumberHandling = jsonConfig.GetValue<JsonNumberHandling>(nameof(JsonSerializerOptions.NumberHandling));
				});

			webBuilder.Services.AddDbContext<CrashContext>(options =>
				options.UseSqlite($"Data Source={argHandler.DatabaseFileName}"));

			// Do we need this?
			webBuilder.WebHost.UseUrls(argHandler.URL);
			webBuilder.Services.AddRazorPages();

			var app = webBuilder.Build();

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
			var app = CreateApplication(args);
			app.Run();
		}
	}
}
