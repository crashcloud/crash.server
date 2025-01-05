using Crash.Server.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Crash.Server.Pages
{

	[Authorize(Roles = Roles.AdminRoleName)]
	public class ManageUsersModel(IWebHostEnvironment env, CrashContext context) : PageModel
	{
		internal CrashContext Context { get; } = context;
		internal IWebHostEnvironment Env { get; } = env;


		public void OnGet()
		{
		}
	}

}
