using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Crash.Server.Model
{
	/// <summary>A User inside the Database. Other implementations will exist elsewhere.</summary>
	public sealed class User
	{
		/// <summary>The Id of the User</summary>
		public string Id { get; set; }

		/// <summary>The Name of the User</summary>
		[Key]
		public string Name { get; set; }

		/// <summary>The Users </summary>
		public string Follows { get; set; }

		/// <summary>Creates a User given a change</summary>
		/// <param name="change"></param>
		/// <returns></returns>
		public static User? FromChange(Change change)
		{
			if (string.IsNullOrEmpty(change?.Payload))
			{
				return null;
			}

			return JsonSerializer.Deserialize<User>(change.Payload);
		}
	}
}
