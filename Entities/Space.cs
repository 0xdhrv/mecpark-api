namespace WebApi.Entities
{
    public class Space
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string TotalCapacity { get; set; }
        public string OccupiedCapacity { get; set; }
        public int AllocationManagerId { get; set; }
        public AllocationManager AllocationManager { get; set; }
        public int GarageId { get; set; }
        public Garage Garage { get; set; }

    }
}
