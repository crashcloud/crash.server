// ReSharper disable HeapView.BoxingAllocation

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Crash.Server.Tests.Endpoints
{
	public sealed class Add : CrashHubEndpoints
	{
		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Add_Successful(Change change)
		{
			var currCount = _crashHub._context.Changes.Count();

			await _crashHub.Add(change);
			Assert.That(_crashHub._context.Changes.Count(), Is.EqualTo(currCount + 1));

			Assert.That(_crashHub._context.TryGetChange(change.Id, out var changeOut), Is.True);
			Assert.That(change, Is.EqualTo(changeOut));
		}

		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Add_Failure(Change change)
		{
			Assert.IsEmpty(_crashHub._context.Changes);
			await _crashHub.Add(null);
			Assert.IsEmpty(_crashHub._context.Changes);
		}
	}
}
