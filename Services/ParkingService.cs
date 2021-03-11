using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface IParkingService
    {
        Parking Book(Parking parking, int userId);
        void CheckIn(int userId);
        void CheckOut(int userId);
        Parking GetByUserId(int userId);
        IEnumerable<Parking> GetActiveParkings();
        IEnumerable<Parking> GetInactiveParkings();
        ParkingHistory GetReceipt(int userId);
        IEnumerable<ParkingHistory> GetParkingHistories();
        IEnumerable<ParkingHistory> GetUserParkingHistories(int userId);
    }
    public class ParkingService : IParkingService
    {
        private readonly DataContext _context;
        private readonly IGarageService _garageService;
        private readonly ISpaceService _spaceService;

        public ParkingService(DataContext context, IGarageService garageService, ISpaceService spaceService)
        {
            _context = context;
            _spaceService = spaceService;
            _garageService = garageService;
        }

        public Parking Book(Parking parking, int userId)
        {
            var user = _context.Users.Find(userId);
            var space = _context.Spaces.Find(parking.SpaceId);
            var garage = _context.Garages.Find(parking.GarageId);

            if (user == null || space == null || garage == null)
                throw new AppException("Can't Book Parking! Missing Data");

            parking.UserId = userId;

            int occupiedCapacityInt = Int32.Parse(space.OccupiedCapacity);
            int totalCapacityInt = Int32.Parse(space.TotalCapacity);

            if (!(occupiedCapacityInt + 1 <= totalCapacityInt))
            {
                throw new AppException("Can't Book Parking!");
            }

            parking.isBooked = true;

            var oldParking = _context.Parkings.Count(p => p.UserId == userId && p.isBooked == true);

            if (oldParking > 0)
            {
                throw new AppException("Already Booked Parking!");
            }

            if (!(parking.withCleaningService && garage.hasCleaningService))
            {
                parking.withCleaningService = false;
            }

            _context.Parkings.Add(parking);
            _context.SaveChanges();

            _garageService.PlusGarageCapacity(garage);
            _spaceService.PlusSpaceCapacity(space);

            return parking;
        }

        public void CheckIn(int userId)
        {
            var parking = _context.Parkings.SingleOrDefault(p => p.UserId == userId);

            if (parking == null)
            {
                throw new AppException("Can't Find Your Parking");
            }

            if (parking.isActive)
            {
                throw new AppException("Already Checked In!");
            }

            parking.isActive = true;
            parking.UserCheckIn = DateTime.Now;
            _context.Parkings.Update(parking);
            _context.SaveChanges();
        }

        public void CheckOut(int userId)
        {
            var parking = _context.Parkings.SingleOrDefault(p => p.UserId == userId);

            if (parking == null)
            {
                throw new AppException("Can't Find Your Parking");
            }
            parking.isActive = false;
            parking.UserCheckOut = DateTime.Now;
            var space = _context.Spaces.Find(parking.SpaceId);
            var garage = _context.Garages.Find(parking.GarageId);
            TimeSpan interval = parking.UserCheckOut - parking.UserCheckIn;
            int parkingRate = Int32.Parse(garage.ParkingRate);
            double parkingCost = (parkingRate * interval.TotalMinutes) / 60;

            int cleaningCost = 0;

            if (parking.withCleaningService)
            {
                cleaningCost = Int32.Parse(garage.CleaningRate);
            }


            double Cost = parkingCost + cleaningCost;

            Console.WriteLine("\n" + parkingCost + cleaningCost + "\n");

            parking.CleaningCost = cleaningCost.ToString();
            parking.ParkingCost = parkingCost.ToString();
            parking.Cost = Cost.ToString();
            _context.Parkings.Update(parking);
            _context.SaveChanges();
            ParkingHistory parkingHistory = new ParkingHistory();
            parkingHistory.UserId = parking.UserId;
            parkingHistory.VehicleNumber = parking.VehicleNumber;
            parkingHistory.DriverName = parking.DriverName;
            parkingHistory.SpaceId = parking.SpaceId;
            parkingHistory.GarageId = parking.GarageId;
            parkingHistory.UserCheckIn = parking.UserCheckIn;
            parkingHistory.UserCheckOut = parking.UserCheckOut;
            parkingHistory.withCleaningService = parking.withCleaningService;
            parkingHistory.Cost = parking.Cost;
            parkingHistory.ParkingCost = parking.ParkingCost;
            parkingHistory.CleaningCost = parking.CleaningCost;
            parkingHistory.interval = ToReadableString(interval);
            _context.ParkingHistories.Add(parkingHistory);
            _context.SaveChanges();

            _context.Parkings.Remove(parking);
            _context.SaveChanges();

            _spaceService.MinusSpaceCapacity(space);
            _garageService.MinusGarageCapacity(garage);
        }

        public Parking GetByUserId(int userId)
        {
            var parking = _context.Parkings.SingleOrDefault(p => p.UserId == userId);
            return parking;
        }

        public ParkingHistory GetReceipt(int userId)
        {
            var parkingHistory = _context.ParkingHistories.Where(ph => ph.UserId == userId).OrderByDescending(ph => ph.UserCheckOut).FirstOrDefault();
            return parkingHistory;
        }

        public IEnumerable<Parking> GetActiveParkings()
        {
            var parkings = _context.Parkings.Where(p => p.isActive == true);
            return parkings;
        }

        public IEnumerable<Parking> GetInactiveParkings()
        {
            var parkings = _context.Parkings.Where(p => p.isActive == false && p.UserCheckOut >= DateTime.Now);
            return parkings;
        }

        public IEnumerable<ParkingHistory> GetParkingHistories()
        {
            return _context.ParkingHistories.OrderByDescending(ph => ph.UserCheckOut);
        }

        public IEnumerable<ParkingHistory> GetUserParkingHistories(int userId)
        {
            var parkingHistories = _context.ParkingHistories.Where(ph => ph.UserId == userId).OrderByDescending(ph => ph.UserCheckOut);
            return parkingHistories;
        }

        // Helper Methods for Interval Storage

        private string ToReadableString(TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? string.Empty : "s") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }

    }
}
