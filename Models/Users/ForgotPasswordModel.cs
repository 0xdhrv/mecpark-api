using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Users
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
