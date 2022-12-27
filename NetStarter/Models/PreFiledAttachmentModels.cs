using NetStarter.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace NetStarter.Models
{
    public class PreFiledAttachment
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        public string PreFiledCaseId { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public string UniqueFileName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}