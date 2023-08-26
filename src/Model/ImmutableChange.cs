using System.ComponentModel.DataAnnotations;

namespace Crash.Server.Model
{

	/// <summary>
	/// A record of a Change.
	/// An immutable change is designed to be created, kept and never modified.
	/// </summary>
	public sealed record ImmutableChange : IChange
	{

		/// <summary>The ConnectionId of the Change</summary>
		[Key]
		public Guid UniqueId { get; init; }

		/// <summary>The Date of Creation</summary>
		[Timestamp]
		public DateTime Stamp { get; init; }
		
		///<inheritdoc/>
		public Guid Id { get; init; }
		
		///<inheritdoc/>
		public string? Owner { get; init; }
		
		///<inheritdoc/>
		public string? Payload { get; init; }
		
		///<inheritdoc/>
		public string Type { get; init; }
		
		///<inheritdoc/>
		public ChangeAction Action { get; set; }

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

	}

}
