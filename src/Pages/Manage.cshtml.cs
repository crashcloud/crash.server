using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Crash.Server.Pages
{

	// [Authorize()] // Admin!
	public class ManageUsersModel(IWebHostEnvironment env, CrashContext context) : PageModel
	{
		internal CrashContext Context { get; } = context;
		internal IWebHostEnvironment Env { get; } = env;


		public void OnGet()
		{
		}
	}

}
