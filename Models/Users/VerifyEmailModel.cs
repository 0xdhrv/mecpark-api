using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Users
{
    public class VerifyEmailModel
    {
        [Required]
        public string Token { get; set; }
    }
}
