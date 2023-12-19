using System.ComponentModel.DataAnnotations;

namespace Crash.Server.Model
{
	/// <summary>
	///     A record of a Change.
	///     An immutable change is designed to be created, kept and never modified.
	/// </summary>
	public sealed record ImmutableChange : IChange
	{
		/// <summary>Deserialization Constructor</summary>
		public ImmutableChange()
		{
			UniqueId = Guid.NewGuid();
		}

		/// <summary>Creates a new Immutable Change</summary>
		public ImmutableChange(IChange change) : this()
		{
			Stamp = change.Stamp;
			Id = change.Id;
			Owner = change.Owner;
			Payload = change.Payload;
			Type = change.Type;
			Action = change.Action;
		}

		/// <summary>The ConnectionId of the Change</summary>
		[Key]
		public Guid UniqueId { get; init; }

		// [Timestamp]
		public DateTime Stamp { get; init; }

		public Guid Id { get; init; }

		public string? Owner { get; init; }

		[Timestamp]
		public string? Payload { get; init; }

		public string Type { get; init; }

		public ChangeAction Action { get; set; }
	}
}
