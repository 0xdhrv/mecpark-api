using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models.Parkings
{
    public class ParkingResponse
    {
        public int SpaceId { get; set; }
        public int GarageId { get; set; }
        public string VehicleNumber { get; set; }
        public string DriverName { get; set; }
        public DateTime UserCheckIn { get; set; }
        public DateTime UserCheckOut { get; set; }
        public bool withCleaningService { get; set; }
        public string Cost { get; set; }
        public bool isActive { get; set; }
        public bool isBooked { get; set; }
    }
}
