using NetStarter.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace NetStarter.Models
{
    public class PreRegistration
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string RERTypeId { get; set; }
        public string RERClassificationId { get; set; }
        public string EmailAddress { get; set; }
        public string MobileNumber { get; set; }
        public string TempUsername { get; set; }
        public string TempPassword { get; set; }
        public string RegistrationStatusId { get; set; }
        public string OneTimePassword { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public class PreRegistrationViewModel
    {
        public string Id { get; set; }
        [Display(Name = "LastName", ResourceType = typeof(Resource))]
        public string LastName { get; set; }
        [Display(Name = "FirstName", ResourceType = typeof(Resource))]
        public string FirstName { get; set; }
        [Display(Name = "RERType", ResourceType = typeof(Resource))]
        public string RERTypeId { get; set; }
        [Display(Name = "RERClassification", ResourceType = typeof(Resource))]
        public string RERClassificationId { get; set; }
        [Display(Name = "EmailAddress", ResourceType = typeof(Resource))]
        public string EmailAddress { get; set; }
        [Display(Name = "MobileNumber", ResourceType = typeof(Resource))]
        public string MobileNumber { get; set; }
        public string RegistrationStatusId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public CreatedAndModifiedViewModel CreatedAndModified { get; set; }
        public List<SelectListItem> RERTypeList { get; set; }
        public List<SelectListItem> CaseNatureSelectList { get; set; }
        public List<SelectListItem> FileCaseStatusSelectList { get; set; }
    }
}