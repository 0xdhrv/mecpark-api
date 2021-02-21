using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.ParkingHistories;
using WebApi.Models.Parkings;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParkingsController : ControllerBase
    {
        private IParkingService _parkingService;
        private IMapper _mapper;

        public ParkingsController(
            IParkingService parkingService,
            IMapper mapper)
        {
            _parkingService = parkingService;
            _mapper = mapper;
        }

        [Authorize(Roles = "User")]
        [HttpPost("book")]
        public IActionResult Book([FromBody] BookParkingModel model)
        {
            var parking = _mapper.Map<Parking>(model);
            var context = HttpContext.User.Identity;
            int id = int.Parse(context.Name);

            try
            {
                _parkingService.Book(parking, id);
                return Ok(new { message = "✓ Parking Booked" });
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "User")]
        [HttpPost("checkin")]
        public IActionResult CheckIn()
        {
            var context = HttpContext.User.Identity;
            int id = int.Parse(context.Name);

            try
            {
                _parkingService.CheckIn(id);
                return Ok(new { message = "✓ Parking CheckIn" });
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "User")]
        [HttpPost("checkout")]
        public IActionResult CheckOut()
        {
            var context = HttpContext.User.Identity;
            int id = int.Parse(context.Name);

            try
            {
                _parkingService.CheckOut(id);
                return Ok(new { message = "✓ Parking CheckOut" });
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("active")]
        public IActionResult GetActiveParkings()
        {
            var parkings = _parkingService.GetActiveParkings();
            var model = _mapper.Map<IList<Parking>>(parkings);
            return Ok(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("inactive")]
        public IActionResult GetInActiveParkings()
        {
            var parkings = _parkingService.GetInactiveParkings();
            var model = _mapper.Map<IList<Parking>>(parkings);
            return Ok(model);
        }

        [Authorize(Roles = "User")]
        [HttpGet("user-history")]
        public IActionResult GetUserParkingHistories()
        {
            var context = HttpContext.User.Identity;
            int id = int.Parse(context.Name);
            var parkingHistories = _parkingService.GetUserParkingHistories(id);
            var model = _mapper.Map<IList<ReceiptModel>>(parkingHistories);
            return Ok(model);
        }

        [Authorize(Roles = "User")]
        [HttpGet("receipt")]
        public IActionResult GetReceipt()
        {
            var context = HttpContext.User.Identity;
            int id = int.Parse(context.Name);
            var parkingHistory = _parkingService.GetReceipt(id);
            var model = _mapper.Map<ReceiptModel>(parkingHistory);
            return Ok(model);
        }
    }
}
