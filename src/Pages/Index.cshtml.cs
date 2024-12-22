using Crash.Server.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Crash.Server.Pages
{

	[AllowAnonymous, Authorize(Roles = Roles.AnonymousRoleName)]
	public class Index : PageModel
	{
		public void OnGet()
		{
		}

	}
}
