﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IsTakipSistemiMVC.Models
{
    public class ToplamIs
    {
        public string personelAdSoyad { get; set; }
        public string personelFotograf { get; set; }
        public string personelDahili { get; set; }

        public int toplamIs { get; set; }
    }
}