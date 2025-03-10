﻿using IsTakipSistemiMVC.Filters;
using IsTakipSistemiMVC.Models;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using OfficeOpenXml;
using System.Drawing;


namespace IsTakipSistemiMVC.Controllers
{

    public class SistemYoneticisiController : Controller
    {
        IsTakipDBEntities entity = new IsTakipDBEntities();

        [AuthFilter(3)]
        public ActionResult Index()
        {

            int birimId = Convert.ToInt32(Session["PersonelBirimId"]);

            var birimler = (from b in entity.Birimler where b.aktiflik == true select b).ToList();

            string labelBirim = "[";

            foreach (var birim in birimler)
            {
                labelBirim += "'" + birim.birimAd + "',";
            }

            labelBirim += "]";

            ViewBag.labelBirim = labelBirim;

            List<int> islerToplam = new List<int>();

            foreach (var birim in birimler)
            {
                int toplam = 0;

                var personeller = (from p in entity.Personeller where p.personelBirimId == birim.birimId && p.aktiflik == true select p).ToList();

                foreach (var personel in personeller)
                {
                    var isler = (from i in entity.Isler where i.isPersonelId == personel.personelId select i).ToList();

                    toplam += isler.Count;

                }
                islerToplam.Add(toplam);
            }

            string dataIs = "[";

            foreach (var i in islerToplam)
            {
                dataIs += "'" + i + "',";
            }

            dataIs += "]";

            ViewBag.dataIs = dataIs;

            int personelId = Convert.ToInt32(Session["PersonelId"]);
            var personel1 = (from p in entity.Personeller where p.personelId == personelId select p).FirstOrDefault();
            ViewBag.personelAdSoyad = personel1.personelAdSoyad;

            // Toplam çalışan sayısını çekme
            ViewBag.TotalEmployees = entity.Personeller.Count(p => p.aktiflik == true);

            // En fazla iş yapan kişiyi bulma
            var enFazlaIsYapanKisi = (from p in entity.Personeller
                                      join i in entity.Isler on p.personelId equals i.isPersonelId
                                      join b in entity.Birimler on p.personelBirimId equals b.birimId
                                      where p.aktiflik == true
                                      group new { p, b } by new { p.personelAdSoyad, b.birimAd } into g
                                      orderby g.Count() descending
                                      select new
                                      {
                                          AdSoyad = g.Key.personelAdSoyad,
                                          BirimAd = g.Key.birimAd,
                                          IsSayisi = g.Count()
                                      }).FirstOrDefault();

            if (enFazlaIsYapanKisi != null)
            {
                ViewBag.EnFazlaIsYapanKisiAdSoyad = enFazlaIsYapanKisi.AdSoyad;
                ViewBag.EnFazlaIsYapanKisiBirim = enFazlaIsYapanKisi.BirimAd;
                ViewBag.EnFazlaIsYapanKisiIsSayisi = enFazlaIsYapanKisi.IsSayisi;
            }
            else
            {
                ViewBag.EnFazlaIsYapanKisiAdSoyad = "No data";
                ViewBag.EnFazlaIsYapanKisiBirim = "No data";
                ViewBag.EnFazlaIsYapanKisiIsSayisi = 0;
            }

            //son eklenen personel
            var sonEklenenPersonel = (from p in entity.Personeller
                                      orderby p.personelIseGiris descending
                                      select p.personelAdSoyad).FirstOrDefault();
            {
                ViewBag.SonPersonel = sonEklenenPersonel;

            }


            // En son tarihli duyuruyu al
            var sonDuyurular = entity.Duyurular
    .Where(d => d.aktiflik == true && d.goruntuleyenBirimId == birimId)
    .OrderByDescending(d => d.duyuruTarih)
    .Take(3)
    .ToList();
            ViewBag.SonDuyurular = sonDuyurular;


            return View();
        }


