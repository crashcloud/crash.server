namespace Crash.Server.Tests.Endpoints
{
	public sealed class Remove : CrashHubEndpoints
	{
		[TestCaseSource(nameof(ValidChanges))]
		public async Task Delete_Succesful(Change change)
		{
			var currCount = _crashHub.Count;

			await _crashHub.Delete(change.Owner, change.Id);
			Assert.That(_crashHub.Count, Is.EqualTo(currCount));

			await _crashHub.Add(change.Owner, change);
			Assert.That(_crashHub.Count, Is.EqualTo(currCount + 1));

			await _crashHub.Delete(change.Owner, change.Id);
			Assert.That(_crashHub.Count, Is.EqualTo(currCount));
		}

		[TestCaseSource(nameof(ValidChanges))]
		public async Task Delete_Failure(Change change)
		{
			var currCount = _crashHub.Count;

			await _crashHub.Delete(null, Guid.Empty);
			await _crashHub.Delete(change.Owner, Guid.Empty);
			await _crashHub.Delete(null, change.Id);

			for (int i = 0; i < 10; i++)
			{
				string user = Path.GetRandomFileName().Replace(".", "");
				Guid guid = Guid.NewGuid();
				await _crashHub.Delete(user, guid);
				Assert.That(_crashHub.Count, Is.EqualTo(currCount));
			}

			Assert.That(_crashHub.Count, Is.EqualTo(currCount));
		}
	}
}
