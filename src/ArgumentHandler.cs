using System.Globalization;
using System.Text.RegularExpressions;

namespace Crash.Server
{
	/// <summary>Handles Arguments for the start up program</summary>
	public sealed class ArgumentHandler
	{
		private const string pattern = @"--([\w]+ [\S]*)";
		internal const string appName = "Crash";
		internal const string dbDirectory = "App_Data";
		internal const string defaultURL = "http://0.0.0.0:5000";
		private static readonly Version? vers = typeof(ArgumentHandler).Assembly.GetName().Version;
		internal static string dbName = $"Database_{vers?.Major}_{vers?.Minor}_{vers?.Build}.db";

		private readonly List<Command> _commands;

		// TODO : Add logging level
		public ArgumentHandler()
		{
			_commands = new List<Command>
			{
				new("urls", HandleUrlArgs, "Supply a custom URL for the serer", "\"http://0.0.0.0:5000\""),
				new("path", _handleDatabasePath, "Supply a custom Path for the Database", "C:\\Crash\\data.db"),
				new("reset", HandleRegenDb, "Empty the current Database", "true"),
				new("help", HandleHelpRequest, "Help! You're here now.")
			};
		}

		/// <summary>The Server URL</summary>
		public string URL { get; private set; }

		/// <summary>The file name for the Database</summary>
		public string DatabaseFileName { get; private set; }

		/// <summary>Resets the Database</summary>
		public bool ResetDB { get; private set; }

		/// <summary>Instructs the program to exit</summary>
		public bool Exit { get; private set; }

		/// <summary>Parses the input Arguments</summary>
		public void ParseArgs(string[] args)
		{
			var flatArgs = string.Join(' ', args)?.ToLower();
			if (flatArgs.Contains("help"))
			{
				HandleHelpRequest(flatArgs);
			}

			var argMatches = Regex.Matches(flatArgs, pattern, RegexOptions.IgnoreCase);

			foreach (Match argMatch in argMatches)
			{
				var group = argMatch?.Groups.Values?.LastOrDefault();
				if (null == group)
				{
					continue;
				}

				var argSplit = group.Value.Split(' ', 2);

				if (null == argSplit || argSplit.Length == 0)
				{
					continue;
				}

				var argPreposition = argSplit[0];
				var argValue = string.Empty;

				if (argSplit?.Length > 0)
				{
					argValue = argSplit[1];
				}

				HandleArgs(argPreposition, argValue);
			}
		}

		/// <summary>Ensures defaults are set incase of no Arguments</summary>
		public void EnsureDefaults()
		{
			if (string.IsNullOrEmpty(URL))
			{
				SetUrl(defaultURL);
			}

			if (string.IsNullOrEmpty(DatabaseFileName))
			{
				var databasePath = GetDefaultDatabaseDirectory();
				_handleDatabasePath(databasePath);
			}
		}

		private void HandleArgs(string argPreposition, string argValue)
		{
			foreach (var command in _commands)
			{
				if (command.Name != argPreposition.ToLower(CultureInfo.InvariantCulture))
				{
					continue;
				}

				try
				{
					command.Action.Invoke(argValue);
				}
				catch
				{
					Console.WriteLine(
						$"Command {command.Name} threw an error, your args might be invalid. Check --help");
				}

				return;
			}

			Console.WriteLine($"Invalid argument {argPreposition} and value {argValue}");
		}

		#region Help

		private void HandleHelpRequest(string helpArg)
		{
			Console.WriteLine("\nusage: crash.server <command> [<args>...]\n");
			Console.WriteLine("Examples:");
			foreach (var command in _commands.GetRange(0, 3))
			{
				Console.WriteLine($"    crash.server --{command.Name} {command.Example}");
			}

			Console.WriteLine("\nThe available crash.server commands are:");

			var extraSpace = 8;
			var maxLength = _commands.Max(c => c.Name.Length);
			var overallLength = extraSpace + maxLength;
			foreach (var command in _commands)
			{
				var spacing = overallLength - command.Name.Length;
				var spaces = string.Join("", Enumerable.Range(0, spacing).Select(r => " "));
				Console.WriteLine($"    {command.Name}{spaces}{command.Description}");
			}

			Console.WriteLine("\nSee 'crash.server --help <command>' to read about a specific subcommand.\n");

			Exit = true;
		}

		#endregion

		internal sealed class Command
		{
			internal readonly Action<string> Action;
			internal readonly string Description;
			internal readonly string Example;
			internal readonly string Name;

