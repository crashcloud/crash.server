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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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

				webBuilder.Services.AddAuthentication((options) =>
				{
					options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				}).AddJwtBearer(options =>
					{
#if DEBUG
						options.RequireHttpsMetadata = false;
#endif
						// Configure the Authority to the expected value for
						// the authentication provider. This ensures the token
						// is appropriately validated.
						options.Authority = "Authority URL"; // TODO: Update URL

						// TODO : Fill out
						options.TokenValidationParameters = new TokenValidationParameters
						{
							ValidateIssuer = true,
							ValidateAudience = true,
							ValidateLifetime = true,
							ValidateIssuerSigningKey = true,
							ValidIssuer = "https://accounts.rhino3d.com"
							// ValidAudience = // It contains the OAuth 2.0 id of the client who requested the token.
							// IssuerSigningKey = new SymmetricSecurityKey(key)
						};

						// We have to hook the OnMessageReceived event in order to
						// allow the JWT authentication handler to read the access
						// token from the query string when a WebSocket or 
						// Server-Sent Events request comes in.

						// Sending the access token in the query string is required when using WebSockets or ServerSentEvents
						// due to a limitation in Browser APIs. We restrict it to only calls to the
						// SignalR hub in this code.
						// See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
						// for more information about security considerations when using
						// the query string to transmit the access token.
						options.Events = new JwtBearerEvents
						{
							OnMessageReceived = context =>
							{
								var bearerToken = context.Request.Headers["Authorization"];
								;

								// HttpClient client = new HttpClient();
								// client.PostAsync("https://accounts.rhino3d.com/oauth2/token?code={ACCESS_CODE}")

								// // If the request is for our hub...
								// var path = context.HttpContext.Request.Path;
								// if (!string.IsNullOrEmpty(accessToken) &&
								// 	(path.StartsWithSegments("/hubs/chat")))
								// {
								// 	// Read the token out of the query string
								// 	context.Token = accessToken;
								// }

								return Task.CompletedTask;
							},
							OnChallenge = context =>
							{
								return Task.CompletedTask;
							},
							OnTokenValidated = options =>
							{
								return Task.CompletedTask;
							},
							OnAuthenticationFailed = options =>
							{
								return Task.CompletedTask;
							},

						};
					});

				webBuilder.Services.AddAuthorization((options) =>
				{
					;
				});

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

				// App.UseHttpsRedirection();
				App.UseStaticFiles();
				// App.UseRouting();
				App.MapRazorPages();
				App.MigrateDatabase<CrashContext>();
				App.MapHub<CrashHub>("/Crash");
				App.UseAuthentication();
				App.UseAuthorization();

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

		private static async Task<int> Main(string[] args)
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
				else
				{
					Console.WriteLine(ex.Message);
				}

				Console.WriteLine("\n");
				return 1;
			}
		}

	}
}
