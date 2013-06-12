using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    [TestClass]
    public class DirectionalityDataCategoryTests : DataCategoryTestSuiteTests
    {
        public DirectionalityDataCategoryTests()
            : base("directionality", "dir")
        {
        }

        [TestMethod] public void TestSuite_Directionality_Html_Dir1() { TestSuiteTestHtml(1); }
        [TestMethod] public void TestSuite_Directionality_Html_Dir2() { TestSuiteTestHtml(2); }
        [TestMethod] public void TestSuite_Directionality_Html_Dir3() { TestSuiteTestHtml(3); }
        [TestMethod] public void TestSuite_Directionality_Html_Dir4() { TestSuiteTestHtml(4); }

        [TestMethod] public void TestSuite_Directionality_Xml_Dir1() { TestSuiteTestXml(1); }
        [TestMethod] public void TestSuite_Directionality_Xml_Dir2() { TestSuiteTestXml(2); }
        [TestMethod] public void TestSuite_Directionality_Xml_Dir3() { TestSuiteTestXml(3); }
        [TestMethod] public void TestSuite_Directionality_Xml_Dir4() { TestSuiteTestXml(4); }
        [TestMethod] public void TestSuite_Directionality_Xml_Dir5() { TestSuiteTestXml(5); }
        [TestMethod] public void TestSuite_Directionality_Xml_Dir6() { TestSuiteTestXml(6); }

        protected override string ElementAndAttributeOutput(XObject e)
        {
            Dictionary<Directionality, string> values = new Dictionary<Directionality, string>()
            {
                { Directionality.LeftToRight, "ltr" },
                { Directionality.RightToLeft, "rtl" },
                { Directionality.LeftToRightOverride, "lro" },
                { Directionality.RightToLeftOverride, "rlo" }
            };

            return "\t" + "dir=\"" + values[e.Annotation<DirectionalityDataCategory>().Directionality] + "\"";
        }
    }
}
