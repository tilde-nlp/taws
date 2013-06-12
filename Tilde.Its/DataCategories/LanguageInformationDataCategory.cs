using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// The Language Information data category is used to express the language of a given piece of content.
    /// <see href="http://www.w3.org/TR/its20/#language-information"/>
    /// </summary>
    public class LanguageInformationDataCategory : SinglePointerDataCategory
    {
        /// <inheritdoc/>
        public LanguageInformationDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>
        /// Language that the content is in.
        /// <see langword="null"/> if the language is not defined.
        /// </summary>
        public string Language
        {
            get { return Value; }
            set { Value = value; } 
        }

        /// <inheritdoc/>
        protected override string GlobalRuleName
        {
            get { return "langRule"; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleAttributeName
        {
            get { return "langPointer"; }
        }

        /// <inheritdoc/>
        protected override XName LocalAttributeName
        {
            get
            {
                // in HTML5 xml:lang should be ignored
                // http://diveintohtml5.info/semantics.html
                return XmlOrHtmlDocument(() => XNamespace.Xml + "lang", () => "lang");
            }
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            return true;
        }
    }
}