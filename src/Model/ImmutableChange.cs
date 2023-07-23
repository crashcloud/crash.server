namespace Crash.Server.Model
{

	public sealed record ImmutableChange : IChange
	{
		public Guid UniqueId { get; init; }
		public DateTime Stamp { get; init; }
		public Guid Id { get; init; }
		public string? Owner { get; init; }
		public string? Payload { get; init; }
		public string Type { get; init; }
		public ChangeAction Action { get; set; }

		public ImmutableChange()
		{
			UniqueId = Guid.NewGuid();
		}

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
