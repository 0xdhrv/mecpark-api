namespace WebApi.Models.Garages
{
    public class UpdateGarageModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Phone { get; set; }
        public bool hasCleaningService { get; set; }
        public string ParkingRate { get; set; }
        public string CleaningRate { get; set; }
        public string TotalCapacity { get; set; }
        public string Space { get; set; }
    }
}
