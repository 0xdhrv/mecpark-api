using System;
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
            var tempParkingManager = new ParkingManager();
            var parkingManager = _context.ParkingManagers.SingleOrDefault(x => x.Email == user.Email);
            if (parkingManager == null && user.Role != "Admin")
            {
                throw new AppException("Not a parking manager");
            }
            if (( parkingManager != null && parkingManager.GarageId > 0) && user.Role != "Admin")
            {
                throw new AppException("ParkingManager has a Garage");
            }
            if(user.Role != "Admin")
            {
                garage.ParkingManager = parkingManager;
                garage.ParkingManagerId = parkingManager.Id;
            }
            garage.TotalCapacity = "0";
            garage.OccupiedCapacity = "0";
            garage.Space = "0";
            if (!garage.hasCleaningService)
            {
                garage.CleaningRate = "N/A";
            }
            _context.Garages.Add(garage);
            _context.SaveChanges();
            if (user.Role == "Admin")
            {
                Console.WriteLine("\n" + garage.ParkingManagerId + "\n");
                if (garage.ParkingManagerId != 0)
                {
                    garage.ParkingManagerId = garage.ParkingManagerId;
                    tempParkingManager = _context.ParkingManagers.Find(garage.ParkingManagerId);
                    tempParkingManager.GarageId = garage.Id;
                    Console.WriteLine("\n" + garage.Id + "\n");
                    _context.ParkingManagers.Update(tempParkingManager);
                    _context.SaveChanges();
                    Console.WriteLine("\n" + tempParkingManager.GarageId + "\n");
                }
            }
            if (user.Role != "Admin")
            {
                parkingManager.GarageId = garage.Id;
                _context.ParkingManagers.Update(parkingManager);
                _context.SaveChanges();
            }
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
            var user = _context.Users.Find(userId);
            var parkingManager = _context.ParkingManagers.SingleOrDefault(x => x.Id == parkingManagerId);
            if (parkingManager == null && user.Role != "Admin")
            {

            }
            else
            {
                if (user.Role != "Admin")
                {
                    if (parkingManager.GarageId == 0)
                    {
                        throw new AppException("Can't Update That Parking");
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(garageParam.Name))
                        {
                            garage.Name = garageParam.Name;
                        }
                        if (!string.IsNullOrWhiteSpace(garageParam.Address))
                        {
                            garage.Address = garageParam.Address;
                        }
                        if (!string.IsNullOrWhiteSpace(garageParam.City))
                        {
                            garage.City = garageParam.City;
                        }
                        if (!string.IsNullOrWhiteSpace(garageParam.State))
                        {
                            garage.State = garageParam.State;
                        }
                        if (!string.IsNullOrWhiteSpace(garageParam.Phone))
                        {
                            garage.Phone = garageParam.Phone;
                        }
                        if (garageParam.hasCleaningService == true || garageParam.hasCleaningService == false)
                        {
                            garage.hasCleaningService = garageParam.hasCleaningService;
                            if (!garageParam.hasCleaningService)
                            {
                                garage.CleaningRate = "N/A";
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(garageParam.ParkingRate))
                        {
                            garage.ParkingRate = garageParam.ParkingRate;
                        }
                        if (!string.IsNullOrWhiteSpace(garageParam.CleaningRate) && garageParam.hasCleaningService)
                        {
                            garage.CleaningRate = garageParam.CleaningRate;
                        }
                        if (!string.IsNullOrWhiteSpace(garageParam.TotalCapacity))
                        {
                            garage.TotalCapacity = garageParam.TotalCapacity;
                        }
                        if (!string.IsNullOrWhiteSpace(garageParam.Space))
                        {
                            garage.Space = garageParam.Space;
                        }
                        _context.Garages.Update(garage);
                        _context.SaveChanges();
                    }
                }else
                {
                    if (!string.IsNullOrWhiteSpace(garageParam.Name))
                    {
                        garage.Name = garageParam.Name;
                    }
                    if (!string.IsNullOrWhiteSpace(garageParam.Address))
                    {
                        garage.Address = garageParam.Address;
                    }
                    if (!string.IsNullOrWhiteSpace(garageParam.City))
                    {
                        garage.City = garageParam.City;
                    }
                    if (!string.IsNullOrWhiteSpace(garageParam.State))
                    {
                        garage.State = garageParam.State;
                    }
                    if (!string.IsNullOrWhiteSpace(garageParam.Phone))
                    {
                        garage.Phone = garageParam.Phone;
                    }
                    if (garageParam.hasCleaningService == true || garageParam.hasCleaningService == false)
                    {
                        garage.hasCleaningService = garageParam.hasCleaningService;
                        if (!garageParam.hasCleaningService)
                        {
                            garage.CleaningRate = "N/A";
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(garageParam.ParkingRate))
                    {
                        garage.ParkingRate = garageParam.ParkingRate;
                    }
                    if (!string.IsNullOrWhiteSpace(garageParam.CleaningRate) && garageParam.hasCleaningService)
                    {
                        garage.CleaningRate = garageParam.CleaningRate;
                    }
                    if (!string.IsNullOrWhiteSpace(garageParam.TotalCapacity))
                    {
                        garage.TotalCapacity = garageParam.TotalCapacity;
                    }
                    if (!string.IsNullOrWhiteSpace(garageParam.Space))
                    {
                        garage.Space = garageParam.Space;
                    }
                    _context.Garages.Update(garage);
                    _context.SaveChanges();
                }
            }
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
            var user = _context.Users.Find(userId);
            int parkingManagerId = _userService.GetParkingManagerId(userId);
            var parkingManager = _context.ParkingManagers.SingleOrDefault(x => x.Id == parkingManagerId);
            if(parkingManager == null && user.Role != "Admin")
            {

            }
            else
            {
                if (user.Role != "Admin")
                {
                    if (parkingManager.GarageId == 0)
                    {
                        throw new AppException("Can't Delete That Parking");
                    }
                    else
                    {
                        var spaces = _context.Spaces.Where(x => x.GarageId == id);
                        var allocationManager = _context.AllocationManagers.SingleOrDefault(x => x.GarageId == parkingManager.GarageId);
                        allocationManager.Space = "0";
                        _context.AllocationManagers.Update(allocationManager);
                        _context.SaveChanges();
                        _context.Spaces.RemoveRange(spaces);
                        parkingManager.GarageId = 0;
                        _context.ParkingManagers.Update(parkingManager);
                        _context.SaveChanges();
                    }
                }
            }
            var garage = _context.Garages.Find(id);
            if(garage == null)
            {
                throw new AppException("");
            }
            _context.Garages.Remove(garage);
            _context.SaveChanges();
        }
    }
}
