using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    [TestClass]
    public class DomainDataCategoryTests : DataCategoryTestSuiteTests
    {
        public DomainDataCategoryTests()
            : base("domain")
        {
        }

        [TestMethod] public void TestSuite_Domain_Html_domain1() { TestSuiteTestHtml(1); }
        [TestMethod] public void TestSuite_Domain_Html_domain2() { TestSuiteTestHtml(2); }
        [TestMethod] public void TestSuite_Domain_Html_domain3() { TestSuiteTestHtml(3); }
        [TestMethod] public void TestSuite_Domain_Html_domain4() { TestSuiteTestHtml(4); }

        [TestMethod] public void TestSuite_Domain_Xml_domain1() { TestSuiteTestXml(1); }
        [TestMethod] public void TestSuite_Domain_Xml_domain2() { TestSuiteTestXml(2); }
        //[TestMethod] public void TestSuite_Domain_Xml_domain3() { TestSuiteTestXml(3); }
        [TestMethod] public void TestSuite_Domain_Xml_domain4() { TestSuiteTestXml(4); }
        [TestMethod] public void TestSuite_Domain_Xml_domain5() { TestSuiteTestXml(5); }
        [TestMethod] public void TestSuite_Domain_Xml_domain6() { TestSuiteTestXml(6); }
        [TestMethod] public void TestSuite_Domain_Xml_domain7() { TestSuiteTestXml(7); }

        protected override string ElementAndAttributeOutput(System.Xml.Linq.XObject e)
        {
            if (e.Annotation<DomainDataCategory>().Value == null)
                return "";

            return "\t" + "domains=\"" + e.Annotation<DomainDataCategory>().Value + "\"";
        }
    }
}
