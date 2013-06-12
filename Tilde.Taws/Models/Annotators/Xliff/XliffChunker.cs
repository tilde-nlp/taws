using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Tilde.Its;

namespace Tilde.Taws.Models
{
    /// <summary>
    /// <see cref="Chunker"/> for an XLIFF document.
    /// </summary>
    public class XliffChunker : Chunker
    {
        /// <summary>
        /// Names of elements whose content to use.
        /// </summary>
        public static readonly string[] ContentElements = new[] { "source", "seg-source", "target" };

        /// <summary>
        /// Creates a new instance for an ITS annotated document.
        /// </summary>
        /// <param name="document">ITS annotated document to use.</param>
        public XliffChunker(ItsXmlDocument document)
            : base(document)
        {
        }

        /// <summary>
        /// Document namespace.
        /// </summary>
        private XNamespace Namespace
        {
            get { return document.Document.Root.Name.Namespace; }
        }

        #region Splitting
        /// <summary>
        /// Extracts independent text flows and returns them grouped by file, language and domain.
        /// </summary>
        /// <returns>Grouped chunk collection.</returns>
        public ChunkCollection Split()
        {
            ChunkCollection chunks = new ChunkCollection();

            foreach (XElement file in document.Document.Root.Elements(Namespace + "file"))
            {
                XElement fileBody = file.Element(Namespace + "body");
                if (fileBody == null)
                    continue;

                // each content element (<source>, <target>)
                // could contain inline elements that are overwritten by its rules
                // to become independent element in another language and even in another domain

                foreach (XElement contentElement in fileBody.Descendants().Where(e => ContentElements.Any(name => Namespace + name == e.Name)))
                    SplitContentElement(chunks, file, contentElement);
            }

            chunks.RemoveEmpty();

            return chunks;
        }

        private void SplitContentElement(ChunkCollection chunks, XElement file, XElement contentElement)
        {
            // override the content element language
            contentElement.Annotation<LanguageInformationDataCategory>().Language = Language(contentElement);

            string[] languages = AllValues(contentElement, e => new[] { e.Annotation<LanguageInformationDataCategory>().Language }).ToArray();
            string[] domains = AllValues(contentElement, e => e.Annotation<DomainDataCategory>().Domains).ToArray();

            foreach (string language in languages)
            {
                foreach (string domain in domains)
                {
                    Predicate<XElement> filter = CreateFilter(language, domain);

                    FileLanguageDomainGroup group = new FileLanguageDomainGroup(file, language, domain);
                    if (!chunks.ContainsKey(group))
                        chunks[group] = new Dictionary<XElement, string[]>();

                    foreach (XElement element in ExtractIndependentElements(contentElement, filter))
                    {
                        string[] texts = MergeExtractedText(ExtractTextFromElement(element, filter)).ToArray();
                        if (texts.Length > 0 && !string.IsNullOrWhiteSpace(string.Join("", texts)))
                            chunks[group][element] = texts;
                    }
                }
            }
        }
        #endregion

        #region Languages
        /// <summary>
        /// Returns the content language of an element.
        /// </summary>
        /// <param name="element">Element to use.</param>
        /// <returns>Content language.</returns>
        private string Language(XElement element)
        {
            if (element.Name == Namespace + "source" ||
                element.Name == Namespace + "seg-source")
            {
                return SourceLanguage(element);
            }

            if (element.Name == Namespace + "target")
            {
                return TargetLanguage(element);
            }

            return ItsLang(element);
        }

        private string SourceLanguage(XElement element)
        {
            return GetLanguage(element, "source-language");
        }

        private string TargetLanguage(XElement element)
        {
            return GetLanguage(element, "target-language");
        }

        /// <summary>
        /// Gets the language of an element.
        /// </summary>
        /// <param name="element">Element to use.</param>
        /// <param name="fileAttribute">Attribute name on the &lt;file&gt; element that contains the language.</param>
        /// <returns>Language.</returns>
        private string GetLanguage(XElement element, string fileAttribute)
        {
            /* string itsLang = ItsLang(element);
            if (itsLang != null)
                return itsLang; */

            string xmlLang = XmlLang(element);
            if (xmlLang != null)
                return xmlLang;

            foreach (XElement ancestor in element.Ancestors())
            {
                if (ancestor.Name == Namespace + "alt-trans")
                {
                    xmlLang = XmlLang(ancestor);
                    if (xmlLang != null)
                        return xmlLang;
                }

                if (ancestor.Name == Namespace + "file")
                {
                    XAttribute languageAttribute = ancestor.Attribute(fileAttribute);
                    if (languageAttribute != null)
                        return languageAttribute.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the language of an element from the ITS Language Information data category annotation.
        /// </summary>
        /// <param name="element">Element to use.</param>
        /// <returns>Language.</returns>
        private string ItsLang(XElement element)
        {
            LanguageInformationDataCategory langInfo = element.Annotation<LanguageInformationDataCategory>();
            if (langInfo != null)
                if (langInfo.Language != null)
                    return langInfo.Language;
            return null;
        }

        /// <summary>
        /// Gets the language of an element from the xml:lang attribute.
        /// </summary>
        /// <param name="element">Element to use.</param>
        /// <returns>Language.</returns>
        private string XmlLang(XElement element)
        {
            XAttribute xmlLang = element.Attribute(XNamespace.Xml + "lang");
            if (xmlLang != null)
                return xmlLang.Value;
            return null;
        }
        #endregion

        #region Overrides
        /// <inheritdoc/>
        protected override bool IsWhitespaceElement(XElement element)
        {
            if (element.Name == Namespace + "mrk" &&
                element.Attribute("mtype") != null &&
                element.Attribute("mtype").Value.ToLowerInvariant().Trim() == "seg")
            {
                return true;
            }   

            return base.IsWhitespaceElement(element);
        }
        #endregion

        #region Chunk
        /// <summary>
        /// Group that is identified by an XLIFF file, a language (case-insensitive, two letters) and a domain.
        /// </summary>
        public class FileLanguageDomainGroup : LanguageDomainGroup, IEquatable<FileLanguageDomainGroup>
        {
            /// <summary>
            /// Creates a new instance of this class.
            /// </summary>
            /// <param name="file">XLIFF file.</param>
            /// <param name="language">Language.</param>
            /// <param name="domain">Domain.</param>
            public FileLanguageDomainGroup(XElement file = null, string language = null, string domain = null)
                : base(language, domain)
            {
                File = file;
            }

            /// <summary>
            /// Gets or sets the XLIFF file.
            /// </summary>
            public XElement File
            {
                get;
                set;
            }

            /// <inheritdoc/>
            public override bool Equals(object obj)
            {
                return Equals(obj as FileLanguageDomainGroup);
            }

            /// <summary>
            /// Checks if two objects have equal values.
            /// </summary>
            /// <param name="other">The other object to compare.</param>
            /// <returns>Whether both instances have equal values.</returns>
            public bool Equals(FileLanguageDomainGroup other)
            {
                return base.Equals(other) && other.File == File;
            }

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                return base.GetHashCode() ^ (File != null ? File.GetHashCode() : 1);
            }
        }
        #endregion
    }
}