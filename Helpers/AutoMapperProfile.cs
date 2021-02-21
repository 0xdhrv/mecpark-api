using AutoMapper;
using WebApi.Entities;
using WebApi.Models.AllocationManagers;
using WebApi.Models.Garages;
using WebApi.Models.ParkingHistories;
using WebApi.Models.ParkingManagers;
using WebApi.Models.Parkings;
using WebApi.Models.Spaces;
using WebApi.Models.Users;


namespace WebApi.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserModel>();
            CreateMap<RegisterModel, User>();
            CreateMap<UpdateModel, User>();

            CreateMap<AllocationManager, AllocationManagerModel>();
            CreateMap<ParkingManager, ParkingManagerModel>();

            CreateMap<Garage, GarageModel>();
            CreateMap<CreateGarageModel, Garage>();
            CreateMap<UpdateGarageModel, Garage>();

            CreateMap<Space, SpaceModel>();
            CreateMap<CreateSpaceModel, Space>();
            CreateMap<UpdateSpaceModel, Space>();

            CreateMap<BookParkingModel, Parking>();
            CreateMap<ParkingHistory, ReceiptModel>();
        }
    }
}