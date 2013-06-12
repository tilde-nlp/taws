using System;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    [TestClass]
    public class AllowedCharactersDataCategoryTests : DataCategoryTestSuiteTests
    {
        public AllowedCharactersDataCategoryTests()
            : base("allowedcharacters")
        {
        }

        [TestMethod] public void TestSuite_AllowedCharacters_Html_allowedcharacters1() { TestSuiteTestHtml(1); }
        [TestMethod] public void TestSuite_AllowedCharacters_Html_allowedcharacters2() { TestSuiteTestHtml(2); }
        [TestMethod] public void TestSuite_AllowedCharacters_Html_allowedcharacters3() { TestSuiteTestHtml(3); }
        [TestMethod] public void TestSuite_AllowedCharacters_Html_allowedcharacters4() { TestSuiteTestHtml(4); }

        [TestMethod] public void TestSuite_AllowedCharacters_Xml_allowedcharacters1() { TestSuiteTestXml(1); }
        [TestMethod] public void TestSuite_AllowedCharacters_Xml_allowedcharacters2() { TestSuiteTestXml(2); }
        [TestMethod] public void TestSuite_AllowedCharacters_Xml_allowedcharacters3() { TestSuiteTestXml(3); }
        [TestMethod] public void TestSuite_AllowedCharacters_Xml_allowedcharacters4() { TestSuiteTestXml(4); }
        [TestMethod] public void TestSuite_AllowedCharacters_Xml_allowedcharacters5() { TestSuiteTestXml(5); }
        [TestMethod] public void TestSuite_AllowedCharacters_Xml_allowedcharacters6() { TestSuiteTestXml(6); }
        [TestMethod] public void TestSuite_AllowedCharacters_Xml_allowedcharacters7() { TestSuiteTestXml(7); }
        [TestMethod] public void TestSuite_AllowedCharacters_Xml_allowedcharacters8() { TestSuiteTestXml(8); }

        protected override string ElementAndAttributeOutput(XObject e)
        {
            if (e.Annotation<AllowedCharactersDataCategory>().AllowedCharacters != null)
                return "\t" + "allowedCharacters=\"" + (e.Annotation<AllowedCharactersDataCategory>().AllowedCharacters) + "\"";

            return "";
        }
    }
}
