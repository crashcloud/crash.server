using System.Collections;

using Crash.Changes;
using Crash.Changes.Extensions;

namespace Crash.Server.Tests.Endpoints
{

	public sealed class Done : CrashHubEndpoints
	{

		[TestCaseSource(nameof(RandomChanges))]
		public async Task Done_Failures(IEnumerable<Change> changes)
		{
			int currCount = _crashHub.Count;

			foreach (var change in changes)
			{
				await _crashHub.Add(change.Owner, change);
			}

			Assert.That(_crashHub.Count, Is.EqualTo(currCount + changes.Count()));

			int tempCount = _crashHub.GetChanges().Select(c => c.HasFlag(ChangeAction.Temporary)).Count();
			Assert.That(tempCount, Is.GreaterThan(0));

			await _crashHub.Done(null);
			await _crashHub.Done(string.Empty);

			Assert.That(tempCount, Is.EqualTo(tempCount));
		}

		[TestCaseSource(nameof(RandomChanges))]
		public async Task Done_Success(IEnumerable<Change> changes)
		{
			int currCount = _crashHub.Count;
			HashSet<string> owners = changes.Select(c => c.Owner).ToHashSet();

			foreach(var change in changes)
			{
				await _crashHub.Add(change.Owner, change);
			}

			Assert.That(_crashHub.Count, Is.EqualTo(currCount + changes.Count()));
			Assert.That(_crashHub.GetChanges().Any(c => c.HasFlag(ChangeAction.Temporary)), Is.True);

			foreach (string owner in owners)
			{
				await _crashHub.Done(owner);
				
				foreach(Change change in _crashHub.GetChanges())
				{
					if (change.Owner.Equals(owner))
					{
						Assert.That(change.Action.HasFlag(ChangeAction.Temporary), Is.False);
					}
				}
			}

			Assert.That(_crashHub.GetChanges().Any(c => c.HasFlag(ChangeAction.Temporary)), Is.False);
		}

	}
}
