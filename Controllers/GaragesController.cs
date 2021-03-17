using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Garages;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class GaragesController : ControllerBase
    {
        private IGarageService _garageService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public GaragesController(
            IGarageService garageService,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _garageService = garageService;
            _mapper = mapper;
            _appSettings = appSettings.Value;

        }

        [Authorize(Roles = "Admin,ParkingManager")]
        [HttpPost("create")]
        public IActionResult Create([FromBody] CreateGarageModel model)
        {
            var garage = _mapper.Map<Garage>(model);
            var context = HttpContext.User.Identity;
            int id = int.Parse(context.Name);
            try
            {
                _garageService.Create(garage, id);
                return Ok(new { message = "✓ Garage Creation Success" });
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin, AllocationManager, User")]
        [HttpGet]
        public IActionResult GetGarages()
        {
            var garages = _garageService.GetGarages();
            var model = _mapper.Map<IList<Garage>>(garages);
            return Ok(model);
        }

        [Authorize(Roles = "Admin, ParkingManager")]
        [HttpGet("{id}")]
        public IActionResult GetGarage(int id)
        {
            var garage = _garageService.GetGarage(id);
            var model = _mapper.Map<Garage>(garage);
            return Ok(model);
        }

        [Authorize(Roles = "Admin, ParkingManager")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateGarageModel model)
        {
            var context = HttpContext.User.Identity;
            var userId = int.Parse(context.Name);
            var garage = _mapper.Map<Garage>(model);
            garage.Id = id;

            try
            {
                _garageService.Update(userId, garage);
                return Ok();
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin, ParkingManager")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var context = HttpContext.User.Identity;
            var userId = int.Parse(context.Name);
            _garageService.Delete(userId, id);
            return Ok();
        }
    }
}
