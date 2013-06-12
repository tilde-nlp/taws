using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

using Tilde.Its;

namespace Tilde.Taws.Models
{
    /// <summary>
    /// Class for extracting independent text flows and importing annotations.
    /// </summary>
    public class Chunker
    {
        /// <summary>
        /// Character that represents whitespace and is used to replace elements
        /// that have no content but appear as whitespace.
        /// </summary>
        private const char WhitespaceCharacter = ' ';

        /// <summary>
        /// ITS annotated document to use for extraction.
        /// </summary>
        protected ItsDocument document;

        /// <summary>
        /// Creates a new instance for an ITS annotated document.
        /// </summary>
        /// <param name="document">ITS annotated document to use.</param>
        public Chunker(ItsDocument document)
        {
            this.document = document;
        }

        /// <summary>
        /// Dictionary of (lemma, part-of-speech) tuples and term annotations.
        /// </summary>
        public Dictionary<Tuple<string, string>, XElement> SingleTerms
        {
            get;
            set;
        }

        #region Splitting
        /// <summary>
        /// Finds independent elements (whose content is an independent text flow) in the provided element's descendants.
        /// </summary>
        /// <param name="element">Element to start the extraction with.</param>
        /// <param name="filter">Function that checks if the element's content is independent or part of it.</param>
        /// <param name="includeSelf">Whether to include the element itself in the results.</param>
        /// <returns>List of independent elements.</returns>
        protected virtual IEnumerable<XElement> ExtractIndependentElements(XElement element, Predicate<XElement> filter, bool includeSelf = true)
        {
            List<XElement> elements = new List<XElement>();

            if (IsIgnoredElement(element))
                return elements;

            if (includeSelf)
                elements.Add(element);

            foreach (XNode node in element.Nodes())
            {
                if (node is XElement)
                {
                    XElement child = (XElement)node;

                    if (IsInlineElement(child))
                    {
                        elements.AddRange(ExtractIndependentElements(child, filter, !filter(child)));
                    }
                    else
                    {
                        elements.AddRange(ExtractIndependentElements(child, filter));
                    }
                }
            }

            return elements;
        }

        /// <summary>
        /// Extracts text that belongs to the element's text flow from the element and its descendants.
        /// </summary>
        /// <param name="element">Element from which to extract text.</param>
        /// <param name="filter">Function that checks if the element's content is independent or part of it.</param>
        /// <returns>Text in traversal order where <see null=""/> represents a break in the flow (i.e. there was another element in that place).</returns>
        protected virtual IEnumerable<string> ExtractTextFromElement(XElement element, Predicate<XElement> filter)
        {
            List<string> texts = new List<string>();

            if (IsIgnoredElement(element))
                return texts;

            if (IsWhitespaceElement(element) && !IsFirstChild(element))
                texts.Insert(0, WhitespaceCharacter.ToString());

            foreach (XNode node in element.Nodes())
            {
                if (node is XText)
                {
                    if (filter(element))
                    {
                        texts.Add(NodeText(node as XText));
                    }
                }

                if (node is XElement)
                {
                    XElement child = node as XElement;

                    if (IsInlineElement(child))
                    {
                        if (filter(child))
                        {
                            texts.AddRange(ExtractTextFromElement(child, filter));
                        }
                        else
                        {
                            texts.Add(null);
                        }
                    }
                    else
                    {
                        if (!IsNestedElement(child))
                        {
                            texts.Add(null);
                        }
                    }
                }
            }

            return texts;
        }

        /// <summary>
        /// Merges all strings in a list that are not separated by a <see langword="null"/>.
        /// <example>
        /// Input: a, b, null, c, d, null, e, null, f, null, g, h, null
        /// Output: ab, cd, e, f, gh
        /// </example>
        /// </summary>
        /// <param name="texts">List of strings.</param>
        /// <returns>Joined strings.</returns>
        protected virtual IEnumerable<string> MergeExtractedText(IEnumerable<string> texts)
        {
            List<string> unjoinedTexts = texts.ToList();
            List<string> result = new List<string>();
            StringBuilder buffer = new StringBuilder();

            for (int i = 0; i < unjoinedTexts.Count; i++)
            {
                if (unjoinedTexts[i] == null)
                {
                    if (buffer.Length > 0)
                    {
                        result.Add(buffer.ToString());
                        buffer.Clear();
                    }
                }
                else
                {
                    buffer.Append(unjoinedTexts[i]);
                }
            }

            if (buffer.Length > 0)
                result.Add(buffer.ToString());

            return result;
        }
        #endregion

