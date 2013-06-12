using System;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    [TestClass]
    public class TranslateDataCategoryTests : DataCategoryTestSuiteTests
    {
        public TranslateDataCategoryTests()
            : base("translate")
        {
        }

        [TestMethod] public void TestSuite_Translate_Html_translate1() { TestSuiteTestHtml(1); }
        [TestMethod] public void TestSuite_Translate_Html_translate2() { TestSuiteTestHtml(2); }
        [TestMethod] public void TestSuite_Translate_Html_translate3() { TestSuiteTestHtml(3); }
        [TestMethod] public void TestSuite_Translate_Html_translate4() { TestSuiteTestHtml(4); }
        [TestMethod] public void TestSuite_Translate_Html_translate5() { TestSuiteTestHtml(5); }
        [TestMethod] public void TestSuite_Translate_Html_translate6() { TestSuiteTestHtml(6); }
        [TestMethod] public void TestSuite_Translate_Html_translate7() { TestSuiteTestHtml(7); }

        [TestMethod] public void TestSuite_Translate_Xml_translate1() { TestSuiteTestXml(1); }
        [TestMethod] public void TestSuite_Translate_Xml_translate2() { TestSuiteTestXml(2); }
        [TestMethod] public void TestSuite_Translate_Xml_translate3() { TestSuiteTestXml(3); }
        [TestMethod] public void TestSuite_Translate_Xml_translate4() { TestSuiteTestXml(4); }
        [TestMethod] public void TestSuite_Translate_Xml_translate5() { TestSuiteTestXml(5); }
        [TestMethod] public void TestSuite_Translate_Xml_translate6() { TestSuiteTestXml(6); }
        [TestMethod] public void TestSuite_Translate_Xml_translate7() { TestSuiteTestXml(7); }
        [TestMethod] public void TestSuite_Translate_Xml_translate8() { TestSuiteTestXml(8); }
        [TestMethod] public void TestSuite_Translate_Xml_translate9() { TestSuiteTestXml(9); }
        [TestMethod] public void TestSuite_Translate_Xml_translate10() { TestSuiteTestXml(10); }

        protected override string ElementAndAttributeOutput(XObject e)
        {
            return "\t" + "translate=\"" + (e.Annotation<TranslateDataCategory>().IsTranslatable ? "yes" : "no") + "\"";
        }
    }
}
