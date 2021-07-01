
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace FRT.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Report",
                url: "Report/{aspx}.aspx",
                defaults: new { httproute = true }

            );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );



        }
    }
}
