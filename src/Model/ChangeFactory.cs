namespace Crash.Server.Model
{
	
	/// <summary>Creates Changes</summary>
	public static class ChangeFactory
	{

		/// <summary>Creates a Delete Record</summary>
		/// <param name="id">The id of the Change to lock</param>
		public static ImmutableChange CreateDeleteRecord(Guid id)
			=> new ()
			{
				Id = id,
				Action = ChangeAction.Remove,
				Stamp = DateTime.UtcNow,
			};
	
		/// <summary>Creates a Lock Record</summary>
		/// <param name="type">Likely the latest Type</param>
		/// <param name="id">The id of the Change to lock</param>
		public static ImmutableChange CreateLockRecord(string type, Guid id)
			=> new ()
			{
				Id = id,
				Action = ChangeAction.Lock,
				Stamp = DateTime.UtcNow,
				Type = type
			};
		
		/// <summary>Creates an Unlock Record</summary>
		/// <param name="type">Likely the latest Type</param>
		/// <param name="id">The id of the Change to unlock</param>
		public static ImmutableChange CreateUnlockRecord(string type, Guid id)
			=> new ()
			{
				Id = id,
				Action = ChangeAction.Unlock,
				Stamp = DateTime.UtcNow,
				Type = type,
			};
		
		/// <summary>Creates a Done Record</summary>
		/// <param name="user">The user who called Done</param>
		/// <param name="type">Likely the latest Type</param>
		/// <param name="id">The id of the Change to mark done</param>
		public static ImmutableChange CreateDoneRecord(string user, string type, Guid id)
			=> new ()
			{
				Action = 0,
				Id = id, // Should this be random?
				Owner = user,
				Stamp = DateTime.UtcNow,
				Type = type
			};

		/// <summary>Duplicates a Record and makes it accessible to everyone</summary>
		/// <param name="change">The Record to Update</param>
		public static ImmutableChange CreateDoneRecord(IChange change)
			=> new()
			{
				Action = change.Action & ~ChangeAction.Temporary
				                       & ~ChangeAction.Lock
				                       & ~ChangeAction.Unlock,
				Id = change.Id,
				Owner = change.Owner,
				Stamp = DateTime.Now,
				Type = change.Type,
				Payload = change.Payload,
			};

		public static Change CombineRecords(IChange previousRecord, IChange newRecord)
		{
			if (previousRecord is null)
				throw new ArgumentException($"{nameof(previousRecord)} is null");

			if (newRecord is null)
				throw new ArgumentException($"{nameof(newRecord)} is null");
			
			Guid combinedId = Guid.Empty;
			if (previousRecord.Id == Guid.Empty ||
			    previousRecord.Id != newRecord.Id)
				throw new ArgumentException("Id is Invalid!");
			
			Change result = new ()
			{
				Id = combinedId,
				Stamp = DateTime.UtcNow,
				Owner = newRecord.Owner ?? previousRecord.Owner,
				Payload = Combinations.CombinePayloads(previousRecord.Payload, newRecord.Payload),
				Type = previousRecord.Type,
				Action = Combinations.CombineActions(previousRecord.Action, newRecord.Action),
			};

			return result;
		}

	}
	
}
