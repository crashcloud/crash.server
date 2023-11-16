﻿using System.Text.Encodings.Web;
using System.Text.Json;

using Crash.Changes.Utils;

namespace Crash.Server.Model
{
	/// <summary>Creates Changes</summary>
	public static class ChangeFactory
	{
		private static readonly JsonSerializerOptions options = new()
		{
			AllowTrailingCommas = true,
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true,
			Encoder = JavaScriptEncoder.Default
			// Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
		};

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
				Type = change.Type
				// Payload = change.Payload
			};

			return doneRecord;
		}

		public static MutableChange CreateMutableFromChange(IChange recievedChange)
		{
			PayloadUtils.TryGetPayloadFromChange(recievedChange, out var packet);

			return MutableChange.CreateWithPacket(recievedChange.Id,
				recievedChange.Owner,
				JsonSerializer.Serialize(packet, options),
				recievedChange.Type,
				recievedChange.Action);
		}

		public static MutableChange CombineRecords(IChange previousRecord, IChange newRecord)
		{
			var combinedChange = ChangeUtils.CombineChanges(previousRecord, newRecord);
			return MutableChange.CreateWithPacket(
				combinedChange.Id,
				combinedChange.Owner,
				combinedChange.Payload,
				combinedChange.Type,
				combinedChange.Action
			);
		}
	}
}
