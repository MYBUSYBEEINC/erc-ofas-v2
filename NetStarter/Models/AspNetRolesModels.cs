using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NetStarter.Resources;
using System.Linq;
using System.Web;

namespace NetStarter.Models
{
    public class AspNetRoles
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(128)]
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        [MaxLength(128)]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool SystemDefault { get; set; }
        public string IsoUtcCreatedOn { get; set; }
        public string IsoUtcModifiedOn { get; set; }
    }

    public class SystemRoleViewModel
    {
        public string Id { get; set; }
        [Required]
        [MaxLength(256)]
        [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessageResourceName = "OnlyLettersAndNumbers", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "RoleName", ResourceType = typeof(Resource))]
        public string Name { get; set; }
        [Display(Name = "CreatedBy", ResourceType = typeof(Resource))]
        public string CreatedBy { get; set; }
        [Display(Name = "CreatedOn", ResourceType = typeof(Resource))]
        public DateTime? CreatedOn { get; set; }
        public string IsoUtcCreatedOn { get; set; }
        [Display(Name = "ModifiedBy", ResourceType = typeof(Resource))]
        public string ModifiedBy { get; set; }
        [Display(Name = "ModifiedOn", ResourceType = typeof(Resource))]
        public DateTime? ModifiedOn { get; set; }
        public string IsoUtcModifiedOn { get; set; }
        public bool SystemDefault { get; set; }
        public CreatedAndModifiedViewModel CreatedAndModified { get; set; }
        [Display(Name = "Dashboard", ResourceType = typeof(Resource))]
        public Permission DashboardPermission { get; set; }
        [Display(Name = "PreFiledCases", ResourceType = typeof(Resource))]
        public Permission PreFiledCasePermission { get; set; }
        [Display(Name = "FiledCases", ResourceType = typeof(Resource))]
        public Permission FiledCasePermission { get; set; }
        [Display(Name = "Hearings", ResourceType = typeof(Resource))]
        public Permission HearingPermission { get; set; }
        [Display(Name = "InitiatoryPleadings", ResourceType = typeof(Resource))]
        public Permission InitiatoryPleadingPermission { get; set; }
        [Display(Name = "PleadingsWithCaseNumbers", ResourceType = typeof(Resource))]
        public Permission PleadingWithCaseNumberPermission { get; set; }
        [Display(Name = "OtherLettersCorrespondences", ResourceType = typeof(Resource))]
        public Permission OtherLetterCorrespondensePermission { get; set; }
        [Display(Name = "DisputeResolution", ResourceType = typeof(Resource))]
        public Permission DisputeResolutionPermission { get; set; }
        [Display(Name = "Transactions", ResourceType = typeof(Resource))]
        public Permission TransactionPermission { get; set; }
        [Display(Name = "Payment", ResourceType = typeof(Resource))]
        public Permission PaymentPermission { get; set; }
        [Display(Name = "UserStatus", ResourceType = typeof(Resource))]
        public Permission UserStatusPermission { get; set; }
        [Display(Name = "UserTypes", ResourceType = typeof(Resource))]
        public Permission UserTypePermission { get; set; }
        [Display(Name = "CaseTypes", ResourceType = typeof(Resource))]
        public Permission CaseTypePermission { get; set; }
        [Display(Name = "CaseNatures", ResourceType = typeof(Resource))]
        public Permission CaseNaturePermission { get; set; }

        [Display(Name = "UserAttachmentType", ResourceType = typeof(Resource))]
        public Permission UserAttachmentTypePermission { get; set; }
        [Display(Name = "RoleManagement", ResourceType = typeof(Resource))]
        public Permission RoleManagementPermission { get; set; }
        [Display(Name = "UserManagement", ResourceType = typeof(Resource))]
        public Permission UserManagementPermission { get; set; }
        [Display(Name = "LoginHistory", ResourceType = typeof(Resource))]
        public Permission LoginHistoryPermission { get; set; }
    }

    public class SystemRoleListing
    {
        public List<SystemRoleViewModel> Listing { get; set; }
        public string IsoLoginDateTime { get; set; }
        public string FormattedDateTime { get; set; }
    }

    public class Permission
    {
        [Display(Name = "ViewList", ResourceType = typeof(Resource))]
        public ViewPermission ViewPermission { get; set; }
        [Display(Name = "Add", ResourceType = typeof(Resource))]
        public AddPermission AddPermission { get; set; }
        [Display(Name = "Edit", ResourceType = typeof(Resource))]
        public EditPermission EditPermission { get; set; }
        [Display(Name = "Delete", ResourceType = typeof(Resource))]
        public DeletePermission DeletePermission { get; set; }
    }

    public class ViewPermission
    {
        public string Type { get; set; }
        public bool IsSelected { get; set; }
    }
    public class AddPermission
    {
        public string Type { get; set; }
        public bool IsSelected { get; set; }
    }
    public class EditPermission
    {
        public string Type { get; set; }
        public bool IsSelected { get; set; }
    }
    public class DeletePermission
    {
        public string Type { get; set; }
        public bool IsSelected { get; set; }
    }
}