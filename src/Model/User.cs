using System.Text.Json;
using System.Text.Json.Serialization;

namespace Crash.Server.Model
{

	public sealed class User
	{

		[JsonIgnore]
		internal string Id { get; set; }

		public string Name { get; set; }
		public string Follows { get; set; }

		public static User? FromChange(Change change)
		{
			return JsonSerializer.Deserialize<User>(change.Payload);
		}
	}
}
