using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Crash.Server.Security;

// TODO : Likely not correctly implemented
internal class AllowAnonymouseHandler : IAuthenticationHandler
{
	public const string Name = "AnonAuth";

	public Task<AuthenticateResult> AuthenticateAsync()
	{
		var success = AuthenticateResult.Success(CrashClaims.GetAnonTicket());
		return Task.FromResult(success);
	}

	public Task ChallengeAsync(AuthenticationProperties properties)
	{
		return Task.CompletedTask;
	}

	public Task ForbidAsync(AuthenticationProperties properties)
	{
		return Task.CompletedTask;
	}

	public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
	{
		return Task.CompletedTask;
	}
}
