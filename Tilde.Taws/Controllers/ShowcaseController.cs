using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

using Tilde.Taws.Models;

namespace Tilde.Taws.Controllers
{
    /// <summary>
    /// Showcase web site.
    /// </summary>
    /// <remarks>
    /// ShowcaseController is responsible for showing the Showcase web site.
    /// It takes input data from a form, passes it on to ApiController and 
    /// then formats and displays the result.
    /// </remarks>
    public class ShowcaseController : Controller
    {
        /// <summary>
        /// Default page.
        /// </summary>
        /// <returns>View.</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// About and help page.
        /// </summary>
        /// <returns>View.</returns>
        public ActionResult About()
        {
            return View();
        }

        /// <summary>
        /// Contact us page.
        /// </summary>
        /// <returns>View.</returns>
        public ActionResult Contact()
        {
            return View();
        }

        /// <summary>
        /// Displays a form for submitting a document as plaintext.
        /// </summary>
        /// <returns>View.</returns>
        [HttpGet]
        public ActionResult Plaintext()
        {
            return View(DefaultForm("plaintext"));
        }

        /// <summary>
        /// Processes the submitted document as plaintext.
        /// </summary>
        /// <param name="doc">Submitted document.</param>
        /// <returns>View.</returns>
        [HttpPost]
        public async Task<ActionResult> Plaintext([ModelBinder(typeof(ModelBinder))] ApiDocument doc)
        {
            return await Results("plaintext", doc);
        }

        /// <summary>
        /// Displays a form for submitting an HTML5 document.
        /// </summary>
        /// <returns>View.</returns>
        [HttpGet]
        public ActionResult Html5()
        {
            return View(DefaultForm("html5"));
        }

        /// <summary>
        /// Processes the submitted HTML5 document.
        /// </summary>
        /// <param name="doc">Submitted document.</param>
        /// <returns>View.</returns>
        [HttpPost]
        public async Task<ActionResult> Html5([ModelBinder(typeof(ModelBinder))] ApiDocument doc)
        {
            return await Results("html5", doc);
        }

        /// <summary>
        /// Displays a form for submitting an XLIFF document.
        /// </summary>
        /// <returns>View.</returns>
        [HttpGet]
        public ActionResult Xliff()
        {
            return View(DefaultForm("xliff"));
        }

        /// <summary>
        /// Processes the submitted XLIFF document.
        /// </summary>
        /// <param name="doc">Submitted document.</param>
        /// <returns>View.</returns>
        [HttpPost]
        public async Task<ActionResult> Xliff([ModelBinder(typeof(ModelBinder))] ApiDocument doc)
        {
            return await Results("xliff", doc);
        }

        /// <summary>
        /// Processess and displays the submitted document.
        /// </summary>
        /// <param name="format">Document format (plaintext, html5, xliff).</param>
        /// <param name="doc">Submitted document.</param>
        /// <returns>View.</returns>
        private async Task<ActionResult> Results(string format, ApiDocument doc)
        {
            // document may be a URL, fetch the contents
            await GetTextFromUrl(doc);
            // document may be uploaded, read its contents
            await GetTextFromUploadedFile(doc);

            // only process the document if no error has already occurred
            if (ViewBag.State == null)
            {
                try
                {
                    // send the document to the API
                    string result = await MakeApiCall(format, doc);
                    // cache the result so that it can be downloaded later
                    string cacheKey = CacheResult(result);

                    ViewBag.State = "loaded";
                    ViewBag.Format = format;
                    ViewBag.Text = doc.Content;
                    ViewBag.Results = result;
                    ViewBag.DownloadID = cacheKey;
                }
                catch (Exception e)
                {
                    ViewBag.State = "error";
                    ViewBag.Error = e;
                }
            }

            return View("Results", doc);
        }

        /// <summary>
        /// Forces the download of the raw output of the processed document.
        /// Document is stored in a cache for a while and can be accessed by a key
        /// which was generated by CacheResult() in Results().
        /// </summary>
        /// <param name="id">Document key.</param>
        /// <param name="format">Document type.</param>
        /// <returns>View.</returns>
        [HttpPost]
        public ActionResult ResultDownload(string id, string format)
        {
            string result = GetCachedResult(id);

            if (result != null)
            {
                string extension = null;
                switch (format)
                {
                    case "plaintext":
                    case "html5": 
                        extension = ".html"; 
                        break;
                    case "xliff": extension = ".xlf"; 
                        break;
                }

                Response.Headers["Content-Disposition"] = "attachment; filename=\"its-document" + extension + "\"";
                return File(Encoding.UTF8.GetBytes((string)result), "application/force-download");
            }

            return Content("Requested document is no longer available.");
        }

        /// <summary>
        /// Creates a model with the default values
        /// and populates the form with choices.
        /// </summary>
        /// <returns>Document with default values.</returns>
        private ApiDocument DefaultForm(string format)
        {
            ApiDocument doc = new ApiDocument();
            doc.Language = "en";

            ViewBag.Languages = from lang in ShowcaseConfig.Settings.Root.Element("languages").Descendants("lang")
                                select new SelectListItem
                                {
                                    Value = lang.Attribute("id").Value,
                                    Text = lang.Value
                                };

            CultureInfo culture = new CultureInfo("en-US");
            ViewBag.Domains = new List<SelectListItem>();
            var mainDomains = from domain in ShowcaseConfig.Settings.Root.Element("eurovoc").Descendants("row")
                              where domain.Attribute("subject_id").Value.Length == 2
                              orderby domain.Attribute("description").Value
                              select domain;
            foreach (XElement domain in mainDomains)
            {
                ViewBag.Domains.Add(new SelectListItem
                {
                    Value = "eurovoc-" + domain.Attribute("subject_id").Value,
                    Text = culture.TextInfo.ToSentenceCase(domain.Attribute("description").Value)
                });

                var subdomains = from subdomain in ShowcaseConfig.Settings.Root.Element("eurovoc").Descendants("row")
                                 where subdomain.Attribute("subject_id").Value.Length == 4
                                 where subdomain.Attribute("subject_id").Value.StartsWith(domain.Attribute("subject_id").Value)
                                 orderby subdomain.Attribute("description").Value
                                 select subdomain;

                foreach (XElement subdomain in subdomains)
                {
                    ViewBag.Domains.Add(new SelectListItem
                    {
                        Value = "eurovoc-" + subdomain.Attribute("subject_id").Value,
                        Text = "-- " + culture.TextInfo.ToSentenceCase(subdomain.Attribute("description").Value)
                    });
                }
            }
            
            ViewBag.Format = format;

            return doc;
        }

