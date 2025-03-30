using Crash.Server.Security;

using Microsoft.AspNetCore.Authorization;

namespace Crash.Server.Pages
{

	[Authorize(Roles = Roles.EditRoleName)]
	public class Changes : DebugBase
	{
		public Changes(IWebHostEnvironment env, CrashContext context, ILoggerProvider provider) : base(env, context, provider)
		{
		}
	}
}
