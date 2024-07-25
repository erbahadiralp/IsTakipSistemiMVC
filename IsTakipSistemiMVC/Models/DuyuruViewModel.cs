using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IsTakipSistemiMVC.Models
{
    public class DuyuruViewModel
    {
        public int DuyuruId { get; set; }
        public string DuyuruBaslik { get; set; }
        public string DuyuruIcerik { get; set; }
        public bool Aktiflik { get; set; }
        public string OlusturanAdSoyad { get; set; }
        public string GoruntuleyenBirim { get; set; }

        public int GoruntuleyenBirimId { get; set; }

        public DateTime DuyuruTarih { get; set; }
    }
}