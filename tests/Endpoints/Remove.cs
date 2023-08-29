namespace Crash.Server.Tests.Endpoints
{
	public sealed class Remove : CrashHubEndpoints
	{
		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Delete_Succesful(Change change)
		{
			var currCount = _crashHub.context.Changes.Count();

			await _crashHub.Add(change);
			Assert.That(_crashHub.context.Changes.Count(), Is.EqualTo(currCount + 1));

			await _crashHub.Delete(change.Id);
			Assert.That(_crashHub.context.Changes.Count(), Is.EqualTo(currCount));
		}

		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Delete_Failure(Change change)
		{
			var currCount = _crashHub.context.Changes.Count();

			await _crashHub.Delete(Guid.Empty);

			for (var i = 0; i < 10; i++)
			{
				var user = Path.GetRandomFileName().Replace(".", "");
				var guid = Guid.NewGuid();
				await _crashHub.Delete(guid);
				Assert.That(_crashHub.context.Changes.Count(), Is.EqualTo(currCount));
			}

			Assert.That(_crashHub.context.Changes.Count(), Is.EqualTo(currCount));
		}
	}
}
