using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using NetStarter.Models;
using NetStarter.Resources;

namespace NetStarter.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {
        private DefaultDBContext db = new DefaultDBContext();
        private GeneralController general = new GeneralController();

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.RoleManagement, "true", "", "", "")]
        public ViewResult Index()
        {
            return View();
        }

        public ActionResult GetPartialViewRoles()
        {
            SystemRoleListing theListing = new SystemRoleListing();
            theListing.Listing = ReadSystemRoles();
            return PartialView("~/Views/Role/_MainList.cshtml", theListing);
        }

        public List<SystemRoleViewModel> ReadSystemRoles()
        {
            List<SystemRoleViewModel> list = new List<SystemRoleViewModel>();
            list = (from t1 in db.AspNetRoles
                    orderby t1.Name
                    select new SystemRoleViewModel
                    {
                        Id = t1.Id,
                        Name = t1.Name,
                        SystemDefault = t1.SystemDefault
                    }).ToList();
            return list;
        }

        public SystemRoleViewModel GetViewModel(string Id, string type)
        {
            SystemRoleViewModel model = new SystemRoleViewModel();
            AspNetRoles roles = db.AspNetRoles.Where(a => a.Id == Id).FirstOrDefault();
            model.Id = roles.Id;
            model.Name = roles.Name;
            model.CreatedBy = roles.CreatedBy;
            model.CreatedOn = roles.CreatedOn;
            model.ModifiedBy = roles.ModifiedBy;
            model.ModifiedOn = roles.ModifiedOn;
            model.IsoUtcCreatedOn = roles.IsoUtcCreatedOn;
            model.IsoUtcModifiedOn = roles.IsoUtcModifiedOn;
            model.SystemDefault = roles.SystemDefault;
            if (type == "View")
            {
                model.CreatedAndModified = general.GetCreatedAndModified(model.CreatedBy, model.IsoUtcCreatedOn, model.ModifiedBy, model.IsoUtcModifiedOn);
            }

            string dashboardModuleId = db.Modules.Where(a => a.Name == "Dashboard").Select(a => a.Id).FirstOrDefault();
            model.DashboardPermission = GetPermission(roles.Id, dashboardModuleId);

            string preFiledCaseModuleId = db.Modules.Where(a => a.Name == "Pre-Filed Cases").Select(a => a.Id).FirstOrDefault();
            model.PreFiledCasePermission = GetPermission(roles.Id, preFiledCaseModuleId);

            string filedCaseModuleId = db.Modules.Where(a => a.Name == "Filed Cases").Select(a => a.Id).FirstOrDefault();
            model.FiledCasePermission = GetPermission(roles.Id, filedCaseModuleId);

            string hearingModuleId = db.Modules.Where(a => a.Name == "Hearings").Select(a => a.Id).FirstOrDefault();
            model.HearingPermission = GetPermission(roles.Id, hearingModuleId);

            string initiatoryPleadingModuleId = db.Modules.Where(a => a.Name == "Initiatory Pleadings").Select(a => a.Id).FirstOrDefault();
            model.InitiatoryPleadingPermission = GetPermission(roles.Id, initiatoryPleadingModuleId);

            string pleadingWithCaseNumberModuleId = db.Modules.Where(a => a.Name == "Pleadings w/ Case Numbers").Select(a => a.Id).FirstOrDefault();
            model.PleadingWithCaseNumberPermission = GetPermission(roles.Id, pleadingWithCaseNumberModuleId);

            string otherLetterCorrespondenseModuleId = db.Modules.Where(a => a.Name == "Other Letters / Correspondences").Select(a => a.Id).FirstOrDefault();
            model.OtherLetterCorrespondensePermission = GetPermission(roles.Id, otherLetterCorrespondenseModuleId);

            string disputeResolutionModuleId = db.Modules.Where(a => a.Name == "Dispute Resolution").Select(a => a.Id).FirstOrDefault();
            model.DisputeResolutionPermission = GetPermission(roles.Id, disputeResolutionModuleId);

            string transactionModuleId = db.Modules.Where(a => a.Name == "Transactions").Select(a => a.Id).FirstOrDefault();
            model.TransactionPermission = GetPermission(roles.Id, transactionModuleId);

            string paymentModuleId = db.Modules.Where(a => a.Name == "Payment").Select(a => a.Id).FirstOrDefault();
            model.PaymentPermission = GetPermission(roles.Id, paymentModuleId);

            string userStatusModuleId = db.Modules.Where(a => a.Name == "User Status").Select(a => a.Id).FirstOrDefault();
            model.UserStatusPermission = GetPermission(roles.Id, userStatusModuleId);

            string userTypeModuleId = db.Modules.Where(a => a.Name == "User Types").Select(a => a.Id).FirstOrDefault();
            model.UserTypePermission = GetPermission(roles.Id, userTypeModuleId);

            string caseTypeModuleId = db.Modules.Where(a => a.Name == "Case Types").Select(a => a.Id).FirstOrDefault();
            model.CaseTypePermission = GetPermission(roles.Id, caseTypeModuleId);

            string caseNatureModuleId = db.Modules.Where(a => a.Name == "Case Natures").Select(a => a.Id).FirstOrDefault();
            model.CaseNaturePermission = GetPermission(roles.Id, caseNatureModuleId);

            string userAttachmentTypeModuleId = db.Modules.Where(a => a.Name == "User Attachment Type").Select(a => a.Id).FirstOrDefault();
            model.UserAttachmentTypePermission = GetPermission(roles.Id, userAttachmentTypeModuleId);

            string roleManagementModuleId = db.Modules.Where(a => a.Name == "Role Management").Select(a => a.Id).FirstOrDefault();
            model.RoleManagementPermission = GetPermission(roles.Id, roleManagementModuleId);

            string userManagementModuleId = db.Modules.Where(a => a.Name == "User Management").Select(a => a.Id).FirstOrDefault();
            model.UserManagementPermission = GetPermission(roles.Id, userManagementModuleId);

            string loginHistoryModuleId = db.Modules.Where(a => a.Name == "Login History").Select(a => a.Id).FirstOrDefault();
            model.LoginHistoryPermission = GetPermission(roles.Id, loginHistoryModuleId);

            return model;
        }

        public Permission GetPermission(string roleId, string moduleId)
        {
            Permission permission = new Permission();
            permission.ViewPermission = new ViewPermission();
            permission.ViewPermission.Type = "View";
            permission.AddPermission = new AddPermission();
            permission.AddPermission.Type = "Add";
            permission.EditPermission = new EditPermission();
            permission.EditPermission.Type = "Edit";
            permission.DeletePermission = new DeletePermission();
            permission.DeletePermission.Type = "Delete";

            RoleModulePermission roleModulePermission = db.RoleModulePermissions.Where(a => a.RoleId == roleId && a.ModuleId == moduleId).FirstOrDefault();
            if (roleModulePermission != null)
            {
                permission.ViewPermission.IsSelected = roleModulePermission.ViewRight;
                permission.AddPermission.IsSelected = roleModulePermission.AddRight;
                permission.EditPermission.IsSelected = roleModulePermission.EditRight;
                permission.DeletePermission.IsSelected = roleModulePermission.DeleteRight;
            }
            else
            {
                permission.ViewPermission.IsSelected = false;
                permission.AddPermission.IsSelected = false;
                permission.EditPermission.IsSelected = false;
                permission.DeletePermission.IsSelected = false;
            }
            return permission;
        }

        public ActionResult GetPartialViewUserInRole(string id)
        {
            UserInRoleListing theListing = new UserInRoleListing();
            theListing.Listing = ReadUserInRole(id);
            string roleName = db.AspNetRoles.Where(a => a.Id == id).Select(a => a.Name).FirstOrDefault();
            theListing.RoleName = roleName;
            return PartialView("~/Views/Role/_UserInRoleList.cshtml", theListing);
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.RoleManagement, "", "true", "true", "")]
        public ActionResult Edit(string Id)
        {
            SystemRoleViewModel model = new SystemRoleViewModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "Edit");
            }
            return View(model);
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.RoleManagement, "true", "", "", "")]
        public ActionResult ViewRecord(string Id)
        {
            SystemRoleViewModel model = new SystemRoleViewModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "View");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(SystemRoleViewModel model)
        {
            ValidateModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SaveRecord(model);
            TempData["NotifySuccess"] = "Record saved successfully!";
            return RedirectToAction("index");
        }

        public void ValidateModel(SystemRoleViewModel model)
        {
            if (model != null)
            {
                bool rolesExisted = false;
                if (model.Id != null)
                {
                    rolesExisted = db.AspNetRoles.Where(a => a.Name == model.Name && a.Id != model.Id).Any();
                }
                else
                {
                    rolesExisted = db.AspNetRoles.Where(a => a.Name == model.Name).Select(a => a.Id).Any();
                }

                if (rolesExisted == true)
                {
                    ModelState.AddModelError("Name", Resource.RoleNameAlreadyExist);
                }
            }
        }

        public void SaveRecord(SystemRoleViewModel model)
        {
            if (model != null)
            {
                DateTime? now = general.GetSystemTimeZoneDateTimeNow();
                string utcNow = general.GetIsoUtcNow();
                //edit
                if (model.Id != null)
                {
                    AspNetRoles roles = db.AspNetRoles.Where(a => a.Id == model.Id).FirstOrDefault();
                    roles.Name = model.Name;
                    roles.ModifiedBy = User.Identity.GetUserId();
                    roles.ModifiedOn = now;
                    roles.IsoUtcModifiedOn = utcNow;
                    db.Entry(roles).State = EntityState.Modified;
                    db.SaveChanges();


                    if (model.DashboardPermission != null)
                    {
                        SaveRoleModulePermission(model.DashboardPermission, roles.Id, "Dashboard", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.DashboardPermission, roles.Id, "Dashboard", true, false);
                    }

                    if (model.PreFiledCasePermission != null)
                    {
                        SaveRoleModulePermission(model.PreFiledCasePermission, roles.Id, "Pre-Filed Cases", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.PreFiledCasePermission, roles.Id, "Pre-Filed Cases", true, false);
                    }

                    if (model.FiledCasePermission != null)
                    {
                        SaveRoleModulePermission(model.FiledCasePermission, roles.Id, "Filed Cases", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.FiledCasePermission, roles.Id, "Filed Cases", true, false);
                    }

                    if (model.HearingPermission != null)
                    {
                        SaveRoleModulePermission(model.HearingPermission, roles.Id, "Hearings", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.HearingPermission, roles.Id, "Hearings", true, false);
                    }

                    if (model.InitiatoryPleadingPermission != null)
                    {
                        SaveRoleModulePermission(model.InitiatoryPleadingPermission, roles.Id, "Initiatory Pleadings", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.InitiatoryPleadingPermission, roles.Id, "Initiatory Pleadings", true, false);
                    }

                    if (model.PleadingWithCaseNumberPermission != null)
                    {
                        SaveRoleModulePermission(model.PleadingWithCaseNumberPermission, roles.Id, "Pleadings w/ Case Numbers", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.PleadingWithCaseNumberPermission, roles.Id, "Pleadings w/ Case Numbers", true, false);
                    }

                    if (model.OtherLetterCorrespondensePermission != null)
                    {
                        SaveRoleModulePermission(model.OtherLetterCorrespondensePermission, roles.Id, "Other Letters / Correspondences", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.OtherLetterCorrespondensePermission, roles.Id, "Other Letters / Correspondences", true, false);
                    }

                    if (model.DisputeResolutionPermission != null)
                    {
                        SaveRoleModulePermission(model.DisputeResolutionPermission, roles.Id, "Dispute Resolution", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.DisputeResolutionPermission, roles.Id, "Dispute Resolution", true, false);
                    }

                    if (model.TransactionPermission != null)
                    {
                        SaveRoleModulePermission(model.TransactionPermission, roles.Id, "Transactions", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.TransactionPermission, roles.Id, "Transactions", true, false);
                    }

                    if (model.PaymentPermission != null)
                    {
                        SaveRoleModulePermission(model.PaymentPermission, roles.Id, "Payment", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.PaymentPermission, roles.Id, "Payment", true, false);
                    }

                    if (model.UserStatusPermission != null)
                    {
                        SaveRoleModulePermission(model.UserStatusPermission, roles.Id, "User Status", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.UserStatusPermission, roles.Id, "User Status", true, false);
                    }

                    if (model.UserTypePermission != null)
                    {
                        SaveRoleModulePermission(model.UserTypePermission, roles.Id, "User Types", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.UserTypePermission, roles.Id, "User Types", true, false);
                    }

                    if (model.CaseTypePermission != null)
                    {
                        SaveRoleModulePermission(model.CaseTypePermission, roles.Id, "Case Types", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.CaseTypePermission, roles.Id, "Case Types", true, false);
                    }

                    if (model.CaseNaturePermission != null)
                    {
                        SaveRoleModulePermission(model.CaseNaturePermission, roles.Id, "Case Natures", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.CaseNaturePermission, roles.Id, "Case Natures", true, false);
                    }

                    if (model.UserAttachmentTypePermission != null)
                    {
                        SaveRoleModulePermission(model.UserAttachmentTypePermission, roles.Id, "User Attachment Type", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.UserAttachmentTypePermission, roles.Id, "User Attachment Type", true, false);
                    }

                    if (model.RoleManagementPermission != null)
                    {
                        SaveRoleModulePermission(model.RoleManagementPermission, roles.Id, "Role Management", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.RoleManagementPermission, roles.Id, "Role Management", true, false);
                    }

                    if (model.UserManagementPermission != null)
                    {
                        SaveRoleModulePermission(model.UserManagementPermission, roles.Id, "User Management", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.UserManagementPermission, roles.Id, "User Management", true, false);
                    }

                    if (model.LoginHistoryPermission != null)
                    {
                        SaveRoleModulePermission(model.LoginHistoryPermission, roles.Id, "Login History", false, false);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.LoginHistoryPermission, roles.Id, "Login History", true, false);
                    }
                }
                else
                {
                    //new record

                    AspNetRoles roles = new AspNetRoles();
                    roles.Id = Guid.NewGuid().ToString();
                    roles.Name = model.Name;
                    roles.CreatedBy = User.Identity.GetUserId();
                    roles.CreatedOn = now;
                    roles.IsoUtcCreatedOn = utcNow;
                    roles.SystemDefault = false; //only system admin and normal user roles are systemDefault = true, for all the new records added by user, set to false
                    db.AspNetRoles.Add(roles);
                    db.SaveChanges();

                    if (model.DashboardPermission != null)
                    {
                        SaveRoleModulePermission(model.DashboardPermission, roles.Id, "Dashboard", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.DashboardPermission, roles.Id, "Dashboard", true, true);
                    }

                    if (model.PreFiledCasePermission != null)
                    {
                        SaveRoleModulePermission(model.PreFiledCasePermission, roles.Id, "Pre-Filed Cases", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.PreFiledCasePermission, roles.Id, "Pre-Filed Cases", true, true);
                    }

                    if (model.FiledCasePermission != null)
                    {
                        SaveRoleModulePermission(model.FiledCasePermission, roles.Id, "Filed Cases", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.FiledCasePermission, roles.Id, "Filed Cases", true, true);
                    }

                    if (model.HearingPermission != null)
                    {
                        SaveRoleModulePermission(model.HearingPermission, roles.Id, "Hearings", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.HearingPermission, roles.Id, "Hearings", true, true);
                    }

                    if (model.InitiatoryPleadingPermission != null)
                    {
                        SaveRoleModulePermission(model.InitiatoryPleadingPermission, roles.Id, "Initiatory Pleadings", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.InitiatoryPleadingPermission, roles.Id, "Initiatory Pleadings", true, true);
                    }

                    if (model.PleadingWithCaseNumberPermission != null)
                    {
                        SaveRoleModulePermission(model.PleadingWithCaseNumberPermission, roles.Id, "Pleadings w/ Case Numbers", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.PleadingWithCaseNumberPermission, roles.Id, "Pleadings w/ Case Numbers", true, true);
                    }

                    if (model.OtherLetterCorrespondensePermission != null)
                    {
                        SaveRoleModulePermission(model.OtherLetterCorrespondensePermission, roles.Id, "Other Letters / Correspondences", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.OtherLetterCorrespondensePermission, roles.Id, "Other Letters / Correspondences", true, true);
                    }

                    if (model.DisputeResolutionPermission != null)
                    {
                        SaveRoleModulePermission(model.DisputeResolutionPermission, roles.Id, "Dispute Resolution", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.DisputeResolutionPermission, roles.Id, "Dispute Resolution", true, true);
                    }

                    if (model.TransactionPermission != null)
                    {
                        SaveRoleModulePermission(model.TransactionPermission, roles.Id, "Transactions", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.TransactionPermission, roles.Id, "Transactions", true, true);
                    }

                    if (model.PaymentPermission != null)
                    {
                        SaveRoleModulePermission(model.PaymentPermission, roles.Id, "Payment", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.PaymentPermission, roles.Id, "Payment", true, true);
                    }

                    if (model.UserStatusPermission != null)
                    {
                        SaveRoleModulePermission(model.UserStatusPermission, roles.Id, "User Status", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.UserStatusPermission, roles.Id, "User Status", true, true);
                    }

                    if (model.UserTypePermission != null)
                    {
                        SaveRoleModulePermission(model.UserTypePermission, roles.Id, "User Types", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.UserTypePermission, roles.Id, "User Types", true, true);
                    }

                    if (model.CaseTypePermission != null)
                    {
                        SaveRoleModulePermission(model.CaseTypePermission, roles.Id, "Case Types", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.CaseTypePermission, roles.Id, "Case Types", true, true);
                    }

                    if (model.CaseNaturePermission != null)
                    {
                        SaveRoleModulePermission(model.CaseNaturePermission, roles.Id, "Case Natures", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.CaseNaturePermission, roles.Id, "Case Natures", true, true);
                    }

                    if (model.UserAttachmentTypePermission != null)
                    {
                        SaveRoleModulePermission(model.UserAttachmentTypePermission, roles.Id, "User Attachment Type", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.UserAttachmentTypePermission, roles.Id, "User Attachment Type", true, true);
                    }

                    if (model.RoleManagementPermission != null)
                    {
                        SaveRoleModulePermission(model.RoleManagementPermission, roles.Id, "Role Management", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.RoleManagementPermission, roles.Id, "Role Management", true, true);
                    }

                    if (model.UserManagementPermission != null)
                    {
                        SaveRoleModulePermission(model.UserManagementPermission, roles.Id, "User Management", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.UserManagementPermission, roles.Id, "User Management", true, true);
                    }

                    if (model.LoginHistoryPermission != null)
                    {
                        SaveRoleModulePermission(model.LoginHistoryPermission, roles.Id, "Login History", false, true);
                    }
                    else
                    {
                        SaveRoleModulePermission(model.LoginHistoryPermission, roles.Id, "Login History", true, true);
                    }
                }
            }
        }

        public void SaveRoleModulePermission(Permission model, string roleId, string moduleName, bool setAllToFalse, bool addNewRecord)
        {
            DateTime? now = general.GetSystemTimeZoneDateTimeNow();
            string utcNow = general.GetIsoUtcNow();
            RoleModulePermission roleModulePermission = new RoleModulePermission();
            string moduleId = db.Modules.Where(a => a.Name == moduleName).Select(a => a.Id).FirstOrDefault();
            if (addNewRecord)
            {
                roleModulePermission.Id = Guid.NewGuid().ToString();
            }
            if (addNewRecord == false)
            {
                roleModulePermission = db.RoleModulePermissions.Where(a => a.RoleId == roleId && a.ModuleId == moduleId).FirstOrDefault();
            }
            if (setAllToFalse)
            {
                roleModulePermission.ViewRight = false;
                roleModulePermission.AddRight = false;
                roleModulePermission.EditRight = false;
                roleModulePermission.DeleteRight = false;
            }
            else
            {
                roleModulePermission.ViewRight = model.ViewPermission != null ? model.ViewPermission.IsSelected : false;
                roleModulePermission.AddRight = model.AddPermission != null ? model.AddPermission.IsSelected : false;
                roleModulePermission.EditRight = model.EditPermission != null ? model.EditPermission.IsSelected : false;
                roleModulePermission.DeleteRight = model.DeletePermission != null ? model.DeletePermission.IsSelected : false;
            }
            roleModulePermission.RoleId = roleId;
            roleModulePermission.ModuleId = moduleId;
            if (addNewRecord)
            {
                roleModulePermission.CreatedBy = User.Identity.GetUserId();
                roleModulePermission.CreatedOn = now;
                roleModulePermission.IsoUtcCreatedOn = utcNow;
                db.RoleModulePermissions.Add(roleModulePermission);
            }
            else
            {
                roleModulePermission.ModifiedBy = User.Identity.GetUserId();
                roleModulePermission.ModifiedOn = now;
                roleModulePermission.IsoUtcModifiedOn = utcNow;
                db.Entry(roleModulePermission).State = EntityState.Modified;
            }
            db.SaveChanges();
        }

        public List<UserInRoleViewModel> ReadUserInRole(string id)
        {
            List<UserInRoleViewModel> userList = new List<UserInRoleViewModel>();
            if (id != null)
            {
                userList = (from t1 in db.AspNetUserRoles
                            join t2 in db.UserProfiles on t1.UserId equals t2.AspNetUserId
                            join t3 in db.AspNetUsers on t1.UserId equals t3.Id
                            where t1.RoleId == id
                            select new UserInRoleViewModel
                            {
                                FullName = t2.FullName,
                                Username = t3.UserName,
                                UserProfileId = t2.Id
                            }).ToList();
            }
            return userList;
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.RoleManagement, "", "", "", "true")]
        public ActionResult Delete(string Id)
        {
            try
            {
                if (Id != null)
                {
                    AspNetRoles roles = db.AspNetRoles.Where(a => a.Id == Id).FirstOrDefault();
                    if (roles != null)
                    {
                        db.AspNetRoles.Remove(roles);
                        db.SaveChanges();
                    }
                }
                TempData["NotifySuccess"] = Resource.RecordDeletedSuccessfully;
            }
            catch (Exception ex)
            {
                AspNetRoles roles = db.AspNetRoles.Where(a => a.Id == Id).FirstOrDefault();
                if (roles == null)
                {
                    TempData["NotifySuccess"] = Resource.RecordDeletedSuccessfully;
                }
                else
                {
                    TempData["NotifyFailed"] = Resource.FailedExceptionError;
                }
            }
            return RedirectToAction("index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (db != null)
                {
                    db.Dispose();
                    db = null;
                }
                if (general != null)
                {
                    general.Dispose();
                    general = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
