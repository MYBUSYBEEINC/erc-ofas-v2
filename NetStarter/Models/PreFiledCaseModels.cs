using NetStarter.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace NetStarter.Models
{
    public class PreFiledCases
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        public string RequestSubject { get; set; }
        public string UserId { get; set; }
        public string CaseTypeId { get; set; }
        public string CaseNatureId { get; set; }
        public string FileCaseStatusId { get; set; }
        public string Remarks { get; set; }
        public string ApprovalRemarks { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class PreFiledCaseViewModel
    {
        public string Id { get; set; }
        [Display(Name = "RequestSubject", ResourceType = typeof(Resource))]
        public string RequestSubject { get; set; }
        [Display(Name = "Username", ResourceType = typeof(Resource))]
        public string UserId { get; set; }
        [Display(Name = "CaseType", ResourceType = typeof(Resource))]
        public string CaseTypeId { get; set; }
        [Display(Name = "CaseNature", ResourceType = typeof(Resource))]
        public string CaseNatureId { get; set; }
        [Display(Name = "FileCaseStatus", ResourceType = typeof(Resource))]
        public string FileCaseStatusId { get; set; }
        public string Remarks { get; set; }
        [Display(Name = "ApproverRemarks", ResourceType = typeof(Resource))]
        public string ApprovalRemarks { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool SystemDefault { get; set; }
        public CreatedAndModifiedViewModel CreatedAndModified { get; set; }
        public List<SelectListItem> CaseTypeSelectList { get; set; }
        public List<SelectListItem> CaseNatureSelectList { get; set; }
        public List<SelectListItem> FileCaseStatusSelectList { get; set; }
        public List<HttpPostedFileBase> Documents { get; set; }
        public string DocumentFileName { get; set; }
        public HttpPostedFileBase[] Files { get; set; }
        public List<PreFiledAttachment> Attachments { get; set; }
    }

    public class PreFiledCaseListing
    {
        public List<PreFiledCaseViewModel> Listing { get; set; }
    }
}