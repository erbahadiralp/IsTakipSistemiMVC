//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IsTakipSistemiMVC.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class MailEkler
    {
        public int eklerId { get; set; }
        public Nullable<int> mailId { get; set; }
        public string eklerAd { get; set; }
        public byte[] eklerVeri { get; set; }
    
        public virtual Mailler Mailler { get; set; }
    }
}
