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
                           where mail.aktiflik == true && mail.mailAliciId == userId && mail.mailArsiv == false
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
        [ValidateInput(false)]
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
                mail.mailArsiv = false; // Explicitly set to false
                mail.mailOkunma = false;

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


        //GİDEN MAİLLER
        public ActionResult GidenMailler()
        {
            int userId = Convert.ToInt32(Session["PersonelId"]);

            var gidenMailler = (from mail in entity.Mailler
                                join gonderici in entity.Personeller on mail.mailGondericiId equals gonderici.personelId
                                join alici in entity.Personeller on mail.mailAliciId equals alici.personelId
                                where mail.aktiflik == true && mail.mailGondericiId == userId && mail.mailArsiv == false
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


        [HttpPost]
        public ActionResult MailSil(List<int> ids)
        {
            if (ids != null && ids.Count > 0)
            {
                foreach (var id in ids)
                {
                    var mail = entity.Mailler.FirstOrDefault(m => m.mailId == id);
                    if (mail != null)
                    {
                        mail.aktiflik = false;
                    }
                }
                entity.SaveChanges();
                TempData["bilgi"] = "Mail(ler) başarıyla silindi.";
            }
            else
            {
                TempData["bilgi"] = "Mail bulunamadı.";
            }
            return Json(new { success = true });
        }


        [HttpPost]
        public ActionResult MailArsivle(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return Json(new { success = false, message = "Arşivlenecek mail seçilmedi." });
            }

            var mailler = entity.Mailler.Where(m => ids.Contains(m.mailId)).ToList();

            if (mailler.Count == 0)
            {
                return Json(new { success = false, message = "Geçersiz mail id'leri." });
            }

            foreach (var mail in mailler)
            {
                mail.mailArsiv = true; // Arşivle
            }

            entity.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult MailArsivdenCikar(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return Json(new { success = false, message = "Arşivden çıkarılacak mail seçilmedi." });
            }

            var mailler = entity.Mailler.Where(m => ids.Contains(m.mailId)).ToList();

            if (mailler.Count == 0)
            {
                return Json(new { success = false, message = "Geçersiz mail id'leri." });
            }

            foreach (var mail in mailler)
            {
                mail.mailArsiv = false; // Arşivden çıkar
            }

            entity.SaveChanges();

            return Json(new { success = true });
        }


        public ActionResult ArsivGoster()
        {

            int userId = Convert.ToInt32(Session["PersonelId"]);


            // Belirli bir kullanıcının arşivlenen maillerini getir (gönderilen ve alınan)
            var arsivlenmisMailler = (from mail in entity.Mailler
                                      join gonderici in entity.Personeller on mail.mailGondericiId equals gonderici.personelId
                                      join alici in entity.Personeller on mail.mailAliciId equals alici.personelId
                                      where mail.mailArsiv == true && (mail.mailGondericiId == userId || mail.mailAliciId == userId)
                                      select new MailViewModel
                                      {
                                          MailId = mail.mailId,
                                          GondericiAdSoyad = gonderici.personelAdSoyad,
                                          AliciAdSoyad = alici.personelAdSoyad,
                                          Konu = mail.mailKonu,
                                          Icerik = mail.mailIcerik,
                                          GonderilmeTarihi = mail.mailGonderilmeTarih,
                                          Arsiv = mail.mailArsiv ?? false
                                      }).ToList();

            // View'e model olarak gönder
            return View(arsivlenmisMailler);
        }


        public ActionResult MailDetay(int id)
        {
            var mailDetay = (from mail in entity.Mailler
                             join gonderici in entity.Personeller on mail.mailGondericiId equals gonderici.personelId
                             join alici in entity.Personeller on mail.mailAliciId equals alici.personelId
                             where mail.mailId == id
                             select new MailViewModel
                             {
                                 MailId = mail.mailId,
                                 GondericiAdSoyad = gonderici.personelAdSoyad,
                                 AliciAdSoyad = alici.personelAdSoyad,
                                 Konu = mail.mailKonu,
                                 Icerik = mail.mailIcerik,
                                 GonderilmeTarihi = mail.mailGonderilmeTarih
                             }).FirstOrDefault();

            if (mailDetay == null)
            {
                return HttpNotFound();
            }

            return View(mailDetay);
        }

        public ActionResult SilinmisMailler()
        {
            int userId = Convert.ToInt32(Session["PersonelId"]);

            var silinmisMailler = (from mail in entity.Mailler
                                   join gonderici in entity.Personeller on mail.mailGondericiId equals gonderici.personelId
                                   join alici in entity.Personeller on mail.mailAliciId equals alici.personelId
                                   where mail.aktiflik == false && (mail.mailGondericiId == userId || mail.mailAliciId == userId)
                                   select new MailViewModel
                                   {
                                       MailId = mail.mailId,
                                       GondericiAdSoyad = gonderici.personelAdSoyad,
                                       AliciAdSoyad = alici.personelAdSoyad,
                                       Konu = mail.mailKonu,
                                       Icerik = mail.mailIcerik,
                                       GonderilmeTarihi = mail.mailGonderilmeTarih
                                   }).ToList();

            return View(silinmisMailler);
        }


        [HttpPost]
        public ActionResult MailiOkunduYap(List<int> ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    var mail = entity.Mailler.Find(id);
                    if (mail != null)
                    {
                        mail.mailOkunma = true;
                    }
                }
                entity.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Hata durumunda hata mesajını geri döndür
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}