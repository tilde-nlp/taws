using System.Linq;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests
{
    [TestClass]
    public class XPathQueryLangugeTests
    {
        [TestMethod]
        public void XPath_InlineNamespace()
        {
            string inputFilename = @"TestData\xpath\inline-namespace.xml";

            ItsDocument doc = new ItsXmlDocument(inputFilename);
            doc.Annotate<TranslateDataCategory>();
            Assert.IsFalse(doc.Document.Descendants(XName.Get("term", "http://mynsuri.example.com")).First().Annotation<TranslateDataCategory>().IsTranslatable);
            Assert.IsFalse(doc.Document.Descendants(XName.Get("term", "http://mynsuri.example.com")).Skip(1).First().Annotation<TranslateDataCategory>().IsTranslatable);
        }

        [TestMethod]
        public void XPath_Functions_id()
        {
            string inputFilename = @"TestData\xpath\id.xml";

            ItsDocument doc = new ItsXmlDocument(inputFilename);
            doc.Annotate<TerminologyDataCategory>();
            Assert.AreEqual("term1", doc.Document.Descendants("term").ElementAt(0).Annotation<TerminologyDataCategory>().Term.Info);
            Assert.AreEqual("term2", doc.Document.Descendants("term").ElementAt(1).Annotation<TerminologyDataCategory>().Term.Info);
            //Assert.AreEqual("term3", doc.Document.Descendants("term").ElementAt(2).Annotation<TerminologyDataCategory>().Term.Info);
            Assert.AreEqual("term4", doc.Document.Descendants("term").ElementAt(3).Annotation<TerminologyDataCategory>().Term.Info);
            Assert.AreEqual(null, doc.Document.Descendants("term").ElementAt(4).Annotation<TerminologyDataCategory>().Term.Info);
        }
    }
}
