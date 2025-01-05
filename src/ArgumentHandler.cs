using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Reflection;

namespace Crash.Server
{
	/// <summary>Handles Arguments for the start up program</summary>
	public record Arguments
	{

		#region Consts & Defaults

		internal const string AppName = "Crash";
		internal const string DbDirectory = "Databases";
		internal const string DefaultURL = "http://0.0.0.0:8080";
		private static Version Vers { get; } = typeof(Arguments).Assembly.GetName().Version;
		internal static string DbName => $"{Vers?.Major}_{Vers?.Minor}_{Vers?.Build}.db";

		#endregion

		#region Properties

		/// <summary>The Server URL</summary>
		public string URL { get; set; } = DefaultURL;

		/// <summary>The file name for the Database</summary>
		public string DatabasePath { get; set; } = GetDefaultDatabasePath(DbName);

		/// <summary>Resets the Database</summary>
		public bool ResetDB { get; set; }

		/// <summary>Instructs the program to exit</summary>
		public bool Exit { get; set; }

		/// <summary>Current Logging Level for the server</summary>
		public LogLevel LoggingLevel { get; private set; }

		public string[] Args { get; private set; } = [];

		public string AdminUser { get; private set; } = string.Empty;

		public bool UseAuth { get; private set; } = true;

		public Arguments() { }

		#endregion
		private static string GetDefaultDatabasePath(string name)
		{
			// OSX : ~/Library/Application Support/Crash 
			var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var databaseDirectory = Path.Combine(appData, AppName, DbDirectory, name);

			return databaseDirectory;
		}

