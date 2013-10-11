using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

using Newtonsoft.Json;

namespace Tilde.Taws.Models
{
    /// <summary>
    /// TaaS client.
    /// </summary>
    public class TaaS
    {
        private const string ParagraphSeparator = "\n ... ... \n";
        private const string FragmentSeparator = "\n .. .. \n";

        #region Configuration
        /// <summary>
        /// Protocol, host and port of the TaaS server.
        /// </summary>
        private static readonly string ServerUrl = ConfigurationManager.AppSettings["TaaS_Server"];
        /// <summary>
        /// Username for TaaS.
        /// </summary>
        private static readonly string Username = ConfigurationManager.AppSettings["TaaS_Username"];
        /// <summary>
        /// Password for TaaS.
        /// </summary>
        private static readonly string Password = ConfigurationManager.AppSettings["TaaS_Password"];
        /// <summary>
        /// How long to wait for a response from TaaS.
        /// </summary>
        private static readonly TimeSpan Timeout = TimeSpan.Parse(ConfigurationManager.AppSettings["TaaS_Timeout"]);
        /// <summary>
        /// Languages that are supported by TaaS.
        /// </summary>
        private static readonly string[] SupportedLanguages = ShowcaseConfig.SupportedLanguages;
        /// <summary>
        /// Max number of characters to send for annotations of a text.
        /// This is to prevent sending long texts that will take a long time to annotate.
        /// </summary>
        private static readonly int MaxContentLength = int.Parse(ConfigurationManager.AppSettings["TaaS_MaxLength"]);
        #endregion

        /// <summary>
        /// Whether to use the statistical annotation method.
        /// </summary>
        public bool UseStatisticalExtraction { get; set; }
        /// <summary>
        /// Wheter to use the term bank based annotation method.
        /// </summary>
        public bool UseTermBankExtraction { get; set; }

        /// <summary>
        /// Method parameter value for TaaS.
        /// </summary>
        private string Method
        {
            get
            {
                if (UseStatisticalExtraction && UseTermBankExtraction)
                    return "3";
                if (UseStatisticalExtraction)
                    return "1";
                if (UseTermBankExtraction)
                    return "2";
                return null;
            }
        }

        /// <summary>
        /// Annotates terminology in a text using TaaS.
        /// </summary>
        /// <param name="language">Language of the text.</param>
        /// <param name="domain">Domain of the text.</param>
        /// <param name="text">Paragraphs consisting of separated pieces of text.</param>
        /// <returns>Annotated text.</returns>
        public async Task<string[][]> Annotate(string language, string domain, string[][] text)
        {
            try
            {
                if (text == null || text.Length == 0)
                    return text;

                string allinone = string.Join(ParagraphSeparator, text.Select(s => string.Join(FragmentSeparator, s)));
                string result = await Annotate(language, domain, allinone);
                return result.Split(new[] { ParagraphSeparator }, StringSplitOptions.None).Select(s => s.Split(new[] { FragmentSeparator }, StringSplitOptions.None).ToArray()).ToArray();
            }
            catch (Exception e)
            {
                throw new TaaSException(e);
            }
        }

        /// <summary>
        /// Annotates terminology in a text using TaaS.
        /// </summary>
        /// <param name="language">Language of the text.</param>
        /// <param name="domain">Domain of the text.</param>
        /// <param name="text">Text to annotate.</param>
        /// <returns>Annotated text.</returns>
        public async Task<string> Annotate(string language, string domain, string text)
        {
            try
            {
                // language
                if (language != null)
                {
                    language = language.ToLowerInvariant();

                    if (language.Length > 2)
                        language = language.Substring(0, 2);
                }
                if (!SupportedLanguages.Contains(language))
                    return text;

                // domain
                if (string.IsNullOrWhiteSpace(domain))
                    domain = null;

                // text
                if (string.IsNullOrWhiteSpace(text))
                    return text;

                if (Method == null)
                    return text;

                string url = ServerUrl + string.Format("/extraction/?sourceLang={0}&domain={1}&method={2}&collection=etb",
                    language, Uri.EscapeUriString(domain ?? ""), Method);

                // send the first n characters of the text
                // append the rest after receiving the response
                string start = text;
                string end = "";
                if (text.Length > MaxContentLength)
                {
                    start = text.Substring(0, MaxContentLength);
                    end = text.Substring(MaxContentLength);
                }

                System.Diagnostics.Debug.WriteLine("TaaS: {0}\n<text>\n{1}\n</text>\n", url, start);
                string resultString = await MakeApiRequest(url, start);
                XDocument result = XDocument.Parse(resultString);
                System.Diagnostics.Debug.WriteLine("TaaS result: {0}\n<result>\n{1}\n</result>\n", url, result.Root.Element("text").Value);
                result.Root.Element("text").Value += end;

                XElement textXml = ToXml(result.Root.Element("text").Value);

                // TBX
                if (result.Root.Element("terms") != null)
                {
                    XDocument terms = XDocument.Parse(result.Root.Element("terms").Value);
                    SaveTermEntries(textXml, terms);
                }

                ExtractSingleTerms(textXml);

                return HttpUtility.HtmlDecode(textXml.ToInnerString());
            }
            catch (Exception e)
            {
                throw new TaaSException(e);
            }
        }

