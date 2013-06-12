using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Tilde.Taws
{
    /// <summary>
    /// ASP.NET application.
    /// </summary>
    public class Application : System.Web.HttpApplication
    {
        /// <summary>
        /// Called when the first resource (such as a page) in an ASP.NET application is requested. 
        /// The Application_Start method is called only one time during the life cycle of an application. 
        /// You can use this method to perform startup tasks such as loading data into the cache and initializing static values.
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.RegisterRoutes(GlobalConfiguration.Configuration.Routes);
            WebApiConfig.RegisterFormatters(GlobalConfiguration.Configuration.Formatters);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}