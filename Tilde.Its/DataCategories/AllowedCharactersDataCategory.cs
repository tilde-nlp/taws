using System.Linq;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// The Allowed Characters data category is used to specify the characters that are permitted in a given piece of content.
    /// <see href="http://www.w3.org/TR/its20/#allowedchars"/>
    /// </summary>
    public class AllowedCharactersDataCategory : DataCategory<string>
    {
        /// <inheritdoc/>
        public AllowedCharactersDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>
        /// The set of characters that are allowed is specified using a regular expression. 
        /// </summary>
        public string AllowedCharacters
        {
            get { return Value; }
            set { Value = value; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleName
        {
            get { return "allowedCharactersRule"; }
        }

        /// <inheritdoc/>
        protected override XName LocalAttributeName
        {
            get { return XmlOrHtmlAttributeName("allowedCharacters"); }
        }

        /// <inheritdoc/>
        protected override string DefaultValue(XObject node)
        {
            return null;
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            return ElementOrAttribute(node, element => true, attribute => false);
        }

        /// <inheritdoc/>
        protected override bool GlobalValue(XObject node, XAttribute attribute, GlobalRule rule, out string value)
        {
            value = null;

            XAttribute valueAttr = rule.RuleElement.Attribute("allowedCharacters");
            XAttribute pointerAttr = rule.RuleElement.Attribute("allowedCharactersPointer");

            if (valueAttr != null)
            {
                value = valueAttr.Value;
            }
            else if (pointerAttr != null)
            {
                string pointerValue = rule.QueryLanguage.SelectPointerValues(node, pointerAttr.Value).FirstOrDefault();
                if (pointerValue != null)
                    value = pointerValue;
            }

            return value != null;
        }

        /// <inheritdoc/>
        protected override bool LocalValue(XElement element, XAttribute attribute, out string value)
        {
            value = attribute.Value;
            return true;
        }
    }
}