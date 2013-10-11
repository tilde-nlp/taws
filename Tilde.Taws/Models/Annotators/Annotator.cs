using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

using Tilde.Its;

namespace Tilde.Taws.Models
{
    /// <summary>
    /// Annotator is responsible for creating a <see cref="Chunker"/> for extracting
    /// text from an ITS annotated document, sending that text to the annotation web service,
    /// adding the annotated terms and also formatting the document to make it adhere to the ITS standard.
    /// 
    /// The document is parsed and ITS annotations loaded in the constructor. 
    /// Every method of this class may modify the parsed document.
    /// An instance of this class can annotate only once.
    /// </summary>
    /// <typeparam name="T">ItsDocument type.</typeparam>
    public abstract class Annotator<T> where T : ItsDocument
    {
        /// <summary>
        /// Vendor its:annotatorsRef attribute value.
        /// </summary>
        public const string AnnotatorsRef = "terminology|http://tilde.com/term-annotation-service";
        /// <summary>
        /// Vendor prefix for term IDs (used in its:termInfoRef).
        /// </summary>
        public const string TermInfoRefPrefix = "tilde-tbx-";
        /// <summary>
        /// ITS 2.0 namespace.
        /// </summary>
        public static readonly XNamespace ItsNamespace = Tilde.Its.ItsDocument.ItsNamespace;
        /// <summary>
        /// Identifies the TWSC tool that finds terms statistically.
        /// </summary>
        private const string StatisticalExtractorUri = "http://taas.eurotermbank.com/extractor";

        /// <summary>
        /// Annotation web service.
        /// </summary>
        protected TaaS taas;
        /// <summary>
        /// Original document.
        /// </summary>
        private ApiDocument source;
        /// <summary>
        /// ITS annotated document.
        /// </summary>
        private T document;