        #region TBX
        /// <summary>
        /// &lt;termEntry&gt; of all terms received from TaaS.
        /// </summary>
        private Dictionary<string, XDocument> tbxTermEntries = new Dictionary<string, XDocument>();

        /// <summary>
        /// Gets term information (also known as TBX) for a term by its ID.
        /// </summary>
        /// <param name="termID">ID of the term.</param>
        /// <returns>Term information in full TBX format.</returns>
        public XDocument GetTermEntry(string termID)
        {
            if (!tbxTermEntries.ContainsKey(termID))
                return null;

            XDocument termEntry = tbxTermEntries[termID];

            // remove duplicate xrefs
            foreach (XElement ntig in termEntry.Descendants("ntig"))
            {
                IEnumerable<XElement> xrefs = ntig.Elements("xref");

                foreach (XElement xref in xrefs)
                {
                    xrefs.Where(e => xref != e)
                         .Where(e => XNode.DeepEquals(xref, e))
                         .Where(e => e.Parent != null)
                         .ToList()
                         .ForEach(e => e.Remove());
                }
            }

            return termEntry;
        }

        /// <summary>
        /// Saves the term entries and replaces ids in the text.
        /// </summary>
        /// <remarks>
        /// Some terms have an id: &lt;tename termID="etb-1"&gt;...  &lt;tename termID="etb-2"&gt;...
        /// These ids are not unique across requests.
        /// That's why we are generating new ids for them.
        /// </remarks>
        /// <param name="text">Annotated text from TaaS.</param>
        /// <param name="terms">Term information from TaaS as an TBX document.</param>
        private void SaveTermEntries(XElement text, XDocument terms)
        {
            if (terms == null)
                return;

            // TBX template
            XDocument tbx = new XDocument(terms);
            // make sure <martif><text><body/></text></martif> exists
            if (tbx.Root.Element("text") == null) tbx.Root.Add(new XElement("text"));
            if (tbx.Root.Element("text").Element("body") == null) tbx.Root.Element("text").Add(new XElement("body"));
            tbx.Root.Element("text").Element("body").Descendants("termEntry").Remove();

            foreach (XElement termEntry in terms.Descendants("termEntry"))
            {
                // generate a new id
                string old = termEntry.Attribute("id").Value;
                string id = "taas-" + (tbxTermEntries.Count + 1) + "-" + old;

                // replace the old id in text
                foreach (XElement tename in text.Descendants().Where(e => e.Attribute("termid") != null && e.Attribute("termid").Value == old))
                    tename.Attribute("termid").Value = id;
                termEntry.Attribute("id").Remove();

                // save each term entry with the full tbx (header, footer, etc.)
                XDocument termEntryTbx = new XDocument(tbx);
                termEntryTbx.Root.Element("text").Element("body").Add(termEntry);

                tbxTermEntries[id] = termEntryTbx;
            }
        }
        #endregion

        #region Single terms
        private Dictionary<Tuple<string, string>, XElement> singleterms = new Dictionary<Tuple<string, string>, XElement>();

        /// <summary>
        /// All terms consisting of just one word.
        /// <example>
        /// &lt;Word, NN&gt; => &lt;tename lemma="NN"&gt;word&lt;/tename&gt;
        /// </example>
        /// </summary>
        public Dictionary<Tuple<string, string>, XElement> SingleTerms
        {
            get { return singleterms; }
        }

