
namespace Crash.Server.Tests.Endpoints
{
	public sealed class Selection : CrashHubEndpoints
	{
		[TestCaseSource(nameof(ValidChanges))]
		public async Task Select_Success(Change change)
		{
			var currCount = _crashHub._context.Changes.Count();

			await _crashHub.Add(change);
			Assert.That(_crashHub._context.Changes.Count(), Is.EqualTo(currCount + 1));

			await _crashHub.Lock(change.Owner, change.Id);
			Assert.That(_crashHub._context.Changes.Count(), Is.EqualTo(currCount + 2));
			
			Assert.That(_crashHub._context.TryGetChange(change.Id, out var changeOut), Is.True);

			Assert.That(changeOut.Action.HasFlag(ChangeAction.Lock), Is.True);
		}

		[TestCaseSource(nameof(ValidChanges))]
		public async Task UnSelect_Success(Change change)
		{
			var currCount = _crashHub._context.Changes.Count();

			await _crashHub.Add(change);
			Assert.That(_crashHub._context.Changes.Count(), Is.EqualTo(currCount + 1));

			await _crashHub.Unlock(change.Owner, change.Id);
			Assert.That(_crashHub._context.TryGetChange(change.Id, out var changeOut), Is.True);

			Assert.That(changeOut.Action.HasFlag(ChangeAction.Lock), Is.False);
		}
	}
}
