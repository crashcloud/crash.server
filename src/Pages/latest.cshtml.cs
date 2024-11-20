using Crash.Server.Model;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Crash.Server.Pages
{
	public class Latest : DebugBase
	{
		public Latest(IWebHostEnvironment env, CrashContext context, ILoggerProvider provider) : base(env, context, provider)
		{
		}
	}
}
