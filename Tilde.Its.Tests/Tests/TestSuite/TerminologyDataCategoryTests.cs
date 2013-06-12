using System.Globalization;
using System.Linq;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    [TestClass]
    public class TerminologyDataCategoryTests : DataCategoryTestSuiteTests
    {
        public TerminologyDataCategoryTests()
            : base("terminology")
        {
        }

        [TestMethod] public void TestSuite_Terminology_Html_terminology1() { TestSuiteTestHtml(1); }
        [TestMethod] public void TestSuite_Terminology_Html_terminology2() { TestSuiteTestHtml(2); }
        [TestMethod] public void TestSuite_Terminology_Html_terminology3() { TestSuiteTestHtml(3); }
        [TestMethod] public void TestSuite_Terminology_Html_terminology4() { TestSuiteTestHtml(4); }
        [TestMethod] public void TestSuite_Terminology_Html_terminology5() { TestSuiteTestHtml(5); }
        [TestMethod] public void TestSuite_Terminology_Html_terminology6() { TestSuiteTestHtml(6); }

        [TestMethod] public void TestSuite_Terminology_Xml_terminology1() { TestSuiteTestXml(1); }
        [TestMethod] public void TestSuite_Terminology_Xml_terminology2() { TestSuiteTestXml(2); }
        [TestMethod] public void TestSuite_Terminology_Xml_terminology3() { TestSuiteTestXml(3); }
        [TestMethod] public void TestSuite_Terminology_Xml_terminology4() { TestSuiteTestXml(4); }
        [TestMethod] public void TestSuite_Terminology_Xml_terminology5() { TestSuiteTestXml(5); }
        [TestMethod] public void TestSuite_Terminology_Xml_terminology6() { TestSuiteTestXml(6); }
        [TestMethod] public void TestSuite_Terminology_Xml_terminology7() { TestSuiteTestXml(7); }
        [TestMethod] public void TestSuite_Terminology_Xml_terminology8() { TestSuiteTestXml(8); }
        [TestMethod] public void TestSuite_Terminology_Xml_terminology9() { TestSuiteTestXml(9); }

        protected override string ElementAndAttributeOutput(XObject e)
        {
            TerminologyDataCategory terminology = e.Annotation<TerminologyDataCategory>();
            AnnotatorAnnotation annotators = e.Annotation<AnnotatorAnnotation>();

            string s = "";
            
            s += annotators.AnnotatorsRef != null ? "\tannotatorsRef=\"" + annotators.AnnotatorsRef + "\"" : "";
            s += "\tterm=\"" + (terminology.IsTerm ? "yes" : "no") + "\"";

            if (terminology.Term != null)
            {
                s += (terminology.Term.Confidence != null ? "\ttermConfidence=\"" + terminology.Term.Confidence.Value.ToString(CultureInfo.InvariantCulture) + "\"" : "")
                   + (terminology.Term.Info != null ? "\ttermInfo=\"" + JoinTrimLines(terminology.Term.Info) + "\"" : "")
                   + (terminology.Term.InfoRef != null ? "\ttermInfoRef=\"" + JoinTrimLines(terminology.Term.InfoRef) + "\"" : "");
            }

            return s;
        }

        private string JoinTrimLines(string s)
        {
            return string.Join(" ", s.Split('\n').Select(ss => ss.Trim()));
        }
    }
}
