using Crash.Server.Model;

namespace Crash.Server.Pages
{

	public class Logging : DebugBase
	{

		public Logging(IWebHostEnvironment env, CrashContext context, ILoggerProvider provider) : base(env, context, provider)
		{

		}

	}

}
