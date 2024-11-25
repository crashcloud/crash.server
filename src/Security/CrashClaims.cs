using System;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;

namespace Crash.Server.Security;

internal static class CrashClaims
{

	private const string ClaimScheme = "CrashUser";

	public static AuthenticationTicket GetTicket()
		=> new(GetPrincipal(), GetPropertes(), ClaimScheme);

	private static AuthenticationProperties GetPropertes() => new()
	{
		AllowRefresh = true,
		IsPersistent = true
	};

	private static ClaimsPrincipal GetPrincipal()
		=> new(GetIdentities());

	private static List<ClaimsIdentity> GetIdentities()
		=> [
			new ClaimsIdentity([
				new Claim(ClaimTypes.Name, ClaimScheme),
			]),
		];
}
