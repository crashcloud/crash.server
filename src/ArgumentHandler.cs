﻿using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics;

namespace Crash.Server
{
	/// <summary>Handles Arguments for the start up program</summary>
	public struct Arguments
	{

		#region Consts & Defaults

		private const string Pattern = @"--([\w]+ [\S]*)";
		internal const string AppName = "Crash";
		internal const string DbDirectory = "App_Data";
		internal const string DefaultURL = "http://0.0.0.0:8080";
		private static readonly Version? Vers = typeof(Arguments).Assembly.GetName().Version;
		internal static string DbName => $"Database_{Vers?.Major}_{Vers?.Minor}_{Vers?.Build}.db";

		#endregion

		#region Properties

		/// <summary>The Server URL</summary>
		public string URL { get; set; } = DefaultURL;

		/// <summary>The file name for the Database</summary>
		public string DatabaseFileName { get; set; } = DbName;

		/// <summary>Resets the Database</summary>
		public bool ResetDB { get; set; } = false;

		/// <summary>Instructs the program to exit</summary>
		public bool Exit { get; set; } = false;

		/// <summary>Current Logging Level for the server</summary>
		public LogLevel LoggingLevel { get; private set; }

		public string[] Args { get; private set; } = Array.Empty<string>();

		public Arguments() { }

		#endregion


		public static async Task<Arguments> ParseArgs(string[] args)
		{
			var validatedArgs = new Arguments() { Args = args };

			var uriOption = new Option<Uri?>(
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

					if (value.Scheme != "http" && value.Scheme != "https")
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

			var pathOption = new Option<FileInfo?>(
				name: "--path",
				description: "Supply a custom Path for the Database");
			pathOption.AddAlias("-p");
			pathOption.SetDefaultValue(Arguments.DbName);

			var resetOption = new Option<bool>(
				name: "--reset",
				description: "Empty the current Database. This is a DESTRUCTIVE operation that CANNOT be undone."
			).FromAmong("true", "false", "True", "False");
			resetOption.AddAlias("-r");
			resetOption.SetDefaultValue(false);

			var environmentOptions = new Option<string>(
				name: "--environment",
				description: "Set the environment for the server to run in. Default is Development."
			).FromAmong("Development", "Production", "Staging", "Testing");
			environmentOptions.AddAlias("-e");
			// TODO : Best default values?
			environmentOptions.SetDefaultValue(Debugger.IsAttached ? "Development" : "Testing");

			var appSettingsOptions = new Option<FileInfo>(
				name: "--appsettings",
				description: "Supply a custom appsettings.json file for the server to use."
			);
			appSettingsOptions.AddAlias("-a");

			var loggingLevelOptions = new Option<LogLevel>(
				name: "--loglevel",
				description: "Set the logging level for the server to use. Default is Information."
			).FromAmong(Enum.GetNames<LogLevel>());
			loggingLevelOptions.AddAlias("-l");
			loggingLevelOptions.SetDefaultValue(LogLevel.Information);

			var versionOption = new Option<bool>(
				name: "--version",
				description: "Display the current version of the server."
			);

			var rootCommand = new RootCommand("Crash.Server - A multi-user communication server designed to work alongside Crash.");

			rootCommand.AddOption(uriOption);
			rootCommand.AddOption(pathOption);
			rootCommand.AddOption(resetOption);
			rootCommand.AddOption(environmentOptions);
			rootCommand.AddOption(appSettingsOptions);
			rootCommand.AddOption(loggingLevelOptions);
			rootCommand.AddOption(versionOption);

			rootCommand.SetHandler((uri, path, reset, environment, appSettings, logLevel, showVersion) =>
				{
					validatedArgs.URL = uri?.ToString() ?? DefaultURL;

					// TODO : Validate : Must be a file - Does FileInfo Validate?
					validatedArgs.DatabaseFileName = path?.FullName ?? validatedArgs.DatabaseFileName;
					validatedArgs.ResetDB = reset;
					validatedArgs.LoggingLevel = logLevel;

					if (showVersion)
					{
						// TODO : Suffix
						var name = typeof(Program).Assembly.GetName();
						var version = name.Version;
						var build = "4debf41"; // TODO : Embedd

						Console.WriteLine($"\n{name.Name} version {version}, build {build}\n");
						validatedArgs.Exit = true;
					}
				}, uriOption, pathOption, resetOption, environmentOptions, appSettingsOptions, loggingLevelOptions, versionOption);


			var commandLineBuilder = new CommandLineBuilder(rootCommand)
				   // .UseVersionOption() // Use Custom Option
				   .UseHelp()
				   .UseEnvironmentVariableDirective()
				   .UseParseDirective()
				   .UseSuggestDirective()
				   .RegisterWithDotnetSuggest()
				   .UseTypoCorrections()
				   .UseParseErrorReporting()
				   .UseExceptionHandler()
				   .CancelOnProcessTermination();

			var parser = commandLineBuilder.Build();

			var result = await parser.InvokeAsync(args);
			if (result != 0) return new() { Exit = true }; ;

			var flatArgs = string.Join(", ", args ?? Array.Empty<string>())?.ToLowerInvariant();
			bool helpRun = flatArgs?.Contains("help") ?? false;
			if (helpRun)
			{
				return new() { Exit = true };
			}
			return validatedArgs;
		}

		private void RegenerateDatabase(bool value)
		{
			if (!value)
			{
				return;
			}

			if (File.Exists(DatabaseFileName))
			{
				File.Delete(DatabaseFileName);
			}
		}

	}
}
