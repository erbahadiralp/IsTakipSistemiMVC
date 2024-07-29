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

    public class ProfilController : Controller
    {
        IsTakipDBEntities entity = new IsTakipDBEntities();

        // GET: Profil
        public ActionResult Index()
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