// ReSharper disable HeapView.BoxingAllocation

namespace Crash.Server.Tests
{
	[TestFixture]
	public class ArgHandlerTests
	{
		[Test]
		public void EnsureDefaultUrLs()
		{
			Arguments arguments = new();

			Assert.Multiple(() =>
			{
				Assert.That(Uri.TryCreate(arguments.URL, UriKind.Absolute, out var result), Is.True,
					"Failed to create the URI");
				Assert.That(result.AbsoluteUri, Does.StartWith(arguments.URL), "URLs are not similiar enough");
			});
		}

		[Test]
		public void EnsureDefaultDbPath()
		{
			Arguments arguments = new();

			var directory = Path.GetDirectoryName(arguments.DatabasePath);
			Assert.Multiple(() =>
			{
				Assert.That(arguments.DatabasePath, Does.EndWith(".db"), "File does not end with .db");
				Assert.That(Directory.Exists(directory), Is.True, "Directory does not exist");
				Assert.That(arguments.DatabasePath, Has.Length.LessThan(255), "Filename is too long");
			});
		}

		[Test]
		public void EnsureDefaultNewDb_DefaultsToFalse()
		{
			Arguments arguments = new();


			Assert.That(arguments.ResetDB, Is.False);
		}

		[Test]
		public async Task EnsureDefaultNewDb()
		{
			Arguments arguments = await Arguments.ParseArgs(new[] { "--reset", "true" });

			Assert.That(arguments.ResetDB, Is.True);
		}

		[Test]
		public async Task EnsureHelp()
		{
			var arguments = await Arguments.ParseArgs(new[] { "--help" });

			Assert.That(arguments.Exit, Is.True);
		}

		[TestCaseSource(typeof(ArgHandlerData), nameof(ArgHandlerData.Invalid_URLArguments))]
		[TestCaseSource(typeof(ArgHandlerData), nameof(ArgHandlerData.Valid_URLArguments))]
		public async Task<bool> ParseUrlArgs(List<string> args)
		{
			Arguments arguments = await Arguments.ParseArgs([.. args]);

			Arguments defaultArgHandler = new();

			return arguments.URL != defaultArgHandler.URL;
		}

		[TestCaseSource(typeof(ArgHandlerData), nameof(ArgHandlerData.Invalid_DBPathArguments))]
		[TestCaseSource(typeof(ArgHandlerData), nameof(ArgHandlerData.Valid_DBPathArguments))]
		public async Task<bool> ParseDbArgs(List<string> args)
		{
			Arguments arguments = await Arguments.ParseArgs([.. args]);

			Arguments defaultArgHandler = new();

			return arguments.DatabasePath != defaultArgHandler.DatabasePath;
		}

		public sealed class ArgHandlerData
		{
			private const int portMin = 1000;
			private const int intPortMax = 9000;

			private const int maxIPNumber = 255;
			private const int ipNumberCount = 4;
			private const char separator = '.';
			private const char UrlPortSeparator = ':';

			private static readonly string[] validPrefixes = { "http://", "https://", "" };

			public static IEnumerable Invalid_URLArguments
			{
				get
				{
					yield return new TestCaseData(new List<string> { "--urls", null }).Returns(false);
					yield return new TestCaseData(new List<string> { "--urls", "htp:/ww.com" }).Returns(false);
					yield return new TestCaseData(new List<string> { "--urls", "192.145.1.1" }).Returns(false);
					yield return new TestCaseData(new List<string> { "--urls", "0.1.2" }).Returns(false);
					yield return new TestCaseData(new List<string> { "--urls", "error;;" }).Returns(false);
					yield return new TestCaseData(new List<string> { "urls", GetRandomValidFullURL() }).Returns(false);
					yield return new TestCaseData(new List<string> { "--rls", GetRandomValidFullURL() }).Returns(false);
					yield return new TestCaseData(new List<string> { GetRandomValidFullURL() }).Returns(false);
				}
			}

