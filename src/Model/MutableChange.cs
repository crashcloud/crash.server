namespace Crash.Server.Model
{
	/// <summary>
	///     A record of a Change.
	///     An immutable change is designed to be created, kept and never modified.
	/// </summary>
	public sealed record MutableChange : IChange
	{
		/// <summary>Deserialization Constructor</summary>
		public MutableChange()
		{
			Id = Guid.NewGuid();
		}

		/// <summary>Creates a new Immutable Change</summary>
		public MutableChange(IChange change) : this()
		{
			Stamp = change.Stamp;
			Id = change.Id;
			Owner = change.Owner;
			// TODO : Make sure this payload contains all the necessary parts
			Payload = change.Payload;
			Type = change.Type;
			Action = change.Action;
		}

		/// <summary>The Date of Creation</summary>
		public DateTime Stamp { get; init; }

		/// <inheritdoc />
		[Key]
		public Guid Id { get; init; }

		/// <inheritdoc />
		public string? Owner { get; init; }

		/// <inheritdoc />
		public string? Payload { get; init; }

		/// <inheritdoc />
		public string? Type { get; init; }

		/// <inheritdoc />
		public ChangeAction Action { get; set; }
	}
}
