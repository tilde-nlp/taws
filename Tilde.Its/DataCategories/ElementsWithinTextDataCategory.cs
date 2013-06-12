using System;
using System.Linq;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// The Elements Within Text data category reveals if and how an element affects the way text content behaves from a linguistic viewpoint.
    /// <see href="http://www.w3.org/TR/its20/#elements-within-text"/>
    /// </summary>
    public class ElementsWithinTextDataCategory : SingleValueDataCategory<WithinText>
    {
        private static readonly string[] InlineElements = new[] { "abbr", "acronym", "br", "cite", "code", "dfn", "kbd", "q", "samp", "span", 
            "strong", "var", "b", "em", "big", "hr", "i", "small", "sub", "sup", "tt", "del", "ins", "bdo", "img", "a", "font", "center", "s", 
            "strike", "u", "isindex", "area", "audio", "bdi", "br", "button", "canvas", "command", "datalist", "embed", "iframe", "input", "keygen", 
            "label", "map", "mark", "math", "meter", "noscript", "object", "output", "progress", "ruby", "script", "select", "svg", "textarea", 
            "time", "video", "wbr" };

        private static readonly string[] NestedElements = new[] { "iframe", "noscript", "script", "textarea" };

        /// <inheritdoc/>
        public ElementsWithinTextDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>
        /// Text content behavior.
        /// </summary>
        public WithinText WithinText
        {
            get { return Value; }
            set { Value = value; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleName
        {
            get { return "withinTextRule"; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleAttributeName
        {
            get { return "withinText"; }
        }

        /// <inheritdoc/>
        protected override XName LocalAttributeName
        {
            get { return XmlOrHtmlAttributeName("withinText"); }
        }

        /// <inheritdoc/>
        protected override bool IsValidValue(string value)
        {
            WithinText result;
            return Enum.TryParse<WithinText>(NormalizeValue(value), true, out result);
        }

        /// <inheritdoc/>
        protected override WithinText ParseValue(string value)
        {
            return (WithinText)Enum.Parse(typeof(WithinText), NormalizeValue(value), true);
        }

        /// <inheritdoc/>
        protected override WithinText DefaultValue(XObject node)
        {
            return XmlOrHtmlDocument(xml: () => WithinText.No, html: () => DefaultValueHtml(node));
        }

        private WithinText DefaultValueHtml(XObject node)
        {
            return ElementOrAttribute(element => DefaultValueHtmlElement(element), attribute => WithinText.No);
        }

        private WithinText DefaultValueHtmlElement(XElement element)
        {
            if (NestedElements.Any(e => e == element.Name.LocalName))
                return Its.WithinText.Nested;
            if (InlineElements.Any(e => e == element.Name.LocalName))
                return WithinText.Yes;
            return WithinText.No;
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            return false;
        }
    }

    /// <summary>
    /// Reveals if and how an element affects the way text content behaves from a linguistic viewpoint.
    /// </summary>
    public enum WithinText
    {
        /// <summary>
        /// The element and its content are part of the flow of its parent element.
        /// </summary>
        Yes,
        /// <summary>
        /// The element splits the text flow of its parent element and its content is an independent text flow.
        /// </summary>
        No,
        /// <summary>
        /// The element is part of the flow of its parent element, its content is an independent flow.
        /// </summary>
        Nested
    }
}