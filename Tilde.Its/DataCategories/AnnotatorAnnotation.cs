using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// ITS tools annotations.
    /// </summary>
    public class AnnotatorAnnotation : Annotation, IEnumerable<AnnotatorsRef>
    {
        /// <summary>
        /// All annotators annotations for the node.
        /// </summary>
        List<AnnotatorsRef> annotatorsRefs = new List<AnnotatorsRef>();

        /// <inheritdoc/>
        public AnnotatorAnnotation(ItsDocument document, XObject node)
            : base(document, node)
        {
            LoadAnnotatorsRefs(ElementOrAttribute(element => element, attribute => attribute.Parent));

            if (annotatorsRefs.Count > 0)
            {
                // remove duplicates, keep inner-most declaration
                foreach (AnnotatorsRef annotatorsRef in annotatorsRefs.ToList())
                {
                    AnnotatorsRef[] categoryAnnotatorsRefs = annotatorsRefs.Where(a => a.DataCategory == annotatorsRef.DataCategory).ToArray();
                    for (int i = 0; i < categoryAnnotatorsRefs.Length - 1; i++)
                        annotatorsRefs.Remove(categoryAnnotatorsRefs[i]);
                }

                // alphabetical sorting
                annotatorsRefs.Sort();
            }
        }

        /// <summary>
        /// All annotators annotations in one line.
        /// </summary>
        public string AnnotatorsRef
        {
            get
            {
                return annotatorsRefs.Count == 0 ? null : string.Join(" ", annotatorsRefs.Select(a => a.ToString()));
            }
        }

        /// <summary>
        /// Annotators annotation for a data category.
        /// </summary>
        /// <param name="category">Data category name.</param>
        /// <returns><see cref="AnnotatorsRef"/> for the data category.</returns>
        public AnnotatorsRef this[string category]
        {
            get
            {
                return annotatorsRefs.Where(a => a.DataCategory == category).FirstOrDefault();
            }
        }

        /// <summary>
        /// Finds all annotators annotations
        /// </summary>
        /// <param name="element"></param>
        private void LoadAnnotatorsRefs(XElement element)
        {
            if (element.Parent != null)
                LoadAnnotatorsRefs(element.Parent);

            XAttribute annotatorsRefAttr = LocalAttribute(element, XmlOrHtmlAttributeName("annotatorsRef"));
            if (annotatorsRefAttr != null)
                foreach (string annotatorRef in annotatorsRefAttr.Value.Split(' '))
                    annotatorsRefs.Add(new AnnotatorsRef(annotatorRef));
        }

        /// <inheritdoc/>
        public IEnumerator<AnnotatorsRef> GetEnumerator()
        {
            return annotatorsRefs.GetEnumerator();
        }

        /// <inheritdoc/>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Annotations of a given data category within the element with information about the processor that generated those data category annotations.
    /// <see href="http://www.w3.org/TR/its20/#its-tool-annotation"/>
    /// </summary>
    public class AnnotatorsRef : IEquatable<AnnotatorsRef>, IComparable<AnnotatorsRef>
    {
        /// <summary>Delimiter that used to separate the data category name from the IRI.</summary>
        public const char Delimiter = '|';

        /// <summary>
        /// Creates an empty instance.
        /// </summary>
        public AnnotatorsRef()
        {
        }

        /// <summary>
        /// Creates a new instance from a string.
        /// </summary>
        /// <param name="annotatorsRef">Data category with the IRI.</param>
        public AnnotatorsRef(string annotatorsRef)
        {
            string[] split = annotatorsRef.Split(Delimiter);
            DataCategory = split[0];
            Iri = split[1];
        }

        /// <summary>
        /// Data category name.
        /// </summary>
        public string DataCategory
        {
            get;
            set;
        }

        /// <summary>
        /// IRI that identifies the tool.
        /// </summary>
        public string Iri
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return DataCategory + Delimiter + Iri;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as AnnotatorsRef);
        }

        /// <inheritdoc/>
        public bool Equals(AnnotatorsRef other)
        {
            return DataCategory == other.DataCategory &&
                   Iri == other.Iri;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (DataCategory != null ? DataCategory.GetHashCode() : 1) ^ (Iri != null ? Iri.GetHashCode() : 1);
        }

        /// <inheritdoc/>
        public int CompareTo(AnnotatorsRef other)
        {
            int cmp = string.Compare(DataCategory ?? "", other.DataCategory ?? "");

            if (cmp == 0)
            {
                cmp = string.Compare(Iri ?? "", other.Iri ?? "");
            }

            return cmp;
        }
    }
}
