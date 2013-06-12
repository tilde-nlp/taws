using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// The Translate data category expresses information about whether the content of an element or attribute should be translated or not. 
    /// <see href="http://www.w3.org/TR/its20/#trans-datacat"/>
    /// </summary>
    public class TranslateDataCategory : SingleValueDataCategory<bool>
    {
        /// <inheritdoc/>
        public TranslateDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>
        /// Whether the content is translatable.
        /// </summary>
        public bool IsTranslatable
        {
            get { return Value; }
            set { Value = value; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleName
        {
            get { return "translateRule"; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleAttributeName
        {
            get { return "translate"; }
        }

        /// <inheritdoc/>
        protected override XName LocalAttributeName
        {
            get { return "translate"; }
        }

        /// <inheritdoc/>
        protected override bool IsValidValue(string value)
        {
            return ParseYesNoValue(value) != null;
        }

        /// <inheritdoc/>
        protected override bool ParseValue(string value)
        {
            return ParseYesNoValue(value).Value;
        }

        /// <inheritdoc/>
        protected override bool DefaultValue(XObject node)
        {
            return ElementOrAttribute(element => true, attribute => false);
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            return ElementOrAttribute(node, element => true, attribute => false);
        }
    }
}