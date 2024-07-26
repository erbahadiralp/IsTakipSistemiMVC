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
            var duyuruList = (from d in entity.Duyurular
                              join p in entity.Personeller on d.duyuruOlusturanId equals p.personelId
                              join b in entity.Birimler on d.goruntuleyenBirimId equals b.birimId
                              where d.aktiflik == true
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
                GoruntuleyenBirim = d.GoruntuleyenBirim, // Birim adı burada atanıyor
                GoruntuleyenBirimId = int.TryParse(d.goruntuleyenBirimId.ToString(), out int birimId) ? birimId : 0
            }).ToList();

            ViewBag.YetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

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
            ViewBag.Birimler = new SelectList(entity.Birimler
                                                .Where(b => b.aktiflik == true) // Aktif birimleri listele
                                                .ToList(),
                                            "birimId",
                                            "birimAd");
            return View();
        }

        [HttpPost, ActFilter("Yeni Duyuru Eklendi"), AuthFilter(1, 3)]
        public ActionResult DuyuruEkle(Duyurular duyuru)
        {
            if (ModelState.IsValid)
            {
                duyuru.duyuruTarih = DateTime.Now;
                duyuru.duyuruOlusturanId = Convert.ToInt32(Session["PersonelId"]);
                duyuru.aktiflik = true;

                entity.Duyurular.Add(duyuru);
                entity.SaveChanges();

                TempData["bilgi"] = "Duyuru başarıyla eklendi.";
                return RedirectToAction("Index");
            }

            // ModelState geçerli değilse, ViewBag.Birimler tekrar doldurulmalı
            ViewBag.Birimler = new SelectList(entity.Birimler
                                                .Where(b => b.aktiflik == true)
                                                .ToList(),
                                            "birimId",
                                            "birimAd");
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
            ViewBag.Birimler = new SelectList(entity.Birimler
                                                .Where(b => b.aktiflik == true) // Aktif birimleri listele
                                                .ToList(),
                                            "birimId",
                                            "birimAd");
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