        public ActionResult Birim()
        {
            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

            if (yetkiTurId == 3)
            {
                var birimler = (from b in entity.Birimler where b.aktiflik == true select b).ToList();
                var personeller = (from p in entity.Personeller where p.aktiflik == true select p).ToList();

                var birimListesi = birimler.Select(b => new BirimViewModel
                {
                    BirimId = b.birimId,
                    BirimAd = b.birimAd,
                    Yoneticiler = string.Join(", ",
                         personeller.Where(p => p.personelBirimId == b.birimId && (p.personelYetkiTurId == 1 || p.personelYetkiTurId == 3))
                           .Select(p => p.personelAdSoyad)
                           .DefaultIfEmpty("Belirtilmemiş"))
                }).ToList();

                return View(birimListesi);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }



        public ActionResult Olustur()
        {
            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

            if (yetkiTurId == 3)
            {

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost, ActFilter("Yeni Birim Eklendi")]
        public ActionResult Olustur(string birimAd)
        {
            Birimler yeniBirim = new Birimler();
            string yeniAd = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(birimAd);
            yeniBirim.birimAd = yeniAd;
            yeniBirim.aktiflik = true;



            entity.Birimler.Add(yeniBirim);
            entity.SaveChanges();

            TempData["bilgi"] = yeniBirim.birimAd;

            return RedirectToAction("Birim");
        }

        public ActionResult Guncelle(int id)
        {
            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

            if (yetkiTurId == 3)
            {
                var birim = (from b in entity.Birimler where b.birimId == id select b).FirstOrDefault();

                return View(birim);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost, ActFilter("Birim Güncellendi")]
        public ActionResult Guncelle(FormCollection fc)
        {
            int birimId = Convert.ToInt32(fc["birimId"]);
            string yeniAd = fc["birimAd"];

            var birim = (from b in entity.Birimler where b.birimId == birimId select b).FirstOrDefault();

            birim.birimAd = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(yeniAd);
            entity.SaveChanges();

            TempData["bilgi"] = birim.birimAd;

            return RedirectToAction("Birim");

        }

        [ActFilter("Birim Silindi")]
        public ActionResult Sil(int id)
        {
            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

            if (yetkiTurId == 3)
            {
                var birim = (from b in entity.Birimler where b.birimId == id select b).FirstOrDefault();

                birim.aktiflik = false;
                entity.SaveChanges();

                TempData["bilgi"] = birim.birimAd;

                return RedirectToAction("Birim");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        [AuthFilter(3)]
        public ActionResult Loglar()
        {
            var loglar = (from l in entity.Loglar orderby l.tarih descending select l).ToList();

            return View(loglar);
        }

        [HttpPost]
        public ActionResult LogSil()
        {
            var loglar = (from l in entity.Loglar select l).ToList();
            entity.Loglar.RemoveRange(loglar);
            entity.SaveChanges();

            return RedirectToAction("Loglar");
        }

        [HttpPost]
        public ActionResult ExceleAktar()
        {
            var logs = entity.Loglar.Include("Personeller").ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Loglar");
                // Başlıkları ekleyin
                worksheet.Cells[1, 1].Value = "Personel Ad Soyad";
                worksheet.Cells[1, 2].Value = "Açıklama";
                worksheet.Cells[1, 3].Value = "Action Adı";
                worksheet.Cells[1, 4].Value = "Controller Adı";
                worksheet.Cells[1, 5].Value = "Tarih";

                // Başlıkları stilize edin
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.White); // Arka plan rengini beyaz yapın
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Log verilerini ekleyin
                for (int i = 0; i < logs.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = logs[i].Personeller.personelAdSoyad;
                    worksheet.Cells[i + 2, 2].Value = logs[i].logAciklama;
                    worksheet.Cells[i + 2, 3].Value = logs[i].actionAd;
                    worksheet.Cells[i + 2, 4].Value = logs[i].controllerAd;
                    worksheet.Cells[i + 2, 5].Value = logs[i].tarih.ToString();
                }

                // Otomatik genişlik ayarı
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Log Kayıtları.xlsx");
            }
        }

        [AuthFilter(3)]
        public ActionResult YemekEkle()
        {
            return View();
        }

        [HttpPost]
        public ActionResult YemekEkle(FormCollection fc)
        {
            DateTime tarih = DateTime.Parse(fc["Tarih"]); // Tarih bilgisini al

            // Veritabanında aynı tarihe sahip bir menü olup olmadığını kontrol et
            var varOlanMenu = entity.YemekTablo.FirstOrDefault(m => m.Tarih == tarih);
            if (varOlanMenu != null)
            {
                TempData["bilgi"] = "Zaten bu tarihe bir menü atanmış.";
                return RedirectToAction("YemekEkle");
            }

            YemekTablo Menu = new YemekTablo();
            Menu.YemekAdi1 = fc["YemekAdi1"];
            Menu.YemekAdi2 = fc["YemekAdi2"];
            Menu.YemekAdi3 = fc["YemekAdi3"];
            Menu.YemekAdi4 = fc["YemekAdi4"];
            Menu.YemekAdi5 = fc["YemekAdi5"];
            Menu.YemekAdi6 = fc["YemekAdi6"];
            Menu.YemekAdi7 = fc["YemekAdi7"];
            Menu.YemekAdi8 = fc["YemekAdi8"];
            Menu.Tarih = tarih;

            entity.YemekTablo.Add(Menu);
            entity.SaveChanges();

            TempData["bilgi"] = "Yemek menüsü başarıyla eklendi.";
            return RedirectToAction("YemekEkle");
        }

    }
}