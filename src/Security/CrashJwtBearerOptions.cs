
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;

namespace Crash.Server.Security;

internal static class CrashJwtBearerOptions
{
	internal static void GetOptions(JwtBearerOptions options)
	{
		options.SaveToken = true;
		options.Validate(RhinoAuthenticationHandler.Name);
#if DEBUG
		options.RequireHttpsMetadata = false;
#else
		options.RequireHttpsMetadata = true;
#endif
		// TODO : Configure the Authority to the expected value for
		// the authentication provider. This ensures the token
		// is appropriately validated.
		// options.Authority = "Authority URL"; // TODO: Update URL

		// TODO : Fill out
		// options.TokenValidationParameters = new TokenValidationParameters
		// {
		// 	// ValidateIssuer = true,
		// 	// ValidateAudience = true,
		// 	ValidateLifetime = true,
		// 	// ValidateIssuerSigningKey = true,
		// 	SaveSigninToken = true,
		// 	ValidIssuer = "https://accounts.rhino3d.com",
		// 	// ValidAudience = // It contains the OAuth 2.0 id of the client who requested the token.
		// 	// IssuerSigningKey = new SymmetricSecurityKey(key)
		// };

		// We have to hook the OnMessageReceived event in order to
		// allow the JWT authentication handler to read the access
		// token from the query string when a WebSocket or 
		// Server-Sent Events request comes in.

		// Sending the access token in the query string is required when using WebSockets or ServerSentEvents
		// due to a limitation in Browser APIs. We restrict it to only calls to the
		// SignalR hub in this code.
		// See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
		// for more information about security considerations when using
		// the query string to transmit the access token.
		options.Events = new JwtBearerEvents
		{
			OnMessageReceived = context =>
			{
				context.Request.Headers.TryGetValue("Authorization", out var bearerTokens);

				var bearerToken = bearerTokens.FirstOrDefault()?.Replace("Bearer ", "");
				if (string.IsNullOrEmpty(bearerToken))
					throw new HubException("No Bearer token found");

				context.Token = bearerToken;

				return Task.CompletedTask;
			},
			OnChallenge = context =>
			{
				throw new HubException("Not Authorized!");
			},
			OnTokenValidated = context =>
			{
				var token = context.SecurityToken;
				return Task.CompletedTask;
			},
			OnAuthenticationFailed = context =>
			{
				return Task.CompletedTask;
			},

		};

	}
}
