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

    public class ParolaKontrolController : Controller
    {
        IsTakipDBEntities entity = new IsTakipDBEntities();
        // GET: ParolaKontrol
        public ActionResult Index()
        {
            int personelId = Convert.ToInt32(Session["PersonelId"]);

            if (personelId == 0) return RedirectToAction("Index", "Login");

            var personel = (from p in entity.Personeller where p.personelId == personelId select p).FirstOrDefault();

            ViewBag.mesaj = null;
            ViewBag.yetkiTurId = null;
            ViewBag.sitil = null;

            return View(personel);
        }

        [HttpPost,ActFilter("Parola Değiştirildi")]
        public ActionResult Index(int personelId, string eskiParola, string yeniParola, string yeniParolaKontrol)
        {

            var personel = (from p in entity.Personeller where p.personelId == personelId select p).FirstOrDefault();

            if(eskiParola != personel.personelParola)
            {
                ViewBag.mesaj = "Eski parolanızı yanlış girdiniz";
                ViewBag.sitil = "alert alert-danger";

                return View(personel);
            }

            if(yeniParola != yeniParolaKontrol)
            {
                ViewBag.mesaj = "Yeni Parola ve Yeni Parola tekrarı eşleşmedi";
                ViewBag.sitil = "alert alert-danger";

                return View(personel);
            }

            if(yeniParola.Length < 6 || yeniParola.Length > 15)
            {
                ViewBag.mesaj = "Yeni Parola en az 6 karakter en çok 15 karakter olmalıdır";
                ViewBag.sitil = "alert alert-danger";
                return View(personel);

            }

            personel.personelParola = yeniParola;
            personel.yeniPersonel = false;
            entity.SaveChanges();

            TempData["bilgi"] = personel.personelKullaniciAd;

            ViewBag.mesaj = "Parolanız Başarıyla Değiştirildi. Anasayfaya yönlendiriliyorsunuz";
            ViewBag.sitil = "alert alert-success";
            ViewBag.yetkiTurId = personel.personelYetkiTurId;

            return View(personel);
        }
    }
}