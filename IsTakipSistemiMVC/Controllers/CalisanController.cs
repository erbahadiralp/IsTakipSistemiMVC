using IsTakipSistemiMVC.Filters;
using IsTakipSistemiMVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace IsTakipSistemiMVC.Controllers
{
    public class IsDurum
    {
        public string isBaslik { get; set; }
        public string isAciklama { get; set; }
        public DateTime? iletilenTarih { get; set; }
        public DateTime? yapilanTarih { get; set; }
        public string durumAd { get; set; }
        public string isYorum { get; set; }
    }

    public class CalisanController : Controller
    {
        IsTakipDBEntities entity = new IsTakipDBEntities();

        public ActionResult Index()
        {
            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

            if (yetkiTurId == 2)
            {
                int birimId = Convert.ToInt32(Session["PersonelBirimId"]);
                var birim = entity.Birimler.FirstOrDefault(b => b.birimId == birimId);
                ViewBag.birimAd = birim?.birimAd;

                int personelId = Convert.ToInt32(Session["PersonelId"]);
                var isler = entity.Isler.Where(i => i.isPersonelId == personelId && i.isOkunma == false)
                                        .OrderByDescending(i => i.iletilenTarih)
                                        .ToList();
                ViewBag.isler = isler;

                var personel = entity.Personeller.FirstOrDefault(p => p.personelId == personelId);
                ViewBag.personelAdSoyad = personel?.personelAdSoyad;

                DateTime bugun = DateTime.Today;
                var bugunYemek = entity.YemekTablo
                    .Where(y => DbFunctions.TruncateTime(y.Tarih) == bugun)
                    .FirstOrDefault();
                ViewBag.BugunYemek = bugunYemek;

                //aynı birimdeki çalışanlar
                var calisanlar = entity.Personeller
                    .Where(p => p.personelBirimId == birimId && p.aktiflik == true)
                    .ToList();

                ViewBag.personeller = calisanlar.Any() ? calisanlar : null;

                // En son tarihli duyuruyu al
                var sonDuyuru = entity.Duyurular
                    .Where(d => d.aktiflik == true && d.goruntuleyenBirimId == birimId)
                    .OrderByDescending(d => d.duyuruTarih)
                    .FirstOrDefault();
                ViewBag.SonDuyuru = sonDuyuru;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        [HttpPost]
        public ActionResult Index(int isId)
        {
            var tekIs = entity.Isler.FirstOrDefault(i => i.isId == isId);

            if (tekIs != null)
            {
                tekIs.isOkunma = true;
                entity.SaveChanges();
            }

            return RedirectToAction("Yap", "Calisan");
        }

        public ActionResult Yap()
        {
            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

            if (yetkiTurId == 2)
            {
                int personelId = Convert.ToInt32(Session["PersonelId"]);

                var isler = entity.Isler.Where(i => i.isPersonelId == personelId && i.isDurumId == 1)
                                        .OrderByDescending(i => i.iletilenTarih)
                                        .ToList();

                ViewBag.isler = isler;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public ActionResult Yap(int isId, string isYorum)
        {
            var tekIs = entity.Isler.FirstOrDefault(i => i.isId == isId);

            if (tekIs != null)
            {
                if (string.IsNullOrEmpty(isYorum)) isYorum = "Kullanıcı Yorum Yapmadı";

                tekIs.yapilanTarih = DateTime.Now;
                tekIs.isDurumId = 2;
                tekIs.isYorum = isYorum;

                entity.SaveChanges();
            }

            return RedirectToAction("Index", "Calisan");
        }

        public ActionResult Takip()
        {
            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

            if (yetkiTurId == 2)
            {
                int personelId = Convert.ToInt32(Session["PersonelId"]);

                var isler = entity.Isler.Where(i => i.isPersonelId == personelId)
                                        .Join(entity.Durumlar,
                                              i => i.isDurumId,
                                              d => d.durumId,
                                              (i, d) => new { i, d })
                                        .OrderByDescending(i => i.i.iletilenTarih)
                                        .ToList();

                var model = new IsDurumModel
                {
                    isDurumlar = isler.Select(i => new IsDurum
                    {
                        isBaslik = i.i.isBaslik,
                        isAciklama = i.i.isAciklama,
                        iletilenTarih = i.i.iletilenTarih,
                        yapilanTarih = i.i.yapilanTarih,
                        durumAd = i.d.durumAd,
                        isYorum = i.i.isYorum
                    }).ToList()
                };

                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult Profil()
        {
            int id = Convert.ToInt32(Session["PersonelId"]);
            var personel = entity.Personeller.FirstOrDefault(p => p.personelId == id);

            if (personel == null)
            {
                return HttpNotFound();
            }

            return View(personel);
        }

       


    }
}
