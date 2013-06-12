using System;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    [TestClass]
    public class ExternalResourceDataCategoryTests : DataCategoryTestSuiteTests
    {
        public ExternalResourceDataCategoryTests()
            : base("externalresource")
        {
        }

        [TestMethod] public void TestSuite_ExternalResource_Html_externalresource1() { TestSuiteTestHtml(1); }
        [TestMethod] public void TestSuite_ExternalResource_Html_externalresource2() { TestSuiteTestHtml(2); }
        [TestMethod] public void TestSuite_ExternalResource_Html_externalresource3() { TestSuiteTestHtml(3); }

        [TestMethod] public void TestSuite_ExternalResource_Xml_externalresource1() { TestSuiteTestXml(1); }
        [TestMethod] public void TestSuite_ExternalResource_Xml_externalresource2() { TestSuiteTestXml(2); }
        [TestMethod] public void TestSuite_ExternalResource_Xml_externalresource3() { TestSuiteTestXml(3); }
        [TestMethod] public void TestSuite_ExternalResource_Xml_externalresource4() { TestSuiteTestXml(4); }
        [TestMethod] public void TestSuite_ExternalResource_Xml_externalresource5() { TestSuiteTestXml(5); }

        protected override string ElementAndAttributeOutput(XObject e)
        {
            string resRef = e.Annotation<ExternalResourceDataCategory>().ExternalResourceIri;

            if (resRef == null)
                return "";

            return "\t" + "externalResourceRef=\"" + resRef + "\"";
        }
    }
}
