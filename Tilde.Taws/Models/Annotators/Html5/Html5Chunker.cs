using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Tilde.Its;

namespace Tilde.Taws.Models
{
    /// <summary>
    /// <see cref="Chunker"/> for an HTML5 document.
    /// </summary>
    public class Html5Chunker : Chunker
    {
        /// <summary>
        /// Names of elements whose content should be ignored.
        /// </summary>
        public static readonly string[] IgnoredElements = new[] { "script", "style", "iframe", "frame" };
        /// <summary>
        /// Names of elements that don't have any content.
        /// </summary>
        public static readonly string[] VoidElements = new[] { "area", "base", "br", "col", "command", "embed", "hr", "img", "input", "keygen", "link", "meta", "param", "source", "track", "wbr" };
        /// <summary>
        /// Names of elements whose content should not be escaped.
        /// </summary>
        public static readonly string[] RawTextElements = new[] { "script", "style" };

        /// <summary>
        /// Creates a new instance for an ITS annotated document.
        /// </summary>
        /// <param name="document">ITS annotated document to use.</param>
        public Html5Chunker(ItsHtmlDocument document)
            : base(document)
        {
        }

        #region Splitting
        /// <summary>
        /// Extracts independent text flows and returns them grouped by language and domain.
        /// </summary>
        /// <param name="root">Element to start with.</param>
        /// <returns>Grouped chunk collection.</returns>
        public ChunkCollection Split(XElement root)
        {
            ChunkCollection chunks = new ChunkCollection();

            string[] languages = AllValues(root, e => new[] { e.Annotation<LanguageInformationDataCategory>().Language }).ToArray();
            string[] domains = AllValues(root, e => e.Annotation<DomainDataCategory>().Domains).ToArray();

            foreach (string language in languages)
            {
                foreach (string domain in domains)
                {
                    Predicate<XElement> filter = CreateFilter(language, domain);

                    LanguageDomainGroup group = new LanguageDomainGroup(language, domain);
                    if (!chunks.ContainsKey(group))
                        chunks[group] = new Dictionary<XElement, string[]>();

                    foreach (XElement element in ExtractIndependentElements(root, filter))
                    {
                        string[] texts = MergeExtractedText(ExtractTextFromElement(element, filter)).ToArray();
                        if (texts.Length > 0 && !string.IsNullOrWhiteSpace(string.Join("", texts)))
                            chunks[group][element] = texts;
                    }
                }
            }

            chunks.RemoveEmpty();

            return chunks;
        }
        #endregion

        #region Overrides
        /// <inheritdoc/>
        protected override bool IsInlineElement(XElement element)
        {
            if (element.Name.Namespace != ItsHtmlDocument.XhtmlNamespace)
                return true;

            return base.IsInlineElement(element);
        }

        /// <inheritdoc/>
        protected override bool IsWhitespaceElement(XElement element)
        {
            return ContainsElement(VoidElements, element);
        }

        /// <inheritdoc/>
        protected override bool IsIgnoredElement(System.Xml.Linq.XElement element)
        {
            return ContainsElement(IgnoredElements, element);
        }

        private bool ContainsElement(string[] names, XElement element)
        {
            return names.Any(name => element.Name == ItsHtmlDocument.XhtmlNamespace + name);
        }
        #endregion
    }
}