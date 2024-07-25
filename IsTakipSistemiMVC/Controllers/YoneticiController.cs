using IsTakipSistemiMVC.Filters;
using IsTakipSistemiMVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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


                //var yemekListesi = entity.YemekTablo.ToList();
                //ViewBag.MenuListesi = yemekListesi;

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

                // ViewBag.yillar'ı ayarlayın
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

                // Günün yemeğini al
                DateTime bugun = DateTime.Today;
                var bugunYemek = entity.YemekTablo
                    .Where(y => DbFunctions.TruncateTime(y.Tarih) == bugun)
                    .FirstOrDefault();
                ViewBag.BugunYemek = bugunYemek;


                // En son tarihli duyuruyu al
                var sonDuyuru = entity.Duyurular
                    .Where(d => d.aktiflik == true && d.goruntuleyenBirimId == birimId)
                    .OrderByDescending(d => d.duyuruTarih)
                    .FirstOrDefault();
                ViewBag.SonDuyuru = sonDuyuru;



                var calisanlar = (from p in entity.Personeller where p.personelBirimId == birimId && p.personelYetkiTurId == 2 && p.aktiflik == true select p).ToList();

                ViewBag.personeller = calisanlar;


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
                    var isler = (from i in entity.Isler where i.isPersonelId == secilenPersoneller.personelId select i).ToList().OrderByDescending(i => i.iletilenTarih);

                    ViewBag.isler = isler;
                    ViewBag.personel = secilenPersoneller;
                    ViewBag.isSayisi = isler.Count();

                    return View();

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
        /*
        public ActionResult AyinElemani()
        {
            int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

            if (yetkiTurId == 1)
            {
                int simdikiYil = DateTime.Now.Year;

                List<int> yillar = new List<int>();

                for(int i = simdikiYil; i >= 2024; i--)
                {
                    yillar.Add(i);
                }

                ViewBag.yillar = yillar;
                ViewBag.ayinElemani = null;

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }*/

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

        public ActionResult Profil()
        {

            int id = Convert.ToInt32(Session["PersonelId"]);
            var personel = (from p in entity.Personeller where p.personelId == id select p).FirstOrDefault();
            if (personel == null)
            {
                return HttpNotFound();
            }
            return View(personel);
        }

        //public ActionResult CalisanListe()
        //{
        //    int yetkiTurId = Convert.ToInt32(Session["PersonelYetkiTurId"]);

        //    if (yetkiTurId == 1)
        //    {
        //        int birimId = Convert.ToInt32(Session["PersonelBirimId"]);



        //        var birim = (from b in entity.Birimler where b.birimId == birimId select b).FirstOrDefault();

        //        ViewBag.birimAd = birim.birimAd;

        //        return View();
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }
        //}
        //DUYURULAR KISMI

        // Duyuruların listelenmesi
        [AuthFilter(1)]
        public ActionResult Duyurular()
        {
            var duyuruList = (from d in entity.Duyurular
                              join p in entity.Personeller on d.duyuruOlusturanId equals p.personelId
                              join b in entity.Birimler on d.goruntuleyenBirimId equals b.birimId
                              where d.aktiflik == true && d.goruntuleyenBirimId == p.personelBirimId
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

            return View(duyurular);
        }



        // Duyuru detayları
        [AuthFilter(1)]
        public ActionResult DuyuruDetay(int id)
        {
            var duyuru = entity.Duyurular.Find(id);
            if (duyuru == null)
            {
                return HttpNotFound();
            }
            return View(duyuru);
        }


        [AuthFilter(1)]
        public ActionResult DuyuruEkle()
        {
            int personelId = Convert.ToInt32(Session["PersonelId"]);
            var personel = entity.Personeller.Find(personelId);
            var personelBirimId = personel.personelBirimId;

            var duyuru = new Duyurular
            {
                duyuruOlusturanId = personelId,
                goruntuleyenBirimId = personelBirimId
            };

            return View(duyuru);
        }

        // Handle form submission to create a new announcement
        [HttpPost, AuthFilter(1)]
        public ActionResult DuyuruEkle(Duyurular duyuru)
        {
            if (ModelState.IsValid)
            {
                int personelId = Convert.ToInt32(Session["PersonelId"]);
                var personel = entity.Personeller.Find(personelId);

                duyuru.duyuruOlusturanId = personelId;
                duyuru.goruntuleyenBirimId = personel.personelBirimId;
                duyuru.duyuruTarih = DateTime.Now;
                duyuru.aktiflik = true;

                entity.Duyurular.Add(duyuru);
                entity.SaveChanges();

                return RedirectToAction("Duyurular");
            }

            return View(duyuru);
        }


        [AuthFilter(1)]
        public ActionResult DuyuruGuncelle(int id)
        {
            var duyuru = entity.Duyurular.Find(id);
            if (duyuru == null)
            {
                return HttpNotFound();
            }

            int currentPersonelId = Convert.ToInt32(Session["PersonelId"]);
            int currentBirimId = Convert.ToInt32(Session["BirimId"]);

            // Check if the current user has permission to update the announcement
            if (duyuru.duyuruOlusturanId != currentPersonelId &&
                !entity.Personeller.Any(p => p.personelBirimId == currentBirimId && p.personelYetkiTurId == 1 && p.personelId == currentPersonelId))
            {
                return RedirectToAction("Error", "Yonetici", new { message = "Bu duyuruyu güncellemeye yetkiniz yok." });
            }

            var birimler = entity.Birimler.Where(b => b.birimId == currentBirimId ||
                entity.Personeller.Any(p => p.personelBirimId == b.birimId && p.personelYetkiTurId == 1 && p.personelId != currentPersonelId)).ToList();

            ViewBag.Birimler = new SelectList(birimler, "birimId", "birimAd", duyuru.goruntuleyenBirimId);

            return View(duyuru);
        }

        [HttpPost, ActFilter("Duyuru Güncellendi")]
        public ActionResult DuyuruGuncelle(Duyurular duyuru)
        {
            var mevcutDuyuru = entity.Duyurular.Find(duyuru.duyuruId);
            if (mevcutDuyuru == null)
            {
                return HttpNotFound();
            }

            int currentPersonelId = Convert.ToInt32(Session["PersonelId"]);

            // Check if the current user has permission to update the announcement
            if (mevcutDuyuru.duyuruOlusturanId != currentPersonelId &&
                !entity.Personeller.Any(p => p.personelBirimId == mevcutDuyuru.goruntuleyenBirimId && p.personelYetkiTurId == 1 && p.personelId == currentPersonelId))
            {
                return RedirectToAction("Error", "Yonetici", new { message = "Bu duyuruyu güncellemeye yetkiniz yok." });
            }

            mevcutDuyuru.duyuruBaslik = duyuru.duyuruBaslik;
            mevcutDuyuru.duyuruIcerik = duyuru.duyuruIcerik;
            mevcutDuyuru.goruntuleyenBirimId = duyuru.goruntuleyenBirimId;
            mevcutDuyuru.duyuruTarih = DateTime.Now; // Tarihi güncelle
            entity.SaveChanges();
            return RedirectToAction("Duyurular");
        }


        // Duyuru silme işlemi 
        [ActFilter("Duyuru Silindi"), AuthFilter(1)]

        public ActionResult DuyuruSil(int id)
        {
            var duyuru = entity.Duyurular.Find(id);
            if (duyuru == null)
            {
                return HttpNotFound();
            }

            // Retrieve the current session's personelId
            int currentPersonelId = (int)Session["personelId"];

            // Check if the announcement was created by the current person
            if (duyuru.duyuruOlusturanId != currentPersonelId)
            {
                // Redirect to the custom error page with a message
                return RedirectToAction("Error", "Yonetici", new { message = "Bu duyuruyu silmeye yetkiniz yok." });
            }

            duyuru.aktiflik = false;
            entity.SaveChanges();
            return RedirectToAction("Duyurular");
        }

        public ActionResult Error(string message)
        {
            ViewBag.ErrorMessage = message;
            ViewBag.ErrorMessage = message;
            return View();
        }




    }
}