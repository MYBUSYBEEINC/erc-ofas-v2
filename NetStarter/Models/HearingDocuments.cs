using System;

namespace NetStarter.Models
{
    public class HearingDocuments
    {        
        public int Id { get; set; }
        
        public string FileName { get; set; }

        public string FilePath { get; set; }

        public int? DocumentType { get; set; }
        
        public int HearingId { get; set; }
        
        public int CreatedBy { get; set; }
        
        public DateTime DateCreated { get; set; }
        
        public int? UpdatedBy { get; set; }
        
        public DateTime? DateUpdated { get; set; }
    }
}