using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;

namespace NetStarter.Models
{
    //project default enum

    public class ProjectEnum
    {

        public enum ModuleCode
        {
            Dashboard,
            StakeholderRegistration,
            PreFiledCase,
            FiledCase,
            Hearing,
            InitiatoryPleading,
            PleadingWithCaseNumber,
            OtherLetterCorrespondense,
            DisputeResolution,
            Transaction,
            Payment,
            UserStatus,
            UserType,
            CaseType,
            CaseNature,
            UserAttachmentType,
            RoleManagement,
            UserManagement,
            LoginHistory
        }

        public enum UserAttachment
        {
            ProfilePicture
        }

        public enum Gender
        {
            Female,
            Male,
            Other
        }

        public enum UserStatus
        {
            Registered,
            Validated,
            NotValidated,
            Banned
        }

        public enum EmailTemplate
        {
            ConfirmEmail,
            PasswordResetByAdmin
        }

    }

}