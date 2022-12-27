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
    public class TransactionController : Controller
    {
        private DefaultDBContext db = new DefaultDBContext();
        private GeneralController general = new GeneralController();

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.Transaction, "true", "", "", "")]
        public ViewResult Index()
        {
            return View();
        }

        public ActionResult GetPartialViewTransactions()
        {
            PreFiledCaseListing theListing = new PreFiledCaseListing();
            theListing.Listing = ReadFiledCases();
            return PartialView("~/Views/Transaction/_MainList.cshtml", theListing);
        }

        public List<PreFiledCaseViewModel> ReadFiledCases()
        {
            List<PreFiledCaseViewModel> list = new List<PreFiledCaseViewModel>();
            list = (from t1 in db.PreFiledCases
                    join t2 in db.UserProfiles on t1.UserId equals t2.AspNetUserId
                    join t3 in db.GlobalOptionSets.Where(x => x.Type == "CaseType") on t1.CaseTypeId equals t3.Id
                    join t4 in db.GlobalOptionSets.Where(x => x.Type == "CaseNature") on t1.CaseNatureId equals t4.Id
                    join t5 in db.GlobalOptionSets.Where(x => x.Type == "FileCaseStatus") on t1.FileCaseStatusId equals t5.Id
                    orderby t1.CreatedOn descending
                    select new PreFiledCaseViewModel
                    {
                        Id = t1.Id,
                        RequestSubject = t1.RequestSubject,
                        UserId = t2.FirstName + " " + t2.LastName,
                        CaseTypeId = t3.DisplayName,
                        CaseNatureId = t4.DisplayName,
                        FileCaseStatusId = t5.DisplayName,
                        CreatedOn = t1.CreatedOn
                    }).ToList();
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
        public ActionResult Edit(string Id)
        {
            PreFiledCaseViewModel model = new PreFiledCaseViewModel();
            if (Id != null)
            {
                model = GetViewModel(Id, "Edit");
            }
            model.CaseTypeSelectList = general.GetCaseTypeList(model.CaseTypeId);
            model.CaseNatureSelectList = general.GetCaseNatureList(model.CaseNatureId);
            model.FileCaseStatusSelectList = general.GetFileCaseStatusList(model.FileCaseStatusId);
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
                model.FileCaseStatusSelectList = general.GetFileCaseStatusList(model.FileCaseStatusId);
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
                if (model.Id != null)
                {
                    PreFiledCases prefiledCase = db.PreFiledCases.Where(a => a.Id == model.Id).FirstOrDefault();
                    prefiledCase.ApprovalRemarks = model.ApprovalRemarks;
                    prefiledCase.FileCaseStatusId = model.FileCaseStatusId;
                    db.Entry(prefiledCase).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
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