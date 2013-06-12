using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using HtmlParserSharp;

namespace Tilde.Its
{
    /// <summary>
    /// ITS 2.0 annotated HTML document.
    /// </summary>
    public class ItsHtmlDocument : ItsDocument
    {
        /// <summary>
        /// XHTML namespace in which also all HTML5 elements reside in.
        /// <see href="http://www.w3.org/TR/2011/WD-html5-20110113/namespaces.html"/>
        /// </summary>
        public static readonly XNamespace XhtmlNamespace = XNamespace.Get("http://www.w3.org/1999/xhtml");

        private const string ScriptElementName = "script";
        private const string ScriptElementTypeAttributeName = "type";
        private const string LinkElementName = "link";
        private const string LinkElementRelAttributeName = "rel";
        private const string LinkElementRelAttributeValue = "its-rules";
        private const string LinkElementHrefAttributeName = "href";

        /// <summary>
        /// Clones an existing document.
        /// </summary>
        /// <param name="document">The document that will be copied.</param>
        public ItsHtmlDocument(ItsHtmlDocument document)
            : base(document)
        {
        }

        /// <summary>
        /// Creates a new document from a file.
        /// </summary>
        /// <param name="uri">Filename.</param>
        public ItsHtmlDocument(string uri)
            : base()
        {
            Document = ParseHtmlAsXml(File.ReadAllText(uri));
            LoadRulesFromHtml(Document, uri);
        }

        /// <summary>
        /// Creates a new document from a string.
        /// </summary>
        /// <param name="html">Document contents.</param>
        /// <param name="uri">The path of the document that is used if there are references to external rules with relative paths.</param>
        public ItsHtmlDocument(string html, string uri = null)
            : base()
        {
            Document = ParseHtmlAsXml(html);
            LoadRulesFromHtml(Document, uri);
        }

        /// <summary>
        /// Processes the HTML document and loads linked global rules.
        /// The rules can be included in the document or linked to an external file.
        /// </summary>
        /// <param name="document">HTML document.</param>
        /// <param name="uri">The path of the document that is used if there are references to external rules with relative paths.</param>
        private void LoadRulesFromHtml(XDocument document, string uri)
        {
            // process global inline and external rules in document order

            foreach (XElement element in document.Descendants())
            {
                // global inline rules
                if (element.Name == XhtmlNamespace + ScriptElementName &&
                    element.Attribute(ScriptElementTypeAttributeName) != null &&
                    element.Attribute(ScriptElementTypeAttributeName).Value.Trim().ToLowerInvariant() == ItsMimeType)
                {
                    try
                    {
                        string contents = (string)element;
                        XElement rules = XElement.Parse(contents);
                        LoadRules(rules, uri);
                    }
                    catch (Exception)
                    {
                        // ignore rules with parse errors
                    }
                }

                // global external rules
                if (element.Name == XhtmlNamespace + LinkElementName &&
                    element.Attribute(LinkElementRelAttributeName) != null &&
                    element.Attribute(LinkElementRelAttributeName).Value.Trim().ToLowerInvariant() == LinkElementRelAttributeValue)
                {
                    LoadExternalRules(ResolveUri(uri, element.Attribute(LinkElementHrefAttributeName).Value));
                }
            }
        }

        /// <summary>
        /// Parses a string with HTML as an XML document.
        /// </summary>
        /// <param name="html">A string containing HTML.</param>
        /// <returns>Parsed HTML as XML.</returns>
        private XDocument ParseHtmlAsXml(string html)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                SimpleHtmlParser parser = new SimpleHtmlParser();
                XmlDocument parsedHtml = parser.ParseString(html);
                parsedHtml.Save(stream);
                stream.Position = 0;
                return XDocument.Load(stream, LoadOptions.PreserveWhitespace);
            }
        }
     }
}