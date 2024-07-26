using IsTakipSistemiMVC.Filters;
using IsTakipSistemiMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static IsTakipSistemiMVC.Models.MailViewModel;

    namespace IsTakipSistemiMVC.Controllers
{
    [LayoutActionFilter]
    public class MailController : Controller
    {

        IsTakipDBEntities entity = new IsTakipDBEntities();

        // GET


        public ActionResult Index()
        {
            int userId = Convert.ToInt32(Session["PersonelId"]);

            var mailler = (from mail in entity.Mailler
                           join gonderici in entity.Personeller on mail.mailGondericiId equals gonderici.personelId
                           join alici in entity.Personeller on mail.mailAliciId equals alici.personelId
                           where mail.aktiflik == true && mail.mailAliciId == userId
                           select new MailViewModel
                           {
                               MailId = mail.mailId,
                               GondericiAdSoyad = gonderici.personelAdSoyad,
                               AliciAdSoyad = alici.personelAdSoyad,
                               Konu = mail.mailKonu,
                               Icerik = mail.mailIcerik,
                               GonderilmeTarihi = mail.mailGonderilmeTarih
                           }).ToList();

            return View(mailler);
        }



        //// Duyuru detayları
        //public ActionResult DuyuruDetay(int id)
        //{
        //    var duyuru = entity.Duyurular
        //                       .FirstOrDefault(d => d.duyuruId == id && d.aktiflik == true); // Aktif duyuruyu bul
        //    if (duyuru == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(duyuru);
        //}

        // Yeni duyuru ekleme sayfası
        public ActionResult MailOlustur()
        {
            ViewBag.Recipients = new SelectList(entity.Personeller
                                                .Where(p => p.aktiflik == true) // Only active personnel
                                                .ToList(),
                                                "personelId",
                                                "personelMail");
            return View();
        }

        [HttpPost, ActFilter("Mail Gönderildi")]
        public ActionResult MailOlustur(Mailler mail)
        {
            if (ModelState.IsValid)
            {
                mail.mailGonderilmeTarih = DateTime.Now;
                mail.mailGondericiId = Convert.ToInt32(Session["PersonelId"]);
                mail.mailKonu = mail.mailKonu;
                mail.mailIcerik = mail.mailIcerik;
                mail.mailAliciId = mail.mailAliciId;
                mail.aktiflik = true;

                entity.Mailler.Add(mail);
                entity.SaveChanges();

                TempData["bilgi"] = "Mail başarıyla gönderildi.";
                return RedirectToAction("Index");
            }
            ViewBag.Recipients = new SelectList(entity.Personeller
                                                .Where(p => p.aktiflik == true) // Only active personnel
                                                .ToList(),
                                                "personelId",
                                                "personelAdSoyad");
            return View(mail);
        }

        public ActionResult GidenMailler()
        {
            int userId = Convert.ToInt32(Session["PersonelId"]);

            var gidenMailler = (from mail in entity.Mailler
                                join gonderici in entity.Personeller on mail.mailGondericiId equals gonderici.personelId
                                join alici in entity.Personeller on mail.mailAliciId equals alici.personelId
                                where mail.aktiflik == true && mail.mailGondericiId == userId
                                select new MailViewModel
                                {
                                    MailId = mail.mailId,
                                    GondericiAdSoyad = gonderici.personelAdSoyad,
                                    AliciAdSoyad = alici.personelAdSoyad,
                                    Konu = mail.mailKonu,
                                    Icerik = mail.mailIcerik,
                                    GonderilmeTarihi = mail.mailGonderilmeTarih
                                }).ToList();

            return View(gidenMailler);
        }




        //Mail Giden - GELEN

        //// Duyuru silme işlemi (aktifliğini false yapar)
        //[ActFilter("Duyuru Silindi"), AuthFilter(3)]

        //public ActionResult DuyuruSil(int id)
        //{
        //    var duyuru = entity.Duyurular
        //                       .FirstOrDefault(d => d.duyuruId == id && d.aktiflik == true); // Aktif duyuruyu bul
        //    if (duyuru == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    duyuru.aktiflik = false;
        //    entity.SaveChanges();

        //    return RedirectToAction("Duyurular");
        //}



    }
}