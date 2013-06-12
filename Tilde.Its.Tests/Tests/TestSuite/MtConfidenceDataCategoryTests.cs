using System.Linq;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    [TestClass]
    public class MtConfidenceDataCategoryTests : DataCategoryTestSuiteTests
    {
        public MtConfidenceDataCategoryTests()
            : base("mtconfidence")
        {
        }

        [TestMethod] public void TestSuite_MtConfidence_Html_mtconfidence1() { TestSuiteTestHtml(1); }
        [TestMethod] public void TestSuite_MtConfidence_Html_mtconfidence2() { TestSuiteTestHtml(2); }
        [TestMethod] public void TestSuite_MtConfidence_Html_mtconfidence3() { TestSuiteTestHtml(3); }
        [TestMethod] public void TestSuite_MtConfidence_Html_mtconfidence4() { TestSuiteTestHtml(4); }
        [TestMethod] public void TestSuite_MtConfidence_Html_mtconfidence5() { TestSuiteTestHtml(5); }

        [TestMethod] public void TestSuite_MtConfidence_Xml_mtconfidence1() { TestSuiteTestXml(1); }
        [TestMethod] public void TestSuite_MtConfidence_Xml_mtconfidence2() { TestSuiteTestXml(2); }
        [TestMethod] public void TestSuite_MtConfidence_Xml_mtconfidence3() { TestSuiteTestXml(3); }
        [TestMethod] public void TestSuite_MtConfidence_Xml_mtconfidence4() { TestSuiteTestXml(4); }
        [TestMethod] public void TestSuite_MtConfidence_Xml_mtconfidence5() { TestSuiteTestXml(5); }
        [TestMethod] public void TestSuite_MtConfidence_Xml_mtconfidence6() { TestSuiteTestXml(6); }
        [TestMethod] public void TestSuite_MtConfidence_Xml_mtconfidence7() { TestSuiteTestXml(7); }

        protected override string ElementAndAttributeOutput(XObject e)
        {
            MtConfidenceDataCategory mtConfidence = e.Annotation<MtConfidenceDataCategory>();
            AnnotatorAnnotation annotators = e.Annotation<AnnotatorAnnotation>();

            return (annotators.AnnotatorsRef != null ? "\tannotatorsRef=\"" + annotators.AnnotatorsRef + "\"" : "")
                 + (mtConfidence.IsAnnotated ? "\tmtConfidence=\"" + mtConfidence.Confidence.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\"" : "");
        }

        private string JoinTrimLines(string s)
        {
            return string.Join(" ", s.Split('\n').Select(ss => ss.Trim()));
        }
    }
}
