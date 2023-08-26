using System.Text.Json;
using System.Text.Json.Serialization;

using Crash.Server.Hubs;

using Microsoft.AspNetCore.SignalR;

namespace Crash.Server.Settings
{

	internal sealed class ConfigHandler
	{
		private readonly IConfiguration _configuration;

		internal CrashConfig Crash { get; set; }

		internal ConfigHandler(string jsonFile = "appsettings.json")
		{
			_configuration = new ConfigurationBuilder()
						.SetBasePath(Directory.GetCurrentDirectory())
						.AddJsonFile(jsonFile, optional: true, reloadOnChange: true)
						// .AddEnvironmentVariables() // Handle this
						.Build();

			Crash = new(_configuration.GetSection(nameof(Crash)));
		}
	}

	internal sealed class CrashConfig
	{
		internal readonly SignalRConfig SignalR;

		internal CrashConfig(IConfigurationSection section)
		{
			SignalR = new(section.GetSection(nameof(SignalR)));
		}
	}

	internal sealed class SignalRConfig
	{
		private readonly IConfiguration Section;

		internal SignalRConfig(IConfigurationSection section)
		{
			Section = section;
		}

		internal void BuildCrashHubConfig<THub>(HubOptions<THub> options) where THub : Microsoft.AspNetCore.SignalR.Hub
		{
			HubOptions<CrashHub> _default = new();
			var crashHubSection = Section.GetSection(nameof(CrashHub));

			options.MaximumParallelInvocationsPerClient = crashHubSection.GetValue<int>(nameof(HubOptions.MaximumParallelInvocationsPerClient), _default.MaximumParallelInvocationsPerClient);
			options.MaximumReceiveMessageSize = crashHubSection.GetValue<long?>(nameof(HubOptions.MaximumReceiveMessageSize), _default.MaximumReceiveMessageSize);
			options.StreamBufferCapacity = crashHubSection.GetValue<int?>(nameof(HubOptions.StreamBufferCapacity), _default.StreamBufferCapacity);
			options.KeepAliveInterval = TimeSpan.FromMilliseconds(crashHubSection.GetValue<int>(nameof(HubOptions.KeepAliveInterval), 10_000));
			options.HandshakeTimeout = TimeSpan.FromMilliseconds(crashHubSection.GetValue<int>(nameof(HubOptions.HandshakeTimeout), 10_000));
			options.EnableDetailedErrors = crashHubSection.GetValue<bool?>(nameof(HubOptions.EnableDetailedErrors), _default.EnableDetailedErrors);

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

			string name = nameof(JsonSerializerOptions.IgnoreReadOnlyFields);
			Console.WriteLine(name);

			options.PayloadSerializerOptions.DefaultIgnoreCondition = jsonSection.GetValue<JsonIgnoreCondition>(nameof(JsonSerializerOptions.DefaultIgnoreCondition), _default.DefaultIgnoreCondition);
			options.PayloadSerializerOptions.PropertyNameCaseInsensitive = jsonSection.GetValue<bool>(nameof(JsonSerializerOptions.PropertyNameCaseInsensitive), _default.PropertyNameCaseInsensitive);
			options.PayloadSerializerOptions.ReadCommentHandling = jsonSection.GetValue<JsonCommentHandling>(nameof(JsonSerializerOptions.ReadCommentHandling), _default.ReadCommentHandling);
			options.PayloadSerializerOptions.IgnoreReadOnlyProperties = jsonSection.GetValue<bool>(nameof(JsonSerializerOptions.IgnoreReadOnlyProperties), _default.IgnoreReadOnlyProperties);
			options.PayloadSerializerOptions.IgnoreReadOnlyFields = jsonSection.GetValue<bool>(nameof(JsonSerializerOptions.IgnoreReadOnlyFields), _default.IgnoreReadOnlyFields);
			options.PayloadSerializerOptions.AllowTrailingCommas = jsonSection.GetValue<bool>(nameof(JsonSerializerOptions.AllowTrailingCommas), _default.AllowTrailingCommas);
			options.PayloadSerializerOptions.NumberHandling = jsonSection.GetValue<JsonNumberHandling>(nameof(JsonSerializerOptions.NumberHandling), _default.NumberHandling);
			options.PayloadSerializerOptions.IncludeFields = jsonSection.GetValue<bool>(nameof(JsonSerializerOptions.IncludeFields), _default.IncludeFields);
			options.PayloadSerializerOptions.WriteIndented = jsonSection.GetValue<bool>(nameof(JsonSerializerOptions.WriteIndented), _default.WriteIndented);
			options.PayloadSerializerOptions.MaxDepth = jsonSection.GetValue<int>(nameof(JsonSerializerOptions.MaxDepth), _default.MaxDepth);
		}
	}
}
