using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetStarter.Models
{
    public class OTPModel
    {
        public string SenderId { get; set; }
        public string ApiKey { get; set; }
        public string ClientId { get; set; }
        public string Message { get; set; }
        public string MobileNumbers { get; set; }
    }
}