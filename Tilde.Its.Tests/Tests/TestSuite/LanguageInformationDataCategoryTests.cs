using System;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    [TestClass]
    public class LanguageInformationDataCategoryTests : DataCategoryTestSuiteTests
    {
        public LanguageInformationDataCategoryTests()
            : base("languageinformation", "languageinfo")
        {
        }

        [TestMethod] public void TestSuite_LanguageInfo_Html_langinfo1() { TestSuiteTestHtml(1); }
        [TestMethod] public void TestSuite_LanguageInfo_Html_langinfo2() { TestSuiteTestHtml(2); }
        [TestMethod] public void TestSuite_LanguageInfo_Html_langinfo3() { TestSuiteTestHtml(3); }

        [TestMethod] public void TestSuite_LanguageInfo_Xml_langinfo1() { TestSuiteTestXml(1); }
        [TestMethod] public void TestSuite_LanguageInfo_Xml_langinfo2() { TestSuiteTestXml(2); }
        [TestMethod] public void TestSuite_LanguageInfo_Xml_langinfo3() { TestSuiteTestXml(3); }
        [TestMethod] public void TestSuite_LanguageInfo_Xml_langinfo4() { TestSuiteTestXml(4); }

        protected override string ElementAndAttributeOutput(XObject e)
        {
            if (e.Annotation<LanguageInformationDataCategory>().Language == null)
                return "";

            return "\t" + "lang=\"" + e.Annotation<LanguageInformationDataCategory>().Language + "\"";
        }
    }
}
