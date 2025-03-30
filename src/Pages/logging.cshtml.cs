using Crash.Server.Security;

using Microsoft.AspNetCore.Authorization;

namespace Crash.Server.Pages
{

	[Authorize(Roles = Roles.AdminRoleName)]
	public class Logging : DebugBase
	{

		public Logging(IWebHostEnvironment env, CrashContext context, ILoggerProvider provider) : base(env, context, provider)
		{

		}

	}

}
