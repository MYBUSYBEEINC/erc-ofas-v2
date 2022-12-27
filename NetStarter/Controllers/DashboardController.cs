using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Microsoft.AspNet.Identity;
using NetStarter.Models;

namespace NetStarter.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private DefaultDBContext db = new DefaultDBContext();
        private GeneralController general = new GeneralController();

        // GET: Dashboard
        //Only user with ViewRight = true for Dashboard module can access this
        //Empty value for AddRight, EditRight, DeleteRight parameters means no need to validate that
        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.Dashboard, "true", "", "", "")]
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            DashboardViewModel model = new DashboardViewModel();
            model.TotalCasesFiled = db.PreFiledCases.Count();
            model.TotalPreFiledCases = db.PreFiledCases.Where(x => x.UserId == userId).Count();
            model.TotalUsers = db.AspNetUsers.Count();
            model.TotalRole = db.AspNetRoles.Select(a => a.Id).Count();
            model.UserByStatus = (from t1 in db.UserProfiles
                                  join t2 in db.GlobalOptionSets on t1.UserStatusId equals t2.Id
                                  where t2.Status == "Active"
                                  group t2.DisplayName by t2.DisplayName into g
                                  select new Chart
                                  {
                                      DataValue = g.Count(),
                                      DataLabel = g.Key
                                  }).ToList();
            var userByRole = (from t1 in db.AspNetUserRoles
                              join t2 in db.AspNetRoles on t1.RoleId equals t2.Id
                              group t2.Name by t2.Name into g
                              select new Chart
                              {
                                  DataValue = g.Count(),
                                  DataLabel = g.Key
                              }).ToList();
            model.TopRole = userByRole.OrderBy(a => a.DataValue).Select(a => a.DataLabel).DefaultIfEmpty("N/A").FirstOrDefault();
            if (model.TopRole != "N/A")
            {
                model.TopRoleId = db.AspNetRoles.Where(a => a.Name == model.TopRole).Select(a => a.Id).FirstOrDefault();
            }
            model.TopStatus = model.UserByStatus.OrderByDescending(a => a.DataValue).Select(a => a.DataLabel).DefaultIfEmpty("N/A").FirstOrDefault();
            if (model.TopStatus != "N/A")
            {
                model.TopStatusId = db.GlobalOptionSets.Where(a => a.DisplayName == model.TopStatus && a.Type == "UserStatus").Select(a => a.Id).FirstOrDefault();
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult GetUserByRole()
        {
            var chartList = (from t1 in db.AspNetRoles
                             join t2 in db.AspNetUserRoles on t1.Id equals t2.RoleId
                             group t1.Name by t1.Name into g
                             select new Chart
                             {
                                 DataValue = g.Count(),
                                 DataLabel = g.Key
                             }).ToList();
            return Json(chartList.OrderBy(o => o.DataValue), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetUserByStatus()
        {
            var userByStatus = (from t1 in db.UserProfiles
                                join t2 in db.GlobalOptionSets on t1.UserStatusId equals t2.Id
                                where t2.Status == "Active"
                                group t2.DisplayName by t2.DisplayName into g
                                select new Chart
                                {
                                    DataValue = g.Count(),
                                    DataLabel = g.Key
                                }).ToList();
            return Json(userByStatus.OrderBy(o => o.DataValue), JsonRequestBehavior.AllowGet);
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