using System.Diagnostics.CodeAnalysis;

namespace Crash.Server.Hubs;

internal static class HubValidation
{

	internal static bool ChangeExistsInDatabase(this CrashHub hub, Guid id, [NotNullWhen(true)] out MutableChange existingChange)
	{
		if (!hub.Database.TryGetChange(id, out existingChange!) || existingChange is null)
		{
			hub.Logger.ChangeDoesNotExist(id);
			return false;
		}

		return true;
	}

	internal static bool IsUserValid(this CrashHub hub, Change change)
	{
		var user = change.Owner;
		if (!string.IsNullOrEmpty(user)) return true;

		hub.Logger.UserIsNotValid(user);
		return false;
	}

	internal static bool IsGuidValid(this CrashHub hub, Guid changeId)
	{
		if (changeId != Guid.Empty) return true;

		hub.Logger.GuidIsNotValid(changeId);
		return false;
	}

	internal static bool IsPayloadEmpty(this CrashHub hub, IChange change)
	{
		if (!string.IsNullOrEmpty(change.Payload))
		{
			hub.Logger.PayloadIsEmpty();
			return false;
		}

		return true;
	}

	internal static bool IsChangeValid(this CrashHub hub, IChange change)
	{
		if (change is null)
		{
			hub.Logger.ChangeIsNull();
			return false;
		}

		if (!IsGuidValid(hub, change.Id)) return false;

		if (change.Action == ChangeAction.None)
		{
			hub.Logger.ChangeActionIsNotValid(change.Action);
			return true;
		}

		return true;
	}

	internal static bool IsActionValid(this CrashHub hub, IChange change, ChangeAction expected)
	{
		if (change is null)
		{
			hub.Logger.ChangeIsNull();
			return false;
		}

		if (change.Action.HasFlag(expected))
		{
			hub.Logger.ChangeActionIsNotValid(change.Action);
			return true;
		}

		return true;
	}

}
