﻿namespace Crash.Server.Tests.Endpoints
{

	// TODO : Should Add verify that given Change has Add Action?
	public sealed class Add : CrashHubEndpoints
	{
		[TestCaseSource(nameof(ValidChanges))]
		public async Task Add_Succesful(Change change)
		{
			var currCount = _crashHub.Count;

			await _crashHub.Add(change);
			Assert.That(_crashHub.Count, Is.EqualTo(currCount + 1));

			Assert.That(_crashHub.TryGet(change.Id, out var changeOut), Is.True);
			Assert.That(change, Is.EqualTo(changeOut));
		}

		[TestCaseSource(nameof(ValidChanges))]
		public async Task Add_Failure(Change change)
		{
			Assert.That(_crashHub.Count, Is.EqualTo(0));

			await _crashHub.Add(null);

			Assert.That(_crashHub.Count, Is.EqualTo(0));
		}
	}
}
