using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// The Id Value data category indicates a value that can be used as unique identifier for a given part of the content.
    /// <see href="http://www.w3.org/TR/its20/#idvalue"/>
    /// </summary>
    public class IdValueDataCategory : SinglePointerDataCategory
    {
        /// <inheritdoc/>
        public IdValueDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>
        /// Unique identifier.
        /// </summary>
        public string ID
        {
            get { return Value; }
            set { Value = value; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleName
        {
            get { return "idValueRule"; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleAttributeName
        {
            get { return "idValue"; }
        }

        /// <inheritdoc/>
        protected override XName LocalAttributeName
        {
            get { return XmlOrHtmlDocument(xml: () => XNamespace.Xml + "id", html: () => "id"); }
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            return false;
        }
    }
}
