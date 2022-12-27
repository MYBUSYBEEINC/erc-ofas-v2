using Microsoft.AspNet.Identity;
using NetStarter.Models;
using NetStarter.Resources;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NetStarter.Controllers
{
    [Authorize]
    public class UserAttachmentController : Controller
    {
        private DefaultDBContext db = new DefaultDBContext();
        private GeneralController general = new GeneralController();

        // GET: UserAttachment
        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.UserManagement, "true", "", "", "")]
        public ActionResult Index(string Id)
        {
            UserAttachmentListing listing = new UserAttachmentListing();
            if (Id != null)
            {
                UserAttachmentViewModel model = new UserAttachmentViewModel();
                model = GetViewModelByUserProfileId(Id);
                listing.Username = model.Username;
                listing.FullName = model.FullName;
                listing.UserProfileId = model.UserProfileId;
                //SetupSelectLists(listing);
            }

            return View(listing);
        }

        //Id = UserAttachment Id
        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.UserManagement, "", "true", "true", "")]
        public ActionResult Edit(string Id)
        {
            UserAttachmentViewModel model = new UserAttachmentViewModel();
            if (Id != null)
            {
                model = GetViewModelByAttachmentId(Id);
            }
            SetupSelectLists(model);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(UserAttachmentViewModel model)
        {
            string typeCode = general.GetGlobalOptionSetCode(model.AttachmentTypeId);
            bool validated = false;
            if (typeCode == "ProfilePicture")
            {
                validated = general.ValidateImageFile(model.FileName);
                if (validated == false)
                {
                    ModelState.AddModelError("AttachmentTypeId", Resource.FailedOnlyJpgJpegPngCanBeSetAsProfilePicture);
                }
            }

            if (!ModelState.IsValid)
            {
                model.CreatedAndModified = new CreatedAndModifiedViewModel();
                model.CreatedAndModified = general.GetCreatedAndModified(model.UploadedBy, model.IsoUtcUploadedOn, null, null);
                SetupSelectLists(model);
                return View(model);
            }
            try
            {
                UserAttachment userAttachment = db.UserAttachments.FirstOrDefault(x => x.Id == model.Id);
                if (userAttachment != null)
                {
                    userAttachment.AttachmentTypeId = model.AttachmentTypeId;
                    userAttachment.ModifiedBy = User.Identity.GetUserId();
                    userAttachment.ModifiedOn = general.GetSystemTimeZoneDateTimeNow();
                    userAttachment.IsoUtcModifiedOn = general.GetIsoUtcNow();
                    db.Entry(userAttachment).State = EntityState.Modified;
                    db.SaveChanges();
                }
                ModelState.Clear();
                TempData["NotifySuccess"] = Resource.RecordSavedSuccessfully;
            }
            catch (Exception ex)
            {
                TempData["NotifyFailed"] = Resource.FailedExceptionError;
            }

            return RedirectToAction("index", new { Id = model.UserProfileId });
        }

        //ID = UserProfile Id
        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.UserManagement, "", "true", "true", "")]
        public ActionResult Upload(string Id)
        {
            UserAttachmentViewModel model = new UserAttachmentViewModel();
            if (Id != null)
            {
                model = GetViewModelByUserProfileId(Id);
            }
            return View(model);
        }

        public void SetupSelectLists(UserAttachmentViewModel model)
        {
            model.UserAttachmentTypeSelectList = general.GetGlobalOptionSets("UserAttachment", model.Id);
        }

        [HttpPost]
        public async Task<ActionResult> Upload(UserAttachmentViewModel model, IEnumerable<HttpPostedFileBase> Files)
        {
            int byteCount = 0;
            //validation
            foreach (HttpPostedFileBase file in Files)
            {
                byteCount += file.ContentLength;
            }

            //if total file size is larger than 50mb
            if (byteCount > 52428800)
            {
                ModelState.AddModelError("Files", Resource.TotalFileSizeCannotBeLargerThan50MB);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                foreach (HttpPostedFileBase file in Files)
                {
                    //Checking file is available to save.  
                    if (file != null)
                    {
                        general.SaveUserAttachment(file, model.UserProfileId, null, User.Identity.GetUserId());
                    }
                }

                ModelState.Clear();
                TempData["NotifySuccess"] = Resource.RecordSavedSuccessfully;
            }
            catch (Exception ex)
            {
                TempData["NotifyFailed"] = Resource.FailedExceptionError;
            }

            return RedirectToAction("index", new { Id = model.UserProfileId });
        }

        public ActionResult SetAttachmentType(string id, string typeid, string upid)
        {
            try
            {
                string typeCode = general.GetGlobalOptionSetCode(typeid);
                bool validated = false;
                UserAttachment userAttachment = db.UserAttachments.FirstOrDefault(x => x.Id == id);
                if (userAttachment != null)
                {
                    if (typeCode == "ProfilePicture")
                    {
                        if (userAttachment.FileName.Contains("jpg", StringComparison.OrdinalIgnoreCase) || userAttachment.FileName.Contains("jpeg", StringComparison.OrdinalIgnoreCase) || userAttachment.FileName.Contains("png", StringComparison.OrdinalIgnoreCase))
                        {
                            validated = true;
                        }
                    }
                    if (validated == false)
                    {
                        TempData["NotifyFailed"] = Resource.FailedOnlyJpgJpegPngCanBeSetAsProfilePicture;
                        return RedirectToAction("index", new { Id = upid });
                    }
                    userAttachment.AttachmentTypeId = typeid;
                    userAttachment.ModifiedBy = User.Identity.GetUserId();
                    userAttachment.ModifiedOn = general.GetSystemTimeZoneDateTimeNow();
                    userAttachment.IsoUtcModifiedOn = general.GetIsoUtcNow();
                    db.Entry(userAttachment).State = EntityState.Modified;
                    db.SaveChanges();
                }
                TempData["NotifySuccess"] = Resource.RecordSavedSuccessfully;
            }
            catch (Exception ex)
            {
                TempData["NotifyFailed"] = Resource.FailedExceptionError;
            }
            return RedirectToAction("index", new { Id = upid });
        }

        //Id = UserAttachment Id
        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.UserManagement, "", "true", "true", "")]
        public FileResult Download(string Id)
        {
            UserAttachment userAttachment = db.UserAttachments.FirstOrDefault(a => a.Id == Id);
            if (userAttachment != null)
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(userAttachment.FileUrl);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, userAttachment.FileName);
            }
            return null;
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.UserManagement, "", "", "", "true")]
        public ActionResult Delete(string Id)
        {
            string upId = "";
            string fileName = "";
            try
            {
                if (Id != null)
                {
                    UserAttachment model = db.UserAttachments.Where(a => a.Id == Id).FirstOrDefault();
                    if (model != null)
                    {
                        upId = model.UserProfileId;
                        fileName = Path.GetFileName(model.FileName);

                        db.UserAttachments.Remove(model);
                        db.SaveChanges();

                        var path = Path.Combine(Request.MapPath("~/UploadedFiles"), fileName);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                    }
                }
                TempData["NotifySuccess"] = Resource.RecordDeletedSuccessfully;
            }
            catch (Exception ex)
            {
                UserAttachment model = db.UserAttachments.Where(a => a.Id == Id).FirstOrDefault();
                if (model == null)
                {
                    TempData["NotifySuccess"] = Resource.RecordDeletedSuccessfully;
                }
                else
                {
                    TempData["NotifyFailed"] = Resource.FailedExceptionError;
                }
            }
            return RedirectToAction("index", new { Id = upId });
        }

        public UserAttachmentViewModel GetViewModelByUserProfileId(string Id)
        {
            UserAttachmentViewModel model = new UserAttachmentViewModel();
            model = (from t1 in db.UserProfiles
                     where t1.Id == Id
                     select new UserAttachmentViewModel
                     {
                         Id = Id,
                         FullName = t1.FullName,
                         UserProfileId = t1.Id,
                         AspNetUserId = t1.AspNetUserId
                     }).FirstOrDefault();
            model.Username = db.AspNetUsers.Where(a => a.Id == model.AspNetUserId).Select(a => a.UserName).FirstOrDefault();
            return model;
        }

        public UserAttachmentViewModel GetViewModelByAttachmentId(string Id)
        {
            UserAttachmentViewModel model = new UserAttachmentViewModel();
            model = (from t1 in db.UserAttachments
                     join t2 in db.UserProfiles on t1.UserProfileId equals t2.Id
                     where t1.Id == Id
                     select new UserAttachmentViewModel
                     {
                         Id = t1.Id,
                         FullName = t2.FullName,
                         UserProfileId = t2.Id,
                         AspNetUserId = t2.AspNetUserId,
                         AttachmentTypeId = t1.AttachmentTypeId,
                         FileName = t1.FileName,
                         FileUrl = t1.FileUrl,
                         UploadedOn = t1.CreatedOn,
                         UploadedBy = t1.CreatedBy,
                         IsoUtcUploadedOn = t1.IsoUtcCreatedOn
                     }).FirstOrDefault();
            model.Username = db.AspNetUsers.Where(a => a.Id == model.AspNetUserId).Select(a => a.UserName).FirstOrDefault();
            model.AttachmentTypeName = model.AttachmentTypeId != null ? general.GetGlobalOptionSetDisplayName(model.AttachmentTypeId) : "N/A";
            model.CreatedAndModified = new CreatedAndModifiedViewModel();
            model.CreatedAndModified = general.GetCreatedAndModified(model.UploadedBy, model.IsoUtcUploadedOn, null, null);
            return model;
        }

        public ActionResult GetPartialViewUserAttachment(string upId)
        {
            UserAttachmentListing theListing = new UserAttachmentListing();
            theListing.Listing = ReadUserAttachments(upId);
            return PartialView("~/Views/UserAttachment/_MainList.cshtml", theListing);
        }

        public List<UserAttachmentViewModel> ReadUserAttachments(string upId)
        {
            List<UserAttachmentViewModel> list = new List<UserAttachmentViewModel>();
            list = (from t1 in db.UserAttachments
                    where t1.UserProfileId == upId
                    select new UserAttachmentViewModel
                    {
                        Id = t1.Id,
                        FileName = t1.FileName,
                        FileUrl = t1.FileUrl,
                        AttachmentTypeId = t1.AttachmentTypeId,
                        AttachmentTypeName = db.GlobalOptionSets.Where(a => a.Id == t1.AttachmentTypeId).Select(a => a.DisplayName).FirstOrDefault(),
                        UserProfileId = t1.UserProfileId,
                        UploadedOn = t1.CreatedOn,
                        UploadedBy = t1.CreatedBy,
                        IsoUtcUploadedOn = t1.IsoUtcCreatedOn
                    }).ToList();
            foreach (var item in list)
            {
                item.CreatedAndModified = new CreatedAndModifiedViewModel();
                item.CreatedAndModified = general.GetCreatedAndModified(item.UploadedBy, item.IsoUtcUploadedOn, null, null);
            }
            return list;
        }
    }
}