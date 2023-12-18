namespace Crash.Server.Tests
{
	public sealed class ProgramTests
	{
		[Test]
		public async Task CreateWebApplication()
		{
			Program.CrashServerCreator server = new("--urls", "http://0.0.0.0:8080");
			var app = server.CreateApplication();
		}
	}
}
