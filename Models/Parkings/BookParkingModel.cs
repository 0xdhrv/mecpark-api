using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Parkings
{
    public class BookParkingModel
    {
        [Required]
        public int SpaceId { get; set; }
        [Required]
        public int GarageId { get; set; }
        [Required]
        public string VehicleNumber { get; set; }
        [Required]
        public string DriverName { get; set; }
        [Required]
        public bool withCleaningService { get; set; }
    }
}
