using MixT.Assessment.Models;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text;

var stopWatch = Stopwatch.StartNew();

var listVehicles = new ConcurrentBag<Vehicle>();
var listPoints = SeedPoints();

Console.WriteLine($"Start reading from file.");

LoadDataFromFile(listVehicles);

Console.WriteLine($"Data loaded from file in {stopWatch.ElapsedMilliseconds}ms.\n");

FindClosestToPoints(listPoints, BuildTree(listVehicles));

Console.WriteLine($"Total execution time is {stopWatch.ElapsedMilliseconds}ms.");

stopWatch.Stop();

#region Methods

static List<Point> SeedPoints()
{
	return new List<Point>() {
	new Point()
	{
		Id = 1,
		Latitude = 34.544909f,
		Longitude = -102.100843f,
	},
	new Point()
	{
		Id = 2,
		Latitude = 32.345544f,
		Longitude = -99.123124f,
	},
	new Point()
	{
		Id = 3,
		Latitude = 33.234235f,
		Longitude = -100.214124f,
	},
	new Point()
	{
		Id = 4,
		Latitude = 35.195739f,
		Longitude = -95.348899f,
	},
	new Point()
	{
		Id = 5,
		Latitude = 31.895839f,
		Longitude = -97.789573f,
	},
	new Point()
	{
		Id = 6,
		Latitude = 32.895839f,
		Longitude = -101.789573f,
	},
	new Point()
	{
		Id = 7,
		Latitude = 34.115839f,
		Longitude = -100.225732f,
	},
	new Point()
	{
		Id = 8,
		Latitude = 32.335839f,
		Longitude = -99.992232f,
	},
	new Point()
	{
		Id = 9,
		Latitude = 33.535339f,
		Longitude = -94.792232f,
	},
	new Point()
	{
		Id = 10,
		Latitude = 32.234235f,
		Longitude = -100.222222f,
	}
};
}

static void LoadDataFromFile(ConcurrentBag<Vehicle> list)
{
	Vehicle ReadData(byte[] buffer, ref int offset)
	{
		string ReadNullTerminatedString(byte[] buffer, ref int offset)
		{
			int startIndex = offset;
			while (offset < buffer.Length && buffer[offset] != 0)
			{
				offset++;
			}

			string str = Encoding.ASCII.GetString(buffer, startIndex, offset - startIndex);
			offset++; // Skip null-terminator
			return str;
		}

		Vehicle data = new Vehicle();

		data.VehicleId = BitConverter.ToInt32(buffer, offset);
		offset += sizeof(int);

		data.VehicleRegistration = ReadNullTerminatedString(buffer, ref offset);
		data.Latitude = BitConverter.ToSingle(buffer, offset);
		offset += sizeof(float);

		data.Longitude = BitConverter.ToSingle(buffer, offset);
		offset += sizeof(float);

		data.RecordedTimeUTC = BitConverter.ToUInt64(buffer, offset);
		offset += sizeof(ulong);

		return data;
	}

	string resourceName = "MixT.Assessment.VehiclePositions.dat"; // Adjust namespace and filename
	Assembly assembly = Assembly.GetExecutingAssembly();

	using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
	{
		if (resourceStream == null)
		{
			Console.WriteLine("Resource not found.");
			return;
		}

		byte[] resourceBytes;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			resourceStream.CopyTo(memoryStream);
			resourceBytes = memoryStream.ToArray();
		}

		Parallel.ForEach(Partitioner.Create(0, resourceBytes.Length), range =>
		{
			for (int i = range.Item1; i < range.Item2;)
			{
				var data = ReadData(resourceBytes, ref i);
				list.Add(data);
			}
		});
	}
}

static void FindClosestToPoints(List<Point> points, KdTree tree)
{
	Vehicle FindClosestVehicle(Point point, KdTree kdTree)
	{
		KdNode nearestNode = kdTree.FindNearestNeighbor(point.Latitude, point.Longitude);
		return nearestNode.Vehicle;
	}

	points.ForEach(p =>
	{
		var closestVehicle = FindClosestVehicle(p, tree);
		Console.WriteLine($"Point {p.Id}");
		Console.WriteLine($"Latitude => {p.Latitude}");
		Console.WriteLine($"Longityde => {p.Longitude}");
		Console.WriteLine($"Closest Vehicle Registration => {closestVehicle.VehicleRegistration}\n");
	});
}

static KdTree BuildTree(ConcurrentBag<Vehicle> vehicles)
{
	KdTree kdTree = new KdTree();
	foreach (var vehicle in vehicles)
		kdTree.Insert(new KdNode(vehicle.Latitude, vehicle.Longitude, vehicle));

	return kdTree;
}

#endregion


