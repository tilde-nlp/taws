using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// The Target Pointer data category is used to associate the node of a given source content and the node of its corresponding target content.
    /// <see href="http://www.w3.org/TR/its20/#target-pointer"/>
    /// </summary>
    public class TargetPointerDataCategory : DataCategory<string>
    {
        /// <inheritdoc/>
        public TargetPointerDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>
        /// The value of the node for the target content corresponding to the selected source node.
        /// </summary>
        public string Target
        {
            get { return Value; }
            set { Value = value; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleName
        {
            get { return "targetPointerRule"; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleAttributeName
        {
            get { return "targetPointer"; }
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            return false;
        }

        /// <inheritdoc/>
        protected override string DefaultValue(XObject node)
        {
            return null;
        }

        /// <inheritdoc/>
        protected override bool LocalValue(XElement element, XAttribute attribute, out string value)
        {
            value = attribute.Value;
            return true;
        }

        /// <inheritdoc/>
        protected override bool GlobalValue(XObject node, XAttribute attribute, GlobalRule rule, out string value)
        {
            if (attribute.Value != null)
            {
                value = ReplaceParams(attribute.Value, rule);
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Replaces parameters in the pointer value.
        /// The value is enclosed in single quotes.
        /// </summary>
        /// <param name="value">Target pointer.</param>
        /// <param name="rule">Rules element that contains all parameters.</param>
        /// <returns>Target pointer without parameters.</returns>
        private string ReplaceParams(string value, GlobalRule rule)
        {
            foreach (XElement param in rule.Rules.Descendants(ItsDocument.ItsNamespace + "param"))
            {
                XAttribute name = param.Attribute("name");
                if (name != null)
                    value = value.Replace("$" + name.Value, "'" + param.Value + "'");
            }

            return value;
        }
    }
}