using System.Text.Encodings.Web;
using System.Text.Json;

using Crash.Changes.Utils;

namespace Crash.Server.Model
{
	/// <summary>Creates Changes</summary>
	public static class ChangeFactory
	{
		/// <summary>Creates a Delete Record</summary>
		/// <param name="id">The id of the Change to lock</param>
		public static ImmutableChange CreateDeleteRecord(Guid id)
		{
			return new ImmutableChange { Id = id, Action = ChangeAction.Remove, Stamp = DateTime.UtcNow };
		}

		/// <summary>Creates a Lock Record</summary>
		/// <param name="type">Likely the latest Type</param>
		/// <param name="id">The id of the Change to lock</param>
		public static ImmutableChange CreateLockRecord(string type, Guid id)
		{
			return new ImmutableChange { Id = id, Action = ChangeAction.Locked, Stamp = DateTime.UtcNow, Type = type };
		}

		/// <summary>Creates an Unlock Record</summary>
		/// <param name="type">Likely the latest Type</param>
		/// <param name="id">The id of the Change to unlock</param>
		public static ImmutableChange CreateUnlockRecord(string type, Guid id)
		{
			return new ImmutableChange
			{
				Id = id, Action = ChangeAction.Unlocked, Stamp = DateTime.UtcNow, Type = type
			};
		}

		/// <summary>Creates a Done Record</summary>
		/// <param name="user">The user who called Done</param>
		/// <param name="type">Likely the latest Type</param>
		/// <param name="id">The id of the Change to mark done</param>
		public static ImmutableChange CreateDoneRecord(string user, string type, Guid id)
		{
			return new ImmutableChange
			{
				Action = ChangeAction.Release,
				Id = id, // Should this be random?
				Owner = user,
				Stamp = DateTime.UtcNow,
				Type = type
			};
		}

		/// <summary>Duplicates a Record and makes it accessible to everyone</summary>
		/// <param name="change">The Record to Update</param>
		public static ImmutableChange CreateDoneRecord(IChange change)
		{
			var action = change.Action | ChangeAction.Release;
			action &= ~ChangeAction.Temporary;
			action &= ~ChangeAction.Locked;
			action &= ~ChangeAction.Unlocked;

			var doneRecord = new ImmutableChange
			{
				Action = action,
				Id = change.Id,
				Owner = change.Owner,
				Stamp = DateTime.Now,
				Type = change.Type,
				Payload = change.Payload
			};

			return doneRecord;
		}

		public static MutableChange CombineRecords(IChange previousRecord, IChange newRecord)
		{
			if (previousRecord is null)
			{
				throw new ArgumentException($"{nameof(previousRecord)} is null");
			}

			if (newRecord is null)
			{
				throw new ArgumentException($"{nameof(newRecord)} is null");
			}

			var combinedId = previousRecord.Id;
			if (previousRecord.Id == Guid.Empty ||
			    previousRecord.Id != newRecord.Id ||
			    previousRecord.Id == newRecord.Id)
			{
				throw new ArgumentException("Id is Invalid!");
			}

			PayloadUtils.TryGetPayloadFromChange(previousRecord, out var previousPacket);
			PayloadUtils.TryGetPayloadFromChange(newRecord, out var newPacket);
			var payload = PayloadUtils.Combine(previousPacket, newPacket);

			var options = new JsonSerializerOptions
			{
				AllowTrailingCommas = true,
				IgnoreReadOnlyFields = true,
				IgnoreReadOnlyProperties = true,
				Encoder = JavaScriptEncoder.Default
				// Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
			};

			MutableChange result = new()
			{
				Id = combinedId,
				Stamp = DateTime.UtcNow,
				Owner = newRecord.Owner ?? previousRecord.Owner,
				Payload = JsonSerializer.Serialize(payload, options),
				Type = previousRecord.Type,
				Action = ChangeUtils.CombineActions(previousRecord.Action, newRecord.Action) |
				         ChangeAction.Transform | ChangeAction.Update
			};

			return result;
		}
	}
}
