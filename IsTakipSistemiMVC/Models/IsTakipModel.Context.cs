﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class IsTakipDBEntities : DbContext
    {
        public IsTakipDBEntities()
            : base("name=IsTakipDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Birimler> Birimler { get; set; }
        public virtual DbSet<Durumlar> Durumlar { get; set; }
        public virtual DbSet<Isler> Isler { get; set; }
        public virtual DbSet<Personeller> Personeller { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<YetkiTurler> YetkiTurler { get; set; }
        public virtual DbSet<Loglar> Loglar { get; set; }
        public virtual DbSet<YemekTablo> YemekTablo { get; set; }
        public virtual DbSet<Duyurular> Duyurular { get; set; }
    }
}
