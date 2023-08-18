using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixT.Assessment.Models
{
	public class Vehicle
	{
		public Int32 VehicleId { get; set; }
		public String VehicleRegistration { get; set; }
		public float Latitude { get; set; }
		public float Longitude { get; set; }
		public UInt64 RecordedTimeUTC { get; set; }
	}
}