        /// <summary>
        /// Creates a new instance of the this class.
        /// </summary>
        /// <param name="source">Document to parse and annotate.</param>
        protected Annotator(ApiDocument source)
        {
            if (source == null)
                throw new ArgumentNullException("source", "No document.");
            if (source.Content == null)
                throw new ArgumentNullException("source.Content", "Document content is empty.");

            this.source = source;
            try
            {
                this.document = (T)Activator.CreateInstance(typeof(T), RemoveControlCharacters(source.Content), source.BaseUri);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
            OnBeforeAnnotate();
            this.document.Annotate(this.document.Document.Descendants(), new[] { 
                typeof(LanguageInformationDataCategory), 
                typeof(DomainDataCategory), 
                typeof(ElementsWithinTextDataCategory), 
                typeof(TerminologyDataCategory), 
                typeof(LocaleFilterDataCategory),
                typeof(AnnotatorAnnotation),
                typeof(IdValueDataCategory)
            });
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~Annotator()
        {
            CachedQueryLanguage.ClearCache();
        }

        /// <summary>
        /// Called before annotating the document.
        /// </summary>
        protected virtual void OnBeforeAnnotate()
        {
        }

        #region Shortcut properties
        /// <summary>
        /// ITS annotated document.
        /// </summary>
        protected T ItsDocument
        {
            get { return document; }
        }

        /// <summary>
        /// The XML document of the ITS annotated document.
        /// </summary>
        protected XDocument Document
        {
            get { return document.Document; }
        }

        /// <summary>
        /// Document root element.
        /// </summary>
        protected XElement Root
        {
            get { return Document.Root; }
        }

        /// <summary>
        /// Document namespace.
        /// </summary>
        protected XNamespace Namespace
        {
            get { return Root.Name.Namespace; }
        }
        #endregion

        #region Annotation and TaaS
        /// <summary>
        /// Annotates chunked text.
        /// Annotation is done asynchronously and in parallel.
        /// </summary>
        /// <param name="chunks">Chunked text.</param>
        /// <returns>Task which signals completion.</returns>
        protected virtual async Task Annotate(Chunker.ChunkCollection chunks)
        {
            taas = new TaaS();
            taas.UseStatisticalExtraction = source.UseStatisticalExtraction;
            taas.UseTermBankExtraction = source.UseTermBankExtraction;

            try
            {
                // using System.Threading.Tasks.Dataflow for async & parallel at the same time
                var block = new ActionBlock<Chunker.LanguageDomainGroup>(async group => await Annotate(chunks, group), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded });

                foreach (var kp in chunks)
                    block.Post(kp.Key);

                block.Complete();

                await block.Completion;
            }
            catch (Exception e)
            {
                throw new Exception("Annotation service error", e);
            }
        }

        /// <summary>
        /// Annotates a single chunk.
        /// </summary>
        /// <param name="chunks">All chunks.</param>
        /// <param name="group">Group that identifies one chunk.</param>
        /// <returns>Task which signals completion.</returns>
        protected virtual async Task Annotate(Chunker.ChunkCollection chunks, Chunker.LanguageDomainGroup group)
        {
            string[][] text = chunks.Export(group);

            string[][] result = await taas.Annotate(group.Language, group.Domain, text);

            chunks.Import(group, result);
        }

        /// <summary>
        /// Removes all control characters from the text.
        /// </summary>
        /// <param name="text">Text in UTF8.</param>
        /// <returns>Text without control characters.</returns>
        protected string RemoveControlCharacters(string text)
        {
            if (text == null)
                return null;

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            List<byte> result = new List<byte>();

            for (int i = 0; i < bytes.Length; i++)
            {
                // all ASCII symbols below 32 except for 10 (LF - Line Feed) and 13 (CR - Carriage Return)
                if (bytes[i] < 32 && bytes[i] != 10 && bytes[i] != 13)
                    continue;
                // utf8 control chars between C2 80 and C2 9F
                if (bytes[i] == 0xC2 && i + 1 < bytes.Length && bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0x9F)
                {
                    i++;
                    continue;
                }

                result.Add(bytes[i]);
            }

            return Encoding.UTF8.GetString(result.ToArray());
        }
        #endregion

        #region ITS
        /// <summary>
        /// Sets the language and domains by overriding the annotation value
        /// if the element didn't already have them set (explicitly or from inheritance).
        /// It uses the values defined in the <see cref="source"/> document.
        /// </summary>
        /// <param name="element">Element to use.</param>
        protected virtual void AddDefaultValues(XElement element)
        {
            // add default language
            if (source.Language != null)
                if (element.Annotation<LanguageInformationDataCategory>().Language == null)
                    element.Annotation<LanguageInformationDataCategory>().Language = source.Language;

            // add default domains
            if (source.Domains != null)
                if (element.Annotation<DomainDataCategory>().Value == null)
                    element.Annotation<DomainDataCategory>().Value = string.Join(DomainDataCategory.Separator.ToString(), source.Domains);
        }

        /// <summary>
        /// Adds the ITS 2.0 namespace delcaration to the element if there is none.
        /// It uses the default "its" prefix if it's available or "its2", "its3", etc. otherwise.
        /// </summary>
        /// <param name="element">Element to which to add the namespace declaration.</param>
        protected void AddItsNamespaceDeclaration(XElement element)
        {
            AddNamespaceDeclaration(element, ItsNamespace, "its");
        }

        /// <summary>
        /// Adds a namespace delcaration to an element.
        /// </summary>
        /// <param name="element">Element to which to add the namespace declaration.</param>
        /// <param name="ns">Namespace.</param>
        /// <param name="prefix">Prefix for the namespace.</param>
        protected void AddNamespaceDeclaration(XElement element, XNamespace ns, string prefix)
        {
            if (!element.Attributes().Any(a => a.IsNamespaceDeclaration && a.Value == ns))
            {
                string currentPrefix = prefix;
                int i = 2;

                while (element.Attributes().Any(a => a.IsNamespaceDeclaration && a.Name == XNamespace.Xmlns + currentPrefix))
                {
                    currentPrefix = prefix + i;
                    i++;
                }

                element.Add(new XAttribute(XNamespace.Xmlns + currentPrefix, ns));
            }
        }

        /// <summary>
        /// Whether the element or its descendants contain a term that was not annotated by us.
        /// </summary>
        /// <param name="element">Element to start with.</param>
        /// <returns>Whether there are other terms.</returns>
        protected bool ContainsOtherTerms(XElement element)
        {
            return element.DescendantsAndSelf().Any(e => (IsTerm(e) || IsNoTerm(e)) && !IsTename(e));
        }

        /// <summary>
        /// Whether the element or its descendants contain an annotatorsRef 
        /// for the Terminology data category that is not ours.
        /// </summary>
        /// <param name="element">Element to start with.</param>
        /// <returns>Whether there are other annotatorsRefs present.</returns>
        protected bool ContainsOtherTerminologyAnnotatorsRefs(XElement element)
        {
            return element.DescendantsAndSelf().Any(e => {
                if (e.Annotation<AnnotatorAnnotation>() == null) return false;
                AnnotatorsRef annRef = e.Annotation<AnnotatorAnnotation>()["terminology"];
                return annRef != null && annRef.ToString() != AnnotatorsRef;
            });
        }

        /// <summary>
        /// Adds its:annotatorsRef attribute to the element.
        /// </summary>
        /// <param name="element">Element to which to add.</param>
        protected void AddAnnotatorsRef(XElement element)
        {
            // create the attribute if it doesn't exist
            XAttribute attr = element.Attribute(AnnotatorsRefAttributeName);
            if (attr == null)
            {
                attr = new XAttribute(AnnotatorsRefAttributeName, AnnotatorsRef);
                element.Add(attr);
                return;
            }

            // add it at the end if it's not already there
            string[] refs = attr.Value.Split(' ');
            if (!refs.Contains(AnnotatorsRef))
                attr.Value += " " + AnnotatorsRef;
        }

        /// <summary>
        /// Name of the annotatorsRef attribute.
        /// </summary>
        protected abstract XName AnnotatorsRefAttributeName { get; }

        /// <summary>
        /// Local attribute name for each term tag that will point to the term information.
        /// </summary>
        protected abstract XName TermInfoRefAttributeName { get; }

        /// <summary>
        /// Adds its:annotatorsRef attribute to the term element
        /// if the element doesn't already inherit it from its ancestors.
        /// </summary>
        /// <param name="tename">Element to which to add the attribute.</param>
        protected virtual void TenameLocalAnnotatorsRef(XElement tename)
        {
            AnnotatorAnnotation annotators = new AnnotatorAnnotation(document, tename);
            if (!annotators.Any(a => a.ToString() == AnnotatorsRef))
                AddAnnotatorsRef(tename);
        }
        #endregion

        #region Terminology
        /// <summary>
        /// Merges nested term tags into one.
        /// If a word was tagged as a term in various domains, it will be enclosed in several term tags.
        /// Such markup is invalid in ITS because the Terminology data category doesn't support inheritance.
        /// This method finds all nested term tags and merges the attributes.
        /// </summary>
        protected virtual void MergeTenames()
        {
            // find all terms, traversing them bottom-to-top
            foreach (Tename tename in document.Document.Descendants().Where(IsTename).Reverse().ToArray())
            {
                // term tag contains another term tag
                // only and only if it contains one child element and no text nodes
                // and the child element is also a term tag
                if (tename.Elements().Count() != 1 || !tename.IsEmptyValue() || !IsTename(tename.Elements().First()))
                    continue;

                Tename child = tename.Elements().First() as Tename;

                // merge confidence attributes by averaging the values
                if (tename.HasConfidence)
                {
                    if (child.HasConfidence)
                    {
                        child.Confidence = (tename.Confidence + child.Confidence) / 2;
                    }
                    else
                    {
                        child.Confidence = tename.Confidence;
                    }
                }

                // merge term ids
                if (tename.TermID != null)
                {
                    child.TermID = child.TermID + Tename.IdSeparator + tename.TermID;
                }

                tename.ReplaceWith(new Tename(child));
            }
        }

        /// <summary>
        /// Removes term tags that are not invalid (e.g. terms that are inside no-terms).
        /// </summary>
        protected virtual void RemoveInvalidTenames()
        {
            // removes term tags that contain something else besides text 
            // because terms can't span other tags because the Terminology data category doesn't support inheritance
            foreach (Tename tename in document.Document.Descendants().Where(IsTename).ToArray())
            {
                if (tename.Nodes().Any(n => !(n is XText)))
                {
                    tename.ReplaceWith(tename.Nodes());
                }
            }

            // replace term tags that are inside another term tags
            // they are invalid because the Terminology data category doesn't support inheritance
            foreach (Tename tename in document.Document.Descendants().Where(IsTename).ToArray())
            {
                if (tename.Ancestors().Any(e => IsTerm(e) || IsNoTerm(e)))
                {
                    tename.ReplaceWith(tename.Nodes());
                }
            }

            RemoveInvalidTenamesForSelectedMethod();
        }

        /// <summary>
        /// Removes extra annotations.
        /// </summary>
        protected virtual void RemoveInvalidTenamesForSelectedMethod()
        {
            // method0: remove all tenames
            if (!source.UseStatisticalExtraction && !source.UseTermBankExtraction)
            {
                foreach (Tename tename in document.Document.Descendants().Where(IsTename).ToArray())
                {
                    tename.ReplaceWith(tename.Nodes());
                }
            }
            // method1: no tenames without termID
            if (source.UseStatisticalExtraction && !source.UseTermBankExtraction)
            {
                foreach (Tename tename in document.Document.Descendants().Where(IsTename).ToArray())
                {
                    if (tename.TermID != null)
                        tename.ReplaceWith(tename.Nodes());
                }
            }
            // method2: only tenames with termID
            if (!source.UseStatisticalExtraction && source.UseTermBankExtraction)
            {
                foreach (Tename tename in document.Document.Descendants().Where(IsTename).ToArray())
                {
                    if (tename.TermID == null)
                        tename.ReplaceWith(tename.Nodes());
                }
            }
            // method3: both tenames with and without termID
        }

        /// <summary>
        /// Checks if the element is a term tag from TaaS.
        /// </summary>
        /// <param name="element">Element check.</param>
        /// <returns>Whether the element was tagged by TaaS.</returns>
        protected virtual bool IsTename(XElement element)
        {
            return element is Tename;
        }

        /// <summary>
        /// Checks if the element is marked as a term.
        /// </summary>
        /// <param name="element">Element to check.</param>
        /// <returns>Whether the element is a tag for a term.</returns>
        protected virtual bool IsTerm(XElement element)
        {
            return IsTename(element) ||
                   (element.Annotation<TerminologyDataCategory>() != null &&
                    element.Annotation<TerminologyDataCategory>().IsTerm);
        }

        /// <summary>
        /// Checks if the element is marked as a no-term.
        /// </summary>
        /// <param name="element">Element to check.</param>
        /// <returns>Whether the element is a tag for content that is known not to be a term.</returns>
        protected virtual bool IsNoTerm(XElement element)
        {
            return (element.Annotation<TerminologyDataCategory>() != null &&
                    element.Annotation<TerminologyDataCategory>().IsTerm == false &&
                    element.Annotation<TerminologyDataCategory>().IsAnnotated == true);
        }

        #region TBX
        /// <summary>
        /// Adds term information (called TBX) to the document.
        /// </summary>
        /// <param name="container">Function that returns the element to which to add the information. Parameter is the term.</param>
        /// <param name="tbxElement">Function that generates the term information element. It takes the tename, the ID and the term information as parameters.</param>
        protected virtual void AddTermInfoRefs(Func<XElement, XElement> container, Func<XElement, string, XDocument, XElement> tbxElement)
        {
            // keep a list of used ids to prevent duplicates
            List<string> usedIDs = new List<string>();
            // termID <=> uniqueTermID, in case termID is already used in the document
            Dictionary<string, string> uniqueIDs = new Dictionary<string, string>();
            // all existing ids used in the document
            List<string> existingIDs = document.Document
                .Descendants()
                .Where(e => e.Annotation<IdValueDataCategory>() != null)
                .Select(e => e.Annotation<IdValueDataCategory>().ID)
                .Distinct()
                .ToList();

            // select all terms that have terminology information (or TBX) i.e. those terms with an id
            foreach (Tename tename in document.Document.Descendants().Where(e => IsTename(e) && (e as Tename).TermID != null))
            {
                // each term can have multiple term entries
                // (if the same term was tagged in two different domains, for example)
                // this code gets all of them
                List<XDocument> termEntries = new List<XDocument>();
                string[] ids = tename.TermID.Split(Tename.IdSeparator);
                bool statisticalOnly = false;
                foreach (string id in ids)
                {
                    XDocument termEntry = GetTermEntry(id);
                    if (termEntry != null)
                    {
                        termEntries.Add(new XDocument(termEntry));

                        // previously tename's without termID were the terms tagged by the statistical tool
                        // now, all terms may have a termID
                        // terms that have no translations are statistically extracted
                        // and we don't add a tbx which is how they are identified in the visualization page i.e. no tbx = statistical
                        var sources = termEntry.Descendants("xref").Where(e => e.Attribute("target") != null).Select(e => e.Attribute("target").Value).Distinct().ToList();
                        if (sources.Count == 1 && sources.Single() == StatisticalExtractorUri)
                        {
                            statisticalOnly = true;
                            break;
                        }
                    }
                }

                if (statisticalOnly)
                    continue;

                if (termEntries.Count > 0)
                {
                    string termID = tename.TermID.Replace(Tename.IdSeparator, '-');
                    uniqueIDs[termID] = termID;

                    if (!usedIDs.Contains(termID)) // prevent duplicates
                    {
                        if (existingIDs.Contains(TermInfoRefPrefix + termID)) // termID is already taken, generate a new one
                        {
                            int i = 1;
                            while (existingIDs.Contains(TermInfoRefPrefix + termID + "-" + i))
                                i++;
                            uniqueIDs[termID] = termID + "-" + i;
                        }

                        XElement containerForTename = container(tename);
                        containerForTename.Add(tbxElement(tename, TermInfoRefPrefix + uniqueIDs[termID], CreateTbxDocument(termEntries)));
                        usedIDs.Add(termID);
                    }

                    tename.Add(new XAttribute(TermInfoRefAttributeName, "#" + TermInfoRefPrefix + uniqueIDs[termID]));
                }
            }
        }

        /// <summary>
        /// Uses the TBX format of the first termEntry as the TBX for all entries.
        /// </summary>
        /// <param name="termEntries">Term entries to put in the document.</param>
        /// <returns>TBX document.</returns>
        protected XDocument CreateTbxDocument(IEnumerable<XDocument> termEntries)
        {
            if (termEntries.Count() == 0)
                return null;

            XDocument tbx = new XDocument(termEntries.First());
            tbx.Root.Element("text").Element("body").Add(termEntries.Skip(1));

            return tbx;
        }

        /// <summary>
        /// Returns term information.
        /// </summary>
        /// <param name="id">ID of the term.</param>
        /// <returns>Term information as an XML element.</returns>
        protected virtual XDocument GetTermEntry(string id)
        {
            return taas.GetTermEntry(id);
        }
        #endregion
        #endregion
    }
}