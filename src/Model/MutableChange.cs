using System.ComponentModel.DataAnnotations;

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

		/// <summary>The Date of Creation</summary>
		public DateTime Stamp { get; init; }

		/// <inheritdoc />
		[Key]
		public Guid Id { get; init; }

		/// <inheritdoc />
		public string Owner { get; init; }

		/// <inheritdoc />
		public string Payload { get; init; }

		/// <inheritdoc />
		public string Type { get; init; }

		/// <inheritdoc />
		public ChangeAction Action { get; set; }

		public static MutableChange CreateWithPacket(Guid id,
			string owner,
			string packet,
			string type,
			ChangeAction action)
		{
			return new MutableChange
			{
				Id = id,
				Stamp = DateTime.UtcNow,
				Owner = owner,
				Payload = packet,
				Type = type,
				Action = action | ChangeAction.Transform | ChangeAction.Update
			};
		}
	}
}
