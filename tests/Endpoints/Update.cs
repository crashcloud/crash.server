// ReSharper disable HeapView.BoxingAllocation

using Crash.Changes.Utils;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Crash.Server.Tests.Endpoints
{
	public sealed class Update : CrashHubEndpoints
	{
		private static bool EqualChanges(IChange left, IChange right)
		{
			if (left.Id != right.Id)
			{
				return false;
			}

			if (left.Owner != right.Owner)
			{
				return false;
			}

			if (left.Action != right.Action)
			{
				return false;
			}

			if (left.Payload != right.Payload)
			{
				return false;
			}

			if (left.Stamp != right.Stamp)
			{
				return false;
			}

			if (left.Type != right.Type)
			{
				return false;
			}

			return true;
		}

		[TestCaseSource(nameof(ValidAddChanges))]
		public async Task Update_Successful(Change change)
		{
			var currCount = _crashHub.Database.Changes.Count();

			await _crashHub.PushChange(change);
			Assert.That(_crashHub.Database.Changes.Count(), Is.EqualTo(currCount + 1));

			var updates = new Dictionary<string, string> { { "Key", "Value" } };
			var payload = JsonSerializer.Serialize(updates);

			var transformChange = new Change
			{
				Id = change.Id, Type = change.Type, Action = ChangeAction.Update, Payload = payload
			};
			await _crashHub.PushChange(transformChange);

			Assert.That(_crashHub.Database.TryGetChange(change.Id, out var latestChange), Is.True);

			Assert.That(change.Id, Is.EqualTo(latestChange.Id));
			Assert.True(PayloadUtils.TryGetPayloadFromChange(latestChange, out var payloadPacket));

			Assert.That(payloadPacket.Updates, Is.Not.Null.Or.Empty);
			Assert.That(payloadPacket.Updates, Is.EqualTo(updates));
		}
	}
}
