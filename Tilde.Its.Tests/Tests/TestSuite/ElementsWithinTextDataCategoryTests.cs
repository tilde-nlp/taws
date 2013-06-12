using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    [TestClass]
    public class ElementsWithinTextDataCategoryTests : DataCategoryTestSuiteTests
    {
        public ElementsWithinTextDataCategoryTests()
            : base("elementswithintext", "withintext")
        {
        }

        [TestMethod] public void TestSuite_ElementsWithinText_Html_elementswithintext1() { TestSuiteTestHtml(1); }
        [TestMethod] public void TestSuite_ElementsWithinText_Html_elementswithintext2() { TestSuiteTestHtml(2); }
        [TestMethod] public void TestSuite_ElementsWithinText_Html_elementswithintext3() { TestSuiteTestHtml(3); }
        [TestMethod] public void TestSuite_ElementsWithinText_Html_elementswithintext4() { TestSuiteTestHtml(4); }

        [TestMethod] public void TestSuite_ElementsWithinText_Xml_elementswithintext1() { TestSuiteTestXml(1); }
        [TestMethod] public void TestSuite_ElementsWithinText_Xml_elementswithintext2() { TestSuiteTestXml(2); }
        [TestMethod] public void TestSuite_ElementsWithinText_Xml_elementswithintext3() { TestSuiteTestXml(3); }
        [TestMethod] public void TestSuite_ElementsWithinText_Xml_elementswithintext4() { TestSuiteTestXml(4); }
        [TestMethod] public void TestSuite_ElementsWithinText_Xml_elementswithintext5() { TestSuiteTestXml(5); }
        [TestMethod] public void TestSuite_ElementsWithinText_Xml_elementswithintext6() { TestSuiteTestXml(6); }

        protected override string ElementOutput(XElement e)
        {
            return "\t" + "withinText=\"" + e.Annotation<ElementsWithinTextDataCategory>().WithinText.ToString().ToLowerInvariant() + "\"";
        }

        protected override string AttributeOutput(XAttribute a)
        {
            return "";
        }
    }
}
