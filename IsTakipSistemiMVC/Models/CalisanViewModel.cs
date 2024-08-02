using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IsTakipSistemiMVC.Models
{
    public class CalisanViewModel
    {
        public int PersonelId { get; set; }
        public string PersonelAdSoyad { get; set; }
        public int TotalJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int UncompletedJobs { get; set; }
        public string PersonelFotograf { get; set; }
    }

}