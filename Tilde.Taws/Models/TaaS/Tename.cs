using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Tilde.Taws.Models
{
    /// <summary>
    /// An annotated term in an XML document.
    /// Annotated terms in text are tagged as ... &lt;TENAME termID="etb-1" SCORE="1.0"&gt;term&lt;/TENAME&gt;.
    /// The original attributes are preserved and can be accessed through properties
    /// but the XML element will have its own set of attributes (i.e. XML will be rendered as &lt;tename&gt;term&lt;/tename&gt;).
    /// </summary>
    public class Tename : XElement
    {
        /// <summary>
        /// Character that separates multiple IDs.
        /// </summary>
        public const char IdSeparator = ',';

        /// <summary>
        /// Original attributes.
        /// </summary>
        List<XAttribute> attributes;

        /// <summary>
        /// Clones an existing <see cref="Tename"/>.
        /// </summary>
        /// <param name="tename"><see cref="Tename"/> to clone.</param>
        public Tename(Tename tename)
            : base(tename)
        {
            attributes = tename.attributes;
        }

        /// <summary>
        /// Creates a term annotation from a &lt;TENAME&gt; tag.
        /// </summary>
        /// <param name="element">&lt;TENAME&gt; annotation.</param>
        public Tename(XElement element)
            : base(element)
        {
            // preserve original attributes
            attributes = Attributes().ToList();
            // clear XML attributes
            Attributes().Remove();
        }

        /// <summary>
        /// Associated term ID or multiple IDs (separated by <see cref="IdSeparator"/>).
        /// </summary>
        public string TermID
        {
            get
            {
                XAttribute attribute = attributes.SingleOrDefault(a => a.Name == "termid");
                if (attribute != null)
                    return attribute.Value;
                return null;
            }
            set
            {
                XAttribute attribute = attributes.SingleOrDefault(a => a.Name == "termid");
                if (attribute == null)
                {
                    attribute = new XAttribute("termid", null);
                    attributes.Add(attribute);
                }
                attribute.Value = value;
            }
        }

        /// <summary>
        /// Whether term confidence is available.
        /// </summary>
        public bool HasConfidence
        {
            get
            {
                // terms from the database i.e. those that have an ID
                // their SCORE is not confidence
                if (TermID != null)
                    return false;

                return attributes.Any(a => a.Name == "score");
            }
        }

        /// <summary>
        /// Confidence that the term has been identified correctly.
        /// Normalizes the score so that it is in the interval [0,1].
        /// </summary>
        public double Confidence
        {
            get
            {
                XAttribute scoreAttr = attributes.Single(a => a.Name == "score");
                try
                {
                    double score = double.Parse(scoreAttr.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
                    if (score > 1) score = 1;
                    if (score < 0) score = 0;
                    return score;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            set
            {
                XAttribute score = attributes.SingleOrDefault(a => a.Name == "score");
                if (score == null)
                {
                    score = new XAttribute("score", null);
                    attributes.Add(score);
                }
                score.Value = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Lemma(s) of all words in the term.
        /// </summary>
        public string Lemma
        {
            get
            {
                XAttribute attribute = attributes.SingleOrDefault(a => a.Name == "lemma");
                if (attribute != null)
                    return attribute.Value;
                return null;
            }
        }

        /// <summary>
        /// Lemmas for each word in the term.
        /// </summary>
        public string[] Lemmas
        {
            get
            {
                string lemma = Lemma;
                if (lemma == null)
                    return null;
                return lemma.Split(' ');
            }
        }

        /// <summary>
        /// Part of Speech of all words in the term.
        /// </summary>
        public string PartOfSpeech
        {
            get
            {
                XAttribute attribute = attributes.SingleOrDefault(a => a.Name == "msd");
                if (attribute != null)
                    return attribute.Value;
                return null;
            }
        }

        /// <summary>
        /// Parts of Speech for each word in the term.
        /// </summary>
        public string[] PartsOfSpeech
        {
            get
            {
                string pos = PartOfSpeech;
                if (pos == null)
                    return null;
                return pos.Split(' ');
            }
        }
    }
}