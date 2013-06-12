using System.Linq;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// Data Category that has a single pointer to a value.
    /// </summary>
    public abstract class SinglePointerDataCategory : DataCategory<string>
    {
        /// <inheritdoc/>
        public SinglePointerDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <inheritdoc/>
        protected override string DefaultValue(XObject node)
        {
            return null;
        }

        /// <summary>
        /// Finds the first matching global rule and returns the value of the node that the pointer points to.
        /// If there is no global rule, then the value is undefined.
        /// </summary>
        /// <param name="node">Element or attribute whose value to get.</param>
        /// <param name="pointerAttr">Attribute that represents the pointer.</param>
        /// <param name="rule">Matching global rule element.</param>
        /// <param name="value">Value of the pointer.</param>
        /// <returns><see langword="true"/> if there was a global rule for this element or attribute; <see langword="false"/> otherwise.</returns>
        protected override bool GlobalValue(XObject node, XAttribute pointerAttr, GlobalRule rule, out string value)
        {
            string pointerValue = rule.QueryLanguage.SelectPointerValues(node, pointerAttr.Value).FirstOrDefault();

            if (pointerValue != null)
            {
                value = pointerValue;
                return true;
            }

            value = null;
            return false;
        }

        /// <inheritdoc/>
        protected override bool LocalValue(XElement element, XAttribute attribute, out string value)
        {
            value = attribute.Value;
            return true;
        }
    }
}
