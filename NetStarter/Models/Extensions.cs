using Microsoft.AspNet.Identity;
using NetStarter.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using NetStarter.Resources;
using System.Text;

namespace NetStarter.Models
{
    public static class Extensions
    {
    }

    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static string ReplaceWhitespace(string input, string replacement)
        {
            Regex sWhitespace = new Regex(@"\s+");
            return sWhitespace.Replace(input, replacement);
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }

    public class UserProfilePictureActionFilter : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            using (AccountController acc = new AccountController())
            {
                string userid = HttpContext.Current.User.Identity.GetUserId();
                filterContext.Controller.ViewBag.Avatar = acc.GetMyProfilePictureName(userid);
            }
        }
    }

    public class PasswordValidation : RequiredAttribute
    {
        public PasswordValidation()
        {
            this.ErrorMessage = Resource.InvalidPassword;
        }

        public override bool IsValid(object value)
        {
            string passwordValue = value as string;
            if (!string.IsNullOrEmpty(passwordValue))
            {
                bool hasNonLetterOrDigit, hasDigit, hasUppercase, hasLowercase = false;

                using (GeneralController controller = new GeneralController())
                {
                    hasNonLetterOrDigit = controller.HasNonLetterOrDigit(passwordValue);
                    hasDigit = controller.HasDigit(passwordValue);
                    hasUppercase = controller.HasUppercase(passwordValue);
                    hasLowercase = controller.HasLowercase(passwordValue);
                }

                if (passwordValue.Length < 6 || hasNonLetterOrDigit == false || hasDigit == false || hasUppercase == false || hasLowercase == false)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
    }

    public class Max5MBAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            var file = value as HttpPostedFileBase;
            //file is null means it's an optional field, no file to validate, return true
            if (file == null)
            {
                return true;
            }
            //file more than 5 mb
            if (file.ContentLength > 5242880)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public class Max50MBAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            var file = value as HttpPostedFileBase;
            //file is null means it's an optional field, no file to validate, return true
            if (file == null)
            {
                return true;
            }
            //file more than 50 mb
            if (file.ContentLength > 52428800)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public class OnlyPngJpgJpeg : RequiredAttribute
    {
        public OnlyPngJpgJpeg()
        {
            this.ErrorMessage = "Only files with Jpg, Png, or Jpeg extension are allowed.";
        }

        public override bool IsValid(object value)
        {
            var file = value as HttpPostedFileBase;
            //file is null means it's an optional field, no file to validate, return true
            if (file == null)
            {
                return true;
            }
            try
            {
                using (var img = Image.FromStream(file.InputStream))
                {
                    if (img.RawFormat.Equals(ImageFormat.Png) || img.RawFormat.Equals(ImageFormat.Jpeg))
                    {
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }
    }

    public static class IdentityExtended
    {
        public static CurrentUserPermission IsAllowed(this IIdentity identity, string moduleCode)
        {
            string userid = identity.GetUserId();
            CurrentUserPermission currentUserPermission = new CurrentUserPermission();
            currentUserPermission.ViewRight = false;
            currentUserPermission.AddRight = false;
            currentUserPermission.EditRight = false;
            currentUserPermission.DeleteRight = false;

            using (GeneralController controller = new GeneralController())
            {
                currentUserPermission = controller.GetCurrentUserPermission(userid, moduleCode);
            }

            return currentUserPermission;
        }
    }

    public class CustomAuthorizeFilter : AuthorizeAttribute
    {
        private string _moduleCode;
        private string _viewRight;
        private string _addRight;
        private string _editRight;
        private string _deleteRight;

        public CustomAuthorizeFilter(ProjectEnum.ModuleCode module, string viewRight, string addRight, string editRight, string deleteRight)
        {
            _moduleCode = module.ToString();
            _viewRight = viewRight;
            _addRight = addRight;
            _editRight = editRight;
            _deleteRight = deleteRight;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool authorized = false;
            string userid = httpContext.User.Identity.GetUserId();
            CurrentUserPermission currentUserPermission = new CurrentUserPermission();
            currentUserPermission.ViewRight = false;
            currentUserPermission.AddRight = false;
            currentUserPermission.EditRight = false;
            currentUserPermission.DeleteRight = false;

            using (GeneralController controller = new GeneralController())
            {
                currentUserPermission = controller.GetCurrentUserPermission(userid, _moduleCode);
            }

            if (!string.IsNullOrEmpty(_viewRight) && currentUserPermission.ViewRight.ToString().ToLower() == _viewRight)
            {
                authorized = true;
            }

            if (!string.IsNullOrEmpty(_addRight) && currentUserPermission.AddRight.ToString().ToLower() == _addRight)
            {
                authorized = true;
            }

            if (!string.IsNullOrEmpty(_editRight) && currentUserPermission.EditRight.ToString().ToLower() == _editRight)
            {
                authorized = true;
            }

            if (!string.IsNullOrEmpty(_deleteRight) && currentUserPermission.DeleteRight.ToString().ToLower() == _deleteRight)
            {
                authorized = true;
            }
            return authorized;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
               new System.Web.Routing.RouteValueDictionary
               {
                    { "controller", "Home" },
                    { "action", "UnAuthorized" }
               });
        }
    }

}