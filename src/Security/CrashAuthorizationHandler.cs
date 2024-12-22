using Microsoft.AspNetCore.Authorization;

namespace Crash.Server.Security;

internal class CrashAuthorizationHandler
{

	public static void Options(AuthorizationOptions options)
	{
		options.InvokeHandlersAfterFailure = true;
		// options.FallbackPolicy // <- Is this required?

		options.AddPolicy(Policies.AdminPolicyName, (options) =>
		{
			options.RequireAuthenticatedUser();
			options.RequireRole(Roles.AdminRoleName);
			// options.RequireClaim(ClaimTypes.Role, Roles.AdminRole);
		});

		options.AddPolicy(Policies.EditPolicyName, (options) =>
		{
			options.RequireAuthenticatedUser();
			options.RequireRole(Roles.EditRoleName);
			// options.RequireClaim(ClaimTypes.Role, Roles.UserRole);
		});

		options.AddPolicy(Policies.ViewPolicyName, (options) =>
		{
			options.RequireAuthenticatedUser();
			options.RequireRole(Roles.ViewOnlyRoleName);
			// options.RequireClaim(ClaimTypes.Role, Roles.ViewOnlyRole);
		});

		options.AddPolicy(Policies.AnonymousPolicyName, (options) =>
		{
			options.RequireAuthenticatedUser();
			options.RequireRole(Roles.AnonymousRoleName);
			// options.RequireClaim(ClaimTypes.Role, Roles.AnonymousRole);
		});

		var userPolicy = options.GetPolicy(Roles.UserRoleName)!;
		options.DefaultPolicy = userPolicy;
	}

}
