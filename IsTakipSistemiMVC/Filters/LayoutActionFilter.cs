using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IsTakipSistemiMVC.Filters
{
    public class LayoutActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as Controller;
            if (controller != null)
            {
                int yetkiTurId = Convert.ToInt32(controller.Session["PersonelYetkiTurId"]);
                string layout = string.Empty;

                switch (yetkiTurId)
                {
                    case 1:
                        layout = "~/Views/Shared/_LayoutYonetici.cshtml";
                        break;
                    case 2:
                        layout = "~/Views/Shared/_LayoutCalisan.cshtml";
                        break;
                    case 3:
                        layout = "~/Views/Shared/_LayoutSistemYoneticisi.cshtml";
                        break;
                    default:
                        layout = "~/Views/Shared/_Layout.cshtml";
                        break;
                }

                controller.ViewBag.Layout = layout;
            }
            base.OnActionExecuting(filterContext);
        }
    }

}