        /// <summary>
        /// If the user submitted a URL, fetch its contents
        /// and use that as the content.
        /// </summary>
        private async Task GetTextFromUrl(ApiDocument doc)
        {
            try
            {
                string url = Request.Form["url"];

                if (!string.IsNullOrEmpty(url))
                {

                    if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                        url = "http://" + url;

                    WebClient browser = new WebClient() { Encoding = Encoding.UTF8 };
                    browser.Headers.Add("User-Agent", "Tilde ITS 2.0 Enriched Terminology Annotation Showcase");
                    doc.Content = await browser.DownloadStringTaskAsync(url);
                    doc.BaseUri = url;
                }
            }
            catch (Exception e)
            {
                ViewBag.State = "error";
                ViewBag.Error = e;
            }
        }

        /// <summary>
        /// If the user uploaded a file, get its contents
        /// and use that as the content.
        /// </summary>
        private async Task GetTextFromUploadedFile(ApiDocument doc)
        {
            try
            {
                if (Request.Files.Count > 0)
                {
                    HttpPostedFileBase file = Request.Files[0];

                    if (file != null && file.ContentLength > 0)
                    {
                        using (var reader = new StreamReader(file.InputStream))
                        {
                            doc.Content = await reader.ReadToEndAsync();
                            doc.BaseUri = file.FileName; // pointless, really
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.State = "error";
                ViewBag.Error = e;
            }
        }

        /// <summary>
        /// Call the TAWS API.
        /// We don't use ApiController directly because the goal of Showcase is to demonstrate how to use the TAWS API.
        /// It's assumed the API is hosted on the same host as this Showcase app.
        /// </summary>
        /// <param name="format">Document format.</param>
        /// <param name="doc">Document data.</param>
        /// <returns>Response from the API.</returns>
        private async Task<string> MakeApiCall(string format, ApiDocument doc)
        {
            if (!doc.UseStatisticalExtraction && !doc.UseTermBankExtraction)
                throw new Exception("No method selected. Please selected at least one method.");
            if (string.IsNullOrWhiteSpace(doc.Content))
                throw new Exception("No text was submitted.");

            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString[ApiController.LangParameter] = doc.Language;
            queryString[ApiController.DomainParameter] = string.Join(",", doc.Domains);
            queryString[ApiController.BaseUriParameter] = doc.BaseUri;
            queryString[ApiController.MethodParameter] = string.Join(",", new[] { 
                doc.UseStatisticalExtraction ? ApiController.StatisticalMethod : "",
                doc.UseTermBankExtraction ? ApiController.TermBankMethod : "" });

            // generate the url for the TAWS API
            string server = Request.Url.GetLeftPart(UriPartial.Authority);
            string url = string.Format("{0}/api/{1}/?{2}", server, format, queryString.ToString());
            
            // call the TAWS API
            HttpContent data = new StringContent(doc.Content, Encoding.UTF8);
            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.Parse(ConfigurationManager.AppSettings["TaaS_Timeout"]).Add(TimeSpan.FromSeconds(3));
            HttpResponseMessage response;
            try
            {
                response = await httpClient.PostAsync(url, data);
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("Annotation was canceled because the process took too long.");
            }
            
            string result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return result;
            }
            else
            {
                throw new Exception("API Exception", new Exception(result));
            }
        }

        /// <summary>
        /// Cache the result so that it can be downloaded later.
        /// </summary>
        /// <param name="result">Contents of the resulting document to be cached.</param>
        /// <returns>Document key to identify this document.</returns>
        private string CacheResult(string result)
        {
            string key = string.Join("", MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(DateTime.UtcNow + result)).Select(b => b.ToString("x2")));
            HttpRuntime.Cache.Insert(key, result, null, DateTime.UtcNow.AddHours(3), System.Web.Caching.Cache.NoSlidingExpiration);
            return key;
        }

        /// <summary>
        /// Returns the cached document.
        /// </summary>
        /// <param name="key">Document key.</param>
        /// <returns>Contents of the cached document.</returns>
        private string GetCachedResult(string key)
        {
            object result = HttpRuntime.Cache.Get(key);

            if (result != null && result is string)
                return (string)result;

            return null;
        }

        /// <summary>
        /// Creates a new <see cref="ApiDocument"/> from the submitted form data.
        /// </summary>
        private class ModelBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                var context = controllerContext.HttpContext;

                ApiDocument doc = new ApiDocument();
                doc.Language = context.Request.Form["Language"];
                doc.Content = context.Request.Unvalidated.Form["Text"];
                doc.Domains = (context.Request.Form["Domains"] ?? "").Split(',');
                doc.UseStatisticalExtraction = (context.Request.Form["Methods"] ?? "").Contains(ApiController.StatisticalMethod);
                doc.UseTermBankExtraction = (context.Request.Form["Methods"] ?? "").Contains(ApiController.TermBankMethod);
                return doc;
            }
        }
    }
}
