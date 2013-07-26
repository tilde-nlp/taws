using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Tilde.Its
{
    /// <summary>
    /// Implementation of XPath 1.0 query language.
    /// <see href="http://www.w3.org/TR/its20/#d0e2063"/>
    /// </summary>
    public class XPathQueryLanguage : IQueryLanguage
    {
        XElement rules;
        XElement ruleElement;

        /// <summary>
        /// Creates a new instance for a particular global rule element.
        /// </summary>
        /// <param name="rules">Global rules that contain the rule.</param>
        /// <param name="ruleElement">Global rule.</param>
        public XPathQueryLanguage(XElement rules, XElement ruleElement)
        {
            this.rules = rules;
            this.ruleElement = ruleElement;
        }

        /// <inheritdoc/>
        public IEnumerable<TNodeType> SelectNodes<TNodeType>(XElement root, string selector)
        {
            if (root == null)
                yield break;
            if (selector == null)
                yield break;

            CustomXsltContext context = new CustomXsltContext();
            AddNamespaces(context, rules);
            AddNamespaces(context, ruleElement);
            AddNamespaces(context, root);
            AddParameters(context);

            // id() workaround
            selector = ReplaceIdFunction(root, selector);

            XPathNavigator navigator = root.CreateNavigator();
            XPathExpression expression = navigator.Compile(selector);
            expression.SetContext(context);
            XPathNodeIterator iterator = navigator.Select(expression);

            foreach (XPathNavigator nav in iterator)
            {
                if (typeof(TNodeType).IsAssignableFrom(nav.UnderlyingObject.GetType()))
                {
                    yield return (TNodeType)nav.UnderlyingObject;
                }

                // in .NET * does not seem to include attributes
                // this is a hackish workaround
                if (expression.Expression.EndsWith("*") &&
                    nav.UnderlyingObject.GetType() == typeof(XElement) && 
                    (typeof(TNodeType) == typeof(XAttribute) || typeof(TNodeType) == typeof(XObject)))
                {
                    foreach (TNodeType attr in (nav.UnderlyingObject as XElement).Attributes().Cast<TNodeType>())
                    {
                        yield return attr;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> SelectPointerValues(XObject node, string selector)
        {
            // SelectNodes() can only use an XElement as the starting point
            // but if the node is an XAttribute and the selector points to itself (.)
            // then the value is returned here
            // otherwise use XAttribute's parent which is an XElement as the parameter to SelectNodes()
            XAttribute attribute = node as XAttribute;
            if (attribute != null)
            {
                if (selector != null)
                {
                    if (selector.Trim() == ".")
                        yield return attribute.Value;
                    if (selector.Trim() == "..")
                        yield return attribute.Parent.Value;
                }

                node = attribute.Parent;
            }

            foreach (object obj in SelectNodes<XObject>((XElement)node, selector))
            {
                if (obj is XElement)
                    yield return ((XElement)obj).Value;
                if (obj is XAttribute)
                    yield return ((XAttribute)obj).Value;
            }
        }

        /// <summary>
        /// Finds all namespace declarations in the rules.
        /// </summary>
        private void AddNamespaces(CustomXsltContext context, XElement element)
        {
            foreach (XAttribute attr in element.Attributes().Where(a => a.Name.Namespace == XNamespace.Xmlns))
                context.AddNamespace(attr.Name.LocalName, attr.Value);
        }

        /// <summary>
        /// Finds all parameters in the rules.
        /// </summary>
        private void AddParameters(CustomXsltContext context)
        {
            foreach (XElement param in rules.Descendants(ItsDocument.ItsNamespace + "param"))
            {
                XAttribute name = param.Attribute("name");
                if (name != null)
                    context.Arguments.AddParam(name.Value, "", param.Value);
            }
        }

        /// <summary>
        /// In .NET XPath doesn't support the id() function.
        /// This workaround looks for the occurrences of the string id( and replaces them with _id(
        /// _id() is a custom function that implements id()'s behavior.
        /// <see href="http://stackoverflow.com/questions/15004559/alternative-to-xpath-id-in-net"/>
        /// </summary>
        /// <param name="root">Starting element.</param>
        /// <param name="selector">Selector that may contain the id() function.</param>
        /// <returns>Selector without the id() function.</returns>
        private string ReplaceIdFunction(XElement root, string selector)
        {
            return Regex.Replace(selector, @"id\s*\(", "_id(");
        }

        /// <summary>
        /// Custom XsltContext that can resolve variables and custom functions.
        /// </summary>
        private class CustomXsltContext : XsltContext
        {
            public CustomXsltContext()
                : base(new NameTable())
            {
                Arguments = new XsltArgumentList();
            }

            public XsltArgumentList Arguments
            {
                get;
                private set;
            }

            public override IXsltContextVariable ResolveVariable(string prefix, string name)
            {
                return new XPathExtensionVariable(Arguments.GetParam(name, prefix));
            }

            public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes)
            {
                if (prefix == IdFunction.Prefix && name == IdFunction.Name)
                    return new IdFunction();

                return null;
            }

            public override int CompareDocument(string baseUri, string nextbaseUri)
            {
                return baseUri.CompareTo(nextbaseUri);
            }

            public override bool PreserveWhitespace(XPathNavigator node)
            {
                return true;
            }

            public override bool Whitespace
            {
                get { return true; }
            }

            private class XPathExtensionVariable : IXsltContextVariable
            {
                object value;

                public XPathExtensionVariable(object value)
                {
                    this.value = value;

                    IsLocal = true;
                    IsParam = true;
                    VariableType = XPathResultType.Any;
                }

                public object Evaluate(XsltContext xsltContext)
                {
                    return value;
                }

                public bool IsLocal
                {
                    get;
                    set;
                }

                public bool IsParam
                {
                    get;
                    set;
                }

                public XPathResultType VariableType
                {
                    get;
                    set;
                }
            }

            private class IdFunction : IXsltContextFunction
            {
                public const string Prefix = "";
                public const string Name = "_id";

                public XPathResultType[] ArgTypes
                {
                    get { return new[] { XPathResultType.Any }; }
                }

                public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
                {
                    if (args == null || args.Length == 0)
                        return null;
                    if (!(args[0] is XPathNodeIterator))
                        return null;

                    XPathNodeIterator iterator = (XPathNodeIterator)args[0];
                    if (iterator.MoveNext() && iterator.Current != null)
                    {
                        XPathNavigator nav = iterator.Current;
                        string idValue = nav.Value;

                        // <its:termRule selector="//term" term="yes" termInfoPointer="id(@def)"/>
                        // <term def="TDPV">discoursal point of view</term>
                        // <gloss xml:id="TDPV">the relationship ...</gloss>
                        
                        // before iterator.MoveNext() iterator.Current was <term>
                        // after iterator.MoveNext() iterator.Current should be @def
                        // and so iterator.Current.Value should be TDPV

                        return nav.Evaluate("//*[@xml:id='" + idValue.Replace("'", "\'") + "']", xsltContext);
                    }

                    return null;
                }

                public int Maxargs
                {
                    get { return 1; }
                }

                public int Minargs
                {
                    get { return 1; }
                }

                public XPathResultType ReturnType
                {
                    get { return XPathResultType.Any; }
                }
            }
        }
    }
}
