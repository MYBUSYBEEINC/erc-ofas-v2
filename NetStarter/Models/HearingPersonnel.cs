using System;

namespace NetStarter.Models
{
    public class HearingPersonnel
    {
        
        public int Id { get; set; }

        
        public int HearingId { get; set; }

        
        public string UserId { get; set; }

        
        public int CreatedBy { get; set; }

        
        public DateTime DateCreated { get; set; }

        
        public int? UpdatedBy { get; set; }

        
        public DateTime? DateUpdated { get; set; }
    }
}