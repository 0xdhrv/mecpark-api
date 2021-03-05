using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface IGarageService
    {
        Garage Create(Garage garage, int parkingManagerId);
        IEnumerable<Garage> GetGarages();
        Garage GetGarage(int id);
        void Update(int userId, Garage garage);
        void PlusGarageCapacity(Garage garage);
        void MinusGarageCapacity(Garage garage);
        void Delete(int userId, int id);
    }
    public class GarageService : IGarageService
    {
        private readonly DataContext _context;
        private IUserService _userService;

        public GarageService(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public Garage Create(Garage garage, int userId)
        {
            var user = _context.Users.Find(userId);
            var parkingManager = _context.ParkingManagers.Single(x => x.Email == user.Email);
            if (parkingManager == null)
            {
                throw new AppException("Not a parking manager");
            }
            if (parkingManager.GarageId > 0)
            {
                throw new AppException("ParkingManager has a Garage");
            }
            garage.ParkingManager = parkingManager;
            garage.ParkingManagerId = parkingManager.Id;
            garage.TotalCapacity = "0";
            garage.OccupiedCapacity = "0";
            garage.Space = "0";
            if (!garage.hasCleaningService)
            {
                garage.CleaningRate = "N/A";
            }
            _context.Garages.Add(garage);
            _context.SaveChanges();
            parkingManager.GarageId = garage.Id;
            _context.ParkingManagers.Update(parkingManager);
            _context.SaveChanges();
            return garage;
        }

        public IEnumerable<Garage> GetGarages()
        {
            return _context.Garages;
        }

        public Garage GetGarage(int id)
        {
            return _context.Garages.Find(id);

        }

        public void Update(int userId, Garage garageParam)
        {
            var garage = _context.Garages.Find(garageParam.Id);
            int parkingManagerId = _userService.GetParkingManagerId(userId);
            var parkingManager = _context.ParkingManagers.Find(parkingManagerId);
            if (parkingManager.GarageId == 0)
                throw new AppException("Can't Update That Parking");

            if (!string.IsNullOrWhiteSpace(garageParam.Name))
            {
                garage.Name = garageParam.Name;
            }
            if (string.IsNullOrWhiteSpace(garageParam.Address))
            {
                garage.Address = garageParam.Address;
            }
            if (string.IsNullOrWhiteSpace(garageParam.City))
            {
                garage.City = garageParam.City;
            }
            if (string.IsNullOrWhiteSpace(garageParam.State))
            {
                garage.State = garageParam.State;
            }
            if (string.IsNullOrWhiteSpace(garageParam.Phone))
            {
                garage.Phone = garageParam.Phone;
            }
            if (garageParam.hasCleaningService == true || garageParam.hasCleaningService == false)
            {
                garage.hasCleaningService = garageParam.hasCleaningService;
            }
            if (string.IsNullOrWhiteSpace(garageParam.TotalCapacity))
            {
                garage.TotalCapacity = garageParam.TotalCapacity;
            }
            if (string.IsNullOrWhiteSpace(garageParam.Space))
            {
                garage.Space = garageParam.Space;
            }
            _context.Garages.Update(garage);
            _context.SaveChanges();
        }

        public void PlusGarageCapacity(Garage garageParam)
        {
            var garage = _context.Garages.Find(garageParam.Id);
            int occupiedCapacity = int.Parse(garage.OccupiedCapacity);
            occupiedCapacity += 1;
            garage.OccupiedCapacity = occupiedCapacity.ToString();
            _context.Garages.Update(garage);
            _context.SaveChanges();
        }

        public void MinusGarageCapacity(Garage garageParam)
        {
            var garage = _context.Garages.Find(garageParam.Id);
            int occupiedCapacity = int.Parse(garage.OccupiedCapacity);
            occupiedCapacity -= 1;
            garage.OccupiedCapacity = occupiedCapacity.ToString();
            _context.Garages.Update(garage);
            _context.SaveChanges();
        }

        public void Delete(int userId, int id)
        {
            int parkingManagerId = _userService.GetParkingManagerId(userId);
            var parkingManager = _context.ParkingManagers.Find(parkingManagerId);
            if (parkingManager.GarageId == 0)
                throw new AppException("Can't Delete That Parking");
            var garage = _context.Garages.Find(id);
            _context.Garages.Remove(garage);
            _context.SaveChanges();
        }
    }
}
