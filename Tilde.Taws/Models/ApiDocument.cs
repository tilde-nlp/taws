namespace Tilde.Taws.Models
{
    /// <summary>
    /// Represents a document submitted to the API for annotation.
    /// </summary>
    public class ApiDocument
    {
        /// <summary>
        /// Content of the document.
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Aboslute path of the document.
        /// Used when the document contains references to external rules
        /// with a relative path.
        /// </summary>
        public string BaseUri { get; set; }
        
        /// <summary>
        /// Language of the document content.
        /// Required if the language is not specified in the document content.
        /// </summary>
        public string Language { get; set; }
        /// <summary>
        /// Topics of the document content.
        /// Used as the default value.
        /// </summary>
        public string[] Domains { get; set; }

        #region Parameters for annotation
        /// <summary>
        /// Unguided (monolingually annotates term candidates using unguided methods).
        /// Also known as:
        /// Statistical terminology annotation,
        /// Unguided term extraction,
        /// Method 1.
        /// </summary>
        public bool UseStatisticalExtraction { get; set; }
        /// <summary>
        /// Term bank based term extraction.
        /// Also known as:
        /// Term bank based terminology annotation,
        /// Method 2.
        /// </summary>
        public bool UseTermBankExtraction { get; set; }
        #endregion
    }
}