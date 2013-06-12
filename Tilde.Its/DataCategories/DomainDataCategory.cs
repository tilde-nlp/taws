using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// The Domain data category is used to identify the topic or subject of a given content.
    /// <see href="http://www.w3.org/TR/its20/#domain"/>
    /// </summary>
    public class DomainDataCategory : DataCategory<string>
    {
        /// <summary>
        /// Character that separates domains in a list.
        /// </summary>
        public const char Separator = ',';

        /// <inheritdoc/>
        public DomainDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>
        /// Comma-separated list of domains of the content.
        /// </summary>
        public new string Value
        {
            get { return base.Value; }
            set { base.Value = value; }
        }

        /// <summary>
        /// List of domains of the content.
        /// </summary>
        public string[] Domains
        {
            get
            {
                if (Value == null)
                    return null;

                string[] domains = Value.Split(Separator)
                                        .Select(s => s.Trim())
                                        .Where(s => s.Length > 0)
                                        .OrderBy(s => s)
                                        .ToArray();

                if (domains.Length == 0)
                    return null;

                return domains;
            }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleName
        {
            get { return "domainRule"; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleAttributeName
        {
            get { return "domainPointer"; }
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            return true;
        }

        /// <inheritdoc/>
        protected override string DefaultValue(XObject node)
        {
            return null;
        }

        /// <inheritdoc/>
        protected override bool LocalValue(XElement element, XAttribute attribute, out string value)
        {
            value = null;
            return false;
        }

        /// <inheritdoc/>
        /// <param name="domainPointerAttr">Attribute that holds the value.</param>
        protected override bool GlobalValue(XObject node, XAttribute domainPointerAttr, GlobalRule rule, out string value)
        {
            value = null;

            // An optional domainMapping attribute that contains a comma separated list of mappings between values in the content and consumer tool specific values. 
            XAttribute domainMappingAttr = rule.RuleElement.Attribute("domainMapping");
            List<KeyValuePair<string, string>> domainMappings = domainMappingAttr != null ?
                ParseDomainMapping(domainMappingAttr.Value) : new List<KeyValuePair<string, string>>();

            XElement element = ElementOrAttribute(e => e, a => a.Parent);

            // for each value that domainPointer points to
            // find domains in that value, use mappings to find the mapped value
            // and add them to the domains list
            IEnumerable<string> values = rule.QueryLanguage.SelectPointerValues(element, domainPointerAttr.Value);
            if (!values.Any())
                return false;
            List<string> domains = new List<string>(); // STEP 1: Set the initial value of the resulting string as an empty string.
            foreach (string nodeValue in values) // STEP 2: Get the list of nodes resulting of the evaluation of the domainPointer attribute.
                domains.AddRange(FindDomains(nodeValue, domainMappings)); // STEP 3: For each node:

            value = string.Join(", ", domains.Distinct()); // STEP 4: Remove duplicated values from the resulting string.
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// The information provided by this data category is a comma-separated list of one or more values which is obtained by applying the following algorithm.
        /// </summary>
        /// <param name="value">Comma-separated list of domains.</param>
        /// <param name="mappings">Already found mappings.</param>
        /// <returns>List of domains.</returns>
        private IEnumerable<string> FindDomains(string value, List<KeyValuePair<string, string>> mappings)
        {
            // STEP 3-1: If the node value contains a COMMA (U+002C):
            if (value.Contains(','))
            {
                // STEP 3-1-1: Split the node value into separate strings using the COMMA (U+002C) as separator.
                // STEP 3-1-2: For each string:
                foreach (string s in value.Split(',')) 
                {
                    value = s.Trim(); // STEP 3-1-2-1: Trim the leading and trailing white spaces of the string.
                    if (value.StartsWith("\"") || value.StartsWith("'")) value = value.Substring(1); // STEP 3-1-2-2: If the first character of the value is an APOSTROPHE (U+0027) or a QUOTATION MARK (U+0022): Remove it.
                    if (value.EndsWith("\"") || value.EndsWith("'")) value = value.Substring(0, value.Length - 1); // STEP 3-1-2-3: If the last character of the value is an APOSTROPHE (U+0027) or a QUOTATION MARK (U+0022): Remove it.

                    // STEP 3-1-2-4: If the value is empty: Go to STEP 3-1-2.
                    if (value == "") 
                        continue;

                    // STEP 3-1-2-5: Check if there is a mapping for the string:
                    // STEP 3-1-2-5-1. If a mapping is found: Add the corresponding value to the result string.
                    // STEP 3-1-2-5-2. Else (if no mapping is found): Add the string to the result string.
                    string mappedDomain = mappings.FirstOrDefault(kp => kp.Key == value).Value;
                    yield return mappedDomain != null ? mappedDomain : value;
                }
            }
            // STEP 3-2: Else (if the node value does not contain a COMMA (U+002C)):
            else
            {
                value = value.Trim(); // STEP 3-2-1: Trim the leading and trailing white spaces of the string.
                if (value.StartsWith("\"") || value.StartsWith("'")) value = value.Substring(1); // STEP 3-2-2: If the first character of the value is an APOSTROPHE (U+0027) or a QUOTATION MARK (U+0022): Remove it.
                if (value.EndsWith("\"") || value.EndsWith("'")) value = value.Substring(0, value.Length - 1); // STEP 3-2-3: If the last character of the value is an APOSTROPHE (U+0027) or a QUOTATION MARK (U+0022): Remove it.

                // STEP 3-2-4: If the value is empty: Go to STEP 3.
                if (value == "")
                    yield break;

                // STEP 3-2-5: Check if there is a mapping for the string:
                // STEP 3-2-5-1: If a mapping is found: Add the corresponding value to the result string.
                // STEP 3-2-5-2: Else (if no mapping is found): Add the string to the result string.
                string mappedDomain = mappings.FirstOrDefault(kp => kp.Key == value).Value;
                yield return  mappedDomain != null ? mappedDomain : value;
            }
        }

        /// <summary>
        /// domainMapping attribute contains a comma separated list of mappings between values in the content and consumer tool specific values. 
        /// The left part of the pair corresponds to the source content and is unique within the mapping and case-insensitive. 
        /// The right part of the mapping belongs to the consumer tool. 
        /// Several left parts can map to a single right part. 
        /// The values in the left or the right part of the mapping may contain spaces; in that case they MUST be delimited by quotation marks, 
        /// that is pairs of APOSTROPHE (U+0027) or QUOTATION MARK (U+0022).
        /// </summary>
        /// <param name="mapping">Parses a string containing domain mappings.</param>
        /// <returns>List of mappings.</returns>
        private List<KeyValuePair<string, string>> ParseDomainMapping(string mapping)
        {
            List<KeyValuePair<string, string>> mappings = new List<KeyValuePair<string, string>>();

            if (mapping == null)
                return mappings;

            bool left = true; // are we in the left part or the right part?
            char? quotes = null; // which quotes have been used (null if none)
            string key = ""; // current key (left part) value
            string value = ""; // current value (left part) value

            foreach (char c in mapping + "," /* extra comma to force adding the last mapping */)
            {
                if (c == ',' && quotes == null) // add a mapping to the list
                {
                    key = key.Trim();
                    value = value.Trim();
                    mappings.Add(new KeyValuePair<string, string>(key, value));

                    key = "";
                    value = "";
                    left = true;
                    quotes = null;
                }
                else if (c == ' ' && quotes == null) // switch from left to right part
                {
                    if (!(key == null && value == null)) // ignore white space after comma
                        left = false;
                }
                else if (c == '"' || c == '\'') // parse quotes
                {
                    if (quotes == null) quotes = c; // open quotes
                    else if (quotes == c) quotes = null; // close quotes
                    else
                    {
                        // add to buffer
                        if (left) key += c;
                        else value += c;
                    }
                }
                else
                {
                    // add to buffer
                    if (left) key += c;
                    else value += c;
                }
            }

            return mappings;
        }
    }
}