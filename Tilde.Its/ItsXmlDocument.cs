using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// ITS 2.0 annotated XML document.
    /// </summary>
    public class ItsXmlDocument : ItsDocument
    {
        /// <summary>
        /// Clones an existing document.
        /// </summary>
        /// <param name="document">The document that will be copied.</param>
        public ItsXmlDocument(ItsXmlDocument document)
            : base(document)
        {
        }

        /// <summary>
        /// Creates a new document from a file.
        /// </summary>
        /// <param name="uri">Filename.</param>
        public ItsXmlDocument(string uri)
        {
            Document = XDocument.Load(uri);
            LoadLocalRules(Document, uri);
        }

        /// <summary>
        /// Creates a new document from a string.
        /// </summary>
        /// <param name="xml">Document contents.</param>
        /// <param name="uri">The path of the document that is used if there are references to external rules with relative paths.</param>
        public ItsXmlDocument(string xml, string uri = null)
        {
            Document = XDocument.Parse(xml);
            LoadLocalRules(Document, uri);
        }

        /// <summary>
        /// Processes the XML document and loads linked global rules.
        /// The rules can be embedded in the document or linked to an external file.
        /// </summary>
        /// <param name="document">XML document.</param>
        /// <param name="uri">The path of the document that is used if there are references to external rules with relative paths.</param>
        private void LoadLocalRules(XDocument document, string uri)
        {
            foreach (XElement rules in document.Descendants(ItsNamespace + RulesElementName))
            {
                LoadRules(rules, uri);
            }
        }
    }
}
