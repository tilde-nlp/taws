using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Tilde.Taws.Models
{
    /// <summary>
    /// <see cref="System.Xml.Linq"/> extension methods.
    /// </summary>
    public static class LinqToXmlExtensions
    {
        /// <summary>
        /// Sets the default namespace for an element and its children.
        /// </summary>
        /// <param name="element">Element to use.</param>
        /// <param name="xmlns">Default namespace.</param>
        public static void SetDefaultNamespace(this XElement element, XNamespace xmlns)
        {
            if (element.Name.NamespaceName == string.Empty)
                element.Name = xmlns + element.Name.LocalName;

            foreach (XElement child in element.Elements())
                child.SetDefaultNamespace(xmlns);
        }

        /// <summary>
        /// Removes the default namespace from an element and its children.
        /// </summary>
        /// <param name="element">Element to use.</param>
        public static void RemoveDefaultNamespace(this XElement element)
        {
            foreach (XElement e in element.DescendantsAndSelf())
            {
                if (e.Name.Namespace != XNamespace.None)
                    e.Name = XNamespace.None.GetName(e.Name.LocalName);

                if (e.Attributes().Where(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None).Any())
                    e.ReplaceAttributes(e.Attributes().Select(a => a.IsNamespaceDeclaration ? null : a.Name.Namespace != XNamespace.None ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value) : a));
            }
        }

        /// <summary>
        /// Replaces the name of an attribute.
        /// </summary>
        /// <param name="attribute">Attribute whose name to replace.</param>
        /// <param name="name">New attribute name.</param>
        public static void ReplaceName(this XAttribute attribute, XName name)
        {
            XAttribute newAttribute = new XAttribute(name, attribute.Value);

            List<XAttribute> attributes = attribute.Parent.Attributes().ToList();
            attributes.Insert(attributes.IndexOf(attribute), newAttribute);
            attributes.Remove(attribute);

            attribute.Parent.ReplaceAttributes(attributes);
        }

        /// <summary>
        /// Converts the element's contents to an XML string.
        /// </summary>
        /// <param name="element">Element to use.</param>
        /// <returns>Element's contents as an XML string.</returns>
        public static string ToInnerString(this XElement element)
        {
            XmlReader reader = element.CreateReader();
            reader.MoveToContent();
            return reader.ReadInnerXml();
        }

        /// <summary>
        /// Checks if an element is empty i.e. contains no empty nodes.
        /// </summary>
        /// <param name="element">Element to check.</param>
        /// <returns>Whether the element is empty.</returns>
        public static bool IsEmptyValue(this XElement element)
        {
            StringBuilder sb = new StringBuilder();

            foreach (XNode node in element.Nodes().Where(n => !(n is XElement)))
                sb.Append(node.ToString());

            return sb.ToString() == string.Empty;
        }
    }

    /// <summary>
    /// Represents a text node whose contents are not escaped.
    /// </summary>
    public class XRawText : System.Xml.Linq.XCData
    {
        string rawText;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="rawText">A string that contains the value.</param>
        public XRawText(string rawText)
            : base(rawText)
        {
            this.rawText = rawText;
        }

        /// <inheritdoc/>
        public override void WriteTo(System.Xml.XmlWriter writer)
        {
            writer.WriteRaw(rawText);
        }
    }
}