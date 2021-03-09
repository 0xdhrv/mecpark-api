using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface ISpaceService
    {
        Space Create(Space space, int allocationManagerId);
        IEnumerable<Space> GetSpaces();
        Space GetSpace(int id);
        IEnumerable<Space> GetSpacesByGarage(int id);
        void Update(int userId, Space space);
        void PlusSpaceCapacity(Space space);
        void MinusSpaceCapacity(Space space);
        void Delete(int userId, int id);
    }
    public class SpaceService : ISpaceService
    {
        private readonly DataContext _context;
        private IUserService _userService;
        public SpaceService(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }
        public Space Create(Space space, int userId)
        {
            var user = _context.Users.Find(userId);
            var allocationManager = _context.AllocationManagers.Single(x => x.Email == user.Email);
            if (allocationManager == null)
            {
                throw new AppException("Not a allocation manager");
            }

            allocationManager.GarageId = space.GarageId;
            _context.Update(allocationManager);
            _context.SaveChanges();

            var garage = _context.Garages.Single(x => x.Id == allocationManager.GarageId);
            var garageSpace = garage.Space;
            int garageSpaceInt = int.Parse(garageSpace);

            var garageCapacity = garage.TotalCapacity;
            int garageCapacityInt = int.Parse(garageCapacity);

            var spaceCount = allocationManager.Space;
            int spaceCountInt = int.Parse(spaceCount);
            spaceCountInt += 1;

            var spaceCapacity = space.TotalCapacity;
            int spaceCapacityInt = int.Parse(spaceCapacity);

            space.AllocationManager = allocationManager;
            space.AllocationManagerId = allocationManager.Id;
            space.GarageId = allocationManager.GarageId;
            space.OccupiedCapacity = "0";
            _context.Spaces.Add(space);
            _context.SaveChanges();
            int spaceParam = int.Parse(garage.Space);
            spaceParam += 1;
            garage.Space = spaceParam.ToString();
            int garageTotalCapacityParam = int.Parse(garage.TotalCapacity);
            int spaceTotalCapacityParam = int.Parse(space.TotalCapacity);
            garageTotalCapacityParam += spaceTotalCapacityParam;
            garage.Spaces.Add(space);
            garage.TotalCapacity = garageTotalCapacityParam.ToString();
            _context.Garages.Update(garage);
            _context.SaveChanges();
            allocationManager.Space = spaceCountInt.ToString();
            allocationManager.Spaces.Add(space);
            _context.AllocationManagers.Update(allocationManager);
            _context.SaveChanges();
            return space;
        }

        public IEnumerable<Space> GetSpaces()
        {
            return _context.Spaces;
        }

        public IEnumerable<Space> GetSpacesByGarage(int id)
        {
            var garage = _context.Garages.Find(id);
            var spaces = _context.Spaces.Where(x => x.GarageId == id);
            return spaces;
        }

        public Space GetSpace(int id)
        {
            return _context.Spaces.Find(id);
        }

        public void Update(int userId, Space spaceParam)
        {
            var space = _context.Spaces.Find(spaceParam.Id);
            int allocationManagerId = _userService.GetAllocationManagerId(userId);
            var allocationManager = _context.AllocationManagers.Find(allocationManagerId);
            var garage = _context.Garages.Single(x => x.Id == space.GarageId);
            var garageCapacity = garage.TotalCapacity;
            int garageCapacityInt = int.Parse(garageCapacity);
            if (space.AllocationManagerId != allocationManager.Id)
                throw new AppException("Can't Update That Space");
            if (!string.IsNullOrWhiteSpace(spaceParam.Code))
            {
                space.Code = spaceParam.Code;
            }
            if (!string.IsNullOrWhiteSpace(spaceParam.TotalCapacity))
            {
                var spaceCapacityInt = int.Parse(space.TotalCapacity);
                garageCapacityInt -= spaceCapacityInt;
                var spaceParamCapacityInt = int.Parse(spaceParam.TotalCapacity);
                garageCapacityInt += spaceParamCapacityInt;
                space.TotalCapacity = spaceParam.TotalCapacity;
                garage.TotalCapacity = garageCapacityInt.ToString();
                _context.Garages.Update(garage);
                _context.SaveChanges();
            }
            _context.Spaces.Update(space);
            _context.SaveChanges();
        }

        public void PlusSpaceCapacity(Space spaceParam)
        {
            var space = _context.Spaces.Find(spaceParam.Id);
            int occupiedCapacity = int.Parse(space.OccupiedCapacity);
            occupiedCapacity += 1;
            space.OccupiedCapacity = occupiedCapacity.ToString();
            _context.Spaces.Update(space);
            _context.SaveChanges();
        }

        public void MinusSpaceCapacity(Space spaceParam)
        {
            var space = _context.Spaces.Find(spaceParam.Id);
            int occupiedCapacity = int.Parse(space.OccupiedCapacity);
            occupiedCapacity -= 1;
            space.OccupiedCapacity = occupiedCapacity.ToString();
            _context.Spaces.Update(space);
            _context.SaveChanges();
        }

        public void Delete(int userId, int id)
        {
            var space = _context.Spaces.Find(id);
            int allocationManagerId = _userService.GetAllocationManagerId(userId);
            var allocationManager = _context.AllocationManagers.Find(allocationManagerId);
            if (space.AllocationManagerId != allocationManager.Id)
                throw new AppException("Can't Delete That Space");
            if (space == null)
            {
                throw new AppException("");
            }
            var garage = _context.Garages.Single(x => x.Id == space.GarageId);
            var garageCapacity = garage.TotalCapacity;
            var spaceCapacity = space.TotalCapacity;
            int garageCapacityInt = int.Parse(garageCapacity);
            garageCapacityInt -= int.Parse(spaceCapacity);
            garage.TotalCapacity = garageCapacityInt.ToString();
            var spaceCount = garage.Space;
            int spaceCountInt = int.Parse(spaceCount);
            spaceCountInt -= 1;
            garage.Space = spaceCountInt.ToString();
            _context.Garages.Update(garage);
            _context.SaveChanges();
            _context.Spaces.Remove(space);
            _context.SaveChanges();
            Console.WriteLine("\n allocationManager.Space" + allocationManager.Space + "\n");
            int spaceParamInt = int.Parse(allocationManager.Space);
            spaceParamInt -= 1;
            allocationManager.Space = spaceParamInt.ToString();
            Console.WriteLine("\n allocationManager.Space" + allocationManager.Space + "\n");
            _context.AllocationManagers.Update(allocationManager);
            _context.SaveChanges();
            Console.WriteLine("\n allocationManager.Space" + allocationManager.Space + "\n");
        }
    }
}
