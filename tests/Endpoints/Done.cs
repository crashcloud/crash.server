using Crash.Server.Hubs;

namespace Crash.Server.Tests.Endpoints
{
	public sealed class Done : CrashHubEndpoints
	{
		[TestCaseSource(nameof(RandomChanges))]
		public async Task Done_Failures(IEnumerable<Change> changes)
		{
			var currCount = _crashHub.Database.Changes.Count();

			foreach (var change in changes)
			{
				await _crashHub.PushChange(change);
			}

			Assert.That(_crashHub.Database.Changes.Count(), Is.Not.EqualTo(0));

			var tempCount = _crashHub.Database.GetChanges().Select(c => c.HasFlag(ChangeAction.Temporary)).Count();
			Assert.That(tempCount, Is.GreaterThan(0));

			await _crashHub.PushChange(new Change
			{
				Owner = string.Empty, Action = ChangeAction.Release, Type = CrashHub.CrashGeometryChange
			});

			Assert.That(tempCount, Is.EqualTo(tempCount));
		}

		[TestCaseSource(nameof(RandomChanges))]
		public async Task Done_Success(IEnumerable<Change> changes)
		{
			var currCount = _crashHub.Database.Changes.Count();
			HashSet<string> owners = changes.Select(c => c.Owner).ToHashSet();

			await _crashHub.PushChanges(changes);

			var changesCount = _crashHub.Database.Changes.Count();
			var inputChangesCount = changes.Count();

			Assert.That(changesCount, Is.Not.EqualTo(0));
			Assert.That(_crashHub.Database.GetChanges().Any(c => c.HasFlag(ChangeAction.Temporary)), Is.True);

			foreach (var owner in owners)
			{
				var doneChange = new Change
				{
					Owner = owner, Type = CrashHub.CrashDoneChange, Action = ChangeAction.Release
				};
				await _crashHub.PushChange(doneChange);

				foreach (var change in _crashHub.Database.GetChanges())
				{
					if (change.Owner.Equals(owner))
					{
						Assert.That(change.Action.HasFlag(ChangeAction.Temporary), Is.False);
					}
				}
			}

			Assert.That(_crashHub.Database.GetChanges().Any(c => c.HasFlag(ChangeAction.Temporary)), Is.False);
		}
	}
}
