using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetStarter.Models
{
    public class AspNetUserLogins
    {
        [Key, Column(Order = 1)]
        [MaxLength(128)]
        public string LoginProvider { get; set; }
        [Key, Column(Order = 2)]
        [MaxLength(128)]
        public string ProviderKey { get; set; }
        [Key, Column(Order = 3)]
        [MaxLength(128)]
        public string UserId { get; set; }
    }
}