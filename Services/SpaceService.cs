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

            var spaceCount = _context.Spaces.Count(s => s.GarageId == garage.Id);
            spaceCount += 1;

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
            allocationManager.Space = spaceParam.ToString();
            allocationManager.Spaces.Add(space);
            _context.AllocationManagers.Update(allocationManager);
            _context.SaveChanges();
            return space;
        }

        public IEnumerable<Space> GetSpaces()
        {
            return _context.Spaces;
        }

        public Space GetSpace(int id)
        {
            return _context.Spaces.Find(id);
        }

        public void Update(int userId, Space spaceParam)
        {
            var space = _context.Spaces.Find(spaceParam.Id);
            int allocationManagerId = _userService.GetParkingManagerId(userId);
            var allocationManager = _context.AllocationManagers.Find(allocationManagerId);
            if (space.AllocationManagerId != allocationManager.Id)
                throw new AppException("Can't Update That Space");
            if (!string.IsNullOrWhiteSpace(spaceParam.Code))
            {
                space.Code = spaceParam.Code;
            }
            if (!string.IsNullOrWhiteSpace(spaceParam.TotalCapacity))
            {
                space.TotalCapacity = spaceParam.TotalCapacity;
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
            int allocationManagerId = _userService.GetParkingManagerId(userId);
            var allocationManager = _context.AllocationManagers.Find(allocationManagerId);
            if (space.AllocationManagerId != allocationManager.Id)
                throw new AppException("Can't Delete That Space");
            _context.Spaces.Remove(space);
            _context.SaveChanges();
        }
    }
}
