using System;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;

namespace Crash.Server.Security;

internal static class CrashClaims
{

	public static AuthenticationTicket GetTicket()
		=> new(Roles.User, GetPropertes(), RhinoAuthenticationHandler.Name);

	private static AuthenticationProperties GetPropertes() => new()
	{
		AllowRefresh = true,
		IsPersistent = true,
	};

}
