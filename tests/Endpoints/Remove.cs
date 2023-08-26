namespace Crash.Server.Tests.Endpoints
{
	public sealed class Remove : CrashHubEndpoints
	{
		[TestCaseSource(nameof(ValidChanges))]
		public async Task Delete_Succesful(Change change)
		{
			var currCount = _crashHub._context.Changes.Count();

			await _crashHub.Delete(change.Id);
			Assert.That(_crashHub._context.Changes.Count(), Is.EqualTo(currCount));

			await _crashHub.Add(change);
			Assert.That(_crashHub._context.Changes.Count(), Is.EqualTo(currCount));

			await _crashHub.Delete(change.Id);
			Assert.That(_crashHub._context.Changes.Count(), Is.EqualTo(currCount));
		}

		[TestCaseSource(nameof(ValidChanges))]
		public async Task Delete_Failure(Change change)
		{
			var currCount = _crashHub._context.Changes.Count();

			await _crashHub.Delete(Guid.Empty);

			for (int i = 0; i < 10; i++)
			{
				string user = Path.GetRandomFileName().Replace(".", "");
				Guid guid = Guid.NewGuid();
				await _crashHub.Delete(guid);
				Assert.That(_crashHub._context.Changes.Count(), Is.EqualTo(currCount));
			}

			Assert.That(_crashHub._context.Changes.Count(), Is.EqualTo(currCount));
		}
	}
}
