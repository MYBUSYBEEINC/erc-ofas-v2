using Newtonsoft.Json;
using System;

namespace NetStarter.Models
{
    public class Hearing
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public int? HearingType { get; set; }

        public DateTime? Schedule { get; set; }

        public string MeetingLink { get; set; }

        public int? HearingStatus { get; set; }
        public string MeetingPassword { get; set; }

        public int CreatedBy { get; set; }

        [JsonIgnore]
        public DateTime DateCreated { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }
    }
}