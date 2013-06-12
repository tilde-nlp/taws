using System;
using System.Threading.Tasks;
using System.Web;

namespace Tilde.Taws.Models
{
    /// <summary>
    /// Coverts a plain text document to an HTML5 document,
    /// finds terms and annotates them.
    /// </summary>
    public class PlaintextAnnotator
    {
        /// <summary>
        /// Document to annotate.
        /// </summary>
        ApiDocument document;
        
        /// <summary>
        /// Creates a new instance of the annotator.
        /// </summary>
        /// <param name="document">Document to annotate.</param>
        public PlaintextAnnotator(ApiDocument document)
        {
            if (document == null)
                throw new ArgumentNullException("document", "No document.");
            if (document.Content == null)
                throw new ArgumentNullException("document.Content", "Document content is empty");

            this.document = document;
        }

        /// <summary>
        /// Template to use when converting text to HTML5.
        /// Variable {0} represents the text.
        /// </summary>
        public string Template
        {
            get;
            set;
        }

        /// <summary>
        /// Finds and annotates terms in the document.
        /// </summary>
        /// <returns>HTML5 document with annotated terms.</returns>
        public async Task<string> Annotate()
        {
            try
            {
                document.Content = ToHtml5();
                Html5Annotator annotator = new Html5Annotator(document);
                return await annotator.Annotate();
            }
            catch (Exception e)
            {
                throw new AnnotatorException(e);
            }
        }

        /// <summary>
        /// Converts a plain text document to an HTML5 document.
        /// </summary>
        /// <returns>HTML5 document with the text in it.</returns>
        private string ToHtml5()
        {
            string text = document.Content;
            string lang = document.Language;

            // escape tags
            text = HttpUtility.HtmlEncode(text);
            // preserve new lines
            text = text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "<br>\n");

            // render the template
            return string.Format(Template ?? string.Empty, text, lang != null ? " lang=\"" + HttpUtility.HtmlEncode(lang) + "\"" : "");
        }
    }
}