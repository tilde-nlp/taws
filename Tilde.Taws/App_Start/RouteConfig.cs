using System.Web.Mvc;
using System.Web.Routing;

namespace Tilde.Taws
{
    /// <summary>
    /// Routing configuration.
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// Registers routes.
        /// </summary>
        /// <param name="routes">Route collection to add new routes to.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            // ignore some ASP.NET files
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // default and only route for Showcase
            routes.MapRoute(
                name: "Default",
                url: "{action}",
                defaults: new { controller = "Showcase", action = "Index" }
            );
        }
    }
}