using System.Globalization;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// The MT Confidence data category is used to communicate the self-reported confidence score from a machine translation engine of the accuracy of a translation it has provided.
    /// <see href="http://www.w3.org/TR/its20/#mtconfidence"/>
    /// </summary>
    public class MtConfidenceDataCategory : SingleValueDataCategory<double?>
    {
        /// <summary>
        /// Culture to use to parse the confidence value.
        /// </summary>
        static readonly CultureInfo cultureInfo = CultureInfo.InvariantCulture;

        /// <inheritdoc/>
        public MtConfidenceDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>
        /// Value that represents the translation confidence score as a rational number in the interval 0 to 1 (inclusive).
        /// </summary>
        public double Confidence
        {
            get { return Value.Value; }
            set { Value = value; }
        }

        /// <summary>
        /// Whether the node was annotated with this data category.
        /// </summary>
        public bool IsAnnotated
        {
            get { return Value.HasValue; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleName
        {
            get { return "mtConfidenceRule"; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleAttributeName
        {
            get { return "mtConfidence"; }
        }

        /// <inheritdoc/>
        protected override XName LocalAttributeName
        {
            get { return XmlOrHtmlAttributeName("mtConfidence"); }
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            return ElementOrAttribute(node, element => true, attribute => false);
        }

        /// <inheritdoc/>
        protected override bool IsValidValue(string value)
        {
            double result;
            if (double.TryParse(NormalizeValue(value), NumberStyles.Float, cultureInfo, out result))
                return result >= 0 && result <= 1;
            return false;
        }

        /// <inheritdoc/>
        protected override double? ParseValue(string value)
        {
            return double.Parse(NormalizeValue(value), NumberStyles.Float, cultureInfo);
        }

        /// <inheritdoc/>
        protected override double? DefaultValue(XObject node)
        {
            return null;
        }
    }
}
