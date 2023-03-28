using System.Text.RegularExpressions;

namespace Crash.Server
{

    /// <summary>Handles Arguments for the start up program</summary>
    public sealed class ArgumentHandler
    {

        const string pattern = @"--([\w]+ [\S]+)";
        private static Version? vers = typeof(ArgumentHandler).Assembly.GetName().Version;
        internal static string dbName = $"Database_{vers.Major}_{vers.Minor}_{vers.Build}.db";
        internal const string appName = "Crash";
        internal const string dbDirectory = "App_Data";
        internal const string defaultURL = "http://0.0.0.0:8080";

        public string URL { get; private set; }

        public string DatabaseFileName { get; private set; }

        public bool FreshDb { get; private set; }

        private readonly Dictionary<string, Action<string>> argDict;

        // TODO : Add logging level
        public ArgumentHandler()
        {
            argDict = new Dictionary<string, Action<string>>
            {
                { "URLS", HandleUrlArgs },
                { "PATH", _handleDatabasePath },
                { "RESET", HandleRegenDb },
                { "HELP", HandleHelpRequest }
            };
        }

        public void ParseArgs(string[] args)
        {
            string flatArgs = string.Join(' ', args);
            MatchCollection argMatches = Regex.Matches(flatArgs, pattern, RegexOptions.IgnoreCase);

            foreach (Match argMatch in argMatches)
            {
                Group? group = argMatch?.Groups.Values?.LastOrDefault();
                if (null == group) continue;

                string[] argSplit = group.Value.Split(' ', 2);

                if (argSplit?.Count() == 0) continue;

                string argPreposition = argSplit[0];
                string argValue = string.Empty;

                if (argSplit?.Length > 0)
                    argValue = argSplit[1];

                HandleArgs(argPreposition, argValue);
            }
        }

        public void EnsureDefaults()
        {
            if (string.IsNullOrEmpty(URL))
            {
                SetUrl(defaultURL);
            }

            if (string.IsNullOrEmpty(DatabaseFileName))
            {
                string databasePath = GetDefaultDatabaseDirectory();
                _handleDatabasePath(databasePath);
            }
        }

        private void HandleArgs(string argPreposition, string argValue)
        {
            if (argDict.TryGetValue(argPreposition.ToUpper(), out var @action))
            {
                try
                {
                    action.Invoke(argValue);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine($"Invalid argument {argPreposition} and value {argValue}");
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
            else
            {
                SetUrl(urlValue);
            }
        }

        private void SetUrl(string urlValue)
        {
            URL = urlValue;
        }

        private bool ValidateUrlInput(ref string url)
        {
            UriBuilder uriBuild;
            try
            {
                uriBuild = new UriBuilder(url);
                if (uriBuild.Uri.HostNameType is UriHostNameType.IPv4 or UriHostNameType.IPv6)
                {
                    return ValidateIPAddress(url);
                }
                else if (uriBuild.Uri.HostNameType is UriHostNameType.Dns)
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

        private bool ValidateUrl(string url)
        {
            // No logic required for now.
            return true;
        }

        private bool ValidateIPAddress(string url)
        {
            UriBuilder uriBuild = new UriBuilder(url);

            if (!uriBuild.Uri.IsDefaultPort) return true;
            if (url.Replace("/", "").EndsWith(uriBuild.Port.ToString())) return true;

            string uriii = uriBuild.ToString();

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

        private void ValidateDatabaseDirectory(string givenPath)
        {
            if (Path.GetInvalidPathChars().Where(c => givenPath.Contains(c)).Any())
            {
                throw new Exception("Invalid Characters in given path!");
            }
        }

        private string GetDefaultDatabaseDirectory()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string databaseDirectory = Path.Combine(appData, appName, dbDirectory);

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

        private void EnsureDatabaseDirectoryExists(string databaseFilePath)
        {
            string directoryName = GetDirectoryOfPath(databaseFilePath);
            Directory.CreateDirectory(directoryName);
        }

        /// <summary>Checks a path for being a Directory or File</summary>
        /// <returns>True on file, false on Directory</returns>
        private bool IsFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("Input path cannot be null for IsFile check");

            return Path.HasExtension(path);
        }

        /// <summary>If given a directory, returns the same string
        /// If given a file, returns the current directory of the file</summary>
        private string GetDirectoryOfPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("Input path cannot be null for GetDirectoryOfPath");

            string fullDirectoryName = path;
            if (IsFile(path))
            {
                fullDirectoryName = Path.GetDirectoryName(path);
            }

            return fullDirectoryName;
        }

        private void HandleRegenDb(string toggleArgs)
        {
            bool value = GetRegenerateDatabaseValue(toggleArgs);
            SetRegenenerateDatabaseValue(value);
            RegenerateDatabase(value);
        }

        private bool GetRegenerateDatabaseValue(string toggleArgs)
        {
            if (!bool.TryParse(toggleArgs, out bool result))
            {
                throw new ArgumentException($"Invalid Argument {toggleArgs}, could not parse into a boolean.");
            }

            return result;
        }

        private void SetRegenenerateDatabaseValue(bool value)
        {
            FreshDb = value;
        }

        private void RegenerateDatabase(bool value)
        {
            if (!value) return;

            if (File.Exists(DatabaseFileName))
            {
                File.Delete(DatabaseFileName);
            }
        }

        #endregion

        #region Help
        private void HandleHelpRequest(string helpArg)
        {
            // ... print help
            string supportedMessage = $"There are 3 supported commands : {string.Join(", ", argDict)} ";
            Console.WriteLine(supportedMessage);

            string syntaxMessages = "Commands must be prefixed with --";
            Console.WriteLine(syntaxMessages);

        }

        #endregion

    }

}