        #region Joining
        /// <summary>
        /// The opposite of Split. Imports annotated text and creates term tags.
        /// </summary>
        /// <param name="chunks">Chunks with annotations.</param>
        public virtual void Join(ChunkCollection chunks)
        {
            foreach (var chunk in chunks)
            {
                Predicate<XElement> filter = CreateFilter(chunk.Key.Language, chunk.Key.Domain);

                foreach (var kp in chunk.Value.OrderBy(kp => kp.Key.Descendants().Count()))
                {
                    XElement element = kp.Key;
                    string[] texts = kp.Value;
                    Join(element, string.Join("", texts), filter);
                }
            }
        }

        /// <summary>
        /// Creates the missing tags as elements in the document.
        /// </summary>
        /// <param name="element">Element to start with.</param>
        /// <param name="text">Element content as a string with the new tags.</param>
        /// <param name="filter">Function that checks if the element's content is independent or part of it.</param>
        protected virtual void Join(XElement element, string text, Predicate<XElement> filter)
        {
            text = NormalizeWhitespace(text);
            XElement openTag = null;
            Join(element, new Queue<char>(text), filter, ref openTag);
        }

        /// <summary>
        /// Creates the missing tags as elements in the document.
        /// </summary>
        /// <param name="element">Element to start with.</param>
        /// <param name="text">Element content as a string with the new tags.</param>
        /// <param name="filter">Function that checks if the element's content is independent or part of it.</param>
        /// <param name="openTag">Currently open tag name and attributes.</param>
        protected virtual void Join(XElement element, Queue<char> text, Predicate<XElement> filter, ref XElement openTag)
        {
            if (IsIgnoredElement(element))
                return;

            List<XNode> nodes = new List<XNode>();

            if (IsWhitespaceElement(element) && !IsFirstChild(element) && text.Count > 0 && text.Peek() == WhitespaceCharacter)
                text.Dequeue(); // WhitespaceCharacter

            foreach (XNode node in element.Nodes())
            {
                if (node is XText)
                {
                    if (filter(element))
                    {
                        string original = NormalizeWhitespace(NodeText(node as XText));
                        string buffer = "";

                        // compare the text in this node to the returned text
                        // and do nothing as long as they are the same
                        // the node text and tagged text must be equal except for term tags
                        // when they are not the same i could mean that
                        // (1) the following part will be tagged
                        // (2) the previous text was tagged
                        // (3) the text is different i.e. besides term tags there are other tags 
                        //     or other characters were inserted or some characters (such as whitespace) were removed
                        for (int i = 0; i < original.Length; i++)
                        {
                            char originalChar = original[i];
                            char newChar = text.Dequeue();

                            if (originalChar == newChar && newChar != '<')
                            {
                                buffer += originalChar;
                                continue;
                            }

                            // we've reached a point where the node text is different than tagged text
                            if (newChar == '<')
                            {
                                if (StartsOrEndsWithNewTag(text, "<"))
                                {
                                    // tag is opened
                                    if (openTag == null)
                                    {
                                        if (text.Count > 0 && text.Peek() == '/')
                                            throw new Exception("Closing an annotation tag when no annotation tags are open");

                                        nodes.Add(CreateTextNode(buffer)); // there's already some text in the buffer before the tag, we add that
                                        buffer = "";
                                        openTag = ReadTagDefinition(text);
                                    }
                                    // tag is closed
                                    else if (openTag != null)
                                    {
                                        if (text.Count > 0 && text.Peek() != '/')
                                            throw new NotImplementedException("Nested annotation tags are not supported");

                                        text.Dequeue(); // char: /

                                        if (openTag.Name != ReadTagDefinition(text).Name)
                                            throw new Exception("Unexpected annotation closing tag");

                                        nodes.Add(CreateNode(openTag, buffer));
                                        buffer = "";
                                        openTag = null;
                                        //ignoreOpenTag = false;
                                    }

                                    // to read the current original char again
                                    i--;
                                }
                                else
                                {
                                    buffer += "<";
                                }
                            }
                            else
                            {
                                throw new Exception("Annotated text different from the original text");
                            }
                        }

                        if (openTag != null)
                        {
                            // we've reach the end of the text node
                            // which means the next node will not be text but rather another element
                            // this means that the tagged text spans another element

                            bool tagWillBeclosed = text.Count >= 2 &&
                                text.ElementAt(0) == '<' && text.ElementAt(1) == '/' &&
                                StartsOrEndsWithNewTag(text);

                            if (!tagWillBeclosed)
                            {
                                openTag.AddAnnotation(new IgnoreOpenedTag()); //ignoreOpenTag = true;
                                nodes.Add(CreateNode(openTag, buffer));
                            }
                            else
                            {
                                nodes.Add(CreateNode(openTag, buffer));
                                openTag.RemoveAnnotations<IgnoreOpenedTag>(); //ignoreOpenTag = false;
                            }

                            // perhaps the currently open tag is closed now?
                            if (tagWillBeclosed)
                            {
                                text.Dequeue(); // <
                                text.Dequeue(); // /

                                if (openTag.Name != ReadTagDefinition(text).Name)
                                    throw new Exception("Unexpected closing annotation tag");

                                openTag = null;
                            }
                        }
                        else
                        {
                            nodes.Add(CreateTextNode(buffer));
                        }
                    }
                    else
                    {
                        nodes.Add(node);
                    }
                }
                else if (node is XElement)
                {
                    XElement child = node as XElement;

                    if (IsInlineElement(child))
                    {
                        if (filter(child))
                        {
                            Join(child, text, filter, ref openTag);
                            nodes.Add(node);
                        }
                        else
                        {
                            nodes.Add(node);
                        }
                    }
                    else
                    {
                        nodes.Add(node);
                    }
                }
                else
                {
                    nodes.Add(node);
                }
            }

            element.ReplaceNodes(nodes);
        }

