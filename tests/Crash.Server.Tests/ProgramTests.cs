using System.CommandLine;

namespace Crash.Server.Tests
{
	public sealed class ProgramTests
	{
		[Test]
		public async Task CreateWebApplication()
		{
			var args = await Arguments.ParseArgs(["--urls", "http://0.0.0.0:8080"]);
			Program.CrashServerCreator server = new(args);
			Assert.That(server.TryCreateApplication(args.Args, out var webApplication));
		}
	}
}
