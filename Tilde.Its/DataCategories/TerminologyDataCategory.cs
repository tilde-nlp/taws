using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// The Terminology data category is used to mark terms and optionally associate them with information, such as definitions.
    /// <see href="http://www.w3.org/TR/its20/#terminology"/>
    /// </summary>
    public class TerminologyDataCategory : DataCategory<Term>
    {
        /// <summary>
        /// Culture to use to parse the confidence value.
        /// </summary>
        static readonly CultureInfo culture = CultureInfo.InvariantCulture;

        /// <inheritdoc/>
        public TerminologyDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>
        /// If the content of the node is a term.
        /// </summary>
        public bool IsTerm
        {
            get { return Value != null ? Value.IsTerm : false; }
        }

        /// <summary>
        /// Whether the node is annotated.
        /// </summary>
        public bool IsAnnotated
        {
            get { return Value != null; }
        }

        /// <summary>
        /// Information about the term.
        /// </summary>
        public Term Term
        {
            get { return Value; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleName
        {
            get { return "termRule"; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleAttributeName
        {
            get { return "term"; }
        }

        /// <inheritdoc/>
        protected override XName LocalAttributeName
        {
            get { return XmlOrHtmlAttributeName("term"); }
        }

        /// <inheritdoc/>
        /// <param name="termAttr">Global rule attribute that holds the value.</param>
        /// <param name="term">Returned value. Undefined if the method returned <see langword="false"/>.</param>
        protected override bool GlobalValue(XObject node, XAttribute termAttr, GlobalRule rule, out Term term)
        {
            XElement element = ElementOrAttribute(e => e, a => a.Parent);

            term = new Term();
            term.IsTerm = ParseYesNoValue(termAttr.Value) ?? false;

            // None or exactly one of the following:
            XAttribute termInfoPointerAttr = rule.RuleElement.Attribute("termInfoPointer");
            XAttribute termInfoRefAttr = rule.RuleElement.Attribute("termInfoRef");
            XAttribute termInfoRefPointerAttr = rule.RuleElement.Attribute("termInfoRefPointer");

            if (termInfoPointerAttr != null)
                term.Info = rule.QueryLanguage.SelectPointerValues(element, termInfoPointerAttr.Value).FirstOrDefault();
            else if (termInfoRefAttr != null)
                term.InfoRef = termInfoRefAttr.Value;
            else if (termInfoRefPointerAttr != null)
                term.InfoRef = rule.QueryLanguage.SelectPointerValues(element, termInfoRefPointerAttr.Value).FirstOrDefault();

            return true;
        }

        /// <inheritdoc/>
        /// <param name="term">Returned value. Undefined if the method returned <see langword="false"/>.</param>
        protected override bool LocalValue(XElement element, XAttribute attribute, out Term term)
        {
            term = new Term();
            term.IsTerm = ParseYesNoValue(attribute.Value) ?? false;

            // An optional termInfoRef attribute that contains an IRI referring to the resource providing information about the term.
            XAttribute infoRefAttr = LocalAttribute(element, XmlOrHtmlAttributeName("termInfoRef"));
            if (infoRefAttr != null)
                term.InfoRef = infoRefAttr.Value;
            // An optional termConfidence attribute with the value of a rational number in the interval 0 to 1 (inclusive).
            XAttribute confidenceAttr = LocalAttribute(element, XmlOrHtmlAttributeName("termConfidence"));
            if (confidenceAttr != null)
            {
                double result;
                if (double.TryParse(confidenceAttr.Value, NumberStyles.Float, culture, out result))
                    term.Confidence = result;
            }

            return true;
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            return false;
        }

        /// <inheritdoc/>
        protected override Term DefaultValue(XObject node)
        {
            return null;
        }
    }

    /// <summary>
    /// Information about a term.
    /// </summary>
    public class Term
    {
        /// <summary>
        /// Whether the content is a term.
        /// </summary>
        public bool IsTerm { get; set; }
        /// <summary>
        /// Represents the confidence of the agents producing the annotation that the values of the term.
        /// </summary>
        public double? Confidence { get; set; }
        /// <summary>
        /// Holds the terminology information.
        /// </summary>
        public string Info { get; set; }
        /// <summary>
        /// IRI referring to the resource providing information about the term.
        /// </summary>
        public string InfoRef { get; set; }
    }
}