        /// <summary>
        /// Used as an annotation to the openTag parameter in the Join method
        /// to indicate that the openTag spans multiple tags (which is not allowed) and should be ignored.
        /// </summary>
        private sealed class IgnoreOpenedTag { }

        /// <summary>
        /// Reads the tag name and attributes.
        /// </summary>
        /// <param name="text">Text that contains the tag definition (without the leading &lt;).</param>
        /// <returns>Tag name and attributes as an <see cref="XElement"/>.</returns>
        private XElement ReadTagDefinition(Queue<char> text)
        {
            string tagName = "";
            string attributes = "";

            char c;
            while (text.Count > 0 && (c = text.Dequeue()) != '>')
                tagName += c;

            if (tagName.IndexOf(' ') != -1)
            {
                attributes = tagName.Substring(tagName.IndexOf(' '));
                tagName = tagName.Substring(0, tagName.IndexOf(' '));
            }

            // remove invalid xml attribute values
            attributes = attributes.Replace("&", "&amp;");
            attributes = attributes.Replace("<", "&lt;");

            return XElement.Parse(string.Format("<{0} {1} />", tagName, attributes));
        }

        /// <summary>
        /// Whether the text in the queue starts with a tag definition or starts with a closing tag.
        /// </summary>
        /// <param name="text">Text queue.</param>
        /// <param name="prefix">String to virtually add to the queue.</param>
        /// <returns>Whether the text starts with a tag definition.</returns>
        private bool StartsOrEndsWithNewTag(Queue<char> text, string prefix = "")
        {
            string tag = prefix ?? "";
            bool attributes = false;

            foreach (char c in text)
            {
                if (!attributes)
                {
                    if (c == ' ')
                    {
                        attributes = true;
                        continue;
                    }

                    tag += c;

                    if (c == '>')
                        break;
                }
                else
                {
                    if (c == '>')
                    {
                        tag += c;
                        break;
                    }
                }
            }

            if (tag.EndsWith(">"))
            {
                if (tag.StartsWith("</")) return IsNewTagName(tag.Substring(2, tag.Length - 3));
                else if (tag.StartsWith("<")) return IsNewTagName(tag.Substring(1, tag.Length - 2));
                else return false;
            }
            return false;
        }

        private bool IsFirstChild(XElement element)
        {
            if (!IsInlineElement(element.Parent))
            {
                return element.Parent.FirstNode == element;
            }

            return false;
        }

