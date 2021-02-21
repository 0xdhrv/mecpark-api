using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Users
{
    public class ResetPasswordModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
