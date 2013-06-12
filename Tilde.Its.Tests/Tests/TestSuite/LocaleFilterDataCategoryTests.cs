using System;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    [TestClass]
    public class LocaleFilterDataCategoryTests : DataCategoryTestSuiteTests
    {
        public LocaleFilterDataCategoryTests()
            : base("localefilter", "locale")
        {
        }

        [TestMethod] public void TestSuite_LocaleFilter_Html_localefilter1() { TestSuiteTestHtml(1); }
        [TestMethod] public void TestSuite_LocaleFilter_Html_localefilter2() { TestSuiteTestHtml(2); }
        [TestMethod] public void TestSuite_LocaleFilter_Html_localefilter3() { TestSuiteTestHtml(3); }
        [TestMethod] public void TestSuite_LocaleFilter_Html_localefilter4() { TestSuiteTestHtml(4); }
        [TestMethod] public void TestSuite_LocaleFilter_Html_localefilter5() { TestSuiteTestHtml(5); }

        [TestMethod] public void TestSuite_LocaleFilter_Xml_localefilter1() { TestSuiteTestXml(1); }
        [TestMethod] public void TestSuite_LocaleFilter_Xml_localefilter2() { TestSuiteTestXml(2); }
        [TestMethod] public void TestSuite_LocaleFilter_Xml_localefilter3() { TestSuiteTestXml(3); }
        [TestMethod] public void TestSuite_LocaleFilter_Xml_localefilter4() { TestSuiteTestXml(4); }
        [TestMethod] public void TestSuite_LocaleFilter_Xml_localefilter5() { TestSuiteTestXml(5); }
        [TestMethod] public void TestSuite_LocaleFilter_Xml_localefilter6() { TestSuiteTestXml(6); }
        [TestMethod] public void TestSuite_LocaleFilter_Xml_localefilter7() { TestSuiteTestXml(7); }
        [TestMethod] public void TestSuite_LocaleFilter_Xml_localefilter8() { TestSuiteTestXml(8); }

        protected override string ElementAndAttributeOutput(XObject e)
        {
            LocaleFilter filter = e.Annotation<LocaleFilterDataCategory>().LocaleFilter;

            return "\t" + "localeFilterList=\"" + filter.Value + "\"" +
                   "\t" + "localeFilterType=\"" + filter.FilterType.ToString().ToLowerInvariant() + "\"";
        }
    }
}
