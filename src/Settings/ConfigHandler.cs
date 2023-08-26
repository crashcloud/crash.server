using System.Text.Json;

using Crash.Server.Hubs;

using Microsoft.AspNetCore.SignalR;

namespace Crash.Server.Settings
{
	internal sealed class ConfigHandler
	{
		private readonly IConfiguration _configuration;

		internal ConfigHandler(string jsonFile = "appsettings.json")
		{
			_configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile(jsonFile, true, true)
				// .AddEnvironmentVariables() // Handle this
				.Build();

			Crash = new CrashConfig(_configuration.GetSection(nameof(Crash)));
		}

		internal CrashConfig Crash { get; set; }
	}

	internal sealed class CrashConfig
	{
		internal readonly SignalRConfig SignalR;

		internal CrashConfig(IConfigurationSection section)
		{
			SignalR = new SignalRConfig(section.GetSection(nameof(SignalR)));
		}
	}

	internal sealed class SignalRConfig
	{
		private readonly IConfiguration Section;

		internal SignalRConfig(IConfigurationSection section)
		{
			Section = section;
		}

		internal void BuildCrashHubConfig<THub>(HubOptions<THub> options) where THub : Hub
		{
			HubOptions<CrashHub> _default = new();
			var crashHubSection = Section.GetSection(nameof(CrashHub));

			options.MaximumParallelInvocationsPerClient = crashHubSection.GetValue(
				nameof(HubOptions.MaximumParallelInvocationsPerClient), _default.MaximumParallelInvocationsPerClient);
			options.MaximumReceiveMessageSize = crashHubSection.GetValue(nameof(HubOptions.MaximumReceiveMessageSize),
				_default.MaximumReceiveMessageSize);
			options.StreamBufferCapacity =
				crashHubSection.GetValue(nameof(HubOptions.StreamBufferCapacity), _default.StreamBufferCapacity);
			options.KeepAliveInterval =
				TimeSpan.FromMilliseconds(crashHubSection.GetValue(nameof(HubOptions.KeepAliveInterval), 10_000));
			options.HandshakeTimeout =
				TimeSpan.FromMilliseconds(crashHubSection.GetValue(nameof(HubOptions.HandshakeTimeout), 10_000));
			options.EnableDetailedErrors =
				crashHubSection.GetValue(nameof(HubOptions.EnableDetailedErrors), _default.EnableDetailedErrors);

			if (options?.SupportedProtocols is not null)
			{
				foreach (var protocol in options.SupportedProtocols)
				{
					Console.WriteLine($"SinalR supports {protocol} protocol.");
				}
			}
		}

		internal void BuildJsonConfig(JsonHubProtocolOptions options)
		{
			JsonSerializerOptions _default = new();

			var jsonSection = Section.GetSection("Json");

			var name = nameof(JsonSerializerOptions.IgnoreReadOnlyFields);
			Console.WriteLine(name);

			options.PayloadSerializerOptions.DefaultIgnoreCondition =
				jsonSection.GetValue(nameof(JsonSerializerOptions.DefaultIgnoreCondition),
					_default.DefaultIgnoreCondition);
			options.PayloadSerializerOptions.PropertyNameCaseInsensitive = jsonSection.GetValue(
				nameof(JsonSerializerOptions.PropertyNameCaseInsensitive), _default.PropertyNameCaseInsensitive);
			options.PayloadSerializerOptions.ReadCommentHandling =
				jsonSection.GetValue(nameof(JsonSerializerOptions.ReadCommentHandling), _default.ReadCommentHandling);
			options.PayloadSerializerOptions.IgnoreReadOnlyProperties = jsonSection.GetValue(
				nameof(JsonSerializerOptions.IgnoreReadOnlyProperties), _default.IgnoreReadOnlyProperties);
			options.PayloadSerializerOptions.IgnoreReadOnlyFields =
				jsonSection.GetValue(nameof(JsonSerializerOptions.IgnoreReadOnlyFields), _default.IgnoreReadOnlyFields);
			options.PayloadSerializerOptions.AllowTrailingCommas =
				jsonSection.GetValue(nameof(JsonSerializerOptions.AllowTrailingCommas), _default.AllowTrailingCommas);
			options.PayloadSerializerOptions.NumberHandling =
				jsonSection.GetValue(nameof(JsonSerializerOptions.NumberHandling), _default.NumberHandling);
			options.PayloadSerializerOptions.IncludeFields =
				jsonSection.GetValue(nameof(JsonSerializerOptions.IncludeFields), _default.IncludeFields);
			options.PayloadSerializerOptions.WriteIndented =
				jsonSection.GetValue(nameof(JsonSerializerOptions.WriteIndented), _default.WriteIndented);
			options.PayloadSerializerOptions.MaxDepth =
				jsonSection.GetValue(nameof(JsonSerializerOptions.MaxDepth), _default.MaxDepth);
		}
	}
}
