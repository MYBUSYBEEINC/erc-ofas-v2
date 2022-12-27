using Microsoft.AspNet.Identity;
using NetStarter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NetStarter.Controllers
{
    [Authorize]
    public class LoginHistoryController : Controller
    {
        private DefaultDBContext db = new DefaultDBContext();
        private GeneralController general = new GeneralController();

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.LoginHistory, "true", "", "", "")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetPartialViewLoginHistories()
        {
            LoginHistoryListing theListing = new LoginHistoryListing();
            theListing.Listing = ReadLoginHistories();
            return PartialView("~/Views/LoginHistory/_MainList.cshtml", theListing);
        }

        public List<LoginHistoryViewModel> ReadLoginHistories()
        {
            List<LoginHistoryViewModel> list = new List<LoginHistoryViewModel>();
            string datetimeFormat = general.GetAppSettingsValue("datetimeFormat");
            string userId = User.Identity.GetUserId();
            var currentUserRole = general.GetCurrentUserRoleNameList(userId);
            if (currentUserRole.Contains("System Admin"))
            {
                list = (from t1 in db.LoginHistories
                        join t2 in db.AspNetUsers on t1.AspNetUserId equals t2.Id
                        join t3 in db.UserProfiles on t2.Id equals t3.AspNetUserId
                        select new LoginHistoryViewModel
                        {
                            Id = t1.Id,
                            UserProfileId = t3.Id,
                            Username = t2.UserName,
                            FullName = t3.FullName,
                            LoginDateTime = t1.LoginDateTime.Value,
                            IsoUtcLoginDateTime = t1.IsoUtcLoginDateTime
                        }).OrderBy(o => o.LoginDateTime).ToList();
            }
            else
            {
                list = (from t1 in db.LoginHistories
                        join t2 in db.AspNetUsers on t1.AspNetUserId equals t2.Id
                        join t3 in db.UserProfiles on t2.Id equals t3.AspNetUserId
                        where t1.AspNetUserId == userId
                        select new LoginHistoryViewModel
                        {
                            Id = t1.Id,
                            UserProfileId = t3.Id,
                            Username = t2.UserName,
                            FullName = t3.FullName,
                            LoginDateTime = t1.LoginDateTime.Value,
                            IsoUtcLoginDateTime = t1.IsoUtcLoginDateTime
                        }).OrderBy(o => o.LoginDateTime).ToList();
            }

            var finalList = list.Select((value, index) => new LoginHistoryViewModel
            {
                Id = value.Id,
                UserProfileId = value.Id,
                Username = value.Username,
                FullName = value.FullName,
                LoginDateTime = value.LoginDateTime,
                IsoUtcLoginDateTime = value.IsoUtcLoginDateTime,
                FormattedLoginDateTime = general.GetFormattedDateTime(value.IsoUtcLoginDateTime),
                LoginDateTimeOrder = index
            }).ToList();

            return finalList;
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