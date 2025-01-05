using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;

namespace Crash.Server.Security;

internal class Roles
{

	public const string Scheme = "CrashRoles";

	public const string AdminRoleName = "Admin";

	public const string UserRoleName = "User";

	public const string EditRoleName = "Edit";

	public const string ViewOnlyRoleName = "ViewOnly";

	public const string AnonymousRoleName = "Anonymous";

#pragma warning disable IDE0060 // Remove unused parameter
	public static void ConfigurePolicy(PolicySchemeOptions options)
#pragma warning restore IDE0060 // Remove unused parameter
	{
		;

	}

	public static ClaimsPrincipal Admin => new(new ClaimsIdentity(AdminClaims, Scheme));
	private static Claim AdminClaim { get; } = new Claim(ClaimTypes.Role, Roles.AdminRoleName);
	private static List<Claim> AdminClaims => [AdminClaim, UserClaim, EditClaim, ViewOnlyClaim, AnonymousClaim];


	public static ClaimsPrincipal User => new(new ClaimsIdentity(UserClaims, Scheme));
	private static Claim UserClaim { get; } = new Claim(ClaimTypes.Role, Roles.UserRoleName);
	private static List<Claim> UserClaims => [UserClaim, EditClaim, ViewOnlyClaim, AnonymousClaim];


	public static ClaimsPrincipal Edit => new(new ClaimsIdentity(EditClaims, Scheme));
	private static Claim EditClaim { get; } = new Claim(ClaimTypes.Role, Roles.EditRoleName);
	private static List<Claim> EditClaims => [EditClaim, ViewOnlyClaim, AnonymousClaim];


	public static ClaimsPrincipal ViewOnly => new(new ClaimsIdentity(ViewOnlyClaims, Scheme));
	private static Claim ViewOnlyClaim { get; } = new Claim(ClaimTypes.Role, Roles.ViewOnlyRoleName);
	private static List<Claim> ViewOnlyClaims => [ViewOnlyClaim, AnonymousClaim];


	public static ClaimsPrincipal Anonymous => new(new ClaimsIdentity(AnonymousClaims, Scheme));
	private static Claim AnonymousClaim { get; } = new Claim(ClaimTypes.Role, Roles.AnonymousRoleName);
	private static List<Claim> AnonymousClaims => [AnonymousClaim];

}
