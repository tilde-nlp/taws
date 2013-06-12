using System.Globalization;

namespace Tilde.Taws.Models
{
    /// <summary>
    /// <see cref="TextInfo"/> extension methods.
    /// </summary>
    public static class TextInfoExtensions
    {
        /// <summary>
        /// Capitalizes the first letter in the text
        /// and lower cases the rest.
        /// </summary>
        /// <param name="textInfo">Culture to use.</param>
        /// <param name="text">Text to capitalize.</param>
        /// <returns>Sentence capitalized text.</returns>
        public static string ToSentenceCase(this TextInfo textInfo, string text)
        {
            return textInfo.ToUpper(text.Substring(0, 1)) + textInfo.ToLower(text.Substring(1));
        }
    }
}