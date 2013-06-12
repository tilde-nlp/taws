using System.Web.Optimization;

namespace Tilde.Taws
{
    /// <summary>
    /// Bundling and minification configuration.
    /// </summary>
    /// <remarks>
    /// Bundling and minification are two techniques you can use to improve request load time.  
    /// Bundling and minification improves load time by reducing the number of requests to the server and reducing the size of requested assets (such as CSS and JavaScript).
    /// <see href="http://www.asp.net/mvc/tutorials/mvc-4/bundling-and-minification"/>
    /// </remarks>
    public class BundleConfig
    {
        /// <summary>
        /// Registers and configures bundles.
        /// </summary>
        /// <param name="bundles">Bundle collection to add new bundles to.</param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            // site css
            bundles.Add(new StyleBundle("~/styles/site").Include("~/Content/styles/site.css"));

            // visualization
            bundles.Add(new StyleBundle("~/styles/visualization").Include("~/Content/styles/visualization.css"));
            bundles.Add(new ScriptBundle("~/scripts/visualization").Include("~/Content/scripts/visualization.js"));

            // jQuery
            bundles.Add(new ScriptBundle("~/scripts/jquery").Include("~/Content/scripts/jquery-{version}.js"));
            // jQuery UI (custom)
            bundles.Add(new ScriptBundle("~/scripts/jquery-ui").Include("~/Content/scripts/jquery-ui-{version}.custom.js"));
            bundles.Add(new StyleBundle("~/styles/jquery-ui").Include("~/Content/styles/jquery-ui.css"));
        }
    }
}