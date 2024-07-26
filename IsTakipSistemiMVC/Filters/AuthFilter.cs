using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IsTakipSistemiMVC.Filters
{
    public class AuthFilter : FilterAttribute, IAuthorizationFilter
    {
        private readonly int[] allowedYetkiTurler;

        public AuthFilter(params int[] yetkiTurler)
        {
            this.allowedYetkiTurler = yetkiTurler;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            int yetkiTurId = Convert.ToInt32(filterContext.HttpContext.Session["PersonelYetkiTurId"]);

            if (!allowedYetkiTurler.Contains(yetkiTurId))
            {
                filterContext.Result = new RedirectResult("/Login/Index");
            }
        }
    }
}
