using System;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    [TestClass]
    public class TargetPointerDataCategoryTests : DataCategoryTestSuiteTests
    {
        public TargetPointerDataCategoryTests()
            : base("targetpointer")
        {
        }

        [TestMethod] public void TestSuite_TargetPointer_Html_targetpointer1() { TestSuiteTestHtml(1); }
        [TestMethod] public void TestSuite_TargetPointer_Html_targetpointer2() { TestSuiteTestHtml(2); }
        [TestMethod] public void TestSuite_TargetPointer_Html_targetpointer3() { TestSuiteTestHtml(3); }

        [TestMethod] public void TestSuite_TargetPointer_Xml_targetpointer1() { TestSuiteTestXml(1); }
        [TestMethod] public void TestSuite_TargetPointer_Xml_targetpointer2() { TestSuiteTestXml(2); }
        [TestMethod] public void TestSuite_TargetPointer_Xml_targetpointer3() { TestSuiteTestXml(3); }
        [TestMethod] public void TestSuite_TargetPointer_Xml_targetpointer4() { TestSuiteTestXml(4); }

        protected override string ElementAndAttributeOutput(XObject e)
        {
            string target = e.Annotation<TargetPointerDataCategory>().Target;

            if (target == null)
                return "";

            return "\t" + "targetPointer=\"" + target + "\"";
        }
    }
}
