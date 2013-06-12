using System;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    [TestClass]
    public class IdValueDataCategoryTests : DataCategoryTestSuiteTests
    {
        public IdValueDataCategoryTests()
            : base("idvalue")
        {
        }

        [TestMethod] public void TestSuite_IdValue_Html_idvalue1() { TestSuiteTestHtml(1); }
        [TestMethod] public void TestSuite_IdValue_Html_idvalue2() { TestSuiteTestHtml(2); }
        [TestMethod] public void TestSuite_IdValue_Html_idvalue3() { TestSuiteTestHtml(3); }

        [TestMethod] public void TestSuite_IdValue_Xml_idvalue1() { TestSuiteTestXml(1); }
        [TestMethod] public void TestSuite_IdValue_Xml_idvalue2() { TestSuiteTestXml(2); }
        [TestMethod] public void TestSuite_IdValue_Xml_idvalue3() { TestSuiteTestXml(3); }
        [TestMethod] public void TestSuite_IdValue_Xml_idvalue4() { TestSuiteTestXml(4); }
        [TestMethod] public void TestSuite_IdValue_Xml_idvalue5() { TestSuiteTestXml(5); }

        protected override string ElementAndAttributeOutput(XObject e)
        {
            string id = e.Annotation<IdValueDataCategory>().ID;
            
            if (id == null)
                return "";

            return "\t" + "idValue=\"" + id + "\"";
        }
    }
}
