using System;
using System.Collections.Generic;

namespace WebApi.Entities
{
    public class AllocationManager
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Phone { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public int GarageId { get; set; }
        public string Space { get; set; }
        public List<Space> Spaces { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public DateTime? Verified { get; set; }
        public DateTime? PasswordReset { get; set; }
        public bool IsVerified { get; set; }
    }
}
