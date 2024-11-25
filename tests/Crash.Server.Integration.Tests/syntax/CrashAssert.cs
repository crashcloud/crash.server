using System.Collections.Concurrent;

using Crash.Changes;
using Crash.Server.Model;

namespace Crash.Server.Integration.Tests;

public class CrashAnd
{
	public CrashAssert And { get; }

	public CrashAnd(CrashAssert assert)
	{
		And = assert;
	}
}

public class CrashAssert
{
	private ConcurrentDictionary<Guid, Queue<Change>> Changes { get; } = new();

	private CrashAnd And { get; }

	public CrashAssert()
	{
		And = new(this);
	}

	public CrashAnd Recieved(Change newChange)
	{
		Assert.That(Changes, Does.ContainKey(newChange.Id).After(5).Seconds.PollEvery(100).MilliSeconds);
		Assert.That(Changes.TryGetValue(newChange.Id, out var changes), Is.True);
		Assert.That(changes.Any(c => c.Equals(newChange)));
		return And;
	}

	public void AddItem(Change change)
	{
		Queue<Change> newChangeRecord = new();
		if (Changes.TryGetValue(change.Id, out var existingChanges))
		{
			newChangeRecord = existingChanges;
			newChangeRecord.Enqueue(change);
		}
		else
		{
			newChangeRecord.Enqueue(change);
			Changes.TryAdd(change.Id, newChangeRecord);
		}
	}

	public CrashAnd Deleted(Change change)
	{
		Assert.That(Changes, Does.ContainKey(change.Id));
		Assert.That(Changes.TryGetValue(change.Id, out var changes), Is.True);
		Assert.That(changes.Any(c => c.Action.HasFlag(ChangeAction.Remove)), Is.True.After(5).Seconds.PollEvery(100).MilliSeconds);
		return And;
	}

	public CrashAnd CanEdit(Change change) => Editship(change, true);

	public CrashAnd CannotEdit(Change change) => Editship(change, false);

	private CrashAnd Editship(Change change, bool canEdit)
	{
		Assert.That(Changes, Does.ContainKey(change.Id).After(5).Seconds.PollEvery(100).MilliSeconds);
		Assert.That(Changes.TryGetValue(change.Id, out var changes), Is.True);
		var sumChange = ChangeFactory.CombineRecords(changes);
		Assert.That(sumChange.IsSome(out var combinedChange));
		bool creator = string.Equals(combinedChange.Owner, change.Owner, StringComparison.OrdinalIgnoreCase);
		bool isEditable = !combinedChange.Action.HasFlag(ChangeAction.Temporary);
		Assert.That(creator || isEditable, canEdit ? Is.True : Is.False);
		return And;
	}

	public CrashAnd Created(Change change) => Authorship(change, true);

	public CrashAnd DidNotCreate(Change change) => Authorship(change, false);

	private CrashAnd Authorship(Change change, bool owns)
	{
		Assert.That(Changes, Does.ContainKey(change.Id).After(5).Seconds.PollEvery(100).MilliSeconds);
		Assert.That(Changes.TryGetValue(change.Id, out var changes), Is.True);
		Assert.That(string.Equals(changes.LastOrDefault()!.Owner, change.Owner, StringComparison.OrdinalIgnoreCase), owns ? Is.True : Is.False);
		return And;
	}

}
