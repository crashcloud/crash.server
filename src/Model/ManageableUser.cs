using System.ComponentModel.DataAnnotations;
using System.Security;

namespace Crash.Server.Model;

public enum AccessStatus { Unknown, Allowed, Banned };


public class ManageableUser
{

	[Key]
	public string Title { get; set; }

	public string EmailPattern { get; set; }

	public AccessStatus Status { get; set; }

	// public PermissionSet TODO : <--
}
