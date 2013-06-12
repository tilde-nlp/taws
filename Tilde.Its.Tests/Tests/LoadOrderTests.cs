using System.Linq;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests
{
    [TestClass]
    public class LoadOrderTests
    {
        [TestMethod] public void LoadOrder_ScriptFirstLinkSecond() { Test(1, "link"); }
        [TestMethod] public void LoadOrder_LinkFirstScriptSecond() { Test(2, "script"); }
        [TestMethod] public void LoadOrder_XlinkFirstLinkSecond() { Test(3, "link"); }
        [TestMethod] public void LoadOrder_DoubleXlink() { Test(4, "xlink1"); }
        [TestMethod] public void LoadOrder_External_NotRoot_AtMostOne() { Test(5, "link"); }
        
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
