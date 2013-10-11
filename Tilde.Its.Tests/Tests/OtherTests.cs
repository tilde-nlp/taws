using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tilde.Its.Tests.TestSuite;

namespace Tilde.Its.Tests
{
    [TestClass]
    public class LoadOrderTests : DataCategoryTestSuiteTests
    {
        [TestMethod]
        public void NewDomain()
        {
            string inputFilename = @"TestData\other\domain.html";
            string outputFilename = @"TestData\other\domain.output.txt";
        }

        private void Test(int testNumber, string expected)
        {
            string inputFilename = @"TestData\loadorder\order" + testNumber + ".html";

            ItsHtmlDocument doc = new ItsHtmlDocument(inputFilename);
            doc.AnnotateAll();
            XElement h1 = doc.Document.Descendants(ItsHtmlDocument.XhtmlNamespace + "h1").Single();

            Assert.AreEqual(true, h1.Annotation<TerminologyDataCategory>().IsTerm);
            Assert.AreEqual(expected, h1.Annotation<TerminologyDataCategory>().Term.InfoRef);
        }
    }
}
