using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace Tilde.Taws
{
    /// <summary>
    /// Web API configuration.
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Registers routes for Web API.
        /// </summary>
        /// <param name="routes">Route collection to add new routes to.</param>
        public static void RegisterRoutes(HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: "API/Plaintext",
                routeTemplate: "api/plaintext",
                defaults: new { controller = "Api", action = "Plaintext" }
            );

            routes.MapHttpRoute(
                name: "API/HTML5",
                routeTemplate: "api/html5",
                defaults: new { controller = "Api", action = "Html5" }
            );

            routes.MapHttpRoute(
                name: "API/XLIFF",
                routeTemplate: "api/xliff",
                defaults: new { controller = "Api", action = "Xliff" }
            );
        }

        /// <summary>
        /// Registers formatters for Web API.
        /// </summary>
        /// <param name="formatters">Formatter collection to modify.</param>
        public static void RegisterFormatters(MediaTypeFormatterCollection formatters)
        {
            formatters.Clear();
            formatters.Add(new PlainTextFormatter());
        }

        private class PlainTextFormatter : MediaTypeFormatter
        {
            public PlainTextFormatter()
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));
            }

            public override bool CanReadType(Type type)
            {
                return true;
            }

            public override bool CanWriteType(Type type)
            {
                return true;
            }

            public override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContent content, IFormatterLogger formatterLogger)
            {
                var reader = new StreamReader(stream);
                string value = reader.ReadToEnd();

                var tcs = new TaskCompletionSource<object>();
                tcs.SetResult(value);
                return tcs.Task;
            }

            public override Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContent content, TransportContext transportContext)
            {
                var writer = new StreamWriter(stream);
                writer.Write(value.ToString());
                writer.Flush();

                var tcs = new TaskCompletionSource<object>();
                tcs.SetResult(null);
                return tcs.Task;
            }
        }
    }
}
