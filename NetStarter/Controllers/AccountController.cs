using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NetStarter.Models;
using NetStarter.Resources;
using Newtonsoft.Json;
using RestSharp;

namespace NetStarter.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private GeneralController general = new GeneralController();
        private DefaultDBContext db = new DefaultDBContext();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string userid = "";

            // if username input contains @ sign, means that user use email to login
            if (model.UserName.Contains("@"))
            {
                // select the UserName of the user from AspNetUsers table and assign to model.UserName because instead of email, SignInManager use username to sign in 
                model.UserName = db.AspNetUsers.Where(a => a.Email == model.UserName).Select(a => a.UserName).DefaultIfEmpty("").FirstOrDefault();
                userid = db.AspNetUsers.Where(a => a.Email == model.UserName).Select(a => a.Id).DefaultIfEmpty("").FirstOrDefault();
            }
            else
            {
                userid = db.AspNetUsers.Where(a => a.UserName == model.UserName).Select(a => a.Id).DefaultIfEmpty("").FirstOrDefault();
            }

            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    //save login history
                    general.SaveLoginHistory(userid);
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);

                    return RedirectToAction("index", "dashboard");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", Resource.InvalidLoginAttempt);
                    return View(model);
            }
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (general.GetAppSettingsValue("environment") == "prod" && User.Identity.GetUserName() == "nsadmin")
            {
                TempData["NotifyFailed"] = Resource.NotAllowToChangePasswordForDemoAccount;
                return RedirectToAction("index", "dashboard");
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                TempData["NotifySuccess"] = Resource.PasswordChangedSuccessfully;
                return RedirectToAction("index", "dashboard");
            }
            AddErrors(result);
            return View(model);
        }

        public ActionResult MyProfile()
        {
            UserProfileViewModel model = new UserProfileViewModel();
            string currentUserId = User.Identity.GetUserId();
            model = general.GetCurrentUserProfile(currentUserId);
            return View(model);
        }

        public ActionResult EditMyProfile()
        {
            UserProfileViewModel model = new UserProfileViewModel();
            string currentUserId = User.Identity.GetUserId();
            model = general.GetCurrentUserProfile(currentUserId);
            ViewBag.DateFormat = general.GetAppSettingsValue("dateFormat");
            ViewBag.dateFormatJs = general.GetAppSettingsValue("dateFormatJs");
            SetupSelectLists(model);
            return View(model);
        }

        public void SetupSelectLists(UserProfileViewModel model)
        {
            model.GenderSelectList = general.GetGlobalOptionSets("Gender", model.GenderId);
            model.CountrySelectList = general.GetCountryList(model.CountryName);
        }

        [HttpPost]
        public ActionResult EditMyProfile(UserProfileViewModel model, HttpPostedFileBase ProfilePicture)
        {
            ValidateEditMyProfile(model);

            //These 2 fields can only be edited by system admin in user management section. When normal user edit profile from here, these 2 fields are not required.
            ModelState.Remove("UserStatusId");
            ModelState.Remove("UserRoleIdList");
            ModelState.Remove("Password");

            if (!ModelState.IsValid)
            {
                ViewBag.DateFormat = general.GetAppSettingsValue("dateFormat");
                ViewBag.dateFormatJs = general.GetAppSettingsValue("dateFormatJs");
                SetupSelectLists(model);
                return View(model);
            }

            bool result = SaveMyProfile(model);
            if (result == false)
            {
                TempData["NotifyFailed"] = Resource.FailedExceptionError;
            }
            else
            {
                ModelState.Clear();
                TempData["NotifySuccess"] = Resource.RecordSavedSuccessfully;
            }
            return RedirectToAction("myprofile");
        }

        public void ValidateEditMyProfile(UserProfileViewModel model)
        {
            if (model != null)
            {
                bool usernameExist = general.UsernameExists(model.Username, model.AspNetUserId);
                bool emailExist = general.EmailExists(model.EmailAddress, model.AspNetUserId);
                if (usernameExist)
                {
                    ModelState.AddModelError("UserName", Resource.UsernameTaken);
                }
                if (emailExist)
                {
                    ModelState.AddModelError("EmailAddress", Resource.EmailAddressTaken);
                }
            }
        }

        public void AssignUserProfileValues(UserProfile userProfile, UserProfileViewModel model)
        {
            userProfile.FullName = model.FullName;
            userProfile.FirstName = model.FirstName;
            userProfile.LastName = model.LastName;
            userProfile.DateOfBirth = model.DateOfBirth;
            userProfile.PhoneNumber = model.PhoneNumber;
            userProfile.IDCardNumber = model.IDCardNumber;
            userProfile.GenderId = model.GenderId;
            userProfile.CountryName = model.CountryName;
            userProfile.PostalCode = model.PostalCode;
            userProfile.Address = model.Address;
            userProfile.ModifiedBy = model.ModifiedBy;
            userProfile.ModifiedOn = general.GetSystemTimeZoneDateTimeNow();
            userProfile.IsoUtcModifiedOn = general.GetIsoUtcNow();
        }

        public string GetMyProfilePictureName(string userid)
        {
            string fileName = "";
            if (!string.IsNullOrEmpty(userid))
            {
                string upId = general.GetUserProfileId(userid);
                string profilePictureTypeId = general.GetGlobalOptionSetId(ProjectEnum.UserAttachment.ProfilePicture.ToString(), "UserAttachment");
                fileName = db.UserAttachments.Where(a => a.UserProfileId == upId && a.AttachmentTypeId == profilePictureTypeId).OrderByDescending(o => o.CreatedOn).Select(a => a.UniqueFileName).FirstOrDefault();
            }
            return fileName;
        }

        public bool SaveMyProfile(UserProfileViewModel model)
        {
            bool result = true;
            if (model != null)
            {
                try
                {
                    model.ModifiedBy = User.Identity.GetUserId();
                    //edit
                    if (model.Id != null)
                    {
                        //save user profile
                        UserProfile userProfile = db.UserProfiles.FirstOrDefault(a => a.Id == model.Id);
                        AssignUserProfileValues(userProfile, model);
                        db.Entry(userProfile).State = EntityState.Modified;
                        //save AspNetUsers and UserProfile
                        db.SaveChanges();
                        if (model.ProfilePicture != null)
                        {
                            string profilePicture = general.GetGlobalOptionSetId(ProjectEnum.UserAttachment.ProfilePicture.ToString(), "UserAttachment");
                            general.SaveUserAttachment(model.ProfilePicture, userProfile.Id, profilePicture, User.Identity.GetUserId());
                        }
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return result;
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            PreRegistrationViewModel model = new PreRegistrationViewModel();
            model.RERTypeList = general.GetRERTypeList(model.RERTypeId);
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult EmailVerification()
        {
            return View();
        }

       private string OneTimePassword()
        {
            Random rnd = new Random();
            int value = rnd.Next(100000, 999999);
            return value.ToString();
        }

        [AllowAnonymous]
        public ActionResult Verify(string id)
        {
            PreRegistration registration = db.PreRegistration.FirstOrDefault(x => x.Id == id);

            SendVerifiedEmail(registration);

            return Redirect("/Account/Login");
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyOTP()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(PreRegistrationViewModel model)
        {
            try
            {
                string username = string.Format("{0}{1}", model.FirstName.Replace(" ", string.Empty), model.LastName.Replace(" ", string.Empty));
                string password = string.Format("{0}{1}{2}", model.FirstName.Replace(" ", string.Empty), model.LastName.Replace(" ", string.Empty), Guid.NewGuid().ToString().Substring(0, 4));

                PreRegistration registration = new PreRegistration();
                registration.Id = Guid.NewGuid().ToString();
                registration.LastName = model.LastName;
                registration.FirstName = model.FirstName;
                registration.RERTypeId = model.RERTypeId;
                registration.EmailAddress = model.EmailAddress;
                registration.MobileNumber = model.MobileNumber;
                registration.TempUsername = username;
                registration.TempPassword = password;
                registration.RegistrationStatusId = "200257E4-E6C8-4159-8A6F-4475E0A95B32";
                registration.CreatedOn = DateTime.Now;
                db.PreRegistration.Add(registration);
                db.SaveChanges();

                AspNetUsers aspNetUser = new AspNetUsers();
                aspNetUser.Id = Guid.NewGuid().ToString();
                aspNetUser.UserName = username.ToLower();
                aspNetUser.Email = model.EmailAddress;
                aspNetUser.EmailConfirmed = false;
                aspNetUser.PasswordHash = HashPassword(password);
                aspNetUser.SecurityStamp = Guid.NewGuid().ToString();
                aspNetUser.PhoneNumber = model.MobileNumber;
                aspNetUser.PhoneNumberConfirmed = false;
                aspNetUser.TwoFactorEnabled = false;
                aspNetUser.LockoutEnabled = true;
                aspNetUser.AccessFailedCount = 0;
                db.AspNetUsers.Add(aspNetUser);
                db.SaveChanges();

                UserProfile profile = new UserProfile();
                profile.Id = Guid.NewGuid().ToString();
                profile.AspNetUserId = aspNetUser.Id;
                profile.FirstName = model.FirstName;
                profile.LastName = model.LastName;
                profile.FullName = string.Format("{0} {1}", model.FirstName, model.LastName);
                profile.PhoneNumber = model.MobileNumber;
                profile.UserStatusId = "6A1672F3-4C0F-41F4-8D38-B25C97C0BCB2";
                profile.IsoUtcCreatedOn = DateTime.Now.ToString();
                db.UserProfiles.Add(profile);
                db.SaveChanges();

                SendInitialEmail(registration);

                return Redirect("/Account/EmailVerification");
            }
            catch (Exception ex)
            {
                TempData["NotifyFailed"] = Resource.FailedExceptionError;
                return RedirectToAction("register");
            }
        }

        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }

        public static bool SendOTP(string mobileNumber, string oneTimePassword)
        {
            var data = JsonConvert.SerializeObject(new OTPModel()
            {
                SenderId = "BUSYBEE",
                ApiKey = "5f/BmFPxs6Palj2Qm34f+r2005s3gP5pSvcDsQdrlHo=",
                ClientId = "4293fa8e-ee21-4707-a9c4-3c7416617189",
                Message = string.Format("Dear Stakeholder, your verification code is {0}. Use this to validate your registration.", oneTimePassword),
                MobileNumbers = mobileNumber
            });
            var client = new RestClient("https://app.brandtxt.io/api/v2/SendSMS");
            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", data, ParameterType.RequestBody);
            var restResponse = client.ExecutePost(request);
            if (!restResponse.IsSuccessful && restResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                if (restResponse.ErrorMessage != null)
                    throw new ArgumentException(restResponse.ErrorMessage);

                return false;
            }
            return true;
        }

        public void SendInitialEmail(PreRegistration model)
        {
            try
            {
                string verificationLink = "https://ofas-web-internal.beesites.net/account/verify/" + model.Id;

                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress("support@mybusybee.net");
                message.To.Add(new MailAddress(model.EmailAddress));
                message.Subject = "Email Confirmation";
                message.IsBodyHtml = true;
                message.Body = "<p>Dear User " + model.FirstName + ",<p>" +
                    "<p>Your request for registration has been received. Before proceeding, please validate your email by clicking the link below: <p>" +
                    "<p><a href=" + verificationLink + ">" + verificationLink + "</a><p>" +
                    "<br />" +
                    "Thank you,<br /><strong>Site Admin</strong>";
                smtp.Port = 587;
                smtp.Host = "smtp-relay.sendinblue.com";
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("support@mybusybee.net", "p6wL0tWIb9kEQhHG");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SendVerifiedEmail(PreRegistration model)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress("support@mybusybee.net");
                message.To.Add(new MailAddress(model.EmailAddress));
                message.Subject = "Email Verified";
                message.IsBodyHtml = true;
                message.Body = "<p>Dear User " + model.FirstName + ",<p>" +
                    "<p>Thank you for confirming your email.<p>" +
                    "<p><strong>Your username is:</strong> " + model.TempUsername + "<br />" +
                    "<strong>Your password is:</strong> " + model.TempPassword + "</p>" +
                    "<br />" +
                    "Thank you,<br /><strong>Site Admin</strong>";
                smtp.Port = 587;
                smtp.Host = "smtp-relay.sendinblue.com";
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("support@mybusybee.net", "p6wL0tWIb9kEQhHG");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void RegisterUserProfile(string userId)
        {
            UserProfile userProfile = new UserProfile();
            userProfile.Id = Guid.NewGuid().ToString();
            userProfile.AspNetUserId = userId;
            userProfile.UserStatusId = general.GetGlobalOptionSetId(ProjectEnum.UserStatus.Registered.ToString(), "UserStatus");
            userProfile.CreatedBy = userId;
            userProfile.CreatedOn = general.GetSystemTimeZoneDateTimeNow();
            userProfile.IsoUtcCreatedOn = general.GetIsoUtcNow();
            db.UserProfiles.Add(userProfile);
            db.SaveChanges();
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                AspNetUsers user = db.AspNetUsers.FirstOrDefault(a => a.Email == model.Email);
                //var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    ModelState.Clear();
                    TempData["NotifySuccess"] = Resource.ResetPasswordEmailSent;
                    return RedirectToAction("login", "account");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                EmailTemplate emailTemplate = general.EmailTemplateForForgotPassword(user.UserName, callbackUrl);
                general.SendEmail(user.Email, emailTemplate.Subject, emailTemplate.Body);

                ModelState.Clear();
                TempData["NotifySuccess"] = Resource.ResetPasswordEmailSent;
                return RedirectToAction("login", "account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = db.AspNetUsers.FirstOrDefault(a => a.Email == model.Email);
            //var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                ModelState.Clear();
                TempData["NotifySuccess"] = Resource.YourPasswordResetSuccessfully;
                return RedirectToAction("login", "account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                ModelState.Clear();
                TempData["NotifySuccess"] = Resource.YourPasswordResetSuccessfully;
                return RedirectToAction("login", "account");
            }
            TempData["NotifyFailed"] = Resource.FailedToResetPassword;
            AddErrors(result);
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("login", "account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }

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

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}