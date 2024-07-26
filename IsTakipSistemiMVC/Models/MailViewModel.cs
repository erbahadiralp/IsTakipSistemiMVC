using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IsTakipSistemiMVC.Models
{

    public class MailViewModel
    {

        public int MailId { get; set; }
        public string GondericiAdSoyad { get; set; }
        public string AliciAdSoyad { get; set; }
        public string Konu { get; set; }
        public string Icerik { get; set; }
        public DateTime GonderilmeTarihi { get; set; }
        public bool Arsiv { get; set; }
    }

}
