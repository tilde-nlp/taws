using System;
using System.Linq;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// The Locale Filter data category specifies that a node is only applicable to certain locales.
    /// <see href="http://www.w3.org/TR/its20/#LocaleFilter"/>
    /// </summary>
    public class LocaleFilterDataCategory : DataCategory<LocaleFilter>
    {
        /// <inheritdoc/>
        public LocaleFilterDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>
        /// Locale filter.
        /// </summary>
        public LocaleFilter LocaleFilter
        {
            get { return Value; }
            set { Value = value; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleName
        {
            get { return "localeFilterRule"; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleAttributeName
        {
            get { return "localeFilterList"; }
        }

        /// <inheritdoc/>
        protected override XName LocalAttributeName
        {
            get { return XmlOrHtmlAttributeName("localeFilterList"); }
        }

        /// <inheritdoc/>
        /// <param name="filterListAttribute">Attribute that holds the value.</param>
        /// <param name="filter">Returned value. Undefined if the method returned <see langword="false"/>.</param>
        protected override bool LocalValue(XElement element, XAttribute filterListAttribute, out LocaleFilter filter)
        {
            LocaleFilterType filterType = LocaleFilterType.Include;
            XAttribute filterTypeAttr = LocalAttribute(element, XmlOrHtmlAttributeName("localeFilterType"));
            if (filterTypeAttr != null)
                filterType = ParseLocaleFilterType(filterTypeAttr.Value);

            filter = new LocaleFilter(filterListAttribute.Value, filterType);
            return true;
        }

        /// <inheritdoc/>
        /// <param name="filterListAttribute">Global rule attribute that holds the value.</param>
        /// <param name="filter">Returned value. Undefined if the method returned <see langword="false"/>.</param>
        protected override bool GlobalValue(XObject node, XAttribute filterListAttribute, GlobalRule rule, out LocaleFilter filter)
        {
            LocaleFilterType filterType = LocaleFilterType.Include;
            XAttribute filterTypeAttr = rule.RuleElement.Attribute("localeFilterType");
            if (filterTypeAttr != null)
                filterType = ParseLocaleFilterType(filterTypeAttr.Value);

            filter = new LocaleFilter(filterListAttribute.Value, filterType);
            return true;
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            return true;
        }

        /// <inheritdoc/>
        protected override LocaleFilter DefaultValue(XObject node)
        {
            return new LocaleFilter("*", LocaleFilterType.Include);
        }

        private LocaleFilterType ParseLocaleFilterType(string s)
        {
            switch (NormalizeValue(s))
            {
                default:
                case "include": return LocaleFilterType.Include;
                case "exclude": return LocaleFilterType.Exclude;
            }
        }
    }

    /// <summary>
    /// How to filter the ranges.
    /// </summary>
    public enum LocaleFilterType
    {
        /// <summary>
        /// Applies to the selected locales.
        /// </summary>
        Include,
        /// <summary>
        /// Doesn't apply to the selected locales.
        /// </summary>
        Exclude
    }

    /// <summary>
    /// Locale filter.
    /// </summary>
    public class LocaleFilter
    {
        /// <summary>
        /// Creates a new instance with the specified values.
        /// </summary>
        /// <param name="value">Language ranges as a comma-separated list.</param>
        /// <param name="filterType"> How to filter the ranges.</param>
        public LocaleFilter(string value, LocaleFilterType filterType)
        {
            Value = value;
            FilterType = filterType;
        }

        /// <summary>
        /// Language ranges as a comma-separated list.
        /// </summary>
        public string Value
        {
            get;
            private set;
        }

        /// <summary>
        /// How to filter the ranges.
        /// </summary>
        public LocaleFilterType FilterType
        {
            get;
            private set;
        }

        /// <summary>
        /// Language ranges.
        /// </summary>
        public string[] Ranges
        {
            get
            {
                if (Value == null || IsEmpty)
                    return new string[1] { string.Empty };

                return Value.Split(',')
                            .Select(s => s.Trim())
                            .Where(s => s.Length > 0)
                            .OrderBy(s => s)
                            .ToArray();
            }
        }

        /// <summary>
        /// Whether the language range is a wildcard.
        /// </summary>
        public bool IsWildcard
        {
            get { return Value.Trim() == "*"; }
        }

        /// <summary>
        /// Whether the language range is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return Value.Trim() == ""; }
        }

        /// <summary>
        /// Checks if the filter applies to the provided locale.
        /// </summary>
        /// <param name="locale">Locale to check.</param>
        /// <returns><see langword="true"/> if it matches; otherwise <see langword="false"/></returns>
        public bool IsMatch(string locale)
        {
            // A single wildcard "*" with a type "include" indicates that the selected content applies to all locales.
            if (IsWildcard && FilterType == LocaleFilterType.Include)
                return true;
            // A single wildcard "*" with a type "exclude" indicates that the selected content applies to no locale.
            if (IsWildcard && FilterType == LocaleFilterType.Exclude)
                return false;
            // An empty string with a type "include" indicates that the selected content applies to no locale.
            if (IsEmpty && FilterType == LocaleFilterType.Include)
                return false;
            // An empty string with a type "exclude" indicates that the selected content applies to all locales.
            if (IsEmpty && FilterType == LocaleFilterType.Exclude)
                return false;

            bool isMatch = Ranges.Any(range => Locale.IsExtendedMatch(locale ?? "", range));

            // Otherwise, with a type "include", the selected content applies to the locales for which the language tag has a match in the list 
            // when using the Extended Filtering algorithm defined in [BCP47].
            if (FilterType == LocaleFilterType.Include)
            {
                return isMatch;
            }
            // If, instead, the type is "exclude", the selected content applies to the locales for which the language tag does not have a match in the list 
            // when using the Extended Filtering algorithm defined in [BCP47].
            else
            {
                return !isMatch;
            }
        }
    }

    /// <summary>
    /// Locale is a set of parameters that defines the user's language, country and any special variant preferences that the user wants to see in their user interface.
    /// </summary>
    public class Locale : IEquatable<Locale>
    {
        /// <summary>
        /// Represents all locales.
        /// </summary>
        public const string Wildcard = "*";
        /// <summary>
        /// Delimiter that separates dialects.
        /// </summary>
        public const char Delimiter = '-';

        /// <summary>
        /// An empty locale.
        /// </summary>
        public static readonly Locale Empty = new Locale();

        /// <summary>
        /// Constructor for the <see cref="Empty"/> locale.
        /// </summary>
        private Locale()
        {
        }

        /// <summary>
        /// Creates a new locale from a string.
        /// </summary>
        /// <param name="locale">Locale string.</param>
        public Locale(string locale)
        {
            if (locale == null)
                throw new ArgumentNullException("locale");

            Value = locale.Trim();

            if (Value == string.Empty)
                Value = null;
        }

        /// <summary>
        /// Locale string.
        /// </summary>
        public string Value
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether the locale matches the range using basic filtering.
        /// </summary>
        /// <param name="range">Language range.</param>
        /// <returns><see langword="true"/> if it matches; otherwise <see langword="false"/></returns>
        public bool IsBasicMatch(string range)
        {
            return IsBasicMatch(Value ?? "", range);
        }

        /// <summary>
        /// Whether the locale matches the range using extended filtering.
        /// </summary>
        /// <param name="range">Language range.</param>
        /// <returns><see langword="true"/> if it matches; otherwise <see langword="false"/></returns>
        public bool IsExtendedMatch(string range)
        {
            return IsExtendedMatch(Value ?? "", range);
        }

        /// <summary>
        /// Whether the locale matches the range using basic filtering.
        /// </summary>
        /// <param name="tag">Language tag.</param>
        /// <param name="range">Language range.</param>
        /// <returns><see langword="true"/> if it matches; otherwise <see langword="false"/></returns>
        public static bool IsBasicMatch(string tag, string range)
        {
            if (tag == null)
                throw new ArgumentNullException("tag");
            if (range == null)
                throw new ArgumentNullException("range");

            tag = tag.ToLowerInvariant();
            range = range.ToLowerInvariant();

            // Language-range has the same syntax as a language-tag, or is the single character "*".
            // A language-range matches a language-tag if it exactly equals the tag,
            // or if it exactly equals a prefix of the tag such that the first character following the prefix is "-".

            if (range == Wildcard)
                return true;
            
            if (range == tag)
                return true;
            
            if (range.Length > tag.Length)
                return false;

            if (tag.Substring(0, range.Length) == range && tag[range.Length] == Delimiter)
                return true;

            return false;
        }

        /// <summary>
        /// Whether the locale matches the range using extended filtering.
        /// </summary>
        /// <param name="tag">Language tag.</param>
        /// <param name="range">Language range.</param>
        /// <returns><see langword="true"/> if it matches; otherwise <see langword="false"/></returns>
        public static bool IsExtendedMatch(string tag, string range)
        {
            if (tag == null)
                throw new ArgumentNullException("tag");
            if (range == null)
                throw new ArgumentNullException("range");

            tag = tag.ToLowerInvariant();
            range = range.ToLowerInvariant();

            if (tag == range)
                return true;

            // Step 1 from RFC 4647 section 3.3.2.
            return IsExtendedMatch(tag.Split(Delimiter), range.Split(Delimiter));
        }

        private static bool IsExtendedMatch(string[] tags, string[] range)
        {
            // Step 2
            if (tags[0] != range[0] && range[0] != Wildcard) 
                return false;

            int t = 1;
            int r = 1;

            // Step 3
            while (r < range.Length) {
                // Step 3A
                if (range[r] == Wildcard) {
                    r++;
                    continue;
                }

                // Step 3B
                if (t >= tags.Length) 
                    return false;

                // Step 3C
                if (tags[t] == range[r]) {
                    t++;
                    r++;
                    continue;
                }

                // Step 3D
                if (tags[t].Length == 1) 
                    return false;

                // Step 3E
                t++;
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as Locale);
        }

        /// <inheritdoc/>
        public bool Equals(Locale other)
        {
            return other != null && Value == other.Value;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Value;
        }
    }
}