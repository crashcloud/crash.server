using System.Text.Json;

namespace Crash.Server.Model
{

	public sealed class User
	{

		public string Id { get; set; }

		public string Name { get; set; }
		public string Follows { get; set; }

		public static User? FromChange(Change change)
		{
			return JsonSerializer.Deserialize<User>(change.Payload);
		}
	}
}
