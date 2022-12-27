using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetStarter.Models
{
    public class HearingType
    {
        
        public byte Id { get; set; }

        
        public string Description { get; set; }

        
        public string Name { get; set; }

        
        public int CreatedBy { get; set; }

        
        public DateTime DateCreated { get; set; }

        
        public int? UpdatedBy { get; set; }

        
        public DateTime? DateUpdated { get; set; }
    }
}