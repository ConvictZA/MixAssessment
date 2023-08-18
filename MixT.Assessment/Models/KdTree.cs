namespace MixT.Assessment.Models
{
	public class KdTree
	{
		private KdNode root;

		public void Insert(KdNode node)
		{
			if (root == null)
				root = node;
			else
				root.Insert(node, 0);
		}

		public KdNode FindNearestNeighbor(float lat, float @long)
		{
			return root?.FindNearestNeighbor(lat, @long, null, double.MaxValue, 0);
		}
	}

	public class KdNode
	{
		public float Latitude { get; }
		public float Longitude { get; }
		public Vehicle Vehicle { get; }

		public KdNode Left { get; set; }
		public KdNode Right { get; set; }

		public KdNode(float lat, float @long, Vehicle vehicle)
		{
			Latitude = lat;
			Longitude = @long;
			Vehicle = vehicle;
		}

		public void Insert(KdNode node, int depth)
		{
			bool compareLat = (depth % 2) == 0;

			if ((compareLat && node.Latitude < Latitude) || (!compareLat && node.Longitude < Longitude))
			{
				if (Left == null)
					Left = node;
				else
					Left.Insert(node, depth + 1);
			}
			else
			{
				if (Right == null)
					Right = node;
				else
					Right.Insert(node, depth + 1);
			}
		}

		public KdNode FindNearestNeighbor(float lat, float @long, KdNode closest, double closestDistance, int depth)
		{
			double dist = CalculateDistance(lat, @long, Latitude, Longitude);

			if (dist < closestDistance)
			{
				closest = this;
				closestDistance = dist;
			}

			bool compareLatitude = (depth % 2) == 0;
			KdNode nextBranch = (compareLatitude ? (lat < Latitude ? Left : Right) : (@long < Longitude ? Left : Right));
			KdNode otherBranch = (nextBranch == Left ? Right : Left);

			if (nextBranch != null)
			{
				closest = nextBranch.FindNearestNeighbor(lat, @long, closest, closestDistance, depth + 1);
				closestDistance = CalculateDistance(lat, @long, closest.Latitude, closest.Longitude);
			}

			double splitDist = compareLatitude ? Math.Abs(Latitude - lat) : Math.Abs(Longitude - @long);

			if (splitDist * splitDist < closestDistance && otherBranch != null)
				closest = otherBranch.FindNearestNeighbor(lat, @long, closest, closestDistance, depth + 1);

			return closest;
		}

		private double CalculateDistance(float lat1, float long1, float lat2, float long2)
		{
			double dx = lat2 - lat1;
			double dy = long2 - long1;
			return dx * dx + dy * dy;
		}
	}
}