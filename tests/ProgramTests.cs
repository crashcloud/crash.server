namespace Crash.Server.Tests
{
	public sealed class ProgramTests
	{
		[Test]
		public async Task CreateWebApplication()
		{
			var app = Program.CreateApplication("--urls", "http://0.0.0.0:8080");
		}
	}
}
