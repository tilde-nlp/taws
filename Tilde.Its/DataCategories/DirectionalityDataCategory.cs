using System.Collections.Generic;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// The Directionality data category allows the user to specify the base writing direction of blocks, embeddings and overrides for the Unicode bidirectional algorithm.
    /// <see href="http://www.w3.org/TR/its20/#directionality"/>
    /// </summary>
    public class DirectionalityDataCategory : SingleValueDataCategory<Directionality>
    {
        /// <summary>
        /// Possible directionality values.
        /// </summary>
        private static readonly Dictionary<string, Directionality> values = new Dictionary<string, Directionality>()
        {
            { "ltr", Directionality.LeftToRight },
            { "rtl", Directionality.RightToLeft },
            { "lro", Directionality.LeftToRightOverride },
            { "rlo", Directionality.RightToLeftOverride }
        };

        /// <inheritdoc/>
        public DirectionalityDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>
        /// The base writing direction of blocks.
        /// </summary>
        public Directionality Directionality
        {
            get { return Value; }
            set { Value = value; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleName
        {
            get { return "dirRule"; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleAttributeName
        {
            get { return "dir"; }
        }

        /// <inheritdoc/>
        protected override XName LocalAttributeName
        {
            get { return "dir"; }
        }

        /// <inheritdoc/>
        protected override bool IsValidValue(string value)
        {
            return values.ContainsKey(NormalizeValue(value));
        }

        /// <inheritdoc/>
        protected override Directionality ParseValue(string value)
        {
            return values[NormalizeValue(value)];
        }

        /// <inheritdoc/>
        protected override Directionality DefaultValue(XObject node)
        {
            return Directionality.LeftToRight;
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            return true;
        }
    }

    /// <summary>
    /// The base writing direction of blocks.
    /// </summary>
    public enum Directionality
    {
        /// <summary>
        /// Left-to-right text.
        /// </summary>
        LeftToRight,
        /// <summary>
        /// Right-to-left text.
        /// </summary>
        RightToLeft,
        /// <summary>
        /// Left-to-right override.
        /// </summary>
        LeftToRightOverride,
        /// <summary>
        /// Right-to-left override.
        /// </summary>
        RightToLeftOverride
    }
}
