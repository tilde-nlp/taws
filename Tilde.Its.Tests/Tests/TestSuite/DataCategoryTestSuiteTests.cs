using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tilde.Its.Tests.TestSuite
{
    public abstract class DataCategoryTestSuiteTests
    {
        private const string InputFilename = @"TestData\TestSuite\inputdata\{0}\{3}\{1}{2}{3}.{3}";
        private const string ExpectedOutputFilename = @"TestData\TestSuite\expected\{0}\{3}\{1}{2}{3}output.txt";
        private const string ActualOutputFilename = @"..\..\TestData\TestSuite\outputimplementors\tilde\{0}\{3}\{1}{2}{3}output.txt";
        private const bool SaveOutput = false;

        protected string foldername;
        protected string filename;

        public DataCategoryTestSuiteTests(string foldernameAndFilename)
            : this(foldernameAndFilename, foldernameAndFilename)
        {
        }

        public DataCategoryTestSuiteTests(string foldername, string filename)
        {
            this.foldername = foldername;
            this.filename = filename;
        }

        protected virtual string ElementAndAttributeOutput(XObject o)
        {
            throw new NotImplementedException();
        }

        protected virtual string ElementOutput(XElement e)
        {
            return ElementAndAttributeOutput(e);
        }

        protected virtual string AttributeOutput(XAttribute a)
        {
            return ElementAndAttributeOutput(a);
        }

        protected void TestSuiteTestHtml(int number)
        {
            TestSuiteTest("html", number, filename => new ItsHtmlDocument(filename));
        }

        protected void TestSuiteTestXml(int number)
        {
            TestSuiteTest("xml", number, filename => new ItsXmlDocument(filename));
        }

        protected void TestSuiteTest(string ext, int number, Func<string, ItsDocument> loadDocument)
        {
            string inputFilename = string.Format(InputFilename, foldername, filename, number, ext);
            string expectedOutputFilename = string.Format(ExpectedOutputFilename, foldername, filename, number, ext);
            string actualOutputFilename = string.Format(ActualOutputFilename, foldername, filename, number, ext);

            ItsDocument doc = loadDocument(inputFilename);;
            doc.AnnotateAll();
            
            TestSuiteOutput testSuiteOutput = new TestSuiteOutput();
            testSuiteOutput.ElementOutput = ElementOutput;
            testSuiteOutput.AttributeOutput = AttributeOutput;

            string output = testSuiteOutput.Output(doc);
            string expected = File.ReadAllText(expectedOutputFilename);

            if (SaveOutput)
                SaveToFile(actualOutputFilename, output);

            AreEqualByLines(expected, output);
        }

        private void SaveToFile(string filename, string contents)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            File.WriteAllText(filename, contents);
        }

        protected static void AreEqualByLines(string expected, string actual)
        {
            string[] expectedLines = expected.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            string[] actualLines = actual.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < actualLines.Length; i++)
            {
                string expectedLine = expectedLines[i];
                string actualLine = actualLines[i];
                Assert.AreEqual(expectedLine, actualLines[i]);
            }
        }

        private class TestSuiteOutput
        {
            public Func<XElement, string> ElementOutput
            {
                get;
                set;
            }

            public Func<XAttribute, string> AttributeOutput
            {
                get;
                set;
            }

            public string Output(ItsDocument doc)
            {
                string[] lines = Output("/" + doc.Document.Root.Name.LocalName, doc.Document.Root).ToArray();
                return string.Join(Environment.NewLine, lines);
            }

            private IEnumerable<string> Output(string path, XElement element)
            {
                yield return path + ElementOutput(element);

                foreach (XAttribute attribute in element.Attributes().Where(a => !a.IsNamespaceDeclaration).OrderBy(a => AttributeName(a)))
                {
                    yield return path + "/@" + AttributeName(attribute) + AttributeOutput(attribute);
                }

                foreach (XElement childElement in element.Elements())
                {
                    string childPath = path + "/" + ElementName(childElement);

                    if (childElement.Name != "html")
                    {
                        int position = element.Elements(childElement.Name).ToList().IndexOf(childElement);
                        childPath += "[" + (position + 1) + "]";
                    }

                    foreach (string s in Output(childPath, childElement))
                        yield return s;
                }
            }

            private string AttributeName(XAttribute a)
            {
                string name = "";

                if (a.Name.Namespace != a.Document.Root.GetDefaultNamespace() &&
                    a.Name.Namespace != ItsHtmlDocument.XhtmlNamespace &&
                    a.Parent.GetPrefixOfNamespace(a.Name.Namespace) != null)
                {
                    name = a.Parent.GetPrefixOfNamespace(a.Name.Namespace) + ":";
                }

                return name + a.Name.LocalName;
            }

            private string ElementName(XElement e)
            {
                string name = "";

                if (e.Name.Namespace != e.Document.Root.GetDefaultNamespace() &&
                    e.Name.Namespace != ItsHtmlDocument.XhtmlNamespace &&
                    e.GetPrefixOfNamespace(e.Name.Namespace) != null)
                {
                    name = e.GetPrefixOfNamespace(e.Name.Namespace) + ":";
                }

                return name + e.Name.LocalName;
            }
        }
    }
}
