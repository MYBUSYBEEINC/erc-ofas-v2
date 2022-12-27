using System;
using System.ComponentModel.DataAnnotations;

namespace NetStarter.Models
{
    public class AspNetUsers
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(256)]
        public string UserName { get; set; }
        [MaxLength(256)]
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
    }
}