			public static IEnumerable Valid_URLArguments
			{
				get
				{
					// Trues

					for (var i = 0; i < 10; i++)
					{
						yield return new TestCaseData(new List<string> { "--urls", GetRandomValidFullURL() })
							.Returns(true);
					}

					for (var i = 0; i < 10; i++)
					{
						yield return new TestCaseData(new List<string> { "--urls", GetRandomValidFullIpAddress() })
							.Returns(true);
					}
				}
			}

			public static IEnumerable Valid_DBPathArguments
			{
				get
				{
					for (var i = 0; i < 5; i++)
					{
						yield return new TestCaseData(new List<string> { "--path", GetRandomValidDbFileName() })
							.Returns(true);
					}

					/* Relative paths not supported yet
					yield return new TestCaseData(new List<string> {
							"--path", @"\App_Data\fileName.db",
						}).Returns(true);
					*/

					/* Just Filenames don't work currently
					yield return new TestCaseData(new List<string> {
							"--path", "fileName.db",
						}).Returns(true);
					*/
				}
			}

			public static IEnumerable Invalid_DBPathArguments
			{
				get
				{
					yield return new TestCaseData(new List<string> { "--pth", GetRandomValidDbFileName() })
						.Returns(false);

					yield return new TestCaseData(new List<string> { "--pth", GetRandomValidDbFileName() })
						.Returns(false);

					yield return new TestCaseData(new List<string> { GetRandomValidDbFileName() }).Returns(false);

					yield return new TestCaseData(new List<string> { "--urls", GetRandomValidDbFileName() })
						.Returns(false);

					yield return new TestCaseData(new List<string> { "--urls", "error", GetRandomValidDbFileName() })
						.Returns(false);

					yield return new TestCaseData(new List<string> { "PATH", GetRandomValidDbFileName() })
						.Returns(false);
				}
			}

			private static string GetRandomValidDbFileName()
			{
				var fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
				var tempDirectory = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
				var fileNameWithDbExt = $"{fileName}.db";
				var path = Directory.GetCurrentDirectory();

				return Path.Combine(path, tempDirectory, fileNameWithDbExt);
			}

			private static string GetRandomValidFullURL()
			{
				var prefixIndex = TestContext.CurrentContext.Random.Next(0, validPrefixes.Length - 1);

				var prefix = validPrefixes[prefixIndex];
				var url = GetRandomValidURL();
				var port = GetRandomValidPort();

				return $"{prefix}{url}{UrlPortSeparator}{port}";
			}

			private static string GetRandomValidFullIpAddress()
			{
				var prefixIndex = TestContext.CurrentContext.Random.Next(0, validPrefixes.Length - 1);

				var prefix = validPrefixes[prefixIndex];
				var ipAddress = GetRandomValidIpAddress();
				var port = GetRandomValidPort();

				return $"{prefix}{ipAddress}{UrlPortSeparator}{port}";
			}

			private static string GetRandomValidPort()
			{
				return TestContext.CurrentContext.Random.Next(portMin, intPortMax).ToString();
			}

			private static string GetRandomValidURL()
			{
				char[] alphabet =
				{
					'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's',
					't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
				};
				string[] domainExtensions = { ".com", ".au", ".ca", ".co.uk", ".org" };
				var urlCount = TestContext.CurrentContext.Random.Next(5, 20);

				var url = string.Empty;
				for (var i = 0; i < urlCount; i++)
				{
					var alphaCharIndex = TestContext.CurrentContext.Random.Next(0, alphabet.Length);
					url += alphabet[alphaCharIndex];
				}

				return url;
			}

			private static string GetRandomValidIpAddress()
			{
				var ipAddress = "";
				for (var i = 0; i < ipNumberCount; i++)
				{
					var ipNum = TestContext.CurrentContext.Random.Next(0, maxIPNumber);
					ipAddress += ipNum.ToString();
					if (i < ipNumberCount - 1)
					{
						ipAddress += separator;
					}
				}

				return ipAddress;
			}
		}
	}
}
