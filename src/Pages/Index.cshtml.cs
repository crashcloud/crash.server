using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Crash.Server.Pages
{
	[AllowAnonymous]
	public class Index : PageModel
	{
		public void OnGet()
		{
		}
	}
}
