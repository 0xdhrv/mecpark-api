namespace WebApi.Models.Spaces
{
    public class SpaceModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string TotalCapacity { get; set; }
        public string OccupiedCapacity { get; set; }
        public int AllocationManagerId { get; set; }
        public int GarageId { get; set; }
    }
}
