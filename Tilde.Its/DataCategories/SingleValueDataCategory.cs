using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// Data Category that has a single value.
    /// </summary>
    /// <typeparam name="T">Type that represents the value.</typeparam>
    public abstract class SingleValueDataCategory<T> : DataCategory<T>
    {
        /// <inheritdoc/>
        public SingleValueDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>
        /// Checks to see if the value for this data category is valid.
        /// </summary>
        /// <param name="value">Value to test.</param>
        /// <returns>True if the value is valid; otherwise false.</returns>
        protected abstract bool IsValidValue(string value);

        /// <summary>
        /// Values are usually specified with an attribute.
        /// This method parses the value of the attribute and returns another representation of this value.
        /// </summary>
        /// <param name="value">Attribute value.</param>
        /// <returns>Parsed value.</returns>
        protected abstract T ParseValue(string value);

        /// <inheritdoc/>
        protected override bool LocalValue(XElement element, XAttribute attribute, out T value)
        {
            if (IsValidValue(attribute.Value))
            {
                value = ParseValue(attribute.Value);
                return true;
            }

            value = default(T);
            return false;
        }

        /// <inheritdoc/>
        protected override bool GlobalValue(XObject node, XAttribute attribute, GlobalRule rule, out T value)
        {
            if (IsValidValue(attribute.Value))
            {
                value = ParseValue(attribute.Value);
                return true;
            }

            value = default(T);
            return false;
        }
    }
}
