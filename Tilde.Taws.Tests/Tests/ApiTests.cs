using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Taws.Tests
{
    [TestClass]
    public class ApiTests
    {
        #region HTML5
        [TestMethod]
        public void API_Html5_Readme()
        {
            string input = @"<html lang=""en""><body>hello world</body></html>";
            string output = @"<html lang=""en""><body its-annotators-ref=""terminology|http://tilde.com/term-annotation-service""><span its-term=""yes"" its-term-confidence=""1"">hello world</span></body></html>";
            string uri = "/api/html5";
            string contentType = "text/html; charset=utf-8";

            Test(HttpStatusCode.OK, uri, input, output, contentType);
        }

        [TestMethod]
        public void API_Html5_Empty()
        {
            string input = @"";
            string output = @"<html><body its-annotators-ref=""terminology|http://tilde.com/term-annotation-service""></body></html>";
            string uri = "/api/html5";
            string contentType = "text/html; charset=utf-8";

            Test(HttpStatusCode.OK, uri, input, output, contentType);
        }

        [TestMethod]
        public void API_Html5_InvalidHtml()
        {
            string input = @"abc";
            string output = @"<html><body its-annotators-ref=""terminology|http://tilde.com/term-annotation-service"">abc</body></html>";
            string uri = "/api/html5";
            string contentType = "text/html; charset=utf-8";

            Test(HttpStatusCode.OK, uri, input, output, contentType);
        }

        [TestMethod]
        public void API_Html5_NoLang()
        {
            string input = @"<html><body>hello world</body></html>";
            string output = @"<html><body its-annotators-ref=""terminology|http://tilde.com/term-annotation-service"">hello world</body></html>";
            string uri = "/api/html5";
            string contentType = "text/html; charset=utf-8";

            Test(HttpStatusCode.OK, uri, input, output, contentType);
        }

        [TestMethod]
        public void API_Html5_UnsupportedLang()
        {
            string input = @"<html lang=""fr""><body>hello world</body></html>";
            string output = @"<html lang=""fr""><body its-annotators-ref=""terminology|http://tilde.com/term-annotation-service"">hello world</body></html>";
            string uri = "/api/html5";
            string contentType = "text/html; charset=utf-8";

            Test(HttpStatusCode.OK, uri, input, output, contentType);
        }

        [TestMethod]
        public void API_Html5_LangParameter_HtmlNoLang()
        {
            string input = @"<html><body>hello world</body></html>";
            string output = @"<html><body its-annotators-ref=""terminology|http://tilde.com/term-annotation-service""><span its-term=""yes"" its-term-confidence=""1"">hello world</span></body></html>";
            string uri = "/api/html5?lang=en";
            string contentType = "text/html; charset=utf-8";

            Test(HttpStatusCode.OK, uri, input, output, contentType);
        }

        [TestMethod]
        public void API_Html5_LangParameter_HtmlLang()
        {
            string input = @"<html lang=""en""><body>hello world</body></html>";
            string output = @"<html lang=""en""><body its-annotators-ref=""terminology|http://tilde.com/term-annotation-service""><span its-term=""yes"" its-term-confidence=""1"">hello world</span></body></html>";
            string uri = "/api/html5?lang=lv";
            string contentType = "text/html; charset=utf-8";

            Test(HttpStatusCode.OK, uri, input, output, contentType);
        }
        #endregion

        #region XLIFF
        [TestMethod]
        public void API_Xliff_Readme()
        {
            #region input
            string input = @"<?xml version=""1.0"" encoding=""utf-8""?>
	            <xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"">
	            <file original=""hello.txt"" source-language=""en-us"" target-language=""lv-lv"" datatype=""plaintext"">
	            <body>
	            <trans-unit id='1'>
	            <source>hello world</source>
	            </trans-unit>
	            </body>
	            </file>
	            </xliff>";
            #endregion
            #region output
            string output = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:its=""http://www.w3.org/2005/11/its"" xmlns:itsx=""http://www.w3.org/ns/its-xliff/"" its:annotatorsRef=""terminology|http://tilde.com/term-annotation-service"">
	            <file original=""hello.txt"" source-language=""en-us"" target-language=""lv-lv"" datatype=""plaintext"">
	            <body>
	            <trans-unit id=""1"">
	            <source><mrk mtype=""term"" itsx:termConfidence=""1"">hello world</mrk></source>
	            </trans-unit>
	            </body>
	            </file>
	            </xliff>";
            #endregion
            string uri = "/api/xliff";
            string contentType = "text/xml; charset=utf-8";

            Test(HttpStatusCode.OK, uri, input, output, contentType);
        }

        [TestMethod]
        public void API_Xliff_Empty()
        {
            string input = @"";
            string output = null;
            string uri = "/api/xliff";
            string contentType = "text/plain; charset=utf-8";

            Test(HttpStatusCode.BadRequest, uri, input, output, contentType);
        }

        [TestMethod]
        public void API_Xliff_InvalidXml()
        {
            string input = @"abc";
            string output = null;
            string uri = "/api/xliff";
            string contentType = "text/plain; charset=utf-8";

            Test(HttpStatusCode.BadRequest, uri, input, output, contentType);
        }

        [TestMethod]
        public void API_Xliff_InvalidXml2()
        {
            string input = @"<xliff>";
            string output = null;
            string uri = "/api/xliff";
            string contentType = "text/plain; charset=utf-8";

            Test(HttpStatusCode.BadRequest, uri, input, output, contentType);
        }
        #endregion

        #region Plaintext
        [TestMethod]
        public void API_Plaintext_Readme()
        {
            string input = @"hello world";
            string output = @"<!DOCTYPE html><html lang=""en""><head><meta charset=""utf-8"" /></head><body its-annotators-ref=""terminology|http://tilde.com/term-annotation-service""><span its-term=""yes"" its-term-confidence=""1"">hello world</span></body></html>";
            string uri = "/api/plaintext?lang=en";
            string contentType = "text/html; charset=utf-8";

            Test(HttpStatusCode.OK, uri, input, output, contentType);
        }

        [TestMethod]
        public void API_Plaintext_Empty()
        {
            string input = @"";
            string output = @"<!DOCTYPE html><html lang=""en""><head><meta charset=""utf-8"" /></head><body its-annotators-ref=""terminology|http://tilde.com/term-annotation-service""></body></html>";
            string uri = "/api/plaintext?lang=en";
            string contentType = "text/html; charset=utf-8";

            Test(HttpStatusCode.OK, uri, input, output, contentType);
        }

        [TestMethod]
        public void API_Plaintext_Html()
        {
            string input = @"hello world <p></p>";
            string output = @"<!DOCTYPE html><html lang=""en""><head><meta charset=""utf-8"" /></head><body its-annotators-ref=""terminology|http://tilde.com/term-annotation-service""><span its-term=""yes"" its-term-confidence=""1"">hello world</span> &lt;p&gt;&lt;/p&gt;</body></html>";
            string uri = "/api/plaintext?lang=en";
            string contentType = "text/html; charset=utf-8";

            Test(HttpStatusCode.OK, uri, input, output, contentType);
        }

        [TestMethod]
        public void API_Plaintext_NoLang()
        {
            string input = @"hello world";
            string output = @"<!DOCTYPE html><html><head><meta charset=""utf-8"" /></head><body its-annotators-ref=""terminology|http://tilde.com/term-annotation-service"">hello world</body></html>";
            string uri = "/api/plaintext";
            string contentType = "text/html; charset=utf-8";

            Test(HttpStatusCode.OK, uri, input, output, contentType);
        }
        #endregion

        #region
        private void Test(HttpStatusCode status, string uri, string input, string output, string outputContentType)
        {
            string url = "http://localhost:49886" + uri;

            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            HttpContent data = new StringContent(input, Encoding.UTF8);
            HttpResponseMessage response = httpClient.PostAsync(url, data).Result;

            Assert.AreEqual(status, response.StatusCode);
            Assert.AreEqual(outputContentType, response.Content.Headers.ContentType.ToString());

            string result = response.Content.ReadAsStringAsync().Result;

            if (output != null)
                Assert.AreEqual(OneLine(output), OneLine(result));
        }

        private string OneLine(string s)
        {
            return string.Join("", s.Replace("\r\n", "\n").Split('\n').Select(ss => ss.Trim()));
        }
        #endregion
    }
}
