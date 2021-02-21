using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Users
{
    public class UpdateModel
    {
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Phone { get; set; }
        [MinLength(6)]
        public string Password { get; set; }
    }
}