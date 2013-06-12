using System;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// The Preserve Space data category indicates how whitespace should be handled in content.
    /// <see href="http://www.w3.org/TR/its20/#preservespace"/>
    /// <see href="http://www.w3.org/TR/2008/REC-xml-20081126/#sec-white-space"/>
    /// </summary>
    public class PreserveSpaceDataCategory : SingleValueDataCategory<PreserveSpace>
    {
        /// <inheritdoc/>
        public PreserveSpaceDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>
        /// How to handle whitespace in content.
        /// </summary>
        public PreserveSpace PreserveSpace
        {
            get { return Value; }
            set { Value = value; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleName
        {
            get { return "preserveSpaceRule"; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleAttributeName
        {
            get { return "space"; }
        }

        /// <inheritdoc/>
        protected override XName LocalAttributeName
        {
            get { return XNamespace.Xml + "space"; }
        }

        /// <inheritdoc/>
        protected override bool IsValidValue(string value)
        {
            PreserveSpace result;
            return Enum.TryParse<PreserveSpace>(NormalizeValue(value), true, out result);
        }

        /// <inheritdoc/>
        protected override PreserveSpace ParseValue(string value)
        {
            return (PreserveSpace)Enum.Parse(typeof(PreserveSpace), NormalizeValue(value), true);
        }

        /// <inheritdoc/>
        protected override PreserveSpace DefaultValue(XObject node)
        {
            return PreserveSpace.Default;
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            return true;
        }
    }

    /// <summary>
    /// How whitespace should be handled in content.
    /// </summary>
    public enum PreserveSpace
    {
        /// <summary>
        /// The value "default" signals that applications' default white-space processing modes are acceptable for this element.
        /// </summary>
        Default,
        /// <summary>
        /// The value "preserve" indicates the intent that applications preserve all the white space.
        /// </summary>
        Preserve
    }
}
