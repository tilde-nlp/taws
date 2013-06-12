using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using Tilde.Taws.Models;

namespace Tilde.Taws.Controllers
{
    /// <summary>
    /// TAWS API.
    /// </summary>
    [ErrorHandler]
    public class ApiController : System.Web.Http.ApiController
    {
        /// <summary>
        /// Name of the query string parameter that specifies the default document content language.
        /// </summary>
        public const string LangParameter = "lang";
        /// <summary>
        /// Name of the query string parameter that specifies the document base path
        /// that should be used then the document contains references to external rules with relative paths.
        /// </summary>
        public const string BaseUriParameter = "baseUri";
        /// <summary>
        /// Name of the query string parameter that specifies the default document content domain.
        /// </summary>
        public const string DomainParameter = "domain";
        /// <summary>
        /// Name of the query string parameter that specifies what method to use to annotate terminology.
        /// </summary>
        public const string MethodParameter = "method";
        /// <summary>
        /// Value of the <see cref="MethodParameter"/> query string parameter that specifies the statistical annotation method.
        /// </summary>
        public const string StatisticalMethod = "statistical";
        /// <summary>
        /// Value of the <see cref="MethodParameter"/> query string parameter that specifies the term bank based annotation method.
        /// </summary>
        public const string TermBankMethod = "termbank";

        /// <summary>
        /// Annotates terminology in plaintext documents.
        /// </summary>
        /// <param name="doc">Submitted plaintext document.</param>
        /// <returns>Annotated document in HTML5 format.</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> Plaintext([ModelBinder(typeof(ModelBinder))] ApiDocument doc)
        {
            PlaintextAnnotator annotator = new PlaintextAnnotator(doc);
            annotator.Template = ShowcaseConfig.Html5TemplateForPlainText;
            string result = await annotator.Annotate();
            return Response(result, "text/html");
        }

        /// <summary>
        /// Annotates terminology in HTML5 documents.
        /// </summary>
        /// <param name="doc">Submitted HTML5 document.</param>
        /// <returns>Annotated document.</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> Html5([ModelBinder(typeof(ModelBinder))] ApiDocument doc)
        {
            Html5Annotator annotator = new Html5Annotator(doc);
            string result = await annotator.Annotate();
            return Response(result, "text/html");
        }

        /// <summary>
        /// Annotates terminology in XLIFF documents.
        /// </summary>
        /// <param name="doc">Submitted XLIFF document.</param>
        /// <returns>Annotated document.</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> Xliff([ModelBinder(typeof(ModelBinder))] ApiDocument doc)
        {
            XliffAnnotator annotator = new XliffAnnotator(doc);
            string result = await annotator.Annotate();
            return Response(result, "text/xml");
        }

        private HttpResponseMessage Response(string content, string contentType)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StringContent(content, Encoding.UTF8, contentType);
            return response;
        }

        /// <summary>
        /// Creates a new <see cref="ApiDocument"/> from the submitted form data.
        /// </summary>
        private class ModelBinder : IModelBinder
        {
            public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                var context = actionContext.Request.Properties["MS_HttpContext"] as System.Web.HttpContextBase;

                ApiDocument doc = new ApiDocument();
                doc.Language = context.Request.QueryString[LangParameter];
                doc.BaseUri = context.Request.QueryString[BaseUriParameter];

                // form body
                doc.Content = actionContext.Request.Content.ReadAsStringAsync().Result;

                string domains = context.Request.QueryString[DomainParameter];
                if (!string.IsNullOrWhiteSpace(domains))
                {
                    doc.Domains = domains.Split(',');
                }
                
                string methods = context.Request.QueryString[MethodParameter];
                if (!string.IsNullOrWhiteSpace(methods))
                {
                    doc.UseStatisticalExtraction = methods.Contains(StatisticalMethod);
                    doc.UseTermBankExtraction = methods.Contains(TermBankMethod);
                }
                else
                {
                    doc.UseStatisticalExtraction = true;
                    doc.UseTermBankExtraction = true;
                }
                
                bindingContext.Model = doc;
                return true;
            }
        }

        private class ErrorHandlerAttribute : ExceptionFilterAttribute
        {
            public override void OnException(HttpActionExecutedContext context)
            {
                HttpResponseMessage response = new HttpResponseMessage(Status(context.Exception));

                StringBuilder sb = new StringBuilder();
                Exception e = context.Exception;
                while (e != null)
                {
                    sb.AppendFormat("{1} ({0})", e.GetType().ToString(), e.Message);
                    e = e.InnerException;
                }

                response.Content = new StringContent(sb.ToString());

                context.Response = response;
            }

            private HttpStatusCode Status(Exception e)
            {
                if (e is ArgumentException || e is System.Xml.XmlException)
                    return HttpStatusCode.BadRequest;
                if (e is AnnotatorException)
                    return HttpStatusCode.InternalServerError;
                return HttpStatusCode.InternalServerError;
            }
        }
    }
}
