using Crash.Server.Model;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Crash.Server.Pages
{
	public abstract class DebugBase : PageModel
	{
		internal CrashContext Context { get; }
		internal IWebHostEnvironment Env { get; }

		protected DebugBase(IWebHostEnvironment env, CrashContext context) // Injected from ASP.NET Core
		{
			Env = env;
			Context = context;
		}

		public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
		{
			// ASPNETCORE_ENVIRONMENT is not set to "Development" in Properties/launchSettings.json: return 404.
			if (!Env.IsDevelopment()) // using Microsoft.Extensions.Hosting;
			{
				context.Result = NotFound();
			}
		}
	}

	public class Debug : DebugBase
	{
		public Debug(IWebHostEnvironment env, CrashContext context) : base(env, context)
		{
		}
	}
}
