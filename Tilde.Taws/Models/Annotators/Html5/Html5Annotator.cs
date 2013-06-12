using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

using Tilde.Its;

namespace Tilde.Taws.Models
{
    /// <summary>
    /// Finds terms in an HTML5 document and annotates them.
    /// </summary>
    public class Html5Annotator : Annotator<ItsHtmlDocument>
    {
        /// <summary>
        /// Creates a new instance of the annotator.
        /// </summary>
        /// <param name="source">HTML5 doucument to annotate.</param>
        public Html5Annotator(ApiDocument source)
            : base(source)
        {
        }

        /// <summary>
        /// Finds and annotates terms in the document.
        /// </summary>
        /// <returns>HTML5 document with annotated terms.</returns>
        public async Task<string> Annotate()
        {
            try
            {
                XElement body = Root.Element(ItsHtmlDocument.XhtmlNamespace + "body");

                AddDefaultValues(body);

                Html5Chunker chunker = new Html5Chunker(ItsDocument);
                Chunker.ChunkCollection chunks = chunker.Split(body);
                await Annotate(chunks);
                if (taas != null)
                    chunker.SingleTerms = taas.SingleTerms;
                chunker.Join(chunks);

                MergeTenames();
                RemoveInvalidTenames();

                if (!ContainsOtherTerms(body) && !ContainsOtherTerminologyAnnotatorsRefs(body))
                    AddAnnotatorsRef(body);
                AddTermInfoRefs();
                ReplaceTenameTags();

                return ToHtml5(Document);
            }
            catch (Exception e)
            {
                throw new AnnotatorException(e);
            }
        }

        /// <summary>
        /// Replaces term tags from TaaS (also known as tename's) with the appropriate XLIFF tags.
        /// </summary>
        private void ReplaceTenameTags()
        {
            foreach (Tename tename in Document.Descendants().Where(IsTename))
            {
                // change the tag name from <tename> to <span>
                tename.Name = ItsHtmlDocument.XhtmlNamespace + "span";

                tename.Add(new XAttribute("its-term", "yes"));

                if (tename.HasConfidence)
                    tename.Add(new XAttribute("its-term-confidence", tename.Confidence));

                TenameLocalAnnotatorsRef(tename);
            }
        }

        /// <summary>
        /// Adds TBXs of the terms in script elements to the head element.
        /// </summary>
        private void AddTermInfoRefs()
        {
            XElement head = Root.Element(ItsHtmlDocument.XhtmlNamespace + "head");

            AddTermInfoRefs(
                container:  (tename) => head, 
                tbxElement: (tename, id, tbx) => 
                    new XElement(ItsHtmlDocument.XhtmlNamespace + "script",
                        new XAttribute("id", id),
                        new XAttribute("type", "text/xml"), Environment.NewLine + ToXmlWithoutInternalSubset(tbx)));
        }

        /// <summary>
        /// Converts an XML document to HTML5.
        /// </summary>
        /// <param name="document">XML document to convert.</param>
        /// <returns>HTML5 document.</returns>
        private string ToHtml5(XDocument document)
        {
            // fix <script/>, <textarea/>, etc.
            foreach (XElement element in document.Descendants().Where(e => e.Name.Namespace == ItsHtmlDocument.XhtmlNamespace && !Html5Chunker.VoidElements.Contains(e.Name.LocalName)))
            {
                if (element.IsEmpty)
                {
                    element.Add("");
                }
            }

            // <script> and <style> are raw elements but during HTML parsing their contents were encoded as text (with escaped HTML entities)
            foreach (XElement element in document.Descendants().Where(e => e.Name.Namespace == ItsHtmlDocument.XhtmlNamespace && Html5Chunker.RawTextElements.Contains(e.Name.LocalName)))
            {
                foreach (XText text in element.Nodes().Where(n => n.NodeType == System.Xml.XmlNodeType.Text))
                {
                    text.ReplaceWith(new XRawText(HttpUtility.HtmlDecode(text.Value)));
                }
            }

            // remove empty head
            XElement head = document.Root.Element(ItsHtmlDocument.XhtmlNamespace + "head");
            if (head != null && !head.HasAttributes && !head.HasElements && head.IsEmptyValue())
            {
                if (head.PreviousNode is XText && string.IsNullOrWhiteSpace(head.PreviousNode.ToString()))
                    head.PreviousNode.Remove();
                if (head.NextNode is XText && string.IsNullOrWhiteSpace(head.NextNode.ToString()))
                    head.NextNode.Remove();

                head.Remove();
            }

            // remove default namespace for non-XHTML documents
            if (document.DocumentType == null || document.DocumentType.SystemId == null || !document.DocumentType.SystemId.StartsWith("http://www.w3.org/TR/xhtml"))
                document.Root.RemoveDefaultNamespace();

            // fix ugly doctype
            if (document.DocumentType != null)
                document.DocumentType.InternalSubset = null;

            string html = document.ToString();

            string html5spacedoctype = "<!DOCTYPE html >".ToLower();
            if (html.Substring(0, html5spacedoctype.Length).ToLower() == html5spacedoctype)
                html = html.Substring(0, html5spacedoctype.Length - 2) + ">" + html.Substring(html5spacedoctype.Length);

            // uncomment to remove the trailing slash e.g. <link /> => <link>
            // html = Regex.Replace(html, @"<(.*?)(\s*)/>", @"<$1>");

            return html;
        }

        /// <inheritdoc/>
        protected override XName AnnotatorsRefAttributeName
        {
            get { return "its-annotators-ref"; }
        }

        /// <inheritdoc/>
        protected override XName TermInfoRefAttributeName
        {
            get { return "its-term-info-ref"; }
        }

        private string ToXmlWithoutInternalSubset(XDocument doc)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var xmlWriter = new NullSubsetXmlTextWriter(stream, Encoding.UTF8);
                xmlWriter.Formatting = System.Xml.Formatting.Indented;
                doc.Save(xmlWriter);
                xmlWriter.Flush();
                xmlWriter.Close();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private class NullSubsetXmlTextWriter : System.Xml.XmlTextWriter
        {
            public NullSubsetXmlTextWriter(TextWriter writer)
                : base(writer)
            {
            }

            public NullSubsetXmlTextWriter(Stream stream, Encoding encoding)
                : base(stream, encoding)
            {
            }

            /// <inheritdoc/>
            public override void WriteDocType(string name, string pubid, string sysid, string subset)
            {
                if (subset == String.Empty)
                {
                    subset = null;
                }

                base.WriteDocType(name, pubid, sysid, subset);
            }
        }
    }
}