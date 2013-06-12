using System.Collections.Generic;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// An interface for a query language of selectors.
    /// <see href="http://www.w3.org/TR/its20/#selectors"/>
    /// </summary>
    public interface IQueryLanguage
    {
        /// <summary>
        /// Selects all nodes of type <typeparamref name="TNodeType"/> relative to the <paramref name="root"/> element 
        /// using the provided selector.
        /// </summary>
        /// <typeparam name="TNodeType">Only select nodes of this type.</typeparam>
        /// <param name="root">Starting element.</param>
        /// <param name="selector">Relative selector.</param>
        /// <returns>All selected nodes.</returns>
        IEnumerable<TNodeType> SelectNodes<TNodeType>(XElement root, string selector);
        /// <summary>
        /// Selects the values of nodes that the relative selector to the <paramref name="node"/> points to.
        /// </summary>
        /// <param name="node">Starting node.</param>
        /// <param name="selector">Relative selector.</param>
        /// <returns>Values of the selected nodes.</returns>
        IEnumerable<string> SelectPointerValues(XObject node, string selector);
    }
}
