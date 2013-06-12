using System;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    [TestClass]
    public class PreserveSpaceDataCategoryTests : DataCategoryTestSuiteTests
    {
        public PreserveSpaceDataCategoryTests()
            : base("preservespace")
        {
        }

        [TestMethod] public void TestSuite_PreserveSpace_Xml_preservespace1() { TestSuiteTestXml(1); }
        [TestMethod] public void TestSuite_PreserveSpace_Xml_preservespace2() { TestSuiteTestXml(2); }
        [TestMethod] public void TestSuite_PreserveSpace_Xml_preservespace3() { TestSuiteTestXml(3); }
        [TestMethod] public void TestSuite_PreserveSpace_Xml_preservespace4() { TestSuiteTestXml(4); }
        [TestMethod] public void TestSuite_PreserveSpace_Xml_preservespace5() { TestSuiteTestXml(5); }
        [TestMethod] public void TestSuite_PreserveSpace_Xml_preservespace6() { TestSuiteTestXml(6); }

        protected override string ElementAndAttributeOutput(XObject e)
        {
            return "\t" + "space=\"" + e.Annotation<PreserveSpaceDataCategory>().PreserveSpace.ToString().ToLowerInvariant() + "\"";
        }
    }
}
