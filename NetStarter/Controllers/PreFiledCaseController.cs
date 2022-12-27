using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using NetStarter.Models;
using NetStarter.Resources;

namespace NetStarter.Controllers
{
    [Authorize]
    public class PreFiledCaseController : Controller
    {
        private DefaultDBContext db = new DefaultDBContext();
        private GeneralController general = new GeneralController();

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.PreFiledCase, "true", "", "", "")]
        public ViewResult Index()
        {
            return View();
        }

        public ActionResult GetPartialViewPreFiledCases()
        {
            string userId = User.Identity.GetUserId();
            PreFiledCaseListing theListing = new PreFiledCaseListing();
            theListing.Listing = ReadPreFiledCases(userId);
            return PartialView("~/Views/PreFiledCase/_MainList.cshtml", theListing);
        }

        public List<PreFiledCaseViewModel> ReadPreFiledCases(string userId)
        {
            List<PreFiledCaseViewModel> list = new List<PreFiledCaseViewModel>();
            list = (from t1 in db.PreFiledCases
                    join t2 in db.AspNetUsers on t1.UserId equals t2.Id
                    join t3 in db.GlobalOptionSets.Where(x => x.Type == "CaseType") on t1.CaseTypeId equals t3.Id
                    join t4 in db.GlobalOptionSets.Where(x => x.Type == "CaseNature") on t1.CaseNatureId equals t4.Id
                    join t5 in db.GlobalOptionSets.Where(x => x.Type == "FileCaseStatus") on t1.FileCaseStatusId equals t5.Id
                    where t1.UserId == userId
                    orderby t1.CreatedOn descending
                    select new PreFiledCaseViewModel
                    {
                        Id = t1.Id,
                        RequestSubject = t1.RequestSubject,
                        UserId = t2.UserName,
                        CaseTypeId = t3.DisplayName,
                        CaseNatureId = t4.DisplayName,
                        FileCaseStatusId = t5.DisplayName,
                        CreatedOn = t1.CreatedOn,
                        ModifiedOn = t1.ModifiedOn
                    }).ToList();
            return list;
        }

        public List<PreFiledAttachment> ReadPreFiledAttachments(string preFiledCaseId)
        {
            List<PreFiledAttachment> list = new List<PreFiledAttachment>();
            list = db.PreFiledAttachments.Where(x => x.PreFiledCaseId == preFiledCaseId).ToList();
            return list;
        }

        public PreFiledCaseViewModel GetViewModel(string Id, string type)
        {
            PreFiledCaseViewModel model = new PreFiledCaseViewModel();
            using (DefaultDBContext db = new DefaultDBContext())
            {
                PreFiledCases preFiledCase = db.PreFiledCases.Where(a => a.Id == Id).FirstOrDefault();
                model.Id = preFiledCase.Id;
                model.RequestSubject = preFiledCase.RequestSubject;
                model.UserId = preFiledCase.UserId;
                model.CaseTypeId = db.GlobalOptionSets.FirstOrDefault(x => x.Id == preFiledCase.CaseTypeId).DisplayName;
                model.CaseNatureId = db.GlobalOptionSets.FirstOrDefault(x => x.Id == preFiledCase.CaseNatureId).DisplayName;
                model.FileCaseStatusId = db.GlobalOptionSets.FirstOrDefault(x => x.Id == preFiledCase.FileCaseStatusId).DisplayName;
                model.Remarks = preFiledCase.Remarks;
                model.ApprovalRemarks = preFiledCase.ApprovalRemarks;
                model.CreatedOn = preFiledCase.CreatedOn;
                model.ModifiedOn = preFiledCase.ModifiedOn;
                if (type == "View")
                {
                    string modifiedBy = preFiledCase.ModifiedOn != null ? preFiledCase.UserId : string.Empty;
                    model.CreatedAndModified = general.GetCreatedAndModified(preFiledCase.UserId, preFiledCase.CreatedOn.ToString(), modifiedBy, preFiledCase.ModifiedOn.ToString());
                }
            }
            return model;
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.PreFiledCase, "", "true", "true", "")]
        public ActionResult DeleteFile(string Id)
        {
            if (Id != null)
            {
                var attachment = db.PreFiledAttachments.FirstOrDefault(x => x.Id == Id);
                db.PreFiledAttachments.Remove(attachment);
                db.SaveChanges();

                if (System.IO.File.Exists(attachment.FileUrl))
                    System.IO.File.Delete(attachment.FileUrl);

                TempData["NotifySuccess"] = "File was deleted successfully.";
            }
            return Redirect(Request.UrlReferrer.ToString());
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.PreFiledCase, "", "true", "true", "")]
        public ActionResult Edit(string Id)
        {
            PreFiledCaseViewModel model = new PreFiledCaseViewModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "Edit");
            }
            model.CaseTypeSelectList = general.GetCaseTypeList(model.CaseTypeId);
            model.CaseNatureSelectList = general.GetCaseNatureList(model.CaseNatureId);
            model.Attachments = ReadPreFiledAttachments(Id);
            return View(model);
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.PreFiledCase, "true", "", "", "")]
        public ActionResult ViewRecord(string Id)
        {
            PreFiledCaseViewModel model = new PreFiledCaseViewModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "View");
            }
            model.Attachments = ReadPreFiledAttachments(Id);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(PreFiledCaseViewModel model)
        {
            ValidateModel(model);

            if (!ModelState.IsValid)
            {
                model.CaseTypeSelectList = general.GetCaseTypeList(model.CaseTypeId);
                model.CaseNatureSelectList = general.GetCaseNatureList(model.CaseNatureId);
                return View(model);
            }

            SaveRecord(model);
            TempData["NotifySuccess"] = Resource.RecordSavedSuccessfully;
            return RedirectToAction("index");
        }

        public void ValidateModel(PreFiledCaseViewModel model)
        {
            if (model != null)
            {
                bool duplicated = false;
                if (model.Id != null)
                {
                    duplicated = db.PreFiledCases.Where(a => a.RequestSubject == model.RequestSubject && a.Id != model.Id).Any();
                }
                else
                {
                    duplicated = db.PreFiledCases.Where(a => a.RequestSubject == model.RequestSubject).Select(a => a.Id).Any();
                }

                if (duplicated == true)
                {
                    ModelState.AddModelError("RequestSubject", Resource.RequestSubjectAlreadyExist);
                }
            }
        }

        public void SaveRecord(PreFiledCaseViewModel model)
        {
            if (model != null)
            {
                //edit
                if (model.Id != null)
                {
                    PreFiledCases prefiledCase = db.PreFiledCases.Where(a => a.Id == model.Id).FirstOrDefault();
                    prefiledCase.RequestSubject = model.RequestSubject;
                    prefiledCase.UserId = User.Identity.GetUserId();
                    prefiledCase.CaseTypeId = model.CaseTypeId;
                    prefiledCase.CaseNatureId = model.CaseNatureId;
                    prefiledCase.Remarks = model.Remarks;
                    prefiledCase.ModifiedOn = general.GetSystemTimeZoneDateTimeNow();
                    db.Entry(prefiledCase).State = EntityState.Modified;
                    db.SaveChanges();
                }
                //new record
                else
                {
                    PreFiledCases prefiledCase = new PreFiledCases();
                    prefiledCase.Id = Guid.NewGuid().ToString();
                    prefiledCase.RequestSubject = model.RequestSubject;
                    prefiledCase.UserId = User.Identity.GetUserId();
                    prefiledCase.CaseTypeId = model.CaseTypeId;
                    prefiledCase.CaseNatureId = model.CaseNatureId;
                    prefiledCase.FileCaseStatusId = "3188CF59-2C55-47AB-8DF0-AC8EB643C825";
                    prefiledCase.Remarks = model.Remarks;
                    prefiledCase.CreatedOn = general.GetSystemTimeZoneDateTimeNow();
                    db.PreFiledCases.Add(prefiledCase);
                    db.SaveChanges();
                }

                general.SavePreFiledAttachment(model.Documents, model.Id, User.Identity.GetUserId());
            }
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.PreFiledCase, "", "", "", "true")]
        public ActionResult Delete(string Id)
        {
            try
            {
                if (Id != null)
                {
                    PreFiledCases prefiledCase = db.PreFiledCases.Where(a => a.Id == Id).FirstOrDefault();
                    if (prefiledCase != null)
                    {
                        db.PreFiledCases.Remove(prefiledCase);
                        db.SaveChanges();
                    }
                }
                TempData["NotifySuccess"] = Resource.RecordDeletedSuccessfully;
            }
            catch (Exception ex)
            {
                PreFiledCases prefiledCase = db.PreFiledCases.Where(a => a.Id == Id).FirstOrDefault();
                if (prefiledCase == null)
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