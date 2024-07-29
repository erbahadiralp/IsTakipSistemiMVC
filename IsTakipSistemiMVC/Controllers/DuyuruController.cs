using IsTakipSistemiMVC.Filters;
using IsTakipSistemiMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IsTakipSistemiMVC.Controllers
{
    [LayoutActionFilter]
    public class DuyuruController : Controller
    {
        IsTakipDBEntities entity = new IsTakipDBEntities();

        // GET: Duyuru
        public ActionResult Index()
        {
            int personelBirimId = Convert.ToInt32(Session["PersonelBirimId"]); // Kullanıcının birim ID'si alınır.
            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]); // Kullanıcının yetki türü alınır.

            var duyuruList = (from d in entity.Duyurular
                              join p in entity.Personeller on d.duyuruOlusturanId equals p.personelId
                              join b in entity.Birimler on d.goruntuleyenBirimId equals b.birimId
                              where d.aktiflik == true && (yetkiTurId == 3 || (yetkiTurId != 3 && d.goruntuleyenBirimId == personelBirimId))
                              select new
                              {
                                  d.duyuruId,
                                  d.duyuruBaslik,
                                  d.duyuruIcerik,
                                  d.duyuruTarih,
                                  GoruntuleyenBirim = b.birimAd, // Birim adı alınıyor
                                  d.goruntuleyenBirimId,
                                  OlusturanAdSoyad = p.personelAdSoyad
                              }).ToList();

            var duyurular = duyuruList.Select(d => new DuyuruViewModel
            {
                DuyuruId = d.duyuruId,
                DuyuruBaslik = d.duyuruBaslik,
                DuyuruIcerik = d.duyuruIcerik,
                DuyuruTarih = d.duyuruTarih,
                OlusturanAdSoyad = d.OlusturanAdSoyad,
                GoruntuleyenBirim = d.GoruntuleyenBirim,
                GoruntuleyenBirimId = int.TryParse(d.goruntuleyenBirimId.ToString(), out int birimId) ? birimId : 0
            }).ToList();

            ViewBag.YetkiTurId = yetkiTurId;

            return View(duyurular);
        }

        // Duyuru detayları
        public ActionResult DuyuruDetay(int id)
        {
            var duyuru = entity.Duyurular.Find(id);

            if (duyuru == null)
            {
                return HttpNotFound();
            }

            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);
            ViewBag.YetkiTurId = yetkiTurId; // Yetki türünü ViewBag ile geçiyoruz

            return View(duyuru);
        }

        // Yeni duyuru ekleme sayfası
        public ActionResult DuyuruEkle()
        {
            int personelId = Convert.ToInt32(Session["PersonelId"]);
            var personel = entity.Personeller.FirstOrDefault(p => p.personelId == personelId);
            if (personel == null)
            {
                return HttpNotFound();
            }

            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);
            if (yetkiTurId == 3)
            {
                ViewBag.Birimler = new SelectList(entity.Birimler
                                                    .Where(b => b.aktiflik == true) // Tüm birimler
                                                    .ToList(),
                                                  "birimId",
                                                  "birimAd");
            }
            else
            {
                ViewBag.Birimler = new SelectList(entity.Birimler
                                                    .Where(b => b.birimId == personel.personelBirimId && b.aktiflik == true) // Sadece kullanıcının birimi
                                                    .ToList(),
                                                  "birimId",
                                                  "birimAd");
            }
            return View();
        }

        [HttpPost, ActFilter("Yeni Duyuru Eklendi"), AuthFilter(1, 3)]
        public ActionResult DuyuruEkle(Duyurular duyuru)
        {
            if (ModelState.IsValid)
            {
                int personelId = Convert.ToInt32(Session["PersonelId"]);
                var personel = entity.Personeller.FirstOrDefault(p => p.personelId == personelId);

                duyuru.duyuruTarih = DateTime.Now;
                duyuru.duyuruOlusturanId = personelId;
                int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

                if (yetkiTurId == 3)
                {
                    // Tüm birimlere duyuru gönderebilecek
                    duyuru.goruntuleyenBirimId = duyuru.goruntuleyenBirimId;
                }
                else
                {
                    // Sadece kendi birimine duyuru gönderebilecek
                    duyuru.goruntuleyenBirimId = personel.personelBirimId;
                }

                duyuru.aktiflik = true;

                entity.Duyurular.Add(duyuru);
                entity.SaveChanges();

                TempData["bilgi"] = "Duyuru başarıyla eklendi.";
                return RedirectToAction("Index");
            }

            // ModelState geçerli değilse, ViewBag.Birimler tekrar doldurulmalı
            int personelIdRetry = Convert.ToInt32(Session["PersonelId"]);
            var personelRetry = entity.Personeller.FirstOrDefault(p => p.personelId == personelIdRetry);
            int yetkiTurIdRetry = Convert.ToInt32(Session["PersonelYetkiTurId"]);
            if (yetkiTurIdRetry == 3)
            {
                ViewBag.Birimler = new SelectList(entity.Birimler
                                                    .Where(b => b.aktiflik == true) // Tüm birimler
                                                    .ToList(),
                                                  "birimId",
                                                  "birimAd");
            }
            else
            {
                ViewBag.Birimler = new SelectList(entity.Birimler
                                                    .Where(b => b.birimId == personelRetry.personelBirimId && b.aktiflik == true) // Sadece kullanıcının birimi
                                                    .ToList(),
                                                  "birimId",
                                                  "birimAd");
            }
            return View(duyuru);
        }

        // Duyuru güncelleme sayfası
        public ActionResult DuyuruGuncelle(int id)
        {
            var duyuru = entity.Duyurular
                               .FirstOrDefault(d => d.duyuruId == id && d.aktiflik == true); // Aktif duyuruyu bul
            if (duyuru == null)
            {
                return HttpNotFound();
            }

            int personelId = Convert.ToInt32(Session["PersonelId"]);
            var personel = entity.Personeller.FirstOrDefault(p => p.personelId == personelId);
            if (personel == null)
            {
                return HttpNotFound();
            }

            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);
            if (yetkiTurId == 3)
            {
                ViewBag.Birimler = new SelectList(entity.Birimler
                                                    .Where(b => b.aktiflik == true) // Tüm birimler
                                                    .ToList(),
                                                  "birimId",
                                                  "birimAd");
            }
            else
            {
                ViewBag.Birimler = new SelectList(entity.Birimler
                                                    .Where(b => b.birimId == personel.personelBirimId && b.aktiflik == true) // Sadece kullanıcının birimi
                                                    .ToList(),
                                                  "birimId",
                                                  "birimAd");
            }
            return View(duyuru);
        }

        [HttpPost, ActFilter("Duyuru Güncellendi"), AuthFilter(1, 3)]
        public ActionResult DuyuruGuncelle(Duyurular duyuru)
        {
            var mevcutDuyuru = entity.Duyurular
                                     .FirstOrDefault(d => d.duyuruId == duyuru.duyuruId && d.aktiflik == true); // Aktif duyuruyu bul
            if (mevcutDuyuru == null)
            {
                return HttpNotFound();
            }

            mevcutDuyuru.duyuruBaslik = duyuru.duyuruBaslik;
            mevcutDuyuru.duyuruIcerik = duyuru.duyuruIcerik;
            mevcutDuyuru.duyuruTarih = DateTime.Now; // Tarihi güncelle

            entity.SaveChanges();
            return RedirectToAction("Index");
        }

        // Duyuru silme işlemi (aktifliğini false yapar)
        [ActFilter("Duyuru Silindi"), AuthFilter(1, 3)]
        public ActionResult DuyuruSil(int id)
        {
            var duyuru = entity.Duyurular
                               .FirstOrDefault(d => d.duyuruId == id && d.aktiflik == true); // Aktif duyuruyu bul
            if (duyuru == null)
            {
                return HttpNotFound();
            }

            duyuru.aktiflik = false;
            entity.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
