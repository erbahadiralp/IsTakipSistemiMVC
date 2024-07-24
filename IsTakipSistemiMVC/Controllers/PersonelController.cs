using IsTakipSistemiMVC.Filters;
using IsTakipSistemiMVC.Models;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using System.Drawing;
using System.Globalization;

namespace IsTakipSistemiMVC.Controllers
{
    public class PersonelController : Controller
    {
        IsTakipDBEntities entity = new IsTakipDBEntities();
        // GET: Personel
        [AuthFilter(3)]
        public ActionResult Index()
        {
            var personeller = (from p in entity.Personeller where p.personelYetkiTurId != 3 && p.aktiflik == true select p).ToList();

            return View(personeller);
        }

        [AuthFilter(3)]
        public ActionResult Olustur()
        {
            BirimYetkiTurler by = birimYetkiTurlerDoldur();

            ViewBag.mesaj = null;
            return View(by);
        }

        /// <summary>
        /// ////
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost, ActFilter("Yeni Personel Eklendi")]
        public ActionResult Olustur(FormCollection fc)
        {
            string personelKullaniciAd = fc["kullaniciAd"];
            var personel = (from p in entity.Personeller where p.personelKullaniciAd == personelKullaniciAd select p).FirstOrDefault();

            int birimId = Convert.ToInt32(fc["birim"]);
            if (Convert.ToInt32(fc["yetkiTur"]) == 1)
            {
                var birimYoneticiKontrol = (from p in entity.Personeller where p.personelBirimId == birimId && p.personelYetkiTurId == 1 select p).FirstOrDefault();

                if (birimYoneticiKontrol != null)
                {
                    BirimYetkiTurler by = birimYetkiTurlerDoldur();

                    ViewBag.mesaj = "Bu biriminin sadece bir yöneticisi olabilir";
                    TempData["bilgi"] = null;
                    return View(by);
                }
            }

            if (personel == null)
            {
                Personeller yeniPersonel = new Personeller();
                yeniPersonel.personelAdSoyad = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fc["adSoyad"].ToLower());
                yeniPersonel.personelKullaniciAd = personelKullaniciAd;
                yeniPersonel.personelParola = fc["parola"];
                yeniPersonel.personelBirimId = Convert.ToInt32(fc["birim"]);
                yeniPersonel.personelYetkiTurId = Convert.ToInt32(fc["yetkiTur"]);
                yeniPersonel.yeniPersonel = true;
                yeniPersonel.aktiflik = true;

                // İşe Giriş Tarihi kontrolü ve atanması
                DateTime iseGirisTarihi;
                if (!DateTime.TryParse(fc["iseGirisTarihi"], out iseGirisTarihi))
                {
                    iseGirisTarihi = DateTime.Now;
                }
                yeniPersonel.personelIseGiris = iseGirisTarihi;

                // yeniPersonel için formdan gelen diğer değerleri kullanın
                yeniPersonel.personelDogumGunu = Convert.ToDateTime(fc["dogumTarihi"]);
                yeniPersonel.personelMail = fc["mail"];
                yeniPersonel.personelAdres = fc["adres"];
                yeniPersonel.personelTelefon = fc["telefon"];
                yeniPersonel.personelDahili = fc["dahiliTelefon"];

                entity.Personeller.Add(yeniPersonel);
                entity.SaveChanges();

                TempData["bilgi"] = yeniPersonel.personelKullaniciAd;
                return RedirectToAction("Index");
            }
            else
            {
                BirimYetkiTurler by = birimYetkiTurlerDoldur();

                ViewBag.mesaj = "Kullanıcı Adı Bulunmaktadır";
                TempData["bilgi"] = null;
                return View(by);
            }
        }


        [AuthFilter(3)]
        public ActionResult Guncelle(int id)
        {
            var personel = (from p in entity.Personeller where p.personelId == id select p).FirstOrDefault();

            return View(personel);
        }

        [HttpPost, ActFilter("Personel Güncellendi")]
        public ActionResult Guncelle(int id, string adSoyad, string Adres, string Telefon)
        {
            var personel = entity.Personeller.FirstOrDefault(p => p.personelId == id);
            if (personel != null)
            {
                personel.personelAdSoyad = adSoyad;
                personel.personelAdres = Adres;
                personel.personelTelefon = Telefon;

                entity.SaveChanges();
                TempData["bilgi"] = personel.personelKullaniciAd;
            }
            return RedirectToAction("Index");
        }

        [AuthFilter(3)]
        public ActionResult Sil(int id)
        {
            Personeller personel = (from p in entity.Personeller where p.personelId == id select p).FirstOrDefault();
            return View(personel);

        }

        [HttpPost, ActFilter("Personel Silindi")]
        public ActionResult Sil(FormCollection fc)
        {
            int personelId = Convert.ToInt32(fc["personelId"]);

            var personel = (from p in entity.Personeller where p.personelId == personelId select p).FirstOrDefault();

            if (personel == null)
            {
                return HttpNotFound();
            }

            personel.aktiflik = false;
            entity.SaveChanges();

            TempData["bilgi"] = personel.personelKullaniciAd;

            return RedirectToAction("Index");
        }


        public BirimYetkiTurler birimYetkiTurlerDoldur()
        {
            BirimYetkiTurler by = new BirimYetkiTurler();

            by.birimlerList = (from b in entity.Birimler where b.aktiflik == true select b).ToList();
            by.yetkiTurlerList = (from y in entity.YetkiTurler where y.yetkiTurId != 3 select y).ToList();

            return by;
        }


        [AuthFilter(3)]
        public ActionResult Detay(int id)
        {

            var personel = (from p in entity.Personeller where p.personelId == id select p).FirstOrDefault();
            if (personel == null)
            {
                return HttpNotFound();
            }
            return View(personel);
        }


        public ActionResult ExceleAktar()
        {

            // Personeller, YetkiTurler ve Birimler tablolarından gerekli verileri çekin
            var data = from p in entity.Personeller
                       join y in entity.YetkiTurler on p.personelYetkiTurId equals y.yetkiTurId
                       join b in entity.Birimler on p.personelBirimId equals b.birimId
                       where p.personelYetkiTurId != 3
                       select new
                       {
                           p.personelAdSoyad,
                           p.personelKullaniciAd,
                           y.yetkiTurAd,
                           b.birimAd,
                           p.personelAdres,
                           p.personelDahili,
                           p.personelDogumGunu,
                           p.personelIseGiris,
                           p.personelIzın,
                           p.personelMail,
                           p.personelTelefon,
                           

                       };

            var dataList = data.ToList();

            // Bulunan veri sayısını loglayın
            System.Diagnostics.Debug.WriteLine($"Bulunan kayıt sayısı: {dataList.Count}");

            if (!dataList.Any())
            {
                // Eğer veri yoksa bir mesaj dönebilirsiniz veya farklı bir işlem yapabilirsiniz.
                return Content("Export edilecek veri bulunamadı.");
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Detay");

                // Başlıkları ekleyin
                worksheet.Cells[1, 1].Value = "Personel Ad Soyad";
                worksheet.Cells[1, 2].Value = "Birim Adı";
                worksheet.Cells[1, 3].Value = "İşe Giriş Tarihi";
                worksheet.Cells[1, 4].Value = "Dahili Numarası";
                worksheet.Cells[1, 5].Value = "Kullanıcı Adı";
                worksheet.Cells[1, 6].Value = "Mail Adresi";
                worksheet.Cells[1, 7].Value = "Yetki Türü";
                worksheet.Cells[1, 8].Value = "Adres";
                worksheet.Cells[1, 9].Value = "Doğum Tarihi";
                worksheet.Cells[1, 10].Value = "Telefon Numarası";
                worksheet.Cells[1, 11].Value = "Kalan İzin Gün Sayısı";

                // Başlıkları stilize edin
                using (var range = worksheet.Cells[1, 1, 1, 10])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.White); // Arka plan rengini beyaz yapın
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Verileri ekleyin
                for (int i = 0; i < dataList.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = dataList[i].personelAdSoyad;
                    worksheet.Cells[i + 2, 2].Value = dataList[i].birimAd;
                    worksheet.Cells[i + 2, 3].Value = dataList[i].personelIseGiris;
                    worksheet.Cells[i + 2, 4].Value = dataList[i].personelDahili;
                    worksheet.Cells[i + 2, 5].Value = dataList[i].personelKullaniciAd;
                    worksheet.Cells[i + 2, 6].Value = dataList[i].personelMail;
                    worksheet.Cells[i + 2, 7].Value = dataList[i].yetkiTurAd;
                    worksheet.Cells[i + 2, 8].Value = dataList[i].personelAdres;
                    worksheet.Cells[i + 2, 9].Value = dataList[i].personelDogumGunu;
                    worksheet.Cells[i + 2, 10].Value = dataList[i].personelTelefon;
                    worksheet.Cells[i + 2, 11].Value = dataList[i].personelIzın;
                }

                // Otomatik genişlik ayarı
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Personel Listesi.xlsx");
            }
        }

    }
}