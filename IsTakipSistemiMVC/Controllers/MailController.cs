    using IsTakipSistemiMVC.Filters;
    using IsTakipSistemiMVC.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    namespace IsTakipSistemiMVC.Controllers
    {
        public class MailController : Controller
        {

            IsTakipDBEntities entity = new IsTakipDBEntities();

        // GET: Mail
        public ActionResult Index()
            {

                int userId = Convert.ToInt32(Session["PersonelId"]);
                int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

                var mailler = entity.Mailler
                                       .Where(m => m.aktiflik == true && m.mailAliciId == userId) // Aktif olan duyuruları filtrele
                                       .ToList();

                switch (yetkiTurId)
                {
                    case 1:
                        ViewBag.Layout = "~/Views/Shared/_LayoutYonetici.cshtml";
                        break;
                    case 2:
                        ViewBag.Layout = "~/Views/Shared/_LayoutCalisan.cshtml";
                        break;
                    case 3:
                        ViewBag.Layout = "~/Views/Shared/_LayoutSistemYoneticisi.cshtml";
                        break;
                }

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
                                                    "personelAdSoyad");

                int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

                switch (yetkiTurId)
                {
                    case 1:
                        ViewBag.Layout = "~/Views/Shared/_LayoutYonetici.cshtml";
                        break;
                    case 2:
                        ViewBag.Layout = "~/Views/Shared/_LayoutCalisan.cshtml";
                        break;
                    case 3:
                        ViewBag.Layout = "~/Views/Shared/_LayoutSistemYoneticisi.cshtml";
                        break;
                }


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
            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

            var gidenMailler = entity.Mailler
                                     .Where(m => m.mailGondericiId == userId && m.aktiflik == true)
                                     .ToList();

            switch (yetkiTurId)
            {
                case 1:
                    ViewBag.Layout = "~/Views/Shared/_LayoutYonetici.cshtml";
                    break;
                case 2:
                    ViewBag.Layout = "~/Views/Shared/_LayoutCalisan.cshtml";
                    break;
                case 3:
                    ViewBag.Layout = "~/Views/Shared/_LayoutSistemYoneticisi.cshtml";
                    break;
            }

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