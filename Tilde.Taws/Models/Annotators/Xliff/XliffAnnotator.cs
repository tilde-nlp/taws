using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Tilde.Its;

namespace Tilde.Taws.Models
{
    /// <summary>
    /// Finds terms in an XLIFF document and annotates them.
    /// </summary>
    public class XliffAnnotator : Annotator<ItsXmlDocument>
    {
        /// <summary>
        /// XLIFF Version 1.2 namespace.
        /// </summary>
        public static readonly XNamespace XliffNamespace12 = XNamespace.Get("urn:oasis:names:tc:xliff:document:1.2");
        /// <summary>
        /// XLIFF Version 1.1 namespace.
        /// </summary>
        public static readonly XNamespace XliffNamespace11 = XNamespace.Get("urn:oasis:names:tc:xliff:document:1.1");
        /// <summary>
        /// XLIFF 1.2 to ITS 2.0 mapping namespace.
        /// </summary>
        public static readonly XNamespace ItsxNamespace = XNamespace.Get("http://www.w3.org/ns/its-xliff/");

        /// <summary>
        /// XLIFF 1.0/1.1/1.2 to ITS 2.0 mapping.
        /// <see href="http://www.w3.org/International/its/wiki/XLIFF_Mapping"/>
        /// </summary>
        private static readonly string XliffMapping = @"
            <its:rules version='2.0'
                    xmlns:its='" + ItsNamespace + @"'
                    xmlns:itsx='" + ItsxNamespace + @"'
                    xmlns:xliff11='" + XliffNamespace11 + @"'
                    xmlns:xliff12='" + XliffNamespace12 + @"'>

                <its:termRule selector='/xliff//mrk[@mtype=""term""]' term=""yes"" />
                <its:termRule selector='//xliff11:mrk[@mtype=""term""]' term=""yes"" />
                <its:termRule selector='//xliff12:mrk[@mtype=""term""]' term=""yes"" />
    
                <its:termRule selector='/xliff//mrk[@mtype=""x-its-term-no""]' term=""no"" />
                <its:termRule selector='//xliff11:mrk[@mtype=""x-its-term-no""]' term=""no"" />
                <its:termRule selector='//xliff12:mrk[@mtype=""x-its-term-no""]' term=""no"" />
    
                <its:domainRule selector='//*' domainPointer='@itsx:domains'/>

                <its:withinTextRule withinText='yes' selector='/xliff//g | /xliff//x | /xliff//bx | /xliff//ex | /xliff//bpt | /xliff//ept | /xliff//it | /xliff//ph | /xliff//mrk'/>
                <its:withinTextRule withinText='yes' selector='//xliff11:g | //xliff11:x | //xliff11:bx | //xliff11:ex | //xliff11:bpt | //xliff11:ept | //xliff11:it | //xliff11:ph | //xliff11:mrk'/>
                <its:withinTextRule withinText='yes' selector='//xliff12:g | //xliff12:x | //xliff12:bx | //xliff12:ex | //xliff12:bpt | //xliff12:ept | //xliff12:it | //xliff12:ph | //xliff12:mrk'/>

                <its:withinTextRule withinText='nested' selector='/xliff//sub'/>
                <its:withinTextRule withinText='nested' selector='//xliff11:sub'/>
                <its:withinTextRule withinText='nested' selector='//xliff12:sub'/>

           </its:rules>";
        
        /// <summary>
        /// Creates a new instance of the annotator.
        /// </summary>
        /// <param name="source">XLIFF doucument to annotate.</param>
        public XliffAnnotator(ApiDocument source)
            : base(source)
        {
            ValidateXliffDocument();
        }

        /// <inheritdoc/>
        protected override void OnBeforeAnnotate()
        {
            ItsDocument.Rules.Insert(0, XElement.Parse(XliffMapping));
        }

        /// <summary>
        /// Finds and annotates terms in the document.
        /// </summary>
        /// <returns>XLIFF document with annotated terms.</returns>
        public async Task<string> Annotate()
        {
            try
            {
                FixXmlLang();
                AddDefaultValues(Root);

                XliffChunker chunker = new XliffChunker(ItsDocument);
                Chunker.ChunkCollection chunks = chunker.Split();
                await Annotate(chunks);
                chunker.Join(chunks);

                MergeTenames();
                RemoveInvalidTenames();

                AddItsNamespaceDeclaration(Root);
                AddNamespaceDeclaration(Root, ItsxNamespace, "itsx");
                if (!ContainsOtherTerms(Root) && !ContainsOtherTerminologyAnnotatorsRefs(Root))
                    AddAnnotatorsRef(Root);
                AddTermInfoRefs();
                ReplaceTenameTags();

                return ToXml();
            }
            catch (Exception e)
            {
                throw new AnnotatorException(e);
            }
        }

        /// <summary>
        /// Checks if the XML document is an XLIFF document.
        /// Throws if there are errors.
        /// </summary>
        private void ValidateXliffDocument()
        {
            // root is not <xliff>
            if (Root.Name.LocalName != "xliff")
                throw new ArgumentException("Document has an invalid root element.");

            // missing namespace declaration in <xliff>
            if (Namespace != XliffNamespace12 && Namespace != XliffNamespace11)
            {
                if (Document.DocumentType != null &&
                    Document.DocumentType.PublicId == "-//XLIFF//DTD XLIFF//EN" &&
                    Document.DocumentType.SystemId == "http://www.oasis-open.org/committees/xliff/documents/xliff.dtd")
                {
                    // xliff 1.0
                }
                else
                {
                    throw new ArgumentException("Document is not in the XLIFF namespace.");
                }
            }

            // don't check version attribute
            // since namespaces already differentiate versions
        }

