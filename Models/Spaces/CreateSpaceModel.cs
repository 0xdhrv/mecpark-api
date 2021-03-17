using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Spaces
{
    public class CreateSpaceModel
    {
        [Required]
        public string Code { get; set; }
        [Required]
        public string TotalCapacity { get; set; }
        [Required]
        public int GarageId { get; set; }
        public int AllocationManagerId { get; set; }
    }
}
