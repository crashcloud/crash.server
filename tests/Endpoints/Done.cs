namespace Crash.Server.Tests.Endpoints
{
	public sealed class Done : CrashHubEndpoints
	{
		[TestCaseSource(nameof(RandomChanges))]
		public async Task Done_Failures(IEnumerable<Change> changes)
		{
			var currCount = _crashHub.context.Changes.Count();

			foreach (var change in changes)
			{
				await _crashHub.Add(change);
			}

			Assert.That(_crashHub.context.Changes.Count(), Is.EqualTo(currCount + changes.Count()));

			var tempCount = _crashHub.context.GetChanges().Select(c => c.HasFlag(ChangeAction.Temporary)).Count();
			Assert.That(tempCount, Is.GreaterThan(0));

			await _crashHub.Done(null);
			await _crashHub.Done(string.Empty);

			Assert.That(tempCount, Is.EqualTo(tempCount));
		}

		[TestCaseSource(nameof(RandomChanges))]
		public async Task Done_Success(IEnumerable<Change> changes)
		{
			var currCount = _crashHub.context.Changes.Count();
			HashSet<string> owners = changes.Select(c => c.Owner).ToHashSet();

			foreach (var change in changes)
			{
				await _crashHub.Add(change);
			}

			Assert.That(_crashHub.context.Changes.Count(), Is.EqualTo(currCount + changes.Count()));
			Assert.That(_crashHub.context.GetChanges().Any(c => c.HasFlag(ChangeAction.Temporary)), Is.True);

			foreach (var owner in owners)
			{
				await _crashHub.Done(owner);

				foreach (var change in _crashHub.context.GetChanges())
				{
					if (change.Owner.Equals(owner))
					{
						Assert.That(change.Action.HasFlag(ChangeAction.Temporary), Is.False);
					}
				}
			}

			Assert.That(_crashHub.context.GetChanges().Any(c => c.HasFlag(ChangeAction.Temporary)), Is.False);
		}
	}
}
