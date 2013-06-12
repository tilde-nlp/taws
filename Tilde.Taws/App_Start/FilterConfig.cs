using System.Web.Mvc;

namespace Tilde.Taws
{
    /// <summary>
    /// Global filters configuration.
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        /// Registers global filters.
        /// </summary>
        /// <param name="filters">Filters to add new filters to.</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}