			internal Command(string name, Action<string> action, string description, string example = "")
			{
				Name = name;
				Action = action;
				Description = description;
				Example = example;
			}
		}

		#region URL Args

		private void HandleUrlArgs(string urlValue)
		{
			ValidateUrlInput(ref urlValue);
			if (!ValidateUrlInput(ref urlValue))
			{
				throw new ArgumentException("Given URL was Invalid.");
			}

			SetUrl(urlValue);
		}

		private void SetUrl(string urlValue)
		{
			URL = urlValue;
		}

		private static bool ValidateUrlInput(ref string url)
		{
			UriBuilder uriBuild;
			try
			{
				uriBuild = new UriBuilder(url);
				if (uriBuild.Uri.HostNameType is UriHostNameType.IPv4 or UriHostNameType.IPv6)
				{
					return ValidateIPAddress(url);
				}

				if (uriBuild.Uri.HostNameType is UriHostNameType.Dns)
				{
					return ValidateUrl(url);
				}

				throw new ArgumentException("Invalid URL. Was not detectable as either an IP or URL");
			}
			catch (UriFormatException)
			{
				throw new ArgumentException("Invalid URL");
			}
		}

		private static bool ValidateUrl(string url)
		{
			// No logic required for now.
			return true;
		}

		private static bool ValidateIPAddress(string url)
		{
			var uriBuild = new UriBuilder(url);

			if (!uriBuild.Uri.IsDefaultPort)
			{
				return true;
			}

			var portString = uriBuild.Port.ToString(CultureInfo.InvariantCulture);
			if (url.Replace("/", "").EndsWith(portString, StringComparison.InvariantCulture))
			{
				return true;
			}

			var uriii = uriBuild.ToString();

			// IP is IPv4/IPv6 - Website is DNS
			if (uriBuild.Uri.HostNameType is UriHostNameType.IPv4 or UriHostNameType.IPv6)
			{
				throw new ArgumentException("Port is required for IP Address");
			}

			return true;
		}

		#endregion

		#region Database Args

		private void _handleDatabasePath(string givenPath)
		{
			ValidateDatabaseDirectory(givenPath);
			EnsureDatabaseDirectoryExists(givenPath);
			SetDatabaseFilePath(givenPath);
		}

		private static void ValidateDatabaseDirectory(string givenPath)
		{
			if (Path.GetInvalidPathChars().Where(c => givenPath.Contains(c)).Any())
			{
				throw new InvalidDataException("Invalid Characters in given path!");
			}
		}

		private static string GetDefaultDatabaseDirectory()
		{
			var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var databaseDirectory = Path.Combine(appData, appName, dbDirectory);

			return databaseDirectory;
		}

		private void SetDatabaseFilePath(string databasePath)
		{
			if (!Path.HasExtension(databasePath))
			{
				DatabaseFileName = Path.Combine(databasePath, dbName);
			}
			else
			{
				DatabaseFileName = databasePath;
			}
		}

		private static void EnsureDatabaseDirectoryExists(string databaseFilePath)
		{
			var directoryName = GetDirectoryOfPath(databaseFilePath);
			try
			{
				Directory.CreateDirectory(directoryName);
			}
			catch (Exception ex)
			{
				throw new ArgumentNullException($"Could not create directory {directoryName} for database file", ex);
			}
		}

		/// <summary>Checks a path for being a Directory or File</summary>
		/// <returns>True on file, false on Directory</returns>
		private static bool IsFile(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException($"Input {path} cannot be null for IsFile check");
			}

			return Path.HasExtension(path);
		}

		/// <summary>
		///     If given a directory, returns the same string
		///     If given a file, returns the current directory of the file
		/// </summary>
		private static string? GetDirectoryOfPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException($"Input {nameof(path)} : {path} " +
				                                $"cannot be null for GetDirectoryOfPath");
			}

			var fullDirectoryName = path;
			if (IsFile(path))
			{
				fullDirectoryName = Path.GetDirectoryName(path);
			}

			return fullDirectoryName;
		}

		private void HandleRegenDb(string toggleArgs)
		{
			var value = GetRegenerateDatabaseValue(toggleArgs);
			SetRegenenerateDatabaseValue(value);
			RegenerateDatabase(value);
		}

		private static bool GetRegenerateDatabaseValue(string toggleArgs)
		{
			if (!bool.TryParse(toggleArgs, out var result))
			{
				throw new ArgumentException($"Invalid Argument {toggleArgs}, could not parse into a boolean.");
			}

			return result;
		}

		private void SetRegenenerateDatabaseValue(bool value)
		{
			ResetDB = value;
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

		#endregion
	}
}
