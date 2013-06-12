using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// Parent data category. All data categories inherit from this class.
    /// </summary>
    public abstract class DataCategory : Annotation
    {
        /// <inheritdoc/>
        protected DataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }
    }

    /// <summary>
    /// Data Categories that contain one value.
    /// Value can be a complex type.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    public abstract class DataCategory<T> : DataCategory
    {
        /// <inheritdoc/>
        protected DataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>Cached value.</summary>
        T overridenValue;
        /// <summary>Whether <see cref="overridenValue"/> has been changed.</summary>
        bool isOverridenValue = false;

        /// <summary>
        /// Value for this Data Category.
        /// This value is loaded lazily.
        /// It is cached, once it's been loaded.
        /// It can be manually overriden, in which case you should reannotate all descendants.
        /// </summary>
        protected T Value
        {
            get
            {
                // we have already determined the value
                // or it's been overriden by the user
                if (isOverridenValue)
                    return overridenValue;

                // determine & cache the value
                return Value = ElementOrAttribute(ValueForElement, ValueForAttribute);
            }
            set
            {
                overridenValue = value;
                isOverridenValue = true;
            }
        }

        /// <summary>
        /// Global rule element name.
        /// <see langword="null"/> if the global rules are not supported.
        /// </summary>
        /// <returns>Name without the namespace.</returns>
        protected virtual string GlobalRuleName
        {
            get { return null; }
        }

        /// <summary>
        /// Global rule element attribute name that points to the value.
        /// <see langword="null"/> if the global rules are not supported.
        /// </summary>
        /// <returns>Name without the namespace.</returns>
        protected virtual string GlobalRuleAttributeName
        {
            get { return null; }
        }

        /// <summary>
        /// Name of the local attribute.
        /// <see langword="null"/> if the local markup is not supported.
        /// </summary>
        /// <returns>Name with or without the namespace.</returns>
        protected virtual XName LocalAttributeName
        {
            get { return null; }
        }

        /// <summary>
        /// Whether this data category supports inheritance for an element or an attribute.
        /// </summary>
        /// <param name="node">XElement or XAttribute.</param>
        /// <returns><see langword="true"/> if there is inheritence; otherwise <see langword="false"/>.</returns>
        protected abstract bool Inheritance(XObject node);

        /// <summary>
        /// The default value for this data category.
        /// </summary>
        /// <param name="node">XElement or XAttribute.</param>
        /// <returns>Default value.</returns>
        protected abstract T DefaultValue(XObject node);

        /// <summary>
        /// Gets the value for an element.
        /// </summary>
        /// <param name="element">Element whose value to get.</param>
        /// <returns>Value for the element.</returns>
        protected virtual T ValueForElement(XElement element)
        {
            if (LocalAttributeName != null)
            {
                // The data category can be expressed locally on an individual element.
                XAttribute attr = LocalAttribute(element, LocalAttributeName);
                if (attr != null && element.Name != ItsDocument.ItsNamespace + GlobalRuleName)
                {
                    T localValue;
                    if (LocalValue(element, attr, out localValue))
                        return localValue;
                }
            }

            if (GlobalRuleName != null)
            {
                // The data category can be expressed with global rules.
                T globalValue;
                if (GlobalValue(element, out globalValue))
                    return globalValue;
            }

            if (Inheritance(element))
            {
                // For elements, the data category information inherits to the textual content of the element, including child elements.
                if (element.Parent != null)
                {
                    return InheritedValue(element.Parent);
                }
            }

            // The default.
            return DefaultValue(element);
        }

        /// <summary>
        /// Gets the value for an attribute.
        /// </summary>
        /// <param name="attribute">Attribute whose value to get.</param>
        /// <returns>Value for the attribute.</returns>
        protected virtual T ValueForAttribute(XAttribute attribute)
        {
            // It is not possible to override the data category settings of attributes using local markup.

            // The data category can be expressed with global rules.
            if (GlobalRuleName != null)
            {
                T globalValue;
                if (GlobalValue(attribute, out globalValue))
                    return globalValue;
            }

            if (Inheritance(attribute))
            {
                if (attribute.Parent != null)
                {
                    return InheritedValue(attribute.Parent);
                }
            }

            // The default.
            return DefaultValue(attribute);
        }

        /// <summary>
        /// Finds the first matching global rule and returns the value for an element or attribute. 
        /// If there is no global rule, then the value is undefined.
        /// </summary>
        /// <param name="node">Element or attribute whose value to get.</param>
        /// <param name="value">Value for the element or attribute.</param>
        /// <returns><see langword="true"/> if there was a global rule for this element or attribute; <see langword="false"/> otherwise.</returns>
        protected virtual bool GlobalValue(XObject node, out T value)
        {
            foreach (GlobalRule rule in GlobalRules(GlobalRuleName, node))
            {
                XAttribute valueAttr = rule.RuleElement.Attribute(GlobalRuleAttributeName);
                if (valueAttr != null || (GlobalRuleName != null && GlobalRuleAttributeName == null))
                {
                    if (GlobalValue(node, valueAttr, rule, out value))
                        return true;
                }
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// Gets the inherited value.
        /// </summary>
        /// <param name="element">Element value to get.</param>
        /// <returns>Inherited value.</returns>
        protected T InheritedValue(XElement element)
        {
            // use annotation if there is one
            DataCategory<T> annotation = element.Annotation(this.GetType()) as DataCategory<T>;
            if (annotation != null)
                return annotation.Value;

            return ValueForElement(element);
        }

        /// <summary>
        /// Gets the value specified by local markup.
        /// </summary>
        /// <param name="element">Element that's been annotated.</param>
        /// <param name="attribute">Attribute that holds the value.</param>
        /// <param name="value">Returned value. Undefined if the method returned <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if it was possible to get the value; otherwise <see langword="false"/>.</returns>
        protected abstract bool LocalValue(XElement element, XAttribute attribute, out T value);

        /// <summary>
        /// Gets the value specified by a global rule.
        /// </summary>
        /// <param name="node">Node that's been annotated.</param>
        /// <param name="attribute">Global rule attribute that holds the value.</param>
        /// <param name="rule">Matching global rule for this element.</param>
        /// <param name="value">Returned value. Undefined if the method returned <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if it was possible to get the value; otherwise <see langword="false"/>.</returns>
        protected abstract bool GlobalValue(XObject node, XAttribute attribute, GlobalRule rule, out T value);

        #region Global Rules
        /// <summary>
        /// Finds all matching global rules for a node.
        /// </summary>
        /// <typeparam name="TNodeType">Type of the node.</typeparam>
        /// <param name="name">Name of the global rule.</param>
        /// <param name="node">Node for which to find global rules.</param>
        /// <returns>List of matching rules.</returns>
        protected IEnumerable<GlobalRule> GlobalRules<TNodeType>(string name, TNodeType node)
        {
            foreach (XElement rules in document.Rules.Reverse())
            {
                foreach (XElement ruleElement in rules.Descendants(ItsHtmlDocument.ItsNamespace + name).Reverse())
                {
                    GlobalRule rule = new GlobalRule();
                    rule.Rules = rules;
                    rule.RuleElement = ruleElement;
                    rule.Selector = ruleElement.Attribute("selector").Value;
                    rule.QueryLanguage = QueryLanguage(rules, ruleElement);

                    if (rule.QueryLanguage == null)
                        continue;

                    if (rule.QueryLanguage.SelectNodes<TNodeType>(document.Document.Root, rule.Selector).Contains(node))
                        yield return rule;
                }
            }
        }

        private IQueryLanguage QueryLanguage(XElement rules, XElement rule)
        {
            XAttribute queryLangAttr = rules.Attribute("queryLanguage");
            if (queryLangAttr != null && queryLangAttr.Value.Trim() != "xpath")
                return null;

            return new CachedQueryLanguage(new XPathQueryLanguage(rules, rule));
        }

        /// <summary>
        /// Global rule.
        /// </summary>
        protected class GlobalRule
        {
            /// <summary>Rules document.</summary>
            public XElement Rules { get; set; }
            /// <summary>Rule element.</summary>
            public XElement RuleElement { get; set; }
            /// <summary>Selector on the rule element.</summary>
            public string Selector { get; set; }
            /// <summary>Selection query language engine for this rules document.</summary>
            public IQueryLanguage QueryLanguage { get; set; }
        }
        #endregion
    }
}