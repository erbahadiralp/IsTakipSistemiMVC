using IsTakipSistemiMVC.Filters;
using IsTakipSistemiMVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;

namespace IsTakipSistemiMVC.Controllers
{
    public class YoneticiController : Controller
    {
        IsTakipDBEntities entity = new IsTakipDBEntities();
        // GET: Yonetici
        public ActionResult Index()
        {
            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

            if (yetkiTurId == 1)
            {
                int birimId = Convert.ToInt32(Session["PersonelBirimId"]);
                var birim = (from b in entity.Birimler where b.birimId == birimId select b).FirstOrDefault();
                ViewBag.birimAd = birim.birimAd;

                int personelId = Convert.ToInt32(Session["PersonelId"]);
                var personeller1 = (from p in entity.Personeller where p.personelId == personelId select p).FirstOrDefault();
                ViewBag.personelAdSoyad = personeller1.personelAdSoyad;

                var personeller = from p in entity.Personeller
                                  join i in entity.Isler on p.personelId equals i.isPersonelId into isler
                                  where p.personelBirimId == birimId && p.personelYetkiTurId != 1
                                  select new
                                  {
                                      personelAd = p.personelAdSoyad,
                                      isler = isler
                                  };

                List<ToplamIs> list = new List<ToplamIs>();
                foreach (var personel in personeller)
                {
                    ToplamIs toplamIs = new ToplamIs();
                    toplamIs.personelAdSoyad = personel.personelAd;

                    if (personel.isler.Count() == 0)
                    {
                        toplamIs.toplamIs = 0;
                    }
                    else
                    {
                        int toplam = 0;
                        foreach (var item in personel.isler)
                        {
                            if (item.yapilanTarih != null)
                            {
                                toplam++;
                            }
                        }
                        toplamIs.toplamIs = toplam;
                    }

                    list.Add(toplamIs);
                }

                IEnumerable<ToplamIs> siraliListe = list.OrderByDescending(i => i.toplamIs);

                int simdikiYil = DateTime.Now.Year;
                List<int> yillar = new List<int>();
                for (int i = simdikiYil; i >= 2024; i--)
                {
                    yillar.Add(i);
                }
                ViewBag.yillar = yillar;

                if (TempData["ayinElemani"] != null)
                {
                    ViewBag.ayinElemani = TempData["ayinElemani"];
                }

                DateTime bugun = DateTime.Today;
                var bugunYemek = entity.YemekTablo
                    .Where(y => DbFunctions.TruncateTime(y.Tarih) == bugun)
                    .FirstOrDefault();
                ViewBag.BugunYemek = bugunYemek;

                var sonDuyurular = entity.Duyurular
                    .Where(d => d.aktiflik == true && d.goruntuleyenBirimId == birimId)
                    .OrderByDescending(d => d.duyuruTarih)
                    .Take(3)
                    .ToList();
                ViewBag.SonDuyurular = sonDuyurular;

                var calisanlar = (from p in entity.Personeller
                                  where p.personelBirimId == birimId && p.personelYetkiTurId == 2 && p.aktiflik == true
                                  select p).ToList();
                ViewBag.personeller = calisanlar;

                // BirimPersonelCarousel işlevselliği
                var carouselPersoneller = (from p in entity.Personeller
                                           where p.personelBirimId == birimId && p.personelYetkiTurId == 2 && p.aktiflik == true
                                           select new CalisanViewModel
                                           {
                                               PersonelAdSoyad = p.personelAdSoyad,
                                               PersonelFotograf = p.personelFotograf
                                           }).ToList();
                ViewBag.CarouselPersoneller = carouselPersoneller;

                return View(siraliListe);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult Ata()
        {

            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

            if (yetkiTurId == 1)
            {
                int birimId = Convert.ToInt32(Session["PersonelBirimId"]);

                var calisanlar = (from p in entity.Personeller where p.personelBirimId == birimId && p.personelYetkiTurId == 2 && p.aktiflik == true select p).ToList();

                ViewBag.personeller = calisanlar;

                var birim = (from b in entity.Birimler where b.birimId == birimId select b).FirstOrDefault();

                ViewBag.birimAd = birim.birimAd;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        [HttpPost]
        public ActionResult Ata(FormCollection formCollection)
        {
            string isBaslik = formCollection["isBaslik"];
            string isAciklama = formCollection["isAciklama"];
            int secilenPersonelId = Convert.ToInt32(formCollection["selectPer"]);

            Isler yeniIs = new Isler();

            yeniIs.isBaslik = isBaslik;
            yeniIs.isAciklama = isAciklama;
            yeniIs.isPersonelId = secilenPersonelId;
            yeniIs.iletilenTarih = DateTime.Now;
            yeniIs.isDurumId = 1;
            yeniIs.isOkunma = false;

            entity.Isler.Add(yeniIs);
            entity.SaveChanges();

            return RedirectToAction("Takip", "Yonetici");
        }

        public ActionResult Takip()
        {
            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

            if (yetkiTurId == 1)
            {
                int birimId = Convert.ToInt32(Session["PersonelBirimId"]);

                var calisanlar = (from p in entity.Personeller
                                  where p.personelBirimId == birimId && p.personelYetkiTurId == 2 && p.aktiflik == true
                                  select new CalisanViewModel
                                  {
                                      PersonelId = p.personelId,
                                      PersonelAdSoyad = p.personelAdSoyad,
                                      TotalJobs = entity.Isler.Count(i => i.isPersonelId == p.personelId),
                                      CompletedJobs = entity.Isler.Count(i => i.isPersonelId == p.personelId && i.isDurumId == 2),
                                      UncompletedJobs = entity.Isler.Count(i => i.isPersonelId == p.personelId && i.isDurumId == 1)
                                  }).ToList();

                ViewBag.birimAd = (from b in entity.Birimler where b.birimId == birimId select b.birimAd).FirstOrDefault();

                return View(calisanlar);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public ActionResult Takip(int selectPer)
        {
            var secilenPersonel = (from p in entity.Personeller where p.personelId == selectPer select p).FirstOrDefault();

            TempData["secilen"] = secilenPersonel;

            return RedirectToAction("Listele", "Yonetici");
        }




        [HttpGet]
        public ActionResult Listele()
        {
            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

            if (yetkiTurId == 1)
            {
                Personeller secilenPersoneller = (Personeller)TempData["secilen"];

                try
                {
                    var isler = (from i in entity.Isler
                                 where i.isPersonelId == secilenPersoneller.personelId
                                 select i).ToList().OrderByDescending(i => i.iletilenTarih).ToList();

                    var model = new PersonelIslerViewModel
                    {
                        Personel = secilenPersoneller,
                        Isler = isler,
                        IsSayisi = isler.Count()
                    };

                    return View(model);

                }
                catch (Exception)
                {
                    return RedirectToAction("Takip", "Yonetici");
                }

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }



        [HttpPost]
        public ActionResult AyinElemani(int aylar, int yillar)
        {
            int birimId = Convert.ToInt32(Session["PersonelBirimId"]);

            var personeller = entity.Personeller.Where(p => p.personelBirimId == birimId).Where(p => p.personelYetkiTurId != 1);

            DateTime baslangicTarih = new DateTime(yillar, aylar, 1);
            DateTime bitisTarih = baslangicTarih.AddMonths(1).AddSeconds(-1);

            var isler = entity.Isler.Where(i => i.yapilanTarih >= baslangicTarih).Where(i => i.yapilanTarih <= bitisTarih);

            var groupJoin = personeller.GroupJoin(isler, p => p.personelId, i => i.isPersonelId, (p, group) => new
            {
                sonucIsler = group,
                personelAd = p.personelAdSoyad
            });

            List<ToplamIs> list = new List<ToplamIs>();

            foreach (var personel in groupJoin)
            {
                ToplamIs toplamIs = new ToplamIs();
                toplamIs.personelAdSoyad = personel.personelAd;

                if (personel.sonucIsler.Count() == 0)
                {
                    toplamIs.toplamIs = 0;
                }
                else
                {
                    int toplam = 0;
                    foreach (var item in personel.sonucIsler)
                    {
                        if (item.yapilanTarih != null)
                        {
                            toplam++;
                        }
                    }

                    toplamIs.toplamIs = toplam;
                }

                list.Add(toplamIs);
            }

            var ayinElemani = list.OrderByDescending(i => i.toplamIs).FirstOrDefault();

            TempData["ayinElemani"] = ayinElemani;

            return RedirectToAction("Index");
        }

        public ActionResult IzinGoruntule()
        {
            int personelBirimId = Convert.ToInt32(Session["PersonelBirimId"]);
            var izinler = entity.Izinler
                .Where(i => i.Personeller.personelBirimId == personelBirimId)
                .ToList();

            return View(izinler);
        }

        [HttpPost]
        public ActionResult IzinOnayla(int izinId, bool onay)
        {
            var izin = entity.Izinler.FirstOrDefault(i => i.izinId == izinId);

            if (izin != null)
            {
                izin.onayDurumu = onay;
                entity.SaveChanges();

                if (onay)
                {
                    var personel = entity.Personeller.FirstOrDefault(p => p.personelId == izin.personelId);
                    if (personel != null)
                    {
                        int izinGunSayisi = ((izin.bitisTarihi - izin.baslangicTarihi).Days) + 1;
                        personel.personelIzın -= izinGunSayisi;
                        entity.SaveChanges();
                    }
                }
            }

            return RedirectToAction("IzinGoruntule");
        }

        [HttpPost]
        public ActionResult IzinReddet(int izinId)
        {
            var izin = entity.Izinler.FirstOrDefault(i => i.izinId == izinId);

            if (izin != null)
            {
                izin.onayDurumu = false;
                entity.SaveChanges();
            }

            return RedirectToAction("IzinGoruntule");
        }

        //public ActionResult BirimPersonelCarousel()
        //{
        //    int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

        //    if (yetkiTurId == 1)
        //    {
        //        int birimId = Convert.ToInt32(Session["PersonelBirimId"]);

        //        var personeller = (from p in entity.Personeller
        //                           where p.personelBirimId == birimId && p.personelYetkiTurId == 2 && p.aktiflik == true
        //                           select new CalisanViewModel
        //                           {
        //                               PersonelAdSoyad = p.personelAdSoyad,
        //                               PersonelFotograf = p.personelFotograf
        //                           }).ToList();

        //        return View(personeller);
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }
        //}


    }
}