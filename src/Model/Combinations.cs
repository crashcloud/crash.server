using System.Text.Json;
using System.Text.Json.Serialization;

using Crash.Geometry;

namespace Crash.Server.Model
{
	public static class Combinations
	{

		/// <summary>Takes two actions and combines them in a valid way for storage</summary>
		/// <param name="left">The base Action</param>
		/// <param name="right">he new action to add on top</param>
		public static ChangeAction CombineActions(ChangeAction left, ChangeAction right)
		{
			ChangeAction result = left;
			
			if (right.HasFlag(ChangeAction.Add))
			{
				result &= ~ChangeAction.Remove;
				result |= ChangeAction.Add;
			}
			else if (right.HasFlag(ChangeAction.Remove))
			{
				result |= ChangeAction.Remove;
				result &= ~ChangeAction.Add;
			}
			
			if (right.HasFlag(ChangeAction.Update))
			{
				result |= ChangeAction.Update;
			}

			if (right.HasFlag(ChangeAction.Transform))
			{
				result |= ChangeAction.Transform;
			}

			if (right.HasFlag(ChangeAction.Lock))
			{
				result &= ~ChangeAction.Unlock;
				result |= ChangeAction.Lock;
			}
			else if (right.HasFlag(ChangeAction.Unlock))
			{
				result &= ~ChangeAction.Lock;
				result |= ChangeAction.Unlock;
			}

			if (right.HasFlag(ChangeAction.Temporary))
			{
				result |= ChangeAction.Temporary;
			}

			if (right.HasFlag(ChangeAction.Camera))
			{
				result |= ChangeAction.Camera;
			}

			return result;
		}

		public static string? CombinePayloads(string payloadLeft, string payloadRight)
		{
			var leftPayload = Deserialize(payloadLeft);
			var rightPayload = Deserialize(payloadRight);

			// TODO : Poke Lukas
			// leftPayload.Transform = CTransform.Combine(leftPayload.Transform, rightPayload.Transform);
			
			
			foreach (var keyValuePair in rightPayload.Updates)
			{
				leftPayload.Updates[keyValuePair.Key] = keyValuePair.Value;
			}
			
			var payload = new Payload()
			{
				Data = rightPayload.Data,
				Transform = leftPayload.Transform,
				Updates = leftPayload.Updates
			};

			var result = JsonSerializer.Serialize(payload);
			return result;
		}

		private static Payload Deserialize(string? json)
		{
			if (string.IsNullOrEmpty(json))
				return new Payload();
			
			var payload = JsonSerializer.Deserialize<Payload>(json);
			payload.Transform = new CTransform(0);
			payload.Updates = new();

			return payload;
		}
		
		// Create Payload object?

		internal record Payload
		{
			internal string Data;
			internal CTransform Transform;
			internal Dictionary<string, string> Updates = new();
		}
		
	}
}
