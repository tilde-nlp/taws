using System;
using System.Linq;
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
            return XmlOrHtmlDocument<bool>(
                xml: () => ElementOrAttribute(element => true, attribute => false),
                html: () => ElementOrAttribute(element => true, attribute => DefaultValueForHtml(attribute)));
        }

        /// <summary>
        /// Returns the default value for an HTML attribute.
        /// </summary>
        /// <param name="attribute">Attribute to check.</param>
        /// <returns>Default value.</returns>
        protected virtual bool DefaultValueForHtml(XAttribute attribute)
        {
            if (attribute == null || attribute.Name == null)
                return false;

            // http://www.whatwg.org/specs/web-apps/current-work/multipage/elements.html#the-translate-attribute
            if (DefaultValueHtmlForSpecificParents(attribute, "abbr", "th")) return true;
            if (DefaultValueHtmlForSpecificParents(attribute, "alt", "area", "img", "input")) return true;
            if (DefaultValueHtmlForSpecificParents(attribute, "content", "meta")) return true; // todo
            if (DefaultValueHtmlForSpecificParents(attribute, "download", "a", "area")) return true;
            if (DefaultValueHtmlForSpecificParents(attribute, "label", "menuitem", "menu", "optgroup", "option", "track")) return true;
            if (DefaultValueHtmlForSpecificParents(attribute, "placeholder", "input", "textarea")) return true;
            if (DefaultValueHtmlForSpecificParents(attribute, "srcdoc", "iframe")) return true;
            if (DefaultValueForHtmlElements(attribute, "lang", "style", "title")) return true;

            return false;
        }

        /// <summary>
        /// Checks if an attribute is translatable give a set of rules. 
        /// </summary>
        /// <param name="attribute">Attribute to check.</param>
        /// <param name="attributeName">Name of the attribute that is translatable by default.</param>
        /// <param name="parentElementNames">Allowed names of parents of the attribute.</param>
        /// <returns>Whether the attribute is translatable.</returns>
        protected virtual bool DefaultValueHtmlForSpecificParents(XAttribute attribute, string attributeName, params string[] parentElementNames)
        {
            if (attribute.Name == attributeName &&
                attribute.Parent != null &&
                attribute.Parent.Name != null &&
                attribute.Parent.Name.Namespace == ItsHtmlDocument.XhtmlNamespace &&
                parentElementNames.Contains(attribute.Parent.Name.LocalName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if an attribute is translatable if it has one of the specified names
        /// and its parent is an HTML element.
        /// </summary>
        /// <param name="attribute">Attribute to check.</param>
        /// <param name="attributeNames">Allowed attribute names.</param>
        /// <returns>Whether the attribute is translatable.</returns>
        protected virtual bool DefaultValueForHtmlElements(XAttribute attribute, params string[] attributeNames)
        {
            foreach (string name in attributeNames)
            {
                if (attribute.Name == name &&
                    attribute.Parent != null && attribute.Parent.Name != null &&
                    attribute.Parent.Name.Namespace == ItsHtmlDocument.XhtmlNamespace)
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            return ElementOrAttribute(node, element => true, attribute => false);
        }
    }
}