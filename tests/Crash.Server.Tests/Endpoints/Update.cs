﻿// ReSharper disable HeapView.BoxingAllocation

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
			var currCount = CrashHub.Database.Changes.Count();

			await CrashHub.PushChange(change);
			Assert.That(CrashHub.Database.Changes.Count(), Is.EqualTo(currCount + 1));

			var updatesPacket = new PayloadPacket() { Updates = new Dictionary<string, string> { { "Key", "Value" } } };
			var payload = JsonSerializer.Serialize(updatesPacket);

			var transformChange = new Change
			{
				Id = change.Id,
				Type = change.Type,
				Action = ChangeAction.Update | ChangeAction.Add,
				Payload = payload,
				Owner = Path.GetRandomFileName().Replace(".", "")
			};
			await CrashHub.PushChange(transformChange);

			Assert.That(CrashHub.Database.TryGetChange(change.Id, out var latestChange), Is.True);

			Assert.That(change.Id, Is.EqualTo(latestChange.Id));
			Assert.That(PayloadUtils.TryGetPayloadFromChange(latestChange, out var payloadPacket));

			Assert.That(payloadPacket.Updates, Is.Not.Null.Or.Empty);
			Assert.That(payloadPacket.Updates, Is.EqualTo(updatesPacket.Updates));
		}
	}
}
