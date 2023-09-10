// ReSharper disable HeapView.BoxingAllocation

using Crash.Server.Hubs;

namespace Crash.Server.Tests.Hubs
{
	[Parallelizable]
	public class HubUtilsTests
	{
		private static IEnumerable ValidUserNames
		{
			get
			{
				yield return "John";
				yield return "JJ";
				yield return "Mr Jeremy, The Ice Cream Seller";
			}
		}

		private static IEnumerable InvalidUserNames
		{
			get
			{
				yield return null;
				yield return string.Empty;
				// yield return "-+=@#"; // Should we validate from this?
			}
		}

		public static IEnumerable ValidChanges
		{
			get
			{
				yield return new Change { Owner = "Jeremy", Action = ChangeAction.Add, Stamp = DateTime.Now };
			}
		}

		public static IEnumerable InvalidChanges
		{
			get
			{
				yield return null;
				yield return new Change();
				yield return new Change { Id = Guid.Empty };
				yield return new Change { Id = Guid.Empty, Owner = "Jeremy" };
				yield return new Change { Id = Guid.Empty, Owner = "Jeremy", Action = ChangeAction.Add };
				yield return new Change
				{
					Id = Guid.Empty, Owner = "Jeremy", Action = ChangeAction.Add, Stamp = DateTime.Now
				};
			}
		}

		public static IEnumerable ValidPayloads
		{
			get
			{
				// Transform Payload
				var transform = new CTransform(200);
				var transformPayload = JsonSerializer.Serialize(transform);
				yield return transformPayload;

				// Update Payload
				var updates = new Dictionary<string, string>();
				var updatesPayload = JsonSerializer.Serialize(updates);
				yield return updatesPayload;

				// "Geometry" Payload
				var geometry = new CPoint(100, 200, 300);
				var geometryPayload = JsonSerializer.Serialize(geometry);
				yield return geometryPayload;

				// Combined Payload
				var packet = new PayloadPacket { Transform = transform, Updates = updates, Data = geometryPayload };
				var packetPayload = JsonSerializer.Serialize(packet);
				yield return packetPayload;
			}
		}

		public static IEnumerable InvalidPayloads
		{
			get
			{
				yield return null;
				yield return string.Empty;
			}
		}

		[TestCaseSource(nameof(ValidUserNames))]
		public void IsUserValid_ValidUsers(string userName)
		{
			Assert.That(HubUtils.IsUserValid(userName), Is.True);
		}

		[TestCaseSource(nameof(InvalidUserNames))]
		public void IsUserValid_InvalidUsers(string? userName)
		{
			Assert.That(HubUtils.IsUserValid(userName), Is.False);
		}

		[TestCaseSource(nameof(ValidChanges))]
		public void IsChangeValid_ValidChanges(IChange change)
		{
			Assert.That(HubUtils.IsChangeValid(change), Is.True);
		}

		[TestCaseSource(nameof(InvalidChanges))]
		public void IsChangeValid_InvalidChanges(IChange change)
		{
			Assert.That(HubUtils.IsChangeValid(change), Is.False);
		}

		[Test]
		public void IsGuidValid()
		{
			Assert.That(HubUtils.IsGuidValid(Guid.NewGuid()), Is.True);
			Assert.That(HubUtils.IsGuidValid(Guid.Empty), Is.False);
		}

		[TestCaseSource(nameof(ValidPayloads))]
		public void IsPayloadValid_ValidPayloads(string payload)
		{
			Change change = new() { Payload = payload };
			Assert.That(HubUtils.IsPayloadValid(change), Is.True);
		}

		[TestCaseSource(nameof(InvalidPayloads))]
		public void IsPayloadValid_InvalidPayloads(string? payload)
		{
			Change change = new() { Payload = payload };
			Assert.That(HubUtils.IsPayloadValid(change), Is.False);
		}
	}
}
