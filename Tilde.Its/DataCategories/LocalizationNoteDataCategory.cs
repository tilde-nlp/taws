using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// The Localization Note data category is used to communicate notes to localizers about a particular item of content.
    /// <see href="http://www.w3.org/TR/its20/#locNote-datacat"/>
    /// </summary>
    public class LocalizationNoteDataCategory : DataCategory<LocalizationNote>
    {
        /// <inheritdoc/>
        public LocalizationNoteDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>
        /// Note to localizers about a particular item of content.
        /// </summary>
        public LocalizationNote Note
        {
            get { return Value; }
            set { Value = value; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleName
        {
            get { return "locNoteRule"; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleAttributeName
        {
            get { return "locNoteType"; }
        }

        /// <inheritdoc/>
        protected override LocalizationNote ValueForElement(XElement element)
        {
            LocalizationNote localValue;
            if (LocalValue(element, null, out localValue))
                return localValue;

            LocalizationNote globalValue;
            if (GlobalValue(element, out globalValue))
                return globalValue;

            return DefaultValue(element);
        }

        /// <inheritdoc/>
        /// <param name="noteAttr">Global rule attribute that holds the value.</param>
        /// <param name="note">Returned value. Undefined if the method returned <see langword="false"/>.</param>
        protected override bool GlobalValue(XObject node, XAttribute noteAttr, GlobalRule rule, out LocalizationNote note)
        {
            XElement element = ElementOrAttribute(e => e, a => a.Parent);

            note = new LocalizationNote();

            XAttribute locNoteTypeAttr = rule.RuleElement.Attribute("locNoteType");
            if (locNoteTypeAttr != null)
                note.Type = (LocalizationNoteType)Enum.Parse(typeof(LocalizationNoteType), NormalizeValue(noteAttr.Value), true);
            else
                return false;

            // Exactly one of the following:
            XElement locNote = rule.RuleElement.Element(ItsDocument.ItsNamespace + "locNote");
            XAttribute locNotePointerAttr = rule.RuleElement.Attribute("locNotePointer");
            XAttribute locNoteRefAttr = rule.RuleElement.Attribute("locNoteRef");
            XAttribute locNoteRefPointerAttr = rule.RuleElement.Attribute("locNoteRefPointer");

            if (locNote != null)
                note.Note = locNote.Value;
            else if (locNotePointerAttr != null)
                note.Note = rule.QueryLanguage.SelectPointerValues(element, locNotePointerAttr.Value).FirstOrDefault();
            else if (locNoteRefAttr != null)
                note.NoteRef = locNoteRefAttr.Value;
            else if (locNoteRefPointerAttr != null)
                note.NoteRef = rule.QueryLanguage.SelectPointerValues(element, locNoteRefPointerAttr.Value).FirstOrDefault();
            else
                return false;

            return true;
        }

        /// <inheritdoc/>
        /// <param name="note">Returned value. Undefined if the method returned <see langword="false"/>.</param>
        protected override bool LocalValue(XElement element, XAttribute attribute, out LocalizationNote note)
        {
            note = new LocalizationNote();

            if (element.Parent != null)
                LocalValue(element.Parent, null, out note);

            // An optional locNoteType attribute with the value "description" or "alert". 
            // If the locNoteType attribute is not present, the type of localization note will be assumed to be "description".
            note.Type = LocalizationNoteType.Description;
            XAttribute typeAttr = LocalAttribute(element, XmlOrHtmlAttributeName("locNoteType"));
            if (typeAttr != null)
                note.Type = (LocalizationNoteType)Enum.Parse(typeof(LocalizationNoteType), NormalizeValue(typeAttr.Value), true);

            // A locNote attribute that contains the note itself.
            XAttribute locNoteAttr = LocalAttribute(element, XmlOrHtmlAttributeName("locNote"));
            // A locNoteRef attribute that contains an IRI referring to the location of the localization note.
            XAttribute locNoteRefAttr = LocalAttribute(element, XmlOrHtmlAttributeName("locNoteRef"));

            // One of the following:
            if (locNoteAttr != null)
            {
                note.Note = locNoteAttr.Value;
                note.NoteRef = null;
            }
            else if (locNoteRefAttr != null)
            {
                note.NoteRef = locNoteRefAttr.Value;
                note.Note = null;
            }

            return note.Note != null || note.NoteRef != null;
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            // handled in LocalValue
            return false;
        }

        /// <inheritdoc/>
        protected override LocalizationNote DefaultValue(XObject node)
        {
            return null;
        }
    }

    /// <summary>
    /// Note to localizers about a particular item of content.
    /// </summary>
    public class LocalizationNote
    {
        /// <summary>
        /// Types of the note.
        /// </summary>
        public LocalizationNoteType Type { get; set; }
        /// <summary>
        /// Note itself.
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// IRI referring to the location of the localization note.
        /// </summary>
        public string NoteRef { get; set; }
    }

    /// <summary>
    /// Types of informative notes.
    /// </summary>
    public enum LocalizationNoteType
    {
        /// <summary>
        /// A description provides useful background information that the translator will refer to only if they wish. 
        /// Example: a clarification of ambiguity in the source text.
        /// </summary>
        Description,
        /// <summary>
        /// An alert contains information that the translator must read before translating a piece of text. 
        /// Example: an instruction to the translator to leave parts of the text in the source language.
        /// </summary>
        Alert
    }
}