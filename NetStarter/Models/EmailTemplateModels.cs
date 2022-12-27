﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NetStarter.Models
{
    public class EmailTemplate
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Type { get; set; }
    }

}