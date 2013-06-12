using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// The External Resource data category indicates that a node represents or references potentially translatable data in a resource outside the document.
    /// <see href="http://www.w3.org/TR/its20/#externalresource"/>
    /// </summary>
    public class ExternalResourceDataCategory : SinglePointerDataCategory
    {
        /// <inheritdoc/>
        public ExternalResourceDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        
        /// <summary>
        /// The IRI of the external resource.
        /// </summary>
        public string ExternalResourceIri
        {
            get { return Value; }
            set { Value = value; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleName
        {
            get { return "externalResourceRefRule"; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleAttributeName
        {
            get { return "externalResourceRefPointer"; }
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            return false;
        }
    }
}
