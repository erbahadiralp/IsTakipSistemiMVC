using IsTakipSistemiMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IsTakipSistemiMVC.Models
{
    public class PersonelIslerViewModel
    {
        public Personeller Personel { get; set; }
        public List<Isler> Isler { get; set; }
        public int IsSayisi { get; set; }
    }
}