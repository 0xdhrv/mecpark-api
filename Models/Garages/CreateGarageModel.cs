using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Garages
{
    public class CreateGarageModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public bool hasCleaningService { get; set; }
        [Required]
        public string ParkingRate { get; set; }
        [Required]
        public string CleaningRate { get; set; }
        public int ParkingManagerId { get; set; }
    }
}
