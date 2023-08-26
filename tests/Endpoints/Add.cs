using Crash.Server.Hubs;

namespace Crash.Server.Tests.Endpoints
{

	// TODO : Should Add verify that given Change has Add Action?
	public sealed class Add : CrashHubEndpoints
	{
		[TestCaseSource(nameof(ValidChanges))]
		public async Task Add_Succesful(Change change)
		{
			var currCount = _crashHub._context.Changes.Count();

			await _crashHub.Add(change);
			Assert.That(_crashHub._context.Changes.Count(), Is.EqualTo(currCount + 1));

			Assert.That(_crashHub._context.TryGetChange(change.Id, out var changeOut), Is.True);
			Assert.That(change, Is.EqualTo(changeOut));
		}

		[TestCaseSource(nameof(ValidChanges))]
		public async Task Add_Failure(Change change)
		{
			Assert.IsEmpty(_crashHub._context.Changes);
			await _crashHub.Add(null);
			Assert.IsEmpty(_crashHub._context.Changes);
		}
	}
}
