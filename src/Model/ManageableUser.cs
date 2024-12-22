using System.ComponentModel.DataAnnotations;
using System.Security;

namespace Crash.Server.Model;


public class ManageableUser(string title, string id, string emailPattern, string role)
{

	[Key]
	public string Id { get; set; } = id;

	public string Title { get; set; } = title;

	public string EmailPattern { get; set; } = emailPattern;

	public string Role { get; set; } = role;
}
