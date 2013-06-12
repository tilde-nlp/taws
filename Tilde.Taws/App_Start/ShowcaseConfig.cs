using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Tilde.Taws
{
    /// <summary>
    /// Showcase configuration.
    /// </summary>
    public static class ShowcaseConfig
    {
        private static XDocument settings;

        /// <summary>
        /// Showcase configuration is stored in an XML file.
        /// </summary>
        public static XDocument Settings
        {
            get
            {
                if (settings == null)
                    settings = XDocument.Load(HttpContext.Current.Server.MapPath(@"~\Showcase.config"));

                return settings;
            }
        }

        /// <summary>
        /// List of languages that are supported in this application.
        /// Two letter language codes.
        /// </summary>
        public static string[] SupportedLanguages
        {
            get
            {
                return Settings.Root.Element("languages")
                                    .Elements("lang")
                                    .Select(e => e.Attribute("id").Value)
                                    .Select(s => s.ToLowerInvariant())
                                    .ToArray();
            }
        }

        /// <summary>
        /// Template to use when converting a plaintext document into an HTML5 document.
        /// </summary>
        public static string Html5TemplateForPlainText
        {
            get
            {
                return Settings.Root.Element("html5template").Value;
            }
        }
    }
}