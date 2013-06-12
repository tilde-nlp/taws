using System;
using System.Linq;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// Annotation in an ITS document.
    /// It can be used to select all ITS related annotations since all annotations inherit from this class.
    /// Contains various helper methods.
    /// </summary>
    public abstract class Annotation
    {
        /// <summary>ITS document the node belongs to.</summary>
        protected ItsDocument document;
        /// <summary>Node to annotate.</summary>
        protected XObject node;

        /// <summary>
        /// Creates a new annotation for a node in an ITS document.
        /// </summary>
        /// <param name="document">ITS document that contains the rules.</param>
        /// <param name="node">Element or attribute to annotate.</param>
        protected Annotation(ItsDocument document, XObject node)
        {
            if (document == null)
                throw new ArgumentNullException("document");
            if (node == null)
                throw new ArgumentNullException("node");
            if (!(node is XElement) && !(node is XAttribute))
                throw new ArgumentException("Unsupported node type. Only XElement and XAttribute are supported.", "node");

            this.document = document;
            this.node = node;
        }

        /// <summary>
        /// Returns the attribute on an element with the specified name.
        /// Sometimes the attribute name can be in the ITS namespace, sometimes it isn't.
        /// This method covers all situations for both XML and HTML documents.
        /// </summary>
        /// <param name="element">Element whose attribute to get.</param>
        /// <param name="attributeName">Attribute name.</param>
        /// <returns>Attribute object.</returns>
        protected XAttribute LocalAttribute(XElement element, XName attributeName)
        {
            // namespace is defined
            if (attributeName.Namespace != null && attributeName.Namespace != "")
                return element.Attribute(attributeName);

            return XmlOrHtmlDocument(
                xml: () =>
                {
                    // <its:span translate="yes">
                    if (element.Name == ItsDocument.ItsNamespace + "span")
                        return element.Attribute(attributeName.LocalName);
                    // <span its:translate="yes">
                    if (element.Name.Namespace != ItsDocument.ItsNamespace)
                        return element.Attribute(ItsDocument.ItsNamespace + attributeName.LocalName);
                    return null;
                },
                html: () =>
                {
                    return element.Attribute(attributeName);
                }
            );
        }

        /// <summary>
        /// Trims and lower cases the value.
        /// Useful for enumerable values.
        /// </summary>
        /// <param name="value">Value to normalize.</param>
        /// <returns>Normalized value.</returns>
        protected string NormalizeValue(string value)
        {
            if (value == null)
                return null;

            return value.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Parses a value containing either "yes" or "no" as a boolean value.
        /// </summary>
        /// <param name="value">Value to parse.</param>
        /// <returns><see langword="true"/> or <see langword="false"/>; <see langword="null"/> if the value was invalid.</returns>
        protected bool? ParseYesNoValue(string value)
        {
            if (value == null)
                return null;

            switch (NormalizeValue(value))
            {
                case "yes": return true;
                case "no": return false;
                default: return null;
            }
        }

        /// <summary>
        /// Returns a value based on the node type.
        /// </summary>
        /// <typeparam name="T">Type of the returned value.</typeparam>
        /// <param name="node">Node whose type to look at.</param>
        /// <param name="element">Function that returns the value when the node is an element.</param>
        /// <param name="attribute">Function that returns the value when the node is an attribute.</param>
        /// <returns>Return value of the used function.</returns>
        protected T ElementOrAttribute<T>(XObject node, Func<XElement, T> element, Func<XAttribute, T> attribute)
        {
            if (node is XElement)
                return element(node as XElement);
            if (node is XAttribute)
                return attribute(node as XAttribute);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns a value based on the current node type.
        /// </summary>
        /// <typeparam name="T">Type of the returned value.</typeparam>
        /// <param name="element">Function that returns the value when the node is an element.</param>
        /// <param name="attribute">Function that returns the value when the node is an attribute.</param>
        /// <returns>Return value of the used function.</returns>
        protected T ElementOrAttribute<T>(Func<XElement, T> element, Func<XAttribute, T> attribute)
        {
            return ElementOrAttribute(node, element, attribute);
        }

        /// <summary>
        /// Returns a value based on the document type.
        /// </summary>
        /// <typeparam name="T">Type of the returned value.</typeparam>
        /// <param name="xml">Function that returns the value when the document is an XML document.</param>
        /// <param name="html">Function that returns the value when the document is an HTML document.</param>
        /// <returns>Return value of the used function.</returns>
        protected T XmlOrHtmlDocument<T>(Func<T> xml, Func<T> html)
        {
            if (document is ItsXmlDocument)
                return xml();
            if (document is ItsHtmlDocument)
                return html();
            throw new NotSupportedException();
        }

        /// <summary>
        /// Depending on the document type, returns the appropriate attribute name.
        /// In HTML, the value is prefixed with "its-" and then transformed.
        /// </summary>
        /// <param name="name">Regular/XML attribute name.</param>
        /// <returns>XML or HTML attribute name.</returns>
        protected XName XmlOrHtmlAttributeName(string name)
        {
            return XmlOrHtmlDocument(
                xml: () => name,
                html: () => "its-" + string.Join("", name.Select(c => char.IsUpper(c) ? "-" + c.ToString().ToLowerInvariant() : c.ToString()))
            );
        }
    }
}
