using System.Linq;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    [TestClass]
    public class LocalizationNoteDataCategoryTests : DataCategoryTestSuiteTests
    {
        public LocalizationNoteDataCategoryTests()
            : base("localizationnote", "locnote")
        {
        }

        [TestMethod] public void TestSuite_LocalizationNote_Html_locnote1() { TestSuiteTestHtml(1); }
        [TestMethod] public void TestSuite_LocalizationNote_Html_locnote2() { TestSuiteTestHtml(2); }
        [TestMethod] public void TestSuite_LocalizationNote_Html_locnote3() { TestSuiteTestHtml(3); }
        [TestMethod] public void TestSuite_LocalizationNote_Html_locnote4() { TestSuiteTestHtml(4); }
        [TestMethod] public void TestSuite_LocalizationNote_Html_locnote5() { TestSuiteTestHtml(5); }
        [TestMethod] public void TestSuite_LocalizationNote_Html_locnote6() { TestSuiteTestHtml(6); }
        [TestMethod] public void TestSuite_LocalizationNote_Html_locnote7() { TestSuiteTestHtml(7); }
        [TestMethod] public void TestSuite_LocalizationNote_Html_locnote8() { TestSuiteTestHtml(8); }
        [TestMethod] public void TestSuite_LocalizationNote_Html_locnote9() { TestSuiteTestHtml(9); }

        [TestMethod] public void TestSuite_LocalizationNote_Xml_locnote1() { TestSuiteTestXml(1); }
        [TestMethod] public void TestSuite_LocalizationNote_Xml_locnote2() { TestSuiteTestXml(2); }
        [TestMethod] public void TestSuite_LocalizationNote_Xml_locnote3() { TestSuiteTestXml(3); }
        [TestMethod] public void TestSuite_LocalizationNote_Xml_locnote4() { TestSuiteTestXml(4); }
        [TestMethod] public void TestSuite_LocalizationNote_Xml_locnote5() { TestSuiteTestXml(5); }
        [TestMethod] public void TestSuite_LocalizationNote_Xml_locnote6() { TestSuiteTestXml(6); }
        [TestMethod] public void TestSuite_LocalizationNote_Xml_locnote7() { TestSuiteTestXml(7); }
        [TestMethod] public void TestSuite_LocalizationNote_Xml_locnote8() { TestSuiteTestXml(8); }
        [TestMethod] public void TestSuite_LocalizationNote_Xml_locnote9() { TestSuiteTestXml(9); }
        [TestMethod] public void TestSuite_LocalizationNote_Xml_locnote10() { TestSuiteTestXml(10); }
        [TestMethod] public void TestSuite_LocalizationNote_Xml_locnote11() { TestSuiteTestXml(11); }

        protected override string ElementAndAttributeOutput(XObject e)
        {
            LocalizationNoteDataCategory locnote = e.Annotation<LocalizationNoteDataCategory>();

            string s = "";

            if (locnote.Note != null)
            {
                if (locnote.Note.Note != null)
                    s += "\tlocNote=\"" + JoinTrimLines(locnote.Note.Note) + "\"";
                if (locnote.Note.NoteRef != null)
                    s += "\tlocNoteRef=\"" + locnote.Note.NoteRef + "\"";
                s += "\tlocNoteType=\"" + locnote.Note.Type.ToString().ToLowerInvariant() + "\"";
            }

            return s;
        }

        private string JoinTrimLines(string s)
        {
            return string.Join(" ", s.Split('\n').Select(ss => ss.Trim())).Trim();
        }
    }
}
