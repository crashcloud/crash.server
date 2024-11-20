﻿using Crash.Server.Hubs;

namespace Crash.Server.Tests.Endpoints
{
	public sealed class Selection : CrashHubEndpoints
	{
		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Select_Success(Change change)
		{
			var currCount = _crashHub.Database.Changes.Count();

			await _crashHub.PushChange(change);
			Assert.That(_crashHub.Database.Changes.Count(), Is.EqualTo(currCount + 1));

			await _crashHub.PushChange(new Change(change)
			{
				Action = ChangeAction.Locked, Id = change.Id, Type = CrashHub.CrashGeometryChange
			});
			Assert.That(_crashHub.Database.Changes.Count(), Is.EqualTo(currCount + 2));
			Assert.That(_crashHub.Database.TryGetChange(change.Id, out var latestChange), Is.True);

			Assert.That(latestChange.Action.HasFlag(ChangeAction.Locked), Is.True);
		}

		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task UnSelect_Success(Change change)
		{
			var currCount = _crashHub.Database.Changes.Count();

			await _crashHub.PushChange(change);
			Assert.That(_crashHub.Database.Changes.Count(), Is.EqualTo(currCount + 1));

			await _crashHub.PushChange(new Change(change) { Action = ChangeAction.Unlocked });
			Assert.That(_crashHub.Database.TryGetChange(change.Id, out var changeOut), Is.True);

			Assert.That(changeOut.Action.HasFlag(ChangeAction.Locked), Is.False);
		}

		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Locked_Failure_NotInDatabase(Change change)
		{
			var currCount = _crashHub.Database.Changes.Count();

			await _crashHub.PushChange(new Change
			{
				Action = ChangeAction.Locked, Id = change.Id, Type = CrashHub.CrashGeometryChange, Owner = "James"
			});

			Assert.That(_crashHub.Database.Changes.Count(), Is.EqualTo(currCount));
		}

		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task UnLocked_Failure_NotInDatabase(Change change)
		{
			var currCount = _crashHub.Database.Changes.Count();

			await _crashHub.PushChange(new Change
			{
				Action = ChangeAction.Unlocked, Id = change.Id, Type = CrashHub.CrashGeometryChange, Owner = "James"
			});

			Assert.That(_crashHub.Database.Changes.Count(), Is.EqualTo(currCount));
		}
	}
}
