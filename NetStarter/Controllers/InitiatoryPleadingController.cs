using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using NetStarter.Models;
using NetStarter.Resources;

namespace NetStarter.Controllers
{
    [Authorize]
    public class InitiatoryPleadingController : Controller
    {
        private DefaultDBContext db = new DefaultDBContext();
        private GeneralController general = new GeneralController();

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.PreFiledCase, "true", "", "", "")]
        public ViewResult Index()
        {
            return View();
        }

        public ActionResult GetPartialViewInitiatoryPleadings()
        {
            string userId = User.Identity.GetUserId();
            InitiatoryPleadingListing theListing = new InitiatoryPleadingListing();
            theListing.Listing = ReadInitiatoryPleadings(userId);
            return PartialView("~/Views/InitiatoryPleading/_MainList.cshtml", theListing);
        }

        public List<InitiatoryPleadingModel> ReadInitiatoryPleadings(string userId)
        {
            List<InitiatoryPleadingModel> list = new List<InitiatoryPleadingModel>();
            list = (from t1 in db.InitiatoryPleadings
                    join t2 in db.AspNetUsers on t1.CreatedBy equals t2.Id
                    join t3 in db.UserProfiles on t2.Id equals t3.AspNetUserId
                    where t1.CreatedBy == userId
                    orderby t1.CreatedOn descending
                    select new InitiatoryPleadingModel()
                    {
                        Id = t1.Id,
                        DocumentName = t1.DocumentName,
                        Description = t1.Description,
                        Barcode = t1.Barcode,
                        CreatedBy = t3.FirstName + " " + t3.LastName,
                        CreatedOn = t1.CreatedOn
                    }).ToList();
            return list;
        }

        public List<InitiatoryPleadingAttachment> ReadInitiatoryPleadingAttachments(string initiatoryPleadingId)
        {
            List<InitiatoryPleadingAttachment> list = new List<InitiatoryPleadingAttachment>();
            list = db.InitiatoryPleadingAttachments.Where(x => x.InitiatoryPleadingId == initiatoryPleadingId).ToList();
            return list;
        }

        public InitiatoryPleadingModel GetViewModel(string Id, string type)
        {
            InitiatoryPleadingModel model = new InitiatoryPleadingModel();
            using (DefaultDBContext db = new DefaultDBContext())
            {
                InitiatoryPleading initiatoryPleading = db.InitiatoryPleadings.Where(a => a.Id == Id).FirstOrDefault();
                model.Id = initiatoryPleading.Id;
                model.DocumentName = initiatoryPleading.DocumentName;
                model.Description = initiatoryPleading.Description;
                model.Barcode = initiatoryPleading.Barcode;
                model.CreatedBy = db.UserProfiles.FirstOrDefault(x => x.AspNetUserId == initiatoryPleading.CreatedBy).FullName;
                model.CreatedOn = initiatoryPleading.CreatedOn;
                model.ModifiedBy = initiatoryPleading.ModifiedBy;
                model.ModifiedOn = initiatoryPleading.ModifiedOn;
                if (type == "View")
                {
                    model.CreatedAndModified = general.GetCreatedAndModified(initiatoryPleading.CreatedBy, initiatoryPleading.CreatedOn.ToString(), initiatoryPleading.ModifiedBy, initiatoryPleading.ModifiedOn.ToString());
                }
            }
            return model;
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.InitiatoryPleading, "", "true", "true", "")]
        public ActionResult DeleteFile(string Id)
        {
            if (Id != null)
            {
                var attachment = db.InitiatoryPleadingAttachments.FirstOrDefault(x => x.Id == Id);
                db.InitiatoryPleadingAttachments.Remove(attachment);
                db.SaveChanges();

                if (System.IO.File.Exists(attachment.FileUrl))
                    System.IO.File.Delete(attachment.FileUrl);

                TempData["NotifySuccess"] = "File was deleted successfully.";
            }
            return Redirect(Request.UrlReferrer.ToString());
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.InitiatoryPleading, "", "true", "true", "")]
        public ActionResult Edit(string Id)
        {
            InitiatoryPleadingModel model = new InitiatoryPleadingModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "Edit");
            }
            model.Attachments = ReadInitiatoryPleadingAttachments(Id);
            return View(model);
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.InitiatoryPleading, "true", "", "", "")]
        public ActionResult ViewRecord(string Id)
        {
            InitiatoryPleadingModel model = new InitiatoryPleadingModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "View");
                model.Attachments = ReadInitiatoryPleadingAttachments(Id);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(InitiatoryPleadingModel model)
        {
            ValidateModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SaveRecord(model);
            TempData["NotifySuccess"] = Resource.RecordSavedSuccessfully;
            return RedirectToAction("index");
        }

        public void ValidateModel(InitiatoryPleadingModel model)
        {
            if (model != null)
            {
                bool duplicated = false;
                if (model.Id != null)
                {
                    duplicated = db.InitiatoryPleadings.Where(a => a.DocumentName == model.DocumentName && a.Id != model.Id).Any();
                }
                else
                {
                    duplicated = db.InitiatoryPleadings.Where(a => a.DocumentName == model.DocumentName).Select(a => a.Id).Any();
                }

                if (duplicated == true)
                {
                    ModelState.AddModelError("DocumentName", Resource.RequestSubjectAlreadyExist);
                }
            }
        }

        public void SaveRecord(InitiatoryPleadingModel model)
        {
            string userId = User.Identity.GetUserId();

            if (model != null)
            {
                //edit
                if (model.Id != null)
                {
                    InitiatoryPleading initiatoryPleading = db.InitiatoryPleadings.Where(a => a.Id == model.Id).FirstOrDefault();
                    initiatoryPleading.DocumentName = model.DocumentName;
                    initiatoryPleading.Description = model.Description;
                    initiatoryPleading.Barcode = model.Barcode;
                    initiatoryPleading.ModifiedBy = userId;
                    initiatoryPleading.ModifiedOn = general.GetSystemTimeZoneDateTimeNow();
                    db.Entry(initiatoryPleading).State = EntityState.Modified;
                    db.SaveChanges();
                }
                //new record
                else
                {
                    InitiatoryPleading initiatoryPleading = new InitiatoryPleading();
                    initiatoryPleading.Id = Guid.NewGuid().ToString();
                    initiatoryPleading.DocumentName = model.DocumentName;
                    initiatoryPleading.Description = model.Description;
                    initiatoryPleading.Barcode = model.Barcode;
                    initiatoryPleading.CreatedBy = userId;
                    initiatoryPleading.CreatedOn = general.GetSystemTimeZoneDateTimeNow();
                    db.InitiatoryPleadings.Add(initiatoryPleading);
                    db.SaveChanges();

                    model.Id = initiatoryPleading.Id;
                }

                general.SaveInitiatoryPleadingAttachment(model.Documents, model.Id, User.Identity.GetUserId());
            }
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.PreFiledCase, "", "", "", "true")]
        public ActionResult Delete(string Id)
        {
            try
            {
                if (Id != null)
                {
                    InitiatoryPleading initiatoryPleading = db.InitiatoryPleadings.Where(a => a.Id == Id).FirstOrDefault();
                    if (initiatoryPleading != null)
                    {
                        db.InitiatoryPleadings.Remove(initiatoryPleading);
                        db.SaveChanges();
                    }
                }
                TempData["NotifySuccess"] = Resource.RecordDeletedSuccessfully;
            }
            catch (Exception ex)
            {
                InitiatoryPleading initiatoryPleading = db.InitiatoryPleadings.Where(a => a.Id == Id).FirstOrDefault();
                if (initiatoryPleading == null)
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