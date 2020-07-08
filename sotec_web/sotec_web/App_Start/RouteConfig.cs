using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;

namespace sotec_web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}/{id2}",
                defaults: new { controller = "Home", action = (ConfigurationManager.AppSettings["yapim_asamasinda_modu"] == "true" ? "ComingSoon" : "Index"), id = UrlParameter.Optional, id2 = UrlParameter.Optional }
            );
        }
    }
}
