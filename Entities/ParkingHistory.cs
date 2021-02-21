using System;

namespace WebApi.Entities
{
    public class ParkingHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string VehicleNumber { get; set; }
        public string DriverName { get; set; }
        public int SpaceId { get; set; }
        public int GarageId { get; set; }
        public User User { get; set; }
        public DateTime UserCheckIn { get; set; }
        public DateTime UserCheckOut { get; set; }
        public bool withCleaningService { get; set; }
        public string Cost { get; set; }
        public string interval { get; set; }
    }
}
