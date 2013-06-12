using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Tilde.Its;
using Tilde.Taws.Models;

namespace Tilde.Taws.Tests
{
    [TestClass]
    public class Html5AnnotatorTests
    {
        [TestMethod]
        public void Html5_ControlChars()
        {
            List<byte> controlChars = new List<byte>();
            for (byte i = 0; i < 32; i++)
                if (i != 10 && i != 13)
                    controlChars.Add(i);
            for (byte i = 0x80; i < 0x9f; i++)
            {
                controlChars.Add(0xC2);
                controlChars.Add(i);
            }
            string controlCharText = Encoding.UTF8.GetString(controlChars.ToArray());

            string input = Html5("en", controlCharText + @"term" + controlCharText);
            string output = Html5Its("en", @"term");

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk { SourceLanguage = "en", Input = new[] { new[] { "term" } }, Output = new[] { new[] { "term" } } });

            Test(input, output, chunks);
        }

        #region Methods
        [TestMethod]
        public void Html5_Methods_0()
        {
            string input = Html5("en", "term1 term2");
            string output = Html5Its("en", "term1 term2");

            ApiDocument doc = new ApiDocument();
            doc.Content = input;
            doc.UseStatisticalExtraction = false;
            doc.UseTermBankExtraction = false;

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Input = new[] { new[] { "term1 term2" } },
                Output = new[] { new[] { "<tename>term1</tename> <tename termid='etb-1'>term2</tename>" } }
            });

            Test(doc, output, chunks);
        }

        [TestMethod]
        public void Html5_Methods_1()
        {
            string input = Html5("en", "term1 term2");
            string output = Html5Its("en", ItsSpan("term1") + " term2");

            ApiDocument doc = new ApiDocument();
            doc.Content = input;
            doc.UseStatisticalExtraction = true;
            doc.UseTermBankExtraction = false;

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Input = new[] { new[] { "term1 term2" } },
                Output = new[] { new[] { "<tename>term1</tename> <tename termid='etb-1'>term2</tename>" } }
            });

            Test(doc, output, chunks);
        }

        [TestMethod]
        public void Html5_Methods_2()
        {
            string input = Html5("en", "term1 term2");
            string output = Html5Its("en", "term1 "  + ItsSpan("term2"));

            ApiDocument doc = new ApiDocument();
            doc.Content = input;
            doc.UseStatisticalExtraction = false;
            doc.UseTermBankExtraction = true;

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Input = new[] { new[] { "term1 term2" } },
                Output = new[] { new[] { "<tename>term1</tename> <tename termid='etb-1'>term2</tename>" } }
            });

            Test(doc, output, chunks);
        }

        [TestMethod]
        public void Html5_Methods_3()
        {
            string input = Html5("en", "term1 term2");
            string output = Html5Its("en", ItsSpan("term1") + " " + ItsSpan("term2"));

            ApiDocument doc = new ApiDocument();
            doc.Content = input;
            doc.UseStatisticalExtraction = true;
            doc.UseTermBankExtraction = true;

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Input = new[] { new[] { "term1 term2" } },
                Output = new[] { new[] { "<tename>term1</tename> <tename termid='etb-1'>term2</tename>" } }
            });

            Test(doc, output, chunks);
        }

        [TestMethod]
        public void Html5_Empty_String()
        {
            string input = "";
            string output = Html5Its("");

            Assert.AreEqual(output, OneLine(Annotate(input)));
        }

        [TestMethod]
        public void Html5_Empty_Html()
        {
            string input = Html5("");
            string output = Html5Its("");

            Assert.AreEqual(output, OneLine(Annotate(input)));
        }

        [TestMethod]
        public void Html5_Empty_Doctype()
        {
            string doctype = "<!DOCTYPE html>";
            string input = doctype + Html5("");
            string output = doctype + Html5Its("");

            Assert.AreEqual(output, OneLine(Annotate(input)));
        }

        [TestMethod]
        public void Html5_Empty_Doctype_Xhtml()
        {
            string doctype = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">";
            string input = doctype + Html5("");
            string output = doctype + Html5Its("").Replace("<html>", "<html xmlns=\"http://www.w3.org/1999/xhtml\">");

            Assert.AreEqual(output, OneLine(Annotate(input)));
        }
        #endregion

        #region
        private string Html5(string content)
        {
            return string.Format("<html>{0}</html>", content);
        }

        private string Html5(string language, string content)
        {
            return string.Format("<html lang=\"{0}\">{1}</html>", language, content);
        }

        private string Html5Its(string content)
        {
            return string.Format("<html><body its-annotators-ref=\"terminology|http://tilde.com/term-annotation-service\">{0}</body></html>", content);
        }

        private string Html5Its(string language, string content)
        {
            return string.Format("<html lang=\"{0}\"><body its-annotators-ref=\"terminology|http://tilde.com/term-annotation-service\">{1}</body></html>", language, content);
        }

        private string ItsSpan(string content, bool isTerm = true)
        {
            return string.Format("<span its-term=\"{1}\">{0}</span>", content, isTerm ? "yes" : "no");
        }

        private void Test(string input, string output, List<Chunk> chunks)
        {
            ApiDocument p = new ApiDocument();
            p.UseStatisticalExtraction = true;
            p.UseTermBankExtraction = true;
            p.Content = OneLine(input);
            Test(p, output, chunks);
        }

        private void Test(ApiDocument input, string output, List<Chunk> chunks)
        {
            TestableHtml5Annotator annotator = new TestableHtml5Annotator(input);
            annotator.TestChunks = actual =>
            {
                List<Chunk> expected = chunks;

                Assert.AreEqual(expected.Count, actual.Count);

                for (int i = 0; i < expected.Count; i++)
                {
                    Chunk expectedChunk = expected[i];
                    var actualChunk = actual.ElementAt(i);

                    Assert.AreEqual(expectedChunk.SourceLanguage, actualChunk.Key.Language);
                    Assert.AreEqual(expectedChunk.Domain, actualChunk.Key.Domain);
                    Assert.AreEqual(expectedChunk.Input.Length, actualChunk.Value.Count);

                    for (int j = 0; j < expectedChunk.Input.Length; j++)
                    {
                        for (int k = 0; k < expectedChunk.Input[j].Length; k++)
                        {
                            Assert.AreEqual(expectedChunk.Input[j][k], actualChunk.Value.ElementAt(j).Value[k]);
                            actualChunk.Value.ElementAt(j).Value[k] = expectedChunk.Output[j][k];
                        }
                    }
                }
            };

            ItsHtmlDocument expectedDocument = new ItsHtmlDocument(OneLine(output), null);
            ItsHtmlDocument outputDocument = new ItsHtmlDocument(OneLine(annotator.Annotate().Result.ToString()), null);

            Assert.IsTrue(XNode.DeepEquals(expectedDocument.Document, outputDocument.Document),
                string.Format("Expected:\n\n{0}\n\nActual:\n\n{1}", expectedDocument.Document, outputDocument.Document));
        }

        private string Annotate(string input)
        {
            ApiDocument doc = new ApiDocument { Content = input };
            TestableHtml5Annotator annotator = new TestableHtml5Annotator(doc);
            annotator.TestChunks = chunks => { return; };
            return annotator.Annotate().Result.ToString();
        }

        private class TestableHtml5Annotator : Html5Annotator
        {
            public TestableHtml5Annotator(ApiDocument source)
                : base(source)
            {
            }

            public Action<Chunker.ChunkCollection> TestChunks
            {
                get;
                set;
            }

            protected override async Task Annotate(Chunker.ChunkCollection chunks)
            {
                TestChunks(chunks);
            }

            protected override XDocument GetTermEntry(string id)
            {
                return null;
            }
        }

        private class Chunk
        {
            public string SourceLanguage { get; set; }
            public string Domain { get; set; }
            public string[][] Input { get; set; }
            public string[][] Output { get; set; }
        }

        private string OneLine(string s)
        {
            return string.Join("", s.Replace("\r\n", "\n").Split('\n').Select(ss => ss.Trim()));
        }
        #endregion
    }
}
