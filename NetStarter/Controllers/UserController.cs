using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ExcelDataReader;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetStarter.Models;
using System.Data;
using System.Globalization;
using NetStarter.Resources;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace NetStarter.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private DefaultDBContext db = new DefaultDBContext();
        private GeneralController general = new GeneralController();

        public UserController()
        {
        }

        public UserController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.UserManagement, "true", "", "", "")]
        public ViewResult Index()
        {
            return View();
        }


        public ActionResult GetPartialViewUser()
        {
            UserProfileListing userProfileListing = new UserProfileListing();
            userProfileListing.Listing = ReadUserProfileList();
            return PartialView("~/Views/User/_MainList.cshtml", userProfileListing);
        }

        public List<UserProfileViewModel> ReadUserProfileList()
        {
            List<UserProfileViewModel> userList = new List<UserProfileViewModel>();
            string datetimeFormat = general.GetAppSettingsValue("datetimeFormat");
            
            userList = (from t1 in db.UserProfiles
                        join t2 in db.AspNetUsers on t1.AspNetUserId equals t2.Id
                        join t3 in db.GlobalOptionSets on t1.UserStatusId equals t3.Id into g1
                        from t4 in g1.DefaultIfEmpty()
                        orderby t1.CreatedOn
                        select new UserProfileViewModel
                        {
                            Id = t1.Id,
                            FullName = t1.FullName,
                            Username = t2.UserName,
                            AspNetUserId = t1.AspNetUserId,
                            UserStatusName = t4 == null ? "" : t4.DisplayName,
                            EmailAddress = t2.Email,
                            PhoneNumber = t1.PhoneNumber,
                            CountryName = t1.CountryName,
                            Address = t1.Address,
                            CreatedOn = t1.CreatedOn,
                            IsoUtcCreatedOn = t1.IsoUtcCreatedOn
                        }).ToList();

            var finalList = userList.Select((value, index) => new UserProfileViewModel {
                Id = value.Id,
                FullName = value.FullName,
                Username = value.Username,
                AspNetUserId = value.AspNetUserId,
                UserStatusName = value.UserStatusName,
                EmailAddress = value.EmailAddress,
                PhoneNumber = value.PhoneNumber,
                CountryName = value.CountryName,
                Address = value.Address,
                CreatedOn = value.CreatedOn,
                IsoUtcCreatedOn = value.IsoUtcCreatedOn,
                FormattedCreatedOn = general.GetFormattedDateTime(value.IsoUtcCreatedOn),
                FormattedCreatedOnOrder = index
            }).ToList();

            return finalList;
        }

        public UserProfileViewModel GetUserProfileViewModel(string Id, string type)
        {
            UserProfileViewModel model = new UserProfileViewModel();
            string profilePicTypeId = general.GetGlobalOptionSetId(ProjectEnum.UserAttachment.ProfilePicture.ToString(), "UserAttachment");
            model = (from t1 in db.UserProfiles
                     join t2 in db.AspNetUsers on t1.AspNetUserId equals t2.Id
                     where t1.Id == Id
                     select new UserProfileViewModel
                     {
                         Id = t1.Id,
                         AspNetUserId = t1.AspNetUserId,
                         FullName = t1.FullName,
                         IDCardNumber = t1.IDCardNumber,
                         FirstName = t1.FirstName,
                         LastName = t1.LastName,
                         DateOfBirth = t1.DateOfBirth,
                         PhoneNumber = t1.PhoneNumber,
                         Address = t1.Address,
                         PostalCode = t1.PostalCode,
                         Username = t2.UserName,
                         EmailAddress = t2.Email,
                         UserStatusId = t1.UserStatusId,
                         GenderId = t1.GenderId,
                         CountryName = t1.CountryName,
                         CreatedBy = t1.CreatedBy,
                         ModifiedBy = t1.ModifiedBy,
                         CreatedOn = t1.CreatedOn,
                         ModifiedOn = t1.ModifiedOn,
                         IsoUtcCreatedOn = t1.IsoUtcCreatedOn,
                         IsoUtcModifiedOn = t1.IsoUtcModifiedOn,
                         IsoUtcDateOfBirth = t1.IsoUtcDateOfBirth
                     }).FirstOrDefault();
            model.UserStatusName = db.GlobalOptionSets.Where(a => a.Id == model.UserStatusId).Select(a => a.DisplayName).DefaultIfEmpty("").FirstOrDefault();
            model.UserTypeIdList = (from t1 in db.AspNetUserTypes
                                    join t2 in db.GlobalOptionSets on t1.UserTypeId equals t2.Id
                                    where t1.UserId == model.AspNetUserId
                                    select t2.DisplayName).ToList();
            model.UserTypeName = String.Join(", ", model.UserTypeIdList);
            model.GenderName = db.GlobalOptionSets.Where(a => a.Id == model.GenderId).Select(a => a.DisplayName).DefaultIfEmpty("").FirstOrDefault();
            model.ProfilePictureFileName = db.UserAttachments.Where(a => a.UserProfileId == model.Id && a.AttachmentTypeId == profilePicTypeId).OrderByDescending(a => a.CreatedOn).Select(a => a.UniqueFileName).FirstOrDefault();
            if (type == "View")
            {
                model.FormattedDateOfBirth = model.DateOfBirth?.ToString(general.GetAppSettingsValue("dateFormat"));
                model.CreatedAndModified = general.GetCreatedAndModified(model.CreatedBy, model.IsoUtcCreatedOn, model.ModifiedBy, model.IsoUtcModifiedOn);
            }
            return model;
        }

        public void SetupSelectLists(UserProfileViewModel model)
        {
            model.GenderSelectList = general.GetGlobalOptionSets("Gender", model.GenderId);
            model.UserTypeSelectList = general.GetUserTypesForMultiSelect(model.UserTypeIdList);
            model.UserStatusSelectList = general.GetGlobalOptionSets("UserStatus", model.UserStatusId);
            model.CountrySelectList = general.GetCountryList(model.CountryName);
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.UserManagement, "", "true", "true", "")]
        public ActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Import(ImportFromExcel model, HttpPostedFileBase File)
        {
            try
            {
                List<string> errors = new List<string>();
                List<ImportFromExcelError> errorsList = new List<ImportFromExcelError>();
                var users = new List<ApplicationUser>();

                UserProfileViewModel upModel = new UserProfileViewModel();

                int successCount = 0;
                int dtRowsCount = 0;
                List<string> columns = new List<string>();
                using (var reader = ExcelReaderFactory.CreateReader(File.InputStream))
                {
                    var ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });

                    var dt = ds.Tables[0];

                    foreach (var col in dt.Columns.Cast<DataColumn>())
                    {
                        col.ColumnName = col.ColumnName.Replace("*", "");
                        columns.Add(col.ColumnName);
                    }
                    dtRowsCount = dt.Rows.Count;

                    errors = general.ValidateColumns(columns, new List<string>
                    {
                        "Username","Email Address","Password",
                        "Confirm Password","Full Name","First Name",
                        "Last Name","Phone Number","Country",
                    });

                    //if all columns validated
                    if (errors.Count == 0)
                    {
                        for (int i = 0; i < dtRowsCount; i++)
                        {
                            try
                            {
                                string userName = dt.Rows[i].Field<string>("Username");
                                string email = dt.Rows[i].Field<string>("Email Address");
                                string pass = dt.Rows[i].Field<string>("Password");
                                string confirm_pass = dt.Rows[i].Field<string>("Confirm Password");
                                string fullName = dt.Rows[i].Field<string>("Full Name");
                                string phone = dt.Rows[i].Field<string>("Phone Number");
                                string country = dt.Rows[i].Field<string>("Country");
                                string firstName = dt.Rows[i].Field<string>("First Name");
                                string lastName = dt.Rows[i].Field<string>("Last Name");

                                upModel.FullName = fullName;
                                upModel.PhoneNumber = phone;
                                upModel.Password = pass;
                                upModel.ConfirmPassword = confirm_pass;
                                upModel.EmailAddress = email;
                                upModel.Username = userName;
                                upModel.CountryName = country;
                                upModel.FirstName = firstName;
                                upModel.LastName = lastName;

                                errors = general.ValidateImportUserFromExcel(upModel);

                                if (errors.Count() > 0)
                                {
                                    ImportFromExcelError importFromExcelError = new ImportFromExcelError();
                                    importFromExcelError.Row = $"At Row {i + 2}";
                                    importFromExcelError.Errors = errors;
                                    errorsList.Add(importFromExcelError);
                                    continue;
                                }

                                //after finish assign value to upModel, create user here
                                var user = new ApplicationUser { UserName = upModel.Username, Email = upModel.EmailAddress };
                                var creationResult = await UserManager.CreateAsync(user, upModel.Password); //create user and save in db
                                if (creationResult.Succeeded)
                                {
                                    //when create user success, add user detail in UserProfile table
                                    UserProfile userProfile = new UserProfile();
                                    userProfile.Id = Guid.NewGuid().ToString();

                                    //write other things like Full name, First name, Last name etc...
                                    userProfile.AspNetUserId = user.Id;
                                    userProfile.FullName = upModel.FullName;
                                    userProfile.FirstName = upModel.FirstName;
                                    userProfile.LastName = upModel.LastName;
                                    userProfile.PhoneNumber = upModel.PhoneNumber;
                                    userProfile.CountryName = upModel.CountryName;
                                    userProfile.UserStatusId = general.GetGlobalOptionSetId(ProjectEnum.UserStatus.Registered.ToString(), "UserStatus");
                                    userProfile.CreatedBy = User.Identity.GetUserId();
                                    userProfile.CreatedOn = general.GetSystemTimeZoneDateTimeNow();
                                    userProfile.IsoUtcCreatedOn = general.GetIsoUtcNow();
                                    db.UserProfiles.Add(userProfile);
                                    db.SaveChanges();

                                    //assign all user with normal user role
                                    var assignNormalUserRole = UserManager.AddToRole(user.Id, "Normal User");

                                    successCount++;
                                }
                                ModelState.Clear();
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"{ex.Message} - Row: {i + 2}");
                            }
                        }
                    }
                    else
                    {
                        ImportFromExcelError importFromExcelError = new ImportFromExcelError();
                        importFromExcelError.Row = Resource.InvalidUserTemplate;
                        importFromExcelError.Errors = errors;
                        errorsList.Add(importFromExcelError);
                    }
                }
                if (errors.Count > 0)
                {
                    TempData["ErrorList"] = errorsList;
                    TempData["UploadResult"] = $"{successCount} {Resource.outof} {dtRowsCount} {Resource.recordsuploaded}";
                    return RedirectToAction("import");
                }
                TempData["NotifySuccess"] = Resource.RecordsImportedSuccessfully;
            }
            catch (Exception ex)
            {
                TempData["NotifyFailed"] = Resource.FailedExceptionError;
            }
            return RedirectToAction("index");
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.UserManagement, "", "true", "true", "")]
        public ActionResult Edit(string Id)
        {
            UserProfileViewModel model = new UserProfileViewModel();
            if (Id != null)
            {
                model = GetUserProfileViewModel(Id, "Edit");
            }
            ViewBag.DateFormat = general.GetAppSettingsValue("dateFormat");
            ViewBag.dateFormatJs = general.GetAppSettingsValue("dateFormatJs");
            SetupSelectLists(model);
            return View(model);
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.UserManagement, "true", "", "", "")]
        public ActionResult ViewRecord(string Id)
        {
            UserProfileViewModel model = new UserProfileViewModel();
            if (Id != null)
            {
                model = GetUserProfileViewModel(Id, "View");
            }
            ViewBag.DateFormat = general.GetAppSettingsValue("dateFormat");
            ViewBag.dateFormatJs = general.GetAppSettingsValue("dateFormatJs");
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(UserProfileViewModel model)
        {
            ValidateModel(model);

            if (!ModelState.IsValid)
            {
                ViewBag.DateFormat = general.GetAppSettingsValue("dateFormat");
                ViewBag.dateFormatJs = general.GetAppSettingsValue("dateFormatJs");
                SetupSelectLists(model);
                return View(model);
            }

            bool result = await SaveRecord(model);
            if (result == false)
            {
                TempData["NotifyFailed"] = Resource.FailedExceptionError;
            }
            else
            {
                ModelState.Clear();
                TempData["NotifySuccess"] = Resource.RecordSavedSuccessfully;
            }
            return RedirectToAction("index", "user");
        }

        public void ValidateModel(UserProfileViewModel model)
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
                if (model.Id == null)
                {
                    if (model.Password == null)
                    {
                        ModelState.AddModelError("Password", Resource.PasswordRequired);
                    }
                    if (model.ConfirmPassword == null)
                    {
                        ModelState.AddModelError("ConfirmPassword", Resource.ConfirmPasswordRequired);
                    }
                }
                else
                {
                    ModelState.Remove("Password");
                    ModelState.Remove("ConfirmPassword");
                }
                if (model.UserStatusId == null || model.UserStatusId == "null")
                {
                    ModelState.AddModelError("UserStatusId", Resource.StatusRequired);
                }
                if (model.UserTypeIdList == null || model.UserTypeIdList.Count() == 0)
                {
                    ModelState.AddModelError("UserRoleIdList", Resource.RoleRequired);
                }
                if (string.IsNullOrEmpty(model.CountryName))
                {
                    ModelState.AddModelError("CountryName", Resource.CountryRequired);
                }
            }
        }

        public void AssignUserProfileValues(UserProfile userProfile, UserProfileViewModel model)
        {
            userProfile.FullName = model.FullName;
            userProfile.FirstName = model.FirstName;
            userProfile.LastName = model.LastName;
            userProfile.DateOfBirth = model.DateOfBirth;
            if (model.DateOfBirth != null)
            {
                userProfile.IsoUtcDateOfBirth = model.DateOfBirth.Value.ToString("o", CultureInfo.InvariantCulture);
            }
            userProfile.PhoneNumber = model.PhoneNumber;
            userProfile.IDCardNumber = model.IDCardNumber;
            userProfile.GenderId = model.GenderId;
            userProfile.CountryName = model.CountryName;
            userProfile.PostalCode = model.PostalCode;
            userProfile.Address = model.Address;
            userProfile.UserStatusId = model.UserStatusId;
            if (model.Id == null)
            {
                userProfile.CreatedBy = model.CreatedBy;
                userProfile.CreatedOn = general.GetSystemTimeZoneDateTimeNow();
                userProfile.IsoUtcCreatedOn = general.GetIsoUtcNow();
            }
            else
            {
                userProfile.ModifiedBy = model.ModifiedBy;
                userProfile.ModifiedOn = general.GetSystemTimeZoneDateTimeNow();
                userProfile.IsoUtcModifiedOn = general.GetIsoUtcNow();
            }
        }

        public async Task<bool> SaveRecord(UserProfileViewModel model)
        {
            bool result = true;
            if (model != null)
            {
                string userId = "";
                string userProfileId = "";
                string profilePictureId = "";
                string type = "";
                try
                {
                    model.CreatedBy = User.Identity.GetUserId();
                    model.ModifiedBy = User.Identity.GetUserId();
                    //edit
                    if (model.Id != null)
                    {
                        type = "update";
                        //if change username & email, need to change in AspNetUsers table
                        AspNetUsers aspNetUsers = db.AspNetUsers.FirstOrDefault(a => a.Id == model.AspNetUserId);
                        aspNetUsers.UserName = model.Username;
                        aspNetUsers.Email = model.EmailAddress;
                        db.Entry(aspNetUsers).State = EntityState.Modified;

                        //save user profile
                        UserProfile userProfile = db.UserProfiles.FirstOrDefault(a => a.Id == model.Id);
                        AssignUserProfileValues(userProfile, model);
                        db.Entry(userProfile).State = EntityState.Modified;

                        //save AspNetUsers and UserProfile
                        db.SaveChanges();

                        userProfileId = userProfile.Id;

                        if (model.UserTypeIdList != null)
                        {
                            var existingUserTypes = db.AspNetUserTypes.Where(x => x.UserId == model.AspNetUserId).ToList();
                            db.AspNetUserTypes.RemoveRange(existingUserTypes);
                            db.SaveChanges();

                            foreach(var item in model.UserTypeIdList)
                            {
                                var userType = db.GlobalOptionSets.FirstOrDefault(x => x.DisplayName == item);
                                AspNetUserTypes aspUserType = new AspNetUserTypes();
                                aspUserType.UserId = model.AspNetUserId;
                                aspUserType.UserTypeId = userType.Id;
                                db.AspNetUserTypes.Add(aspUserType);
                                db.SaveChanges();
                            }

                            string[] removeRoles = UserManager.GetRoles(model.AspNetUserId).ToArray();
                            UserManager.RemoveFromRoles(model.AspNetUserId, removeRoles);

                            foreach (var item in model.UserTypeIdList)
                            {
                                var userType = db.GlobalOptionSets.FirstOrDefault(x => x.DisplayName == item);
                                AspNetUserRoles aspUserRole = new AspNetUserRoles();
                                aspUserRole.UserId = model.AspNetUserId;
                                aspUserRole.RoleId = userType.ReferenceId;
                                db.AspNetUserRoles.Add(aspUserRole);
                                db.SaveChanges();
                            }
                        }

                    }
                    //register new user record
                    else
                    {
                        type = "create";
                        var user = new ApplicationUser { UserName = model.Username, Email = model.EmailAddress };
                        //var creationResult = account.CreateNewUserIdentity(user, model.Password);
                        var creationResult = await UserManager.CreateAsync(user, model.Password); //create user and save in db
                        if (creationResult.Succeeded)
                        {
                            userId = user.Id;
                            if (model.UserTypeSelectList != null)
                            {
                                string[] roles = model.UserTypeIdList.ToArray();
                                var assignRoleResult = UserManager.AddToRoles(user.Id, roles);
                            }

                            //save user profile
                            UserProfile userProfile = new UserProfile();
                            AssignUserProfileValues(userProfile, model);
                            userProfile.Id = Guid.NewGuid().ToString();
                            userProfile.AspNetUserId = user.Id;
                            userProfile.UserStatusId = general.GetGlobalOptionSetId(ProjectEnum.UserStatus.Registered.ToString(), "UserStatus");
                            db.UserProfiles.Add(userProfile);
                            db.SaveChanges();
                            userProfileId = userProfile.Id;

                            // Send an email with this link
                            if (general.GetAppSettingsValue("confirmEmailToLogin") == "true")
                            {
                                string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                                EmailTemplate emailTemplate = general.EmailTemplateForConfirmEmail(user.UserName, callbackUrl);
                                general.SendEmail(user.Email, emailTemplate.Subject, emailTemplate.Body);
                            }
                        }
                    }

                    if (model.ProfilePicture != null)
                    {
                        string profilePicture = general.GetGlobalOptionSetId(ProjectEnum.UserAttachment.ProfilePicture.ToString(), "UserAttachment");
                        general.SaveUserAttachment(model.ProfilePicture, userProfileId, profilePicture, User.Identity.GetUserId());
                    }

                }
                catch (Exception ex)
                {
                    //Exception when creating new record, means record creation incomplete due to error, undo the record creation
                    if (type == "create")
                    {
                        if (!string.IsNullOrEmpty(userId))
                        {
                            AspNetUsers aspNetUsers = db.AspNetUsers.FirstOrDefault(a => a.Id == userId);
                            if (aspNetUsers != null)
                            {
                                db.AspNetUsers.Remove(aspNetUsers);
                                db.SaveChanges();
                            }
                        }
                        if (!string.IsNullOrEmpty(userProfileId))
                        {
                            UserProfile userProfile = db.UserProfiles.FirstOrDefault(a => a.Id == userProfileId);
                            if (userProfile != null)
                            {
                                db.UserProfiles.Remove(userProfile);
                                db.SaveChanges();
                            }
                        }
                        if (!string.IsNullOrEmpty(profilePictureId))
                        {
                            UserAttachment userAttachment = db.UserAttachments.FirstOrDefault(a => a.Id == profilePictureId);
                            if (userAttachment != null)
                            {
                                db.UserAttachments.Remove(userAttachment);
                                db.SaveChanges();
                            }
                        }
                    }
                    return false;
                }
            }
            return result;
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.UserManagement, "true", "true", "true", "")]
        public ActionResult AdminChangePassword(string Id)
        {
            AdminChangePasswordViewModel model = new AdminChangePasswordViewModel();
            if (Id != null)
            {
                model = GetAdminChangePasswordViewModel(Id);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> AdminChangePassword(AdminChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //Not allowed to change demo account's password
            string username = db.AspNetUsers.Where(a => a.Id == model.AspNetUserId).Select(a => a.UserName).FirstOrDefault();
            if (general.GetAppSettingsValue("environment") == "prod" && username == "nsadmin")
            {
                TempData["NotifyFailed"] = Resource.NotAllowToChangePasswordForDemoAccount;
                return RedirectToAction("index", "dashboard");
            }

            bool userExists = db.AspNetUsers.Any(a => a.Id == model.AspNetUserId);
            if (userExists)
            {
                var token = await UserManager.GeneratePasswordResetTokenAsync(model.AspNetUserId);
                var result = await UserManager.ResetPasswordAsync(model.AspNetUserId, token, model.NewPassword);
                if (result.Succeeded)
                {
                    ModelState.Clear();
                    TempData["NotifySuccess"] = Resource.PasswordChangedSuccessfully;

                    //send email to notify the user
                    string userEmail = db.AspNetUsers.Where(a => a.Id == model.AspNetUserId).Select(a => a.Email).FirstOrDefault();
                    string resetById = User.Identity.GetUserId();
                    string resetByName = db.UserProfiles.Where(a => a.AspNetUserId == resetById).Select(a => a.FullName).DefaultIfEmpty("").FirstOrDefault();
                    EmailTemplate emailTemplate = general.EmailTemplateForPasswordResetByAdmin(model.Username, resetByName, model.NewPassword);
                    general.SendEmail(userEmail, emailTemplate.Subject, emailTemplate.Body);
                }
                else
                {
                    TempData["NotifyFailed"] = Resource.FailedToResetPassword;
                }
                return RedirectToAction("index", "user");
            }
            else
            {
                TempData["NotifyFailed"] = Resource.UserNotExist;
                return RedirectToAction("index", "user");
            }
        }

        public AdminChangePasswordViewModel GetAdminChangePasswordViewModel(string Id)
        {
            AdminChangePasswordViewModel model = new AdminChangePasswordViewModel();
            model = (from t1 in db.UserProfiles
                     join t2 in db.AspNetUsers on t1.AspNetUserId equals t2.Id
                     where t1.Id == Id
                     select new AdminChangePasswordViewModel
                     {
                         Id = Id,
                         FullName = t1.FullName,
                         Username = t2.UserName,
                         EmailAddress = t2.Email,
                         AspNetUserId = t1.AspNetUserId
                     }).FirstOrDefault();
            return model;
        }

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.UserManagement, "", "", "", "true")]
        public ActionResult Delete(string Id)
        {
            try
            {
                if (Id != null)
                {
                    AspNetUsers users = db.AspNetUsers.Where(a => a.Id == Id).FirstOrDefault();
                    if (users != null)
                    {
                        if (general.GetAppSettingsValue("environment") == "prod" && users.UserName == "nsadmin")
                        {
                            TempData["NotifyFailed"] = Resource.DemoAccountCannotBeDeleted;
                            return RedirectToAction("index");
                        }
                        db.AspNetUsers.Remove(users);
                        db.SaveChanges();
                    }
                }
                TempData["NotifySuccess"] = Resource.RecordDeletedSuccessfully;
            }
            catch (Exception ex)
            {
                AspNetUsers users = db.AspNetUsers.Where(a => a.Id == Id).FirstOrDefault();
                if (users == null)
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

        public ActionResult GetUsers()
        {
            IList<UserProfileViewModel> userProfileViewModels = new List<UserProfileViewModel>();

            UserProfileViewModel model = new UserProfileViewModel();
            string profilePicTypeId = general.GetGlobalOptionSetId(ProjectEnum.UserAttachment.ProfilePicture.ToString(), "UserAttachment");
            userProfileViewModels = (from t1 in db.UserProfiles
                                     join t2 in db.AspNetUsers on t1.AspNetUserId equals t2.Id
                                     select new UserProfileViewModel
                                     {
                                         Id = t1.Id,
                                         AspNetUserId = t1.AspNetUserId,
                                         FullName = t1.FullName,
                                         IDCardNumber = t1.IDCardNumber,
                                         FirstName = t1.FirstName,
                                         LastName = t1.LastName,
                                         DateOfBirth = t1.DateOfBirth,
                                         PhoneNumber = t1.PhoneNumber,
                                         Address = t1.Address,
                                         PostalCode = t1.PostalCode,
                                         Username = t2.UserName,
                                         EmailAddress = t2.Email,
                                         UserStatusId = t1.UserStatusId,
                                         GenderId = t1.GenderId,
                                         CountryName = t1.CountryName,
                                         CreatedBy = t1.CreatedBy,
                                         ModifiedBy = t1.ModifiedBy,
                                         CreatedOn = t1.CreatedOn,
                                         ModifiedOn = t1.ModifiedOn,
                                         IsoUtcCreatedOn = t1.IsoUtcCreatedOn,
                                         IsoUtcModifiedOn = t1.IsoUtcModifiedOn,
                                         IsoUtcDateOfBirth = t1.IsoUtcDateOfBirth
                                     }).ToList();

            return Json(userProfileViewModels, JsonRequestBehavior.AllowGet);
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

    }
}