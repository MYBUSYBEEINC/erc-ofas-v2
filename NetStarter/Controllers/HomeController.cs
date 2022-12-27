using NetStarter.Resources;
using System;
using System.Data.Linq;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace NetStarter.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //if first time running the web application, run the sql to create tables
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            try
            {
                if (string.IsNullOrEmpty(connection))
                {
                    return View("ConfigurationError");
                }
                using (DataContext dc = new DataContext(connection))
                {
                    var result = dc.ExecuteCommand("select top(1)Id from aspnetusers");
                    dc.Connection.Close();
                    dc.Dispose();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Invalid object name"))
                {
                    try
                    {
                        string fileContents = String.Empty;
                        var path = Request.MapPath("~/App_Data/SQL/1.0.0.sql");
                        if (System.IO.File.Exists(path))
                        {
                            fileContents = System.IO.File.ReadAllText(path);
                            using (DataContext dc = new DataContext(connection))
                            {
                                var result = dc.ExecuteCommand(fileContents);
                                dc.Connection.Close();
                                dc.Dispose();
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            }

            if (Request.IsAuthenticated)
            {
                return RedirectToAction("index", "dashboard");
            }
            return View();
        }

        public ActionResult Unauthorized()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("UnauthorizedTwo");
            }
            return RedirectToAction("UnauthorizedOne");
        }

        public ActionResult UnauthorizedOne()
        {
            ViewBag.Message = Resource.YouDontHavePermissionToAccess;
            return View();
        }
        public ActionResult UnauthorizedTwo()
        {
            ViewBag.Message = Resource.YouDontHavePermissionToAccess;
            return View();
        }

    }
}