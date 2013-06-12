using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tilde.Its
{
    /// <summary>
    /// The Storage Size data category is used to specify the maximum storage size of a given content.
    /// <see href="http://www.w3.org/TR/its20/#storagesize"/>
    /// </summary>
    public class StorageSizeDataCategory : DataCategory<StorageSize>
    {
        private static readonly Dictionary<string, LineBreakType> lineBreaks = new Dictionary<string, LineBreakType>()
        {
            { "lf", LineBreakType.LineFeed },
            { "cr", LineBreakType.CarriageReturn },
            { "crlf", LineBreakType.CarriageReturnLineFeed },
            { "nel", LineBreakType.Nel },
        };

        /// <inheritdoc/>
        public StorageSizeDataCategory(ItsDocument document, XObject node)
            : base(document, node)
        {
        }

        /// <summary>
        /// The Storage Size is used to specify the maximum storage size of a given content.
        /// </summary>
        public StorageSize StorageSize
        {
            get { return Value; }
            set { Value = value; }
        }

        /// <inheritdoc/>
        protected override string GlobalRuleName
        {
            get { return "storageSizeRule"; }
        }

        /// <inheritdoc/>
        protected override XName LocalAttributeName
        {
            get { return XmlOrHtmlAttributeName("storageSize"); }
        }

        /// <inheritdoc/>
        protected override StorageSize DefaultValue(XObject node)
        {
            return null;
        }

        /// <inheritdoc/>
        protected override bool Inheritance(XObject node)
        {
            return false;
        }

        /// <inheritdoc/>
        protected override bool GlobalValue(XObject node, XAttribute attribute, GlobalRule rule, out StorageSize value)
        {
            XElement element = ElementOrAttribute(e => e, a => a.Parent);

            value = new StorageSize();
            value.Encoding = "UTF-8";
            value.LineBreak = LineBreakType.LineFeed;

            XAttribute sizeAttr = rule.RuleElement.Attribute("storageSize");
            XAttribute sizePointerAttr = rule.RuleElement.Attribute("storageSizePointer");
            if (sizeAttr != null)
            {
                int result;
                if (int.TryParse(sizeAttr.Value, out result))
                    value.Size = result;
            }
            else if (sizePointerAttr != null)
            {
                string pointerValue = rule.QueryLanguage.SelectPointerValues(node, sizePointerAttr.Value).FirstOrDefault();
                if (pointerValue != null)
                {
                    int result;
                    if (int.TryParse(pointerValue, out result))
                        value.Size = result;
                }
            }
            else
            {
                return false;
            }

            XAttribute encodingAttr = rule.RuleElement.Attribute("storageEncoding");
            XAttribute encodingPointerAttr = rule.RuleElement.Attribute("storageEncodingPointer");
            if (encodingAttr != null)
            {
                value.Encoding = encodingAttr.Value;
            }
            else if (encodingPointerAttr != null)
            {
                string pointerValue = rule.QueryLanguage.SelectPointerValues(node, encodingPointerAttr.Value).FirstOrDefault();
                if (pointerValue != null)
                    value.Encoding = pointerValue;
            }

            XAttribute linebreakAttr = LocalAttribute(element, XmlOrHtmlAttributeName("lineBreakType"));
            if (linebreakAttr != null)
                value.LineBreak = ParseLineBreakValue(linebreakAttr.Value);

            return true;
        }

        /// <inheritdoc/>
        protected override bool LocalValue(XElement element, XAttribute attribute, out StorageSize value)
        {
            value = new StorageSize();
            value.Encoding = "UTF-8";
            value.LineBreak = LineBreakType.LineFeed;
            
            int result;
            if (int.TryParse(attribute.Value, out result))
                value.Size = result;

            XAttribute encodingAttr = LocalAttribute(element, XmlOrHtmlAttributeName("storageEncoding"));
            if (encodingAttr != null)
                value.Encoding = encodingAttr.Value;
            
            XAttribute linebreakAttr = LocalAttribute(element, XmlOrHtmlAttributeName("lineBreakType"));
            if (linebreakAttr != null)
                value.LineBreak = ParseLineBreakValue(linebreakAttr.Value);

            return true;
        }

        private LineBreakType ParseLineBreakValue(string value)
        {
            value = NormalizeValue(value);

            if (lineBreaks.ContainsKey(value))
                return lineBreaks[value];

            return LineBreakType.LineFeed;
        }
    }

    /// <summary>
    /// Used to specify the maximum storage size of a given content.
    /// </summary>
    public class StorageSize
    {
        /// <summary>
        /// The storage size is always expressed in bytes and excludes any leading Byte-Order-Markers.
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// The name of the character set encoding used to calculate the number of bytes of the selected text.
        /// </summary>
        public string Encoding { get; set; }
        /// <summary>
        /// It indicates what type of line breaks the storage uses.
        /// </summary>
        public LineBreakType LineBreak { get; set; }
    }

    /// <summary>
    /// Indicates what type of line breaks the storage uses.
    /// </summary>
    public enum LineBreakType
    {
        /// <summary>
        /// "lf" for LINE FEED (U+000A)
        /// </summary>
        LineFeed,
        /// <summary>
        /// "cr" for CARRIAGE RETURN (U+000D)
        /// </summary>
        CarriageReturn,
        /// <summary>
        /// "crlf" for CARRIAGE RETURN (U+000D) followed by LINE FEED (U+000A)
        /// </summary>
        CarriageReturnLineFeed,
        /// <summary>
        /// Undocumented
        /// </summary>
        Nel
    }
}