        /// <summary>
        /// Split compound terms into single terms.
        /// For instance, "word order" (NN NN) would be split into two entries with a single term each: "word" (NN) and "order" (NN).
        /// If there is a term with the "word" (NN) annotation, it's added to the list.
        /// </summary>
        /// <param name="text">Annotated text from TaaS.</param>
        private void ExtractSingleTerms(XElement text)
        {
            IEnumerable<XElement> tenames = text.Descendants("tename")
                                                .Where(e => e.Attribute("lemma") != null)
                                                .Where(e => e.Attribute("msd") != null);

            foreach (XElement tename in tenames)
            {
                string[] lemmas = tename.Attribute("lemma").Value.Split(' ');
                string[] poses = tename.Attribute("msd").Value.Split(' ');
                    
                if (lemmas.Length == poses.Length)
                {
                    for (int i = 0; i < lemmas.Length; i++)
                    {
                        var key = Tuple.Create(lemmas[i], poses[i]);
                        singleterms[key] = null;
                    }
                }
            }

            foreach (var key in singleterms.Keys.ToArray())
            {
                XElement candidate = tenames.Where(e => e.Attribute("lemma").Value == key.Item1)
                                            .Where(e => e.Attribute("msd").Value == key.Item2)
                                            .FirstOrDefault();

                if (candidate != null)
                {
                    singleterms[key] = new XElement(candidate);
                    singleterms[key].RemoveNodes();
                }
                else
                {
                    singleterms.Remove(key);
                }
            }
        }
        #endregion

        /// <summary>
        /// Makes an API call.
        /// </summary>
        /// <param name="url">URL with parameters.</param>
        /// <param name="text">Text to send.</param>
        /// <returns>Returned value.</returns>
        private async Task<string> MakeApiRequest(string url, string text)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(Username + ":" + Password)));
            httpClient.Timeout = Timeout;
            httpClient.DefaultRequestHeaders.Add("Accept", "application/xml");

            HttpContent data = new StringContent(JsonConvert.SerializeObject(text), Encoding.UTF8, "application/json");
            
            HttpResponseMessage response = await httpClient.PostAsync(url, data);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Converts annotated text to an XML element where term annotations are XML tags.
        /// </summary>
        /// <param name="text">Annotated text.</param>
        /// <returns>XML element.</returns>
        private static XElement ToXml(string text)
        {
            StringBuilder xml = new StringBuilder();

            string tenameTag = "tename";
            string startTag = "<" + tenameTag.ToLowerInvariant();
            string endTag = "</" + tenameTag.ToLowerInvariant() + ">";

            // keep <tename> tags but escape everything else
            for (int i = 0; i < text.Length; i++)
            {
                // tename tag is opened
                if (i + startTag.Length <= text.Length && text.Substring(i, startTag.Length).ToLowerInvariant() == startTag)
                {
                    int pos = text.IndexOf(">", i); // where opening tag ends
                    if (pos > -1)
                    {
                        string attributes = text.Substring(i + startTag.Length, pos - i - startTag.Length); // tag attributes after startTag and before >
                        //attributes = Regex.Replace(attributes, "&[a-zA-Z]+;", "fixme"); // hack todo fixme http://stackoverflow.com/questions/281682/reference-to-undeclared-entity-exception-while-working-with-xml
                        // invalid xml attribute values
                        attributes = attributes.Replace("&", "&amp;");
                        attributes = attributes.Replace("<", "&lt;");

                        xml.Append("<" + tenameTag);
                        xml.Append(!string.IsNullOrWhiteSpace(attributes) ? " " + attributes : "");
                        xml.Append(">");

                        i = pos + 1; // jump to next char after the opening tag
                    }
                }
                // tename tag is closed
                else if (i + endTag.Length <= text.Length && text.Substring(i, endTag.Length).ToLowerInvariant() == endTag)
                {
                    xml.Append(string.Format("</{0}>", tenameTag));
                    i += endTag.Length; // jump to the next char after the closing tag
                }

                // reached the end of the string
                if (i >= text.Length)
                    break;

                // escape and append the current character
                xml.Append(HttpUtility.HtmlEncode(text[i]));
            }

            XElement doc = XElement.Parse("<text>" + xml.ToString() + "</text>", LoadOptions.PreserveWhitespace);

            // change tename attributes to lower case
            foreach (XElement element in doc.Descendants())
                foreach (XAttribute attribute in element.Attributes().ToList())
                    attribute.ReplaceName(XName.Get(attribute.Name.LocalName.ToLowerInvariant(), attribute.Name.NamespaceName));

            return doc;
        }
    }
}