        /// <summary>
        /// Removes xml:lang elements (by overriding the Language Information data category value) from elements that
        /// aren't allowed to have this attribute.
        /// 
        /// <para>
        /// There are a number of elements in the XLIFF specification that are allowed to have an xml:lang attribute. 
        /// But what should we do when the xml:lang is on another element?
        /// If the document is parsed as an XLIFF document, it should be ignored because it is not in the spec.
        /// If the document is parsed as an ITS document, it should be taken into account because xml:lang is local markup in the Language Information data category.
        /// It was decided to treat the document as an XLIFF document first and foremost and that's why we remove xml:lang attributes that are not mentioned in the spec.
        /// </para>
        /// </summary>
        private void FixXmlLang()
        {
            // http://docs.oasis-open.org/xliff/v1.2/os/xliff-core.html#xml_lang
            string[] allowed = { "xliff", "note", "prop", "source", "target", "alt-trans" };

            // elements that are not in the whitelist with the xml:lang attribute
            IEnumerable<XElement> elementsWithXmlLang = from element in Document.Descendants()
                                                        where element.Attribute(XNamespace.Xml + "lang") != null
                                                        where !allowed.Any(name => Namespace + name == element.Name)
                                                        where !element.Ancestors().Any(e => XliffChunker.ContentElements.Any(name => Namespace + name == e.Name))
                                                        select element;

            foreach (XElement element in elementsWithXmlLang)
            {
                element.Annotation<LanguageInformationDataCategory>().Language = null;
            }
        }

        /// <summary>
        /// Replaces term tags from TaaS (also known as tename's) with the appropriate XLIFF tags.
        /// </summary>
        private void ReplaceTenameTags()
        {
            foreach (Tename tename in Document.Descendants().Where(IsTename))
            {
                // <mrk mtype="term">...</mrk>
                tename.Name = Namespace + "mrk";

                tename.Add(new XAttribute("mtype", "term"));
                
                if (tename.HasConfidence)
                    tename.Add(new XAttribute(ItsxNamespace + "termConfidence", tename.Confidence));

                // make sure there is an annotatorsRef
                TenameLocalAnnotatorsRef(tename);
            }
        }

        /// <summary>
        /// Adds TBXs of the terms to each file's header.
        /// </summary>
        private void AddTermInfoRefs()
        {
            // tename => header
            Dictionary<XElement, XElement> tenameContainer = new Dictionary<XElement, XElement>();
            // header => termEntryTbx[]
            Dictionary<XElement, List<XDocument>> tbxByContainer = new Dictionary<XElement, List<XDocument>>();

            // traverse all tenames
            // and group them by <file>
            // and then put them in one TBX document
            // instead of creating a TBX document for every single term
            AddTermInfoRefs(
                container: (tename) =>
                {
                    // TBX goes into the header element
                    // create it if it doesn't already exist
                    XElement file = tename.Ancestors(Namespace + "file").Last();
                    XElement header = file.Element(Namespace + "header");
                    if (header == null)
                    {
                        header = new XElement(Namespace + "header");
                        file.AddFirst(header);
                    }

                    tenameContainer[tename] = header;
                    if (!tbxByContainer.ContainsKey(header))
                        tbxByContainer[header] = new List<XDocument>();

                    return header;
                },
                tbxElement: (tename, id, tbx) =>
                {
                    foreach (XElement termEntry in tbx.Descendants("termEntry"))
                    {
                        // add an xml:id so that terms can reference it
                        termEntry.Add(new XAttribute(XNamespace.Xml + "id", id));
                    }
                    tbxByContainer[tenameContainer[tename]].Add(tbx);
                    return null;
                });

            foreach (XElement header in tbxByContainer.Keys)
            {
                XDocument tbx = MergeTbxDocuments(tbxByContainer[header]);
                if (tbx != null && tbx.Root.Name.Namespace == "")
                    tbx.Root.SetDefaultNamespace(tbx.DocumentType.SystemId);
                header.Add(tbx.Root);
            }
        }

        private XDocument MergeTbxDocuments(IEnumerable<XDocument> tbxDocuments)
        {
            if (tbxDocuments.Count() == 0)
                return null;

            XDocument tbx = new XDocument(tbxDocuments.First());
            tbx.Root.Element("text").Element("body").Elements("termEntry").Remove();

            foreach (XDocument tbxDoc in tbxDocuments)
                tbx.Root.Element("text").Element("body").Add(tbxDoc.Descendants("termEntry"));

            return tbx;
        }

        /// <inheritdoc/>
        protected override XName AnnotatorsRefAttributeName
        {
            get { return ItsNamespace + "annotatorsRef"; }
        }

        /// <inheritdoc/>
        protected override XName TermInfoRefAttributeName
        {
            get { return ItsxNamespace + "termInfoRef"; }
        }

        /// <summary>
        /// Converts the document to an XML string.
        /// Can't use .ToString() because it doesn't inclue the xml declaration.
        /// </summary>
        /// <returns>XML string.</returns>
        private string ToXml()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter strw = new StreamWriter(stream, Encoding.UTF8))
                {
                    Document.Save(strw);
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
        }
    }
}