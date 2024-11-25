using System;
using System.Text.Json.Serialization;

namespace Crash.Server.Security;

public readonly record struct RhinoUserAccountInfo
{

	[JsonPropertyName("sub")]
	public string Id { get; init; }
	public string Email { get; init; }

	[JsonPropertyName("com.rhino3d.accounts.emails")]
	public List<string> Emails { get; init; }

	[JsonPropertyName("email_verified")]
	public bool EmailVerified { get; init; }
	public string Name { get; init; }
	public string Locale { get; init; }
	public string Picture { get; init; }

	[JsonPropertyName("com.rhino3d.accounts.member_groups")]
	public List<RhinoUserAccountGroup> MemberGroups { get; init; }

	[JsonPropertyName("com.rhino3d.accounts.admin_groups")]
	public List<RhinoUserAccountGroup> AdminGroups { get; init; }

	[JsonPropertyName("com.rhino3d.accounts.owner_groups")]
	public List<RhinoUserAccountGroup> OwnerGroups { get; init; }

	public RhinoUserAccountInfo()
	{
		EmailVerified = false;
		Emails = [];
		MemberGroups = [];
		AdminGroups = [];
		OwnerGroups = [];
	}
}

public readonly record struct RhinoUserAccountGroup
{
	public string Id { get; init; }
	public string Name { get; init; }
	public string Domain { get; init; }

	[JsonPropertyName("domain_login_provider_name")]
	public string DomainLoginProviderName { get; init; }
}