        private string NormalizeWhitespace(string s)
        {
            return s.Replace("\r\n", "\n") // Windows newlines
                    .Replace("\r", "\n") // Macintosh newlines
                    .Replace("\t", "");
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Creates a filter based on language, domain and locale filter.
        /// </summary>
        /// <param name="language">Language to filter.</param>
        /// <param name="domain">Domain to filter.</param>
        /// <returns>Function that checks whether the element's content is in the given language, in the given domain and included by the Locale Filter data category.</returns>
        protected Predicate<XElement> CreateFilter(string language, string domain)
        {
            return element =>
            {
                if (element.Annotation<DataCategory>() == null)
                {
                    if (element.Parent == null)
                        return false;

                    return CreateFilter(language, domain)(element.Parent);
                }

                string elementLanguage = element.Annotation<LanguageInformationDataCategory>().Language;
                string[] elementDomains = element.Annotation<DomainDataCategory>().Domains ?? new string[] { null };

                if ((language ?? "").ToLowerInvariant() != (elementLanguage ?? "").ToLowerInvariant())
                    return false;

                if (!elementDomains.Contains(domain))
                    return false;

                LocaleFilter localeFilter = element.Annotation<LocaleFilterDataCategory>().LocaleFilter;
                return localeFilter.IsMatch(language);
            };
        }

        /// <summary>
        /// Checks if this element is considered to be an inline element 
        /// i.e. the element and its content are part of the flow of its parent element.
        /// </summary>
        /// <param name="element">Element to check.</param>
        /// <returns><see langword="true"/> if the element is inline; otherwise, <see langword="false"/>.</returns>
        protected virtual bool IsInlineElement(XElement element)
        {
            if (element.Annotation<ElementsWithinTextDataCategory>() == null)
                return true;

            return element.Annotation<ElementsWithinTextDataCategory>().WithinText == WithinText.Yes;
        }

        /// <summary>
        /// Checks if this element is considered to be a nested element .
        /// </summary>
        /// <param name="element">Element to check.</param>
        /// <returns><see langword="true"/> if the element is nested; otherwise, <see langword="false"/>.</returns>
        protected virtual bool IsNestedElement(XElement element)
        {
            if (element.Annotation<ElementsWithinTextDataCategory>() == null)
                return false;

            return element.Annotation<ElementsWithinTextDataCategory>().WithinText == WithinText.Nested;
        }

        /// <summary>
        /// Checks to see if an element is displayed as whitespace (i.e. it separates the text flow).
        /// </summary>
        /// <param name="element">Element to check.</param>
        /// <returns><see langword="true"/> if the element is dispalyed as whitespace; otherwise, <see langword="false"/>.</returns>
        protected virtual bool IsWhitespaceElement(XElement element)
        {
            return false;
        }

        /// <summary>
        /// Checks if this element, its content and all of its children should be ignored.
        /// </summary>
        /// <param name="element">Element to check.</param>
        /// <returns><see langword="true"/> if the element should be ignored; otherwise, <see langword="false"/>.</returns>
        protected virtual bool IsIgnoredElement(XElement element)
        {
            return false;
        }

        /// <summary>
        /// Creates a new node for a term.
        /// </summary>
        /// <param name="openTag">Term annotation tag.</param>
        /// <param name="buffer">Term.</param>
        /// <returns>New node.</returns>
        protected virtual XNode CreateNode(XElement openTag, string buffer)
        {
            // there is no annotation tag, create a text node
            if (openTag == null)
                return CreateTextNode(buffer);

            bool ignoreOpenTag = openTag.Annotation<IgnoreOpenedTag>() != null;

            if (ignoreOpenTag)
            {
                // check if individual parts of the term are terms
                Tename tename = CreateElement(openTag, null) as Tename;
                if (SingleTerms != null && tename != null && tename.Lemmas != null && tename.PartOfSpeech != null)
                {
                    List<string> lemmas = tename.Lemmas.ToList();
                    int index = lemmas.IndexOf(buffer.ToLower());
                    if (index > -1)
                    {
                        string lemma = lemmas[index];
                        string partOfSpeech = tename.PartsOfSpeech[index];
                        var key = Tuple.Create(lemma, partOfSpeech);

                        if (SingleTerms.ContainsKey(key) && SingleTerms[key] != null)
                        {
                            return CreateElement(SingleTerms[key], buffer);
                        }
                    }
                }

                // if text in the tag spans across one or more elements, we ignore the tag
                return CreateTextNode(buffer);
            }
            else
            {
                // create a new term tag
                return CreateElement(openTag, buffer);
            }
        }

        /// <summary>
        /// Creates a new element to add to the document.
        /// </summary>
        /// <param name="tag">Name and attributes of the new element.</param>
        /// <param name="content">Content of the element.</param>
        /// <returns>New element.</returns>
        protected virtual XNode CreateElement(XElement tag, string content)
        {
            Tename tename = new Tename(tag);
            tename.Add(content);
            return tename;
        }

        /// <summary>
        /// Creates a new text element.
        /// </summary>
        /// <param name="text">Content of the element.</param>
        /// <returns>New node with text.</returns>
        protected virtual XNode CreateTextNode(string text)
        {
            return new XText(text); // encodes entities automatically
        }

        /// <summary>
        /// Gets the text of a text node.
        /// </summary>
        /// <param name="node">Node whose text to get.</param>
        /// <returns>The content of the node as text.</returns>
        protected virtual string NodeText(XText node)
        {
            // decode the entities since the text used in joining is not escaped
            return HttpUtility.HtmlDecode(node.ToString());
        }

        /// <summary>
        /// Checks if it's a known new tag name.
        /// </summary>
        /// <param name="tag">Tag name to check.</param>
        /// <returns>Whether it's a known new tag name.</returns>
        protected virtual bool IsNewTagName(string tag)
        {
            return tag == "tename";
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Finds all possible values that the element and its descendants have.
        /// </summary>
        /// <param name="element">Element to start with.</param>
        /// <param name="valuesForElement">Function that returns values for an element.</param>
        /// <returns>All possible unique values, including <see langword="null"/>.</returns>
        protected virtual IEnumerable<string> AllValues(XElement element, Func<XElement, IEnumerable<string>> valuesForElement)
        {
            HashSet<string> values = new HashSet<string>();

            values.Add(null);

            foreach (var s in element.DescendantsAndSelf().Select(e => valuesForElement(e)).Where(s => s != null))
                foreach (var s2 in s)
                    values.Add(s2);

            return values;
        }
        #endregion

        #region Chunk
        /// <summary>
        /// Group that is identified by a language (case-insensitive, two letters) and a domain.
        /// </summary>
        public class LanguageDomainGroup : IEquatable<LanguageDomainGroup>
        {
            string domain;
            string language;

            /// <summary>
            /// Creates a new instance of this class.
            /// </summary>
            /// <param name="language">Language.</param>
            /// <param name="domain">Domain.</param>
            public LanguageDomainGroup(string language = null, string domain = null)
            {
                Language = language;
                Domain = domain;
            }

            /// <summary>
            /// Gets or sets the domain.
            /// </summary>
            public string Domain
            {
                get { return domain; }
                set { domain = value; }
            }

            /// <summary>
            /// Gets or sets the language.
            /// Language is case-insensitive and only the first two letters are used.
            /// </summary>
            public string Language
            {
                get { return language; }
                set { language = value == null ? null : value.ToLowerInvariant(); }
            }

            /// <inheritdoc/>
            public override bool Equals(object obj)
            {
                return Equals(obj as LanguageDomainGroup);
            }

            /// <summary>
            /// Checks if two objects have equal values.
            /// </summary>
            /// <param name="other">The other object to compare.</param>
            /// <returns>Whether both instances have equal values.</returns>
            public bool Equals(LanguageDomainGroup other)
            {
                return other != null
                    && other.Language == Language
                    && other.Domain == Domain;
            }

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                return (Language != null ? Language.GetHashCode() : 1)
                     ^ (Domain != null ? Domain.GetHashCode() : 1);
            }
        }

        /// <summary>
        /// Gropued collection of text flows for elements.
        /// </summary>
        public class ChunkCollection : Dictionary<LanguageDomainGroup, Dictionary<XElement, string[]>>
        {
            /// <summary>
            /// Gets the text flow for a group.
            /// </summary>
            /// <param name="group">Group to get the text flow for.</param>
            /// <returns>Paragraphs that contain broken text.</returns>
            public string[][] Export(LanguageDomainGroup group)
            {
                if (!this.ContainsKey(group))
                    return new[] { new string[0] };

                return this[group].Values.Select(s => s.ToArray()).ToArray();
            }

            /// <summary>
            /// Replaces the text flow with annotated text flow for a group.
            /// </summary>
            /// <param name="group">Group of the text flow.</param>
            /// <param name="text">Annotated text.</param>
            public void Import(LanguageDomainGroup group, string[][] text)
            {
                if (!this.ContainsKey(group))
                    return;

                for (int i = 0; i < text.Length; i++)
                {
                    var key = this[group].ElementAt(i).Key;
                    this[group][key] = text[i];
                }
            }

            /// <summary>
            /// Removes groups that have no text flows.
            /// </summary>
            public void RemoveEmpty()
            {
                foreach (var key in this.Keys.ToArray())
                    if (this[key].Count == 0)
                        this.Remove(key);
            }
        }
        #endregion
    }
}