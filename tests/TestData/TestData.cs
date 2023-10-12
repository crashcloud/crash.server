using Crash.Server.Hubs;

namespace Crash.Server.Tests
{
	public static class TestData
	{
		public static IEnumerable ValidUserNames
		{
			get
			{
				yield return "Erika";
				yield return "Moustafa";
				yield return "Russell Feathers";
			}
		}

		public static IEnumerable InvalidUserNames
		{
			get
			{
				yield return null;
				yield return string.Empty;
				// yield return "-+=@#"; // Should we validate from this?
			}
		}

		public static IEnumerable ValidSecondChanges
		{
			get
			{
				yield return new Change
				{
					Owner = "Morteza",
					Action = ChangeAction.Add,
					Stamp = DateTime.Now,
					Type = CrashHub.CrashGeometryChange,
					Payload = "{}"
				};

				yield return new Change
				{
					Owner = "Curtis",
					Action = ChangeAction.Release,
					Stamp = DateTime.Now,
					Type = CrashHub.CrashDoneChange
				};

				yield return new Change
				{
					Owner = "Erika",
					Action =
						ChangeAction.Temporary | ChangeAction.Add | ChangeAction.Update | ChangeAction.Transform,
					Stamp = DateTime.Now,
					Type = CrashHub.CrashGeometryChange,
					Payload = JsonSerializer.Serialize(new PayloadPacket())
				};

				yield return new Change
				{
					Owner = "Bob",
					Action = ChangeAction.Locked,
					Stamp = DateTime.Now,
					Type = CrashHub.CrashGeometryChange
				};

				yield return new Change
				{
					Owner = "Steve",
					Action = ChangeAction.Unlocked,
					Stamp = DateTime.Now,
					Type = CrashHub.CrashGeometryChange
				};

				yield return new Change
				{
					Owner = "Dale",
					Action = ChangeAction.Remove,
					Stamp = DateTime.Now,
					Type = CrashHub.CrashGeometryChange
				};

				yield return new Change
				{
					Owner = "Curtis",
					Action = ChangeAction.Transform,
					Stamp = DateTime.Now,
					Type = CrashHub.CrashGeometryChange,
					Payload = JsonSerializer.Serialize(new CTransform(200))
				};
			}
		}

		public static IEnumerable ValidFirstChanges
		{
			get
			{
				yield return new Change
				{
					Owner = "Morteza",
					Action = ChangeAction.Add,
					Stamp = DateTime.Now,
					Type = CrashHub.CrashGeometryChange,
					Payload = "{}"
				};

				yield return new Change
				{
					Owner = "Erika",
					Action =
						ChangeAction.Temporary | ChangeAction.Add | ChangeAction.Update | ChangeAction.Transform,
					Stamp = DateTime.Now,
					Type = CrashHub.CrashGeometryChange,
					Payload = JsonSerializer.Serialize(new PayloadPacket())
				};
			}
		}

		public static IEnumerable InvalidChanges
		{
			get
			{
				yield return null;
				yield return new Change();
				yield return new Change { Id = Guid.Empty };
				yield return new Change { Id = Guid.Empty, Owner = "Curtis" };
				yield return new Change { Id = Guid.Empty, Owner = "Callum", Action = ChangeAction.Add };
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
	}
}
