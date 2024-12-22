using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Crash.Server.Security;

internal class RhinoAuthenticationHandler : IAuthenticationHandler
{
	private const string USER_INFO_URL = "https://accounts.rhino3d.com/oauth2/userinfo";
	public const string Name = "RhinoAuth";

	private string BearerToken { get; set; }

	public async Task<AuthenticateResult> AuthenticateAsync()
	{
		if (string.IsNullOrEmpty(BearerToken))
			return AuthenticateResult.Fail("No Bearer token found");

		var client = new HttpClient();
		using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, USER_INFO_URL))
		{
			requestMessage.Headers.Add("Authorization", $"Bearer {BearerToken}");
			// requestMessage.Headers.Authorization =
			// new AuthenticationHeaderValue(BearerToken);

			var result = await client.SendAsync(requestMessage);

			if (result.IsSuccessStatusCode)
			{
				var info = await result.Content.ReadFromJsonAsync<RhinoUserAccountInfo>();

				if (!info.EmailVerified)
					return AuthenticateResult.Fail("Email not verified");

				if (string.IsNullOrEmpty(info.Id))
					return AuthenticateResult.Fail("No user ID found");

				if (string.IsNullOrEmpty(info.Name))
					return AuthenticateResult.Fail("No user name found");

				if (string.IsNullOrEmpty(info.Email))
					return AuthenticateResult.Fail("No user email found");

				// TODO : Does not check user is allowed to access model! Add Checks!

				// NOTE : This returns A USER Role
				// TODO : Add more roles
				var success = AuthenticateResult.Success(CrashClaims.GetTicket());
				return success;
			}
		}

		return AuthenticateResult.Fail("Not Authorized!");
	}

	public Task ChallengeAsync(AuthenticationProperties? properties)
	{
		throw new NotImplementedException();
	}

	public Task ForbidAsync(AuthenticationProperties? properties)
	{
		properties ??= new AuthenticationProperties();

		if (properties.ExpiresUtc.HasValue && properties.ExpiresUtc.Value < DateTime.UtcNow)
			throw new HubException("Token is expired");

		return Task.CompletedTask;
	}

	public async Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
	{
		// Header Auth already included! Likely Desktop
		if (context.Request.Headers.TryGetValue("Authorization", out var bearerTokens))
		{
			var bearerToken = bearerTokens.FirstOrDefault()?.Replace("Bearer ", "");
			if (string.IsNullOrEmpty(bearerToken))
				throw new HubException("No Bearer token found");

			BearerToken = bearerToken;
		}
		// Header Token misssing
		// https://docs.google.com/document/d/1-U0FYt6iQAM3UA6Rio4z0sDVXBSdc0kQk5e4zumnKig/edit?tab=t.0
		// Use Authorization Flow
		else if (context.Request.Cookies.TryGetValue("Authorization", out var cookieToken))
		{
			// Cookies?
		}
		else
		{
			var bearerToken = await HandleBrowserAuth(scheme, context);
			if (string.IsNullOrEmpty(bearerToken))
				throw new HubException("No Authorization header found");

			BearerToken = bearerToken;
		}

		return;
	}

	private const string AUTH_URL = "https://accounts.rhino3d.com/oauth2/auth";
#pragma warning disable IDE0060 // Remove unused parameter
	private async Task<string> HandleBrowserAuth(AuthenticationScheme scheme, HttpContext context)
#pragma warning restore IDE0060 // Remove unusedxwx``z parameter
	{
		var id = Guid.NewGuid();
		HttpClient client = new HttpClient();


		Dictionary<string, string?> queryParams = new()
		{
			{ "Content-Type", "application/x-www-form-urlencoded" },
			{ "response_type", "code" },
			{ "client_id", "crash" },
			{ "client_secret", "LtWnkrbZ1pGDCV7sXyzfmYhBchgedRW" },
			{ "redirect_uri", "https://0.0.0.0:8080/" }, // <-- How to get endpoint?
			{ "scope", "openid profile email" },
			{ "state", id.ToString() },
			{ "prompt", "consent login" },
			{ "max_age", "3600" }, // Seconds -> 1 hour? // <-- Change this!,
			{ "nonce", id.ToString() },

			// // Using the Authorization Code flow:
			// { "access_token" , "" },
			// { "id_token" , "" },
			// { "expires_in" , "3000" }, // What units are this?
			// { "scope" , "profile" },
			// { "token_type" , "bearer" },
		};
		var url = QueryHelpers.AddQueryString(AUTH_URL, queryParams);
		using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, AUTH_URL))
		{
			requestMessage.Content = new StringContent(JsonSerializer.Serialize(queryParams));
			var responseMessage = client.SendAsync(requestMessage);
			var response = responseMessage.Result;
			var content = await response.Content.ReadAsStringAsync();
			;

		}

		return string.Empty;
	}

	public static void Options(AuthenticationOptions options)
	{
		options.DefaultAuthenticateScheme = RhinoAuthenticationHandler.Name;
		options.DefaultChallengeScheme = RhinoAuthenticationHandler.Name;
		options.DefaultSignInScheme = RhinoAuthenticationHandler.Name;
		options.RequireAuthenticatedSignIn = true;

		options.AddScheme<RhinoAuthenticationHandler>(RhinoAuthenticationHandler.Name, RhinoAuthenticationHandler.Name);
		options.AddScheme<AllowAnonymouseHandler>(Roles.AnonymousRoleName, Roles.AnonymousRoleName);
	}

}

// TODO : Likely not correctly implemented
internal class AllowAnonymouseHandler : IAuthenticationHandler
{
	public Task<AuthenticateResult> AuthenticateAsync()
		=> Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), Roles.AnonymousRoleName)));

	public Task ChallengeAsync(AuthenticationProperties? properties) => Task.CompletedTask;

	public Task ForbidAsync(AuthenticationProperties? properties) => Task.CompletedTask;

	public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context) => Task.CompletedTask;
}
