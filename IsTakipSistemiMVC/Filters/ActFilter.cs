using IsTakipSistemiMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IsTakipSistemiMVC.Filters
{
    public class ActFilter : FilterAttribute, IActionFilter
    {
        IsTakipDBEntities entity = new IsTakipDBEntities();

        protected string aciklama;
        public ActFilter(string actAciklama) 
        { 
            this.aciklama = actAciklama;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Controller.TempData["bilgi"] != null)
            {
                Loglar log = new Loglar();

                log.logAciklama = this.aciklama + " (" + filterContext.Controller.TempData["bilgi"] + ")";
                log.actionAd = filterContext.ActionDescriptor.ActionName;
                log.controllerAd = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                log.tarih = DateTime.Now;
                log.personelId = Convert.ToInt32(filterContext.HttpContext.Session["PersonelId"]);

                entity.Loglar.Add(log);

                entity.SaveChanges();
            }
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }
    }
}