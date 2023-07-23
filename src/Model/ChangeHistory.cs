using System.Collections.Concurrent;

using Crash.Changes.Extensions;

namespace Crash.Server.Model
{
	public sealed class ChangeHistory
	{

		public Guid ChangeId => CurrentChange.Id;

		internal Change CurrentChange { get; set; }

		private ConcurrentStack<IChange> Changes { get; set; }

		// TODO : Is this necessary for the db etc?
		public ChangeHistory()
		{

		}

		internal ChangeHistory(Change change)
		{
			if (change.Id == Guid.Empty)
				throw new ArgumentException("Change must have a valid Id!");

			Changes = new ConcurrentStack<IChange>();
			Changes.Push(change);
			CurrentChange = change;
		}

		internal async Task<bool> PushAsync(IChange change)
		{
			if (change.Id == Guid.Empty)
				return false;

			if (change.Id != ChangeId)
				return false;

			Changes.Push(change);
			// This seems like the computationally cheapest option
			await SetCurrentComputedChange(change);

			return true;
		}

		private async Task SetCurrentComputedChange(IChange newChange)
		{
			// Set Payload
			if (!string.IsNullOrEmpty(newChange.Payload))
			{
				CurrentChange.Payload = newChange.Payload;
			}

			// Update Add/Remove
			SetToggleableChange(newChange, ChangeAction.Add, ChangeAction.Remove);

			// Toggle Temporary)
			if (newChange.HasFlag(ChangeAction.Temporary))
			{
				CurrentChange.AddAction(ChangeAction.Temporary);
			}
			else
			{
				CurrentChange.RemoveAction(ChangeAction.Temporary);
			}

			// Update Lock/Unlock
			SetToggleableChange(newChange, ChangeAction.Lock, ChangeAction.Unlock);

			// Update steps ??
			// Transform steps ??
		}

		private void SetToggleableChange(IChange change, ChangeAction affirmative, ChangeAction negative)
		{
			if (change.HasFlag(affirmative))
			{
				CurrentChange.RemoveAction(negative);
				CurrentChange.AddAction(affirmative);
			}
			else if (change.HasFlag(negative))
			{
				CurrentChange.RemoveAction(affirmative);
				CurrentChange.AddAction(negative);
			}
		}

		internal async Task DoneAsync()
		{
			var doneChange = new Change
			{
				Action = 0,
				Id = CurrentChange.Id,
				Owner = CurrentChange.Owner,
				Stamp = DateTime.UtcNow,
				Type = CurrentChange.Type,
			};
			doneChange.RemoveAction(ChangeAction.Temporary);
			doneChange.RemoveAction(ChangeAction.Lock);
			doneChange.RemoveAction(ChangeAction.Unlock);

			await PushAsync(doneChange);
		}
	}

}

