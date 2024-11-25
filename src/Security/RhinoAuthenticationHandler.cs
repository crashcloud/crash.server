

using System.Net.Http.Headers;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.SignalR;

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
				;

				if (!info.EmailVerified)
					return AuthenticateResult.Fail("Email not verified");

				if (string.IsNullOrEmpty(info.Id))
					return AuthenticateResult.Fail("No user ID found");

				if (string.IsNullOrEmpty(info.Name))
					return AuthenticateResult.Fail("No user name found");

				if (string.IsNullOrEmpty(info.Email))
					return AuthenticateResult.Fail("No user email found");

				return AuthenticateResult.Success(CrashClaims.GetTicket());
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

	public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
	{
		if (!context.Request.Headers.TryGetValue("Authorization", out var bearerTokens))
			throw new HubException("No Authorization header found");

		var bearerToken = bearerTokens.FirstOrDefault()?.Replace("Bearer ", "");
		if (string.IsNullOrEmpty(bearerToken))
			throw new HubException("No Bearer token found");

		BearerToken = bearerToken;

		return Task.CompletedTask;
	}
}
