using System;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;

namespace Crash.Server.Security;

internal static class CrashClaims
{

	public static AuthenticationTicket GetAnonTicket()
		=> new(Roles.User, GetPropertes(), AllowAnonymouseHandler.Name);

	public static AuthenticationTicket GetRhinoTicket()
		=> new(Roles.User, GetPropertes(), RhinoAuthenticationHandler.Name);

	private static AuthenticationProperties GetPropertes() => new()
	{
		AllowRefresh = true,
		IsPersistent = true,
	};

}
