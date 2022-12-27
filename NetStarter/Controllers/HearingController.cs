using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using NetStarter.Helper;
using NetStarter.Models;

namespace NetStarter.Controllers
{
    [Authorize]
    public class HearingController : Controller
    {
        private DefaultDBContext db = new DefaultDBContext();

        [CustomAuthorizeFilter(ProjectEnum.ModuleCode.Hearing, "true", "", "", "")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RequestHearing()
        {
            return View();
        }

        public ActionResult HearingList()
        {
            return View();
        }

        [HttpPost]        
        public async Task<ActionResult> PostRequestHearing(Hearing data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            bool saved;

            var request = new Hearing()
            {
                Description = data.Description,
                HearingType = data.HearingType,
                Schedule = data.Schedule,
                MeetingLink = data.MeetingLink,
                HearingStatus = 3,
                CreatedBy = 0,
                DateCreated = DateTime.Now
            };

            try
            {
                db.Hearings.Add(request);
                saved = await db.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

            return Json(saved, JsonRequestBehavior.AllowGet);
        }

        //[HttpPut]
        public async Task<ActionResult> UpdateHearing(Hearing data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            bool update = false;
            var hearing = db.Hearings.Where(x => x.Id == data.Id).FirstOrDefault();

            if(hearing != null)
            {
                hearing.Schedule = data.Schedule;
                hearing.MeetingLink = data.MeetingLink;
                hearing.HearingStatus = data.HearingStatus;
                hearing.Description = data.Description;
                hearing.MeetingPassword = data.MeetingPassword;

                update = await db.SaveChangesAsync() > 0;
            }

            if(update && data.HearingStatus == 2)
            {
                // Notify stakeholder/services here.
                string stakeHolderId = "";
                var emailAddress1 = db.AspNetUsers.Where(x => x.Id == stakeHolderId).ToList();

                var hearingInfo = db.Hearings.Where(x => x.Id == data.Id).FirstOrDefault();

                string subject = "Virtual Hearing Cancelled!";
                string htmlMessage = "Dear Stakeholder/services";
                htmlMessage += $"\r\nVirtual Hearing has been cancelled.";
                htmlMessage += $"\r\nId: { hearingInfo.Id}";
                htmlMessage += $"\r\nSchedule: {hearingInfo.Schedule}";
                htmlMessage += $"\r\nMeeting Link: {hearingInfo.MeetingLink}";
                htmlMessage += $"\r\nPassword: {hearingInfo.MeetingPassword}";
                htmlMessage += $"\r\nDescription: {hearingInfo.Description}";

                emailAddress1.ForEach(x => 
                {
                    SendEmail(x.Email, subject, htmlMessage);
                });
            }

            return Json(update, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetHearingList(string searchValue = "", int currentPage = 1, int pageSize = 10, int? userId = null)
        {
            IQueryable<Hearing> items = (from item in db.Hearings
                                         select item);
            //select new Hearing()
            //{
            //    Id = item.Id,
            //    HearingType = item.HearingType,
            //    Description = item.Description,
            //    Schedule = item.Schedule,
            //    MeetingLink = item.MeetingLink,
            //    HearingStatus = item.HearingStatus,
            //    DateCreated = item.DateCreated
            //});            

            if (!string.IsNullOrEmpty(searchValue))
                items = items.Where(x => x.Description.ToLower().Contains(searchValue.ToLower()));

            var paginatedList = await PaginatedList<Hearing>.CreateAsync(items.OrderByDescending(x => x.Id), currentPage, pageSize);

            var pagination = new
            {
                totalItems = items.Count(),
                paginatedList.PageCount,
                paginatedList.PageSize
            };

            return Json(new { items, pagination }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet()]
        public ActionResult GetHearingScheduleById(int id)
        {
            var entity = db.Hearings.Where(x => x.Id == id).FirstOrDefault();

            return Json(entity, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> PostCreateHearingDocument(HearingDocuments data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            bool saved = false;

            var request = new HearingDocuments()
            {
                FileName = data.FileName,
                DocumentType = data.DocumentType,
                HearingId = data.HearingId,
                CreatedBy = 0,
                DateCreated = DateTime.Now
            };

            try
            {
                db.HearingDocuments.Add(request);
                saved = await db.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

            return Json(saved, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Schedule()
        {
            return View();
        }

        public ActionResult ScheduleSet()
        {
            return View();
        }

        public ActionResult UploadHearingDocument()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UploadDocument(IFormFile formFile, int hearingId, int? documentType)
        {
            bool uploadDocument = false;
            //string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", $"documents/hearings");
            string filePath = string.Empty;
            string fileName = string.Empty;
            try
            {
                HttpFileCollectionBase files = Request.Files;

                for (int i = 0; i < files.Count; i++)
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + "UploadedFiles/";  
                    //string filename = Path.GetFileName(Request.Files[i].FileName);  

                    HttpPostedFileBase file = files[i];

                    // Checking for Internet Explorer  
                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        string[] testfiles = file.FileName.Split(new char[] { '\\' });
                        fileName = testfiles[testfiles.Length - 1];
                    }
                    else
                    {
                        fileName = file.FileName;
                    }

                    // Get the complete folder path and store the file inside it.  
                    filePath = Path.Combine(path, fileName);
                    file.SaveAs(filePath);
                    uploadDocument = true;
                }
                //if (formFile != null)
                //{
                //    fileName = ContentDispositionHeaderValue.Parse(formFile.ContentDisposition).FileName.Trim('"');
                //    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(formFile.FileName);
                //    filePath = Path.Combine(folderPath, formFile.FileName.Trim());
                //    using (Stream stream = new FileStream(filePath, FileMode.Create))
                //    {
                //        await formFile.CopyToAsync(stream);
                //        uploadDocument = true;
                //    }
                //}
            }
            catch (Exception ex)
            {
                
            }

            // Save document 
            if (uploadDocument)
            {
                var request = new HearingDocuments()
                {
                    FileName = fileName,
                    FilePath = filePath,
                    DocumentType = documentType,
                    HearingId = hearingId,
                    CreatedBy = 0,
                    DateCreated = DateTime.Now
                };

                db.HearingDocuments.Add(request);
                uploadDocument = await db.SaveChangesAsync() > 0;
            }

            return Json(uploadDocument, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> AssignPersonnel(HearingPersonnel hearingPersonnel)
        {            
            bool saved = false;

            var personnel = db.HearingPersonnels.Where(x => x.HearingId == hearingPersonnel.HearingId 
                && x.UserId == hearingPersonnel.UserId).FirstOrDefault();

            if (personnel == null)
            {
                var request = new HearingPersonnel()
                {
                    HearingId = hearingPersonnel.HearingId,
                    UserId = hearingPersonnel.UserId,
                    CreatedBy = 0,
                    DateCreated = DateTime.Now
                };

                try
                {
                    db.HearingPersonnels.Add(request);
                    saved = await db.SaveChangesAsync() > 0;
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                saved = true;
            }

            if (saved)
            {
                // Update hearing status
                var hearingStatus = db.Hearings.Where(x => x.Id == hearingPersonnel.HearingId).FirstOrDefault();
                if (hearingStatus != null)
                {
                    hearingStatus.HearingStatus = 5; // For signing and initial order.

                    await db.SaveChangesAsync();
                }
            }           

            return Json(saved, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Order()
        {
            return View();
        }

        public ActionResult Notify()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Notify(int id, string stakeHolderId, string serviceId)
        {
            var emailAddress1 = db.AspNetUsers.Where(x => x.Id == stakeHolderId).FirstOrDefault();
            var emailAddress2 = db.AspNetUsers.Where(x => x.Id == serviceId).FirstOrDefault();

            var hearing = db.Hearings.Where(x => x.Id == id).FirstOrDefault();

            string subject = "Virtual Hearing Detail!";
            string htmlMessage = "Dear Stakeholder/services";
            htmlMessage += $"\r\nVirtual Hearing has been scheduled.";
            htmlMessage += $"\r\nId: { hearing.Id}";
            htmlMessage += $"\r\nSchedule: {hearing.Schedule}";
            htmlMessage += $"\r\nMeeting Link: {hearing.MeetingLink}";
            htmlMessage += $"\r\nPassword: {hearing.MeetingPassword}";
            htmlMessage += $"\r\nDescription: {hearing.Description}";

            if (emailAddress1 != null)
                SendEmail(emailAddress1.Email, subject, htmlMessage);

            if (emailAddress2 != null)
                SendEmail(emailAddress2.Email, subject, htmlMessage);


            return Json(true, JsonRequestBehavior.AllowGet);
        }

        private Task SendEmail(string emailAddress, string subject, string htmlMessage)
        {
            var host = System.Configuration.ConfigurationManager.AppSettings["smtpHost"];
            var port = System.Configuration.ConfigurationManager.AppSettings["smtpPort"];
            var userName = System.Configuration.ConfigurationManager.AppSettings["smtpUserName"];
            var password = System.Configuration.ConfigurationManager.AppSettings["smtpPassword"];
                    
            if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(port) && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                var client = new SmtpClient(host, 587)
                {
                    Credentials = new NetworkCredential(userName, password),
                    EnableSsl = Convert.ToBoolean(true),
                    TargetName = "STARTTLS/smtp.gmail.com"
                    //TargetName = "STARTTLS/smtp.office365.com"
                };
                          
                try
                {
                    return client.SendMailAsync(new MailMessage(userName, emailAddress, subject, htmlMessage)
                    {
                        IsBodyHtml = true
                    });
                }
                catch (Exception ex)
                {
                    return Task.CompletedTask; // for update
                }
            }
            else
            {
                return Task.CompletedTask; // for update
            }
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterParticipants(int id, string name1, string emailAddress1, string name2, string emailAddress2,
            string name3, string emailAddress3, string name4, string emailAddress4, string name5, string emailAddress5)
        {
            bool saved = false;            

            if (!string.IsNullOrEmpty(name1) && !string.IsNullOrEmpty(emailAddress1))
            {
                NotifyParticipant(id, name1, emailAddress1);
                saved = true;
            }

            if (!string.IsNullOrEmpty(name2) && !string.IsNullOrEmpty(emailAddress2))
            {
                NotifyParticipant(id, name2, emailAddress2);
                saved = true;
            }

            if (!string.IsNullOrEmpty(name3) && !string.IsNullOrEmpty(emailAddress3))
            {
                NotifyParticipant(id, name3, emailAddress3);
                saved = true;
            }

            if (!string.IsNullOrEmpty(name4) && !string.IsNullOrEmpty(emailAddress4))
            {
                NotifyParticipant(id, name4, emailAddress4);
                saved = true;
            }

            if (!string.IsNullOrEmpty(name5) && !string.IsNullOrEmpty(emailAddress5))
            {
                NotifyParticipant(id, name5, emailAddress5);
                saved = true;
            }

            return Json(saved, JsonRequestBehavior.AllowGet);
        }

        private void NotifyParticipant(int id, string name, string emailAddress)
        {
            var request = new HearingPersonnel()
            {
                HearingId = id,
                UserId = new Guid().ToString(),
                CreatedBy = 0,                
                DateCreated = DateTime.Now
            };

            try
            {
                db.HearingPersonnels.Add(request);
                db.SaveChangesAsync();

                // Notify participant
                var hearing = db.Hearings.Where(x => x.Id == id).FirstOrDefault();

                string subject = "Virtual Hearing Registration!";
                string htmlMessage = $"Dear {name}";
                htmlMessage += $"\r\nYou have been registered for the virtual hearing with the ff: details.";
                htmlMessage += $"\r\nId: { hearing.Id}";
                htmlMessage += $"\r\nSchedule: {hearing.Schedule}";
                htmlMessage += $"\r\nMeeting Link: {hearing.MeetingLink}";
                htmlMessage += $"\r\nPassword: {hearing.MeetingPassword}";
                htmlMessage += $"\r\nDescription: {hearing.Description}";

                SendEmail(emailAddress, subject, htmlMessage);
            }
            catch (Exception ex)
            {

            }
        }
    }
}