		public static async Task<Arguments> ParseArgs(string[] args)
		{
			args = args.Where(a => !string.IsNullOrEmpty(a)).ToArray();
			var validatedArgs = new Arguments() { Args = args };

			var uriOption = new Option<Uri>(
				name: "--urls",
				description: "Supply a custom URL for the server"
			);
			uriOption.AddAlias("-u");
			uriOption.SetDefaultValue(Arguments.DefaultURL);
			uriOption.AddValidator(result =>
			{
				try
				{
					var value = result.GetValueForOption(uriOption);

					if (value is null) return;

					if (value.IsFile)
					{
						result.ErrorMessage = "Given URL was a file path. Please provide a valid URL.";
						return;
					}

					if (value.Scheme is not "http" and not "https")
					{
						result.ErrorMessage = "Given URL was not a valid HTTP or HTTPS URL. Please provide a valid URL.";
						return;
					}

					result.ErrorMessage = null;
				}
				catch
				{
					// TODO : Switch on exceptions
					result.ErrorMessage = $"Given URL {result} was not a valid URL. Please provide a valid URL.";
					return;
				}
			});

			var pathOption = new Option<FileInfo>(
				name: "--path",
				description: "Supply a custom Path for the Database file");
			pathOption.AddAlias("-p");
			pathOption.SetDefaultValue(GetDefaultDatabasePath(Arguments.DbName));
			pathOption.AddValidator(result =>
			{
				var fileInfo = result.GetValueForOption(pathOption);
				if (fileInfo is null) return;
				if (fileInfo.Extension != ".db")
				{
					result.ErrorMessage = "Database filepath must end in \".db\"";
				}
			});

			var resetOption = new Option<bool>(
				name: "--reset",
				description: "Empty the current Database. This is a DESTRUCTIVE operation that CANNOT be undone."
			).FromAmong("true", "false", "True", "False");
			resetOption.AddAlias("-r");
			resetOption.SetDefaultValue(false);

			var environmentOptions = new Option<string>(
				name: "--environment",
				description: "Set the environment for the server to run in."
			).FromAmong("Development", "Production", "Staging", "Testing");
			environmentOptions.AddAlias("-e");
			// TODO : Best default values?
			environmentOptions.SetDefaultValue(Debugger.IsAttached ? "Development" : "Production");

			var appSettingsOptions = new Option<FileInfo>(
				name: "--appsettings",
				description: "Supply a custom appsettings.json file for the server to use."
			);
			appSettingsOptions.AddAlias("-a");

			var loggingLevelOptions = new Option<LogLevel>(
				name: "--loglevel",
				description: "Set the logging level for the server to use"
			).FromAmong(Enum.GetNames<LogLevel>());
			loggingLevelOptions.AddAlias("-l");
			loggingLevelOptions.SetDefaultValue(LogLevel.Information);

			var versionOption = new Option<bool>(
				name: "--version",
				description: "Display the current version of the server."
			);
			versionOption.AddAlias("-v");

			var adminOption = new Option<string>(
				name: "--admin",
				description: "Create a default administrator user for the server."
			);
			adminOption.AddAlias("-ad");
			adminOption.AddValidator(result =>
			{
				var value = result.GetValueForOption(adminOption);
				if (string.IsNullOrEmpty(value))
				{
					Console.WriteLine("No admin user provided. You will not be able to manage the server.");
					return;
				}
				if (!System.Net.Mail.MailAddress.TryCreate(value, out var mailAddress))
				{
					result.ErrorMessage = "Invalid email address provided. Please provide a valid email address.";
				}
			});

			var authOption = new Option<bool>(
				name: "--auth",
				description: "Enable authentication and authorization for the server. Default is true. False is ONLY recommended for development."
			);
			authOption.AddAlias("-au");
			authOption.SetDefaultValue(true);

			var description = "Crash.Server - A multi-user communication server designed to work alongside Crash.";
			description += "\n               Consult the docs at http://crsh.cloud";
			var rootCommand = new RootCommand(description);

			// Environment Options
			rootCommand.AddOption(uriOption);
			rootCommand.AddOption(pathOption);
			rootCommand.AddOption(resetOption);
			rootCommand.AddOption(environmentOptions);
			rootCommand.AddOption(appSettingsOptions);
			rootCommand.AddOption(loggingLevelOptions);

			// Authorization and Authentication Options
			rootCommand.AddOption(adminOption);
			rootCommand.AddOption(authOption);

			// Misc Options
			rootCommand.AddOption(versionOption);

			rootCommand.SetHandler((InvocationContext context) =>
			{
				// Environment Options
				validatedArgs.URL = context.ParseResult.FindResultFor(uriOption).GetValueOrDefault<Uri>()?.ToString() ?? Arguments.DefaultURL;

				var path = context.ParseResult.FindResultFor(pathOption).GetValueOrDefault<FileInfo>();
				if (path is not null)
				{
					var finalPath = path?.FullName ?? GetDefaultDatabasePath(DbName);
					if (string.IsNullOrEmpty(path?.DirectoryName))
					{
						finalPath = GetDefaultDatabasePath(path.FullName);
					}
					validatedArgs.DatabasePath = finalPath;
				}

				validatedArgs.ResetDB = context.ParseResult.FindResultFor(resetOption)?.GetValueOrDefault<bool>() ?? false;
				validatedArgs.LoggingLevel = context.ParseResult.FindResultFor(loggingLevelOptions)?.GetValueOrDefault<LogLevel>() ?? LogLevel.Information;

				// Authorization and Authentication Options
				validatedArgs.UseAuth = context.ParseResult.FindResultFor(authOption)?.GetValueOrDefault<bool>() ?? true;
				validatedArgs.AdminUser = context.ParseResult.FindResultFor(adminOption)?.GetValueOrDefault<string>() ?? string.Empty;

				// Misc Options
				bool showVersion = context.ParseResult.FindResultFor(versionOption)?.GetValueOrDefault<bool>() ?? false;
				if (showVersion && TryGetVersionInfo(out string name, out string version, out string suffix, out string commit))
				{
					Console.WriteLine($"\n{name} version {version}{suffix}, build {commit}\n");

					validatedArgs.Exit = true;
				}
			});


			var commandLineBuilder = new CommandLineBuilder(rootCommand)
				   // .UseVersionOption() // Use Custom Option
				   .UseHelp()
				   .UseEnvironmentVariableDirective()
				   //    .UseParseDirective()
				   //    .UseSuggestDirective()
				   //    .RegisterWithDotnetSuggest()
				   .UseTypoCorrections()
				   .UseParseErrorReporting()
				   .UseExceptionHandler()
				   .CancelOnProcessTermination();

			var parser = commandLineBuilder.Build();

			var result = await parser.InvokeAsync(args);
			if (result != 0) return new() { Exit = true }; ;

			var flatArgs = string.Join(", ", args ?? [])?.ToLowerInvariant();
			bool helpRun = flatArgs?.Contains("help") ?? false;
			if (helpRun)
			{
				return new() { Exit = true };
			}
			return validatedArgs;
		}

		private static char[] Splitters { get; } = ['+', '-'];
		private static bool TryGetVersionInfo(out string name, out string version, out string suffix, out string commit)
		{
			// TODO : Suffix
			var assemblyName = typeof(Program).Assembly.GetName();
			var assemblyVersion = assemblyName.Version;

			var assembly = Assembly.GetExecutingAssembly();
			var commitInfo = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
			var customAttributeInfo = commitInfo.InformationalVersion.Split(Splitters);

			var commitHash = customAttributeInfo.LastOrDefault()[..7];
			var versionSuffix = string.Empty;
			if (customAttributeInfo.Length == 3 && !string.IsNullOrEmpty(customAttributeInfo[1]))
			{
				versionSuffix = $"-{customAttributeInfo[1]}";
			}

			name = assemblyName?.Name ?? "unknown";
			version = assemblyVersion?.ToString() ?? "x.x.x.x";
			commit = commitHash;
			suffix = versionSuffix;

			return true;
		}

	}
}
