using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    [TestClass]
    public class StorageSizeDataCategoryTests : DataCategoryTestSuiteTests
    {
        public StorageSizeDataCategoryTests()
            : base("storagesize")
        {
        }

        [TestMethod] public void TestSuite_StorageSize_Html_storagesize1() { TestSuiteTestHtml(1); }
        [TestMethod] public void TestSuite_StorageSize_Html_storagesize2() { TestSuiteTestHtml(2); }
        [TestMethod] public void TestSuite_StorageSize_Html_storagesize3() { TestSuiteTestHtml(3); }
        [TestMethod] public void TestSuite_StorageSize_Html_storagesize4() { TestSuiteTestHtml(4); }

        [TestMethod] public void TestSuite_StorageSize_Xml_storagesize1() { TestSuiteTestXml(1); }
        [TestMethod] public void TestSuite_StorageSize_Xml_storagesize2() { TestSuiteTestXml(2); }
        [TestMethod] public void TestSuite_StorageSize_Xml_storagesize3() { TestSuiteTestXml(3); }
        [TestMethod] public void TestSuite_StorageSize_Xml_storagesize4() { TestSuiteTestXml(4); }
        [TestMethod] public void TestSuite_StorageSize_Xml_storagesize5() { TestSuiteTestXml(5); }
        [TestMethod] public void TestSuite_StorageSize_Xml_storagesize6() { TestSuiteTestXml(6); }
        [TestMethod] public void TestSuite_StorageSize_Xml_storagesize7() { TestSuiteTestXml(7); }
        [TestMethod] public void TestSuite_StorageSize_Xml_storagesize8() { TestSuiteTestXml(8); }
        [TestMethod] public void TestSuite_StorageSize_Xml_storagesize9() { TestSuiteTestXml(9); }

        protected override string ElementAndAttributeOutput(XObject e)
        {
            StorageSize size = e.Annotation<StorageSizeDataCategory>().StorageSize;

            Dictionary<LineBreakType, string> values = new Dictionary<LineBreakType, string>()
            {
                { LineBreakType.LineFeed, "lf" },
                { LineBreakType.CarriageReturn, "cr" },
                { LineBreakType.CarriageReturnLineFeed, "crlf" },
                { LineBreakType.Nel, "nel" }
            };

            if (size != null)
            {
                string s = "";

                s += "\tlineBreakType=\"" + values[size.LineBreak] + "\"";
                s += "\tstorageEncoding=\"" + size.Encoding + "\"";
                s += "\tstorageSize=\"" + size.Size + "\"";

                return s;
            }

            return "";
        }
    }
}
