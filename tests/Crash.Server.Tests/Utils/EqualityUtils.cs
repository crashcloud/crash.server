namespace Crash.Server.Tests.Utils
{
	public class EqualityUtils
	{
		
		public static bool CompareChanges(IChange left, IChange right,
		bool payloadsMustBeEqual = true,
		bool flagsMustContainOtherFlag = true,
		bool stampsMustBeEqual = false)
		{
			if (left.Id != right.Id)
			{
				return false;
			}

			if (!string.Equals(left.Owner, right.Owner, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (left.Action.HasFlag(right.Action))
			{
				return !flagsMustContainOtherFlag;
			}

			if (payloadsMustBeEqual)
			{
				var leftPacket = JsonSerializer.Deserialize<PayloadPacket>(left.Payload);
				var rightPacket = JsonSerializer.Deserialize<PayloadPacket>(right.Payload);
				
				if (!TransformEquals(leftPacket.Transform, rightPacket.Transform))
					throw new AssertionException("Transforms are not equal!");
				
				if (!UpdatesEquals(leftPacket.Updates, rightPacket.Updates))
					throw new AssertionException("Updates are not equal!");
				
				if (!string.Equals(leftPacket.Data, rightPacket.Data, StringComparison.OrdinalIgnoreCase))
					throw new AssertionException("Data is not equal!");
			}

			if (left.Stamp != right.Stamp)
			{
				return !stampsMustBeEqual;
			}

			if (!string.Equals(left.Type, right.Type, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return true;
		}
		
		private static bool TransformEquals(CTransform left, CTransform right)
		{
			for (int row = 0; row < 4; row++)
			{
				for (int col = 0; col < 4; col++)
				{
					if (Math.Abs(left[row, col] - right[row, col]) > 0.00001)
					{
						return false;
					}
				}
			}

			return true;
		}

		private static bool UpdatesEquals(Dictionary<string, string> left, Dictionary<string, string> right)
		{
			foreach (string key in left.Keys)
			{
				if (!right.TryGetValue(key, out string rightValue))
					return false;

				if (!string.Equals(rightValue, left[key], StringComparison.OrdinalIgnoreCase))
					return false;
			}
			
			foreach (string key in right.Keys)
			{
				if (!left.TryGetValue(key, out string leftValue))
					return false;

				if (!string.Equals(leftValue, right[key], StringComparison.OrdinalIgnoreCase))
					return false;
			}

			return true;
		}
		
	}
}
