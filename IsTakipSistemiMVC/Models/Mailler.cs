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
    
    public partial class Mailler
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Mailler()
        {
            this.MailEkler = new HashSet<MailEkler>();
        }
    
        public int mailId { get; set; }
        public Nullable<int> mailGondericiId { get; set; }
        public Nullable<int> mailAliciId { get; set; }
        public string mailKonu { get; set; }
        public string mailIcerik { get; set; }
        public Nullable<bool> mailOkunma { get; set; }
        public Nullable<bool> aktiflik { get; set; }
        public Nullable<int> mailGonderilmeDurumu { get; set; }
        public System.DateTime mailGonderilmeTarih { get; set; }
        public Nullable<System.DateTime> mailZamanliGonderilmeTarih { get; set; }
        public Nullable<bool> mailArsiv { get; set; }
    
        public virtual Durumlar Durumlar { get; set; }
        public virtual Personeller Personeller { get; set; }
        public virtual Personeller Personeller1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MailEkler> MailEkler { get; set; }
    }
}
