using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.AllocationManagers;
using WebApi.Models.ParkingManagers;
using WebApi.Models.Users;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public UsersController(
            IUserService userService,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Email, model.Password);

            if (user == null)
                return BadRequest(new { message = "Email or Password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info and authentication token
            return Ok(new
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Created = user.Created,
                isVerfied = user.IsVerified,
                Token = tokenString
            });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            // map model to entity
            var user = _mapper.Map<User>(model);

            try
            {
                // create user
                _userService.Create(user, model.Password, Request.Headers["origin"]);
                return Ok(new { message = "✓ Registration Success, Please Check Your Email for Verification!" });
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("verify-email")]
        public IActionResult VerifyEmail([FromBody] VerifyEmailModel model)
        {
            _userService.VerifyEmail(model.Token);
            return Ok(new { message = "✓ Verification Success, You Can Now Login!" });
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswordModel model)
        {
            _userService.ForgotPassword(model, Request.Headers["origin"]);
            return Ok(new { message = "✓ Password Reset Token Success, Please Check Your Email for Password Reset Instructions!" });
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public IActionResult ResetPassword(ResetPasswordModel model)
        {
            _userService.ResetPassword(model);
            return Ok(new { message = "✓ Password Reset Success, You Can Login In with New Password!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _userService.GetUsers();
            var model = _mapper.Map<IList<UserModel>>(users);
            return Ok(model);
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            var context = HttpContext.User.Identity;
            int userId = int.Parse(context.Name);
            if (id != userId && _userService.getRole(userId) != "Admin")
                return Unauthorized(new { message = "Unauthorized" });
            var user = _userService.GetUserById(id);
            var model = _mapper.Map<UserModel>(user);
            return Ok(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("allocationmanagers")]
        public IActionResult GetAllocationManagers()
        {
            var allocationManagers = _userService.GetAllocationManagers();
            var model = _mapper.Map<IList<AllocationManagerModel>>(allocationManagers);
            return Ok(model);
        }

        [Authorize]
        [HttpGet("allocationmanagers/{id}")]
        public IActionResult GetAllocationManagerById(int id)
        {
            var context = HttpContext.User.Identity;
            int userId = int.Parse(context.Name);
            if (id != _userService.GetAllocationManagerId(userId) && _userService.getRole(userId) != "Admin")
                return Unauthorized(new { message = "Unauthorized" });
            var allocationManager = _userService.GetAllocationManagerById(id);
            var model = _mapper.Map<AllocationManagerModel>(allocationManager);
            return Ok(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("parkingmanagers")]
        public IActionResult GetParkingManagers()
        {
            var parkingManagers = _userService.GetParkingManagers();
            var model = _mapper.Map<IList<ParkingManagerModel>>(parkingManagers);
            return Ok(model);
        }

        [Authorize(Roles ="Admin, ParkingManager")]
        [HttpGet("parkingmanagers/{id}")]
        public IActionResult GetParkingManagerById(int id)
        {
            var context = HttpContext.User.Identity;
            int userId = int.Parse(context.Name);
            if (id != userId)
                return Unauthorized(new { message = "Unauthorized" });

            var parkingManager = _userService.GetParkingManagerById(_userService.GetParkingManagerId(id));
            var model = _mapper.Map<ParkingManagerModel>(parkingManager);
            return Ok(model);
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateModel model)
        {
            var context = HttpContext.User.Identity;
            int userId = int.Parse(context.Name);
            if (id != userId && _userService.getRole(userId) != "Admin")
                return Unauthorized(new { message = "Unauthorized" });
            // map model to entity and set id
            var user = _mapper.Map<User>(model);
            user.Id = id;

            try
            {
                // update user 
                _userService.Update(user, model.Password);
                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var context = HttpContext.User.Identity;
            int userId = int.Parse(context.Name);
            if (id != userId && _userService.getRole(userId) != "Admin")
                return Unauthorized(new { message = "Unauthorized" });
            _userService.Delete(id);
            return Ok();
        }
    }
}
