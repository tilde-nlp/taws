using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// Wrapper around a <see cref="IQueryLanguage"/> that caches the results for performance.
    /// All instances of this class use the same cache.
    /// </summary>
    public class CachedQueryLanguage : IQueryLanguage
    {
        /// <summary>
        /// Cache that stores the cached results.
        /// </summary>
        static readonly Dictionary<Tuple<XElement, string, Type>, object> cache = new Dictionary<Tuple<XElement, string, Type>, object>();

        /// <summary>
        /// Query language to use.
        /// </summary>
        IQueryLanguage queryLanguage;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryLanguage">Query language to use.</param>
        public CachedQueryLanguage(IQueryLanguage queryLanguage)
        {
            this.queryLanguage = queryLanguage;
        }

        /// <inheritdoc/>
        public IEnumerable<TNodeType> SelectNodes<TNodeType>(XElement root, string selector)
        {
            var key = Tuple.Create(root, selector, typeof(TNodeType));
            if (cache.ContainsKey(key))
                return (List<TNodeType>)cache[key];

            List<TNodeType> results = new List<TNodeType>();
            results.AddRange(queryLanguage.SelectNodes<TNodeType>(root, selector));

            cache[key] = results;

            return results;
        }

        /// <inheritdoc/>
        public IEnumerable<string> SelectPointerValues(XObject node, string selector)
        {
            return queryLanguage.SelectPointerValues(node, selector);
        }

        /// <summary>
        /// Clears the cache by deleting all cached results.
        /// </summary>
        public static void ClearCache()
        {
            cache.Clear();
        }
    }
}
