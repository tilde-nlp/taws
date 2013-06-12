using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Tilde.Its;
using Tilde.Taws.Models;

namespace Tilde.Taws.Tests
{
    [TestClass]
    public class Html5ChunkerTests
    {
        static readonly string[] empty = new string[0];

        #region SplitJoin
        #region Simple tests
        [TestMethod]
        public void Chunker_Simple_NoTags()
        {
            SplitJoinTest("<p>Test</p>", 
                          "<p>Test</p>", 
                            new[] { new[] { "Test" } }, 
                            new[] { new[] { "Test" } });
        }

        [TestMethod]
        public void Chunker_Simple_NoTags2()
        {
            SplitJoinTest("Test",
                          "Test",
                            new[] { new[] { "Test" } },
                            new[] { new[] { "Test" } });
        }

        [TestMethod]
        public void Chunker_Simple_OneTag()
        {
            SplitJoinTest("<p>Test</p>",
                          "<p><tag>Test</tag></p>",
                            new[] { new[] { "Test" } },
                            new[] { new[] { "<tag>Test</tag>" } });
        }

        [TestMethod]
        public void Chunker_Simple_OneTag_Beginning()
        {
            SplitJoinTest("<p>One Two Three</p>",
                          "<p><tag>One Two</tag> Three</p>",
                            new[] { new[] { "One Two Three" } }, 
                            new[] { new[] { "<tag>One Two</tag> Three" } });
        }

        [TestMethod]
        public void Chunker_Simple_OneTag_End()
        {
            SplitJoinTest("<p>One Two Three</p>",
                          "<p>One <tag>Two Three</tag></p>",
                            new[] { new[] { "One Two Three" } }, 
                            new[] { new[] { "One <tag>Two Three</tag>" } });
        }

        [TestMethod]
        public void Chunker_Simple_OneTag_Middle()
        {
            SplitJoinTest("<p>One Two Three</p>",
                          "<p>One <tag>Two</tag> Three</p>",
                            new[] { new[] { "One Two Three" } }, 
                            new[] { new[] { "One <tag>Two</tag> Three" } });
        }

        [TestMethod]
        public void Chunker_Simple_TwoTags()
        {
            SplitJoinTest("<p>One Two, Three</p>",
                          "<p><tag>One Two</tag>, <tag2>Three</tag2></p>",
                            new[] { new[] { "One Two, Three" } },
                            new[] { new[] { "<tag>One Two</tag>, <tag2>Three</tag2>" } });
        }
        #endregion

        #region Span tests
        [TestMethod]
        public void Chunker_Span_Inline_Ignore1()
        {
            SplitJoinTest("1<strong>2</strong>3<strong>4<strong>5<strong>6</strong>7</strong>8</strong>9",
                          "1<strong>2</strong>3<strong>4<strong>5<strong>6</strong>7</strong>8</strong>9",
                            new[] { new[] { "123456789" } },
                            new[] { new[] { "<tag>123456789</tag>" } });
        }

        [TestMethod]
        public void Chunker_Span_Inline_Ignore2()
        {
            SplitJoinTest("1<strong>2</strong>3<strong>4<strong>5<strong>6</strong>7</strong>8</strong>9",
                          "1<strong>2</strong>3<strong>4<strong>5<strong>6</strong>7</strong>8</strong>9",
                            new[] { new[] { "123456789" } },
                            new[] { new[] { "<tag>12</tag>3456789" } });
        }

        [TestMethod]
        public void Chunker_Span_Inline_Simple()
        {
            SplitJoinTest("1<strong>2</strong>3<strong>4<strong>5<strong>6</strong>7</strong>8</strong>9",
                          "<tag>1</tag><strong>2</strong>3<strong><tag>4</tag><strong>5<strong>6</strong>7</strong>8</strong><tag>9</tag>",
                            new[] { new[] { "123456789" } },
                            new[] { new[] { "<tag>1</tag>23<tag>4</tag>5678<tag>9</tag>" } });
        }

        [TestMethod]
        public void Chunker_Span_Inline_Independent()
        {
            SplitJoinTest("1<strong>2</strong>3<strong><p>4</p><strong>5<strong>6</strong>7</strong>8</strong>9",
                          "<tag>1</tag><strong>2</strong>3<strong><p><tag>4</tag></p><strong>5<strong>6</strong>7</strong>8</strong>9",
                            new[] { new[] { "123", "56789" }, new[] { "4" } },
                            new[] { new[] { "<tag>1</tag>23", "<tag>56789</tag>" }, new[] { "<tag>4</tag>" } });
        }

        [TestMethod]
        public void Chunker_Span_Nested()
        {
            SplitJoinTest("<div>1<noscript>2</noscript>3</div>",
                          "<div>1<noscript><tag>2</tag></noscript>3</div>",
                            new[] { new[] { "13" }, new[] { "2" } },
                            new[] { new[] { "<tag>13</tag" }, new[] { "<tag>2</tag>" } });
        }
        #endregion

        #region Misc tests
        [TestMethod]
        public void Chunker_Preserve_Attributes()
        {
            SplitJoinTest("<p class='p1'>.<span class='c1' id='h1'>Hello1</span>.<span id='h2' class='c2'>Hello2</span>.</p>",
                          "<p class='p1'>.<span class='c1' id='h1'><tag>Hello1</tag></span>.<span id='h2' class='c2'>Hello2</span>.</p>",
                            new[] { new[] { ".Hello1.Hello2." } },
                            new[] { new[] { ".<tag>Hello1</tag>.Hello2." } });
        }

        [TestMethod]
        public void Chunker_Same_Text_Different_Tags_Span()
        {
            SplitJoinTest("<div>.<span>Hello</span> <span>Hello</span>.</div>", 
                          "<div>.<span><tag>Hello</tag></span> <span>Hello</span>.</div>",
                           new[] { new[] { ".Hello Hello." } }, 
                           new[] { new[] { ".<tag>Hello</tag> Hello." } });
        }

        [TestMethod]
        public void Chunker_Same_Text_Different_Tags_Span2()
        {
            SplitJoinTest("<div>.<span>Hello</span> <span>Hello</span>.</div>",
                          "<div>.<span>Hello</span> <span>Hello</span>.</div>",
                            new[] { new[] { ".Hello Hello." } },
                            new[] { new[] { ".<tag>Hello Hello</tag>." } });
        }

        [TestMethod]
        public void Chunker_Same_Text_Different_Tags_P()
        {
            SplitJoinTest("<p>Hello</p> <p>Hello</p>", 
                          "<p><tag>Hello</tag></p> <p>Hello</p>",
                            new[] { new[] { "Hello" }, new[] { "Hello" } }, 
                            new[] { new[] { "<tag>Hello</tag>" }, new[] { "Hello" } });
        }

        [TestMethod]
        public void Chunker_Replace_Void_Elements_With_Whitespace()
        {
            SplitJoinTest("a<br>b<img>c<input>d",
                          "<tag>a</tag><br>b<img>c<input><tag>d</tag>",
                            new[] { new[] { "a b c d" } },
                            new[] { new[] { "<tag>a</tag> <tag>b c</tag> <tag>d</tag>" } });
        }

        [TestMethod]
        public void Chunker_Replace_Void_Elements_With_Whitespace_Hr()
        {
            SplitJoinTest("a<br>b<hr>c",
                          "a<br>b<hr><tag>c</tag>",
                            new[] { new[] { "a b c" } },
                            new[] { new[] { "<tag>a b</tag> <tag>c</tag>" } });
        }

        [TestMethod]
        public void Chunker_Replace_Void_Elements_With_Whitespace_Hr2()
        {
            SplitJoinTest("a<br>b<hr>c",
                          "<tag>a</tag><br>b<hr>c",
                            new[] { new[] { "a b c" } },
                            new[] { new[] { "<tag>a</tag> <tag>b c</tag>" } });
        }
        #endregion

        #region Languages
        [TestMethod]
        public void Chunker_Different_Languages1a()
        {
            SplitJoinTest("<p>One <span lang='es'>Dos</span> Three</p>",
                          "<p><tag>One</tag> <span lang='es'>Dos</span> Three</p>",
                            new[] { new[] { "One ", " Three" } },
                            new[] { new[] { "<tag>One</tag> ", " Three" } });
        }

        [TestMethod]
        public void Chunker_Different_Languages1b()
        {
            SplitJoinTest("<p>One <span lang='es'>Dos</span> Three</p>",
                          "<p>One <span lang='es'><tag>Dos</tag></span> Three</p>",
                            new[] { new[] { "Dos" } },
                            new[] { new[] { "<tag>Dos</tag>" } },
                            "es");
        }

        [TestMethod]
        public void Chunker_Different_Languages2a()
        {
            SplitJoinTest("<div lang='ru'>personālais dators <div lang='lv'>personālais dators</div></div>",
                          "<div lang='ru'>personālais dators <div lang='lv'><tag>personālais dators</tag></div></div>",
                            new[] { new[] { "personālais dators" } },
                            new[] { new[] { "<tag>personālais dators</tag>" } },
                            "lv");
        }

        [TestMethod]
        public void Chunker_Different_Languages2b()
        {
            SplitJoinTest("<div lang='ru'>personālais dators <div lang='lv'>personālais dators</div></div>",
                          "<div lang='ru'><tag>personālais dators</tag> <div lang='lv'>personālais dators</div></div>",
                            new[] { new[] { "personālais dators " } },
                            new[] { new[] { "<tag>personālais dators</tag> " } },
                            "ru");
        }

        [TestMethod]
        public void Chunker_Different_Languages_Dialects1a()
        {
            SplitJoinTest("<div lang='lv'><div lang='lv-LV'>personālais dators</div><div lang='lv-US'>personālais dators 2</div></div>",
                          "<div lang='lv'><div lang='lv-LV'><tag>personālais dators</tag></div><div lang='lv-US'>personālais dators 2</div></div>",
                            new[] { new[] { "personālais dators" } },
                            new[] { new[] { "<tag>personālais dators</tag>" } },
                            "lv-lv");
        }

        [TestMethod]
        public void Chunker_Different_Languages_Dialects1b()
        {
            SplitJoinTest("<div lang='lv'><div lang='lv-LV'>personālais dators</div><div lang='lv-US'>personālais dators 2</div></div>",
                          "<div lang='lv'><div lang='lv-LV'>personālais dators</div><div lang='lv-US'><tag>personālais dators 2</tag></div></div>",
                            new[] { new[] { "personālais dators 2" } },
                            new[] { new[] { "<tag>personālais dators 2</tag>" } },
                            "lv-us");
        }
        
        [TestMethod]
        public void Chunker_Different_Languages_Dialects1c()
        {
            SplitJoinTest("<div lang='lv'><div lang='lv-LV'>personālais dators</div><div lang='lv-US'>personālais dators 2</div></div>",
                          "<div lang='lv'><div lang='lv-LV'>personālais dators</div><div lang='lv-US'>personālais dators 2</div></div>",
                            new[] { empty },
                            new[] { empty },
                            "lv");
        }
        #endregion

        #region Locale Filter
        [TestMethod]
        public void Chunker_LocaleFilter_Wildcard_Empty()
        {
            SplitJoinTest(@"<div lang='en'>outside<div its-locale-filter-list='*'>all</div><div its-locale-filter-list=''>none</div></div>",
                          @"<div lang='en'><tag>outside</tag><div its-locale-filter-list='*'><tag>all</tag></div><div its-locale-filter-list=''>none</div></div>", 
                          new[] { new[] { "outside" }, new[] { "all" } }, 
                          new[] { new[] { "<tag>outside</tag>" }, new[] { "<tag>all</tag>" } }, 
                          "en");
        }

        [TestMethod]
        public void Chunker_LocaleFilter_DifferentLanguages()
        {
            SplitJoinTest(@"<div lang='en'>outside<div its-locale-filter-list='de'>de</div><div its-locale-filter-list='lv, fr'>lv fr</div></div>",
                          @"<div lang='en'><tag>outside</tag><div its-locale-filter-list='de'>de</div><div its-locale-filter-list='lv, fr'>lv fr</div></div>",
                          new[] { new[] { "outside" } },
                          new[] { new[] { "<tag>outside</tag>" } },
                          "en");
        }

        [TestMethod]
        public void Chunker_LocaleFilter_DifferentLanguage_Inside()
        {
            SplitJoinTest(@"<div lang='en'>outside<div its-locale-filter-list='de'>de<span its-locale-filter-list='en'>inside</span></div><div its-locale-filter-list='lv, fr'>lv fr</div></div>",
                          @"<div lang='en'><tag>outside</tag><div its-locale-filter-list='de'>de<span its-locale-filter-list='en'><tag>inside</tag></span></div><div its-locale-filter-list='lv, fr'>lv fr</div></div>",
                          new[] { new[] { "outside" }, new[] { "inside" } },
                          new[] { new[] { "<tag>outside</tag>" }, new[] { "<tag>inside</tag>" } },
                          "en");
        }

        [TestMethod]
        public void Chunker_LocaleFilter_Sublocale_Lang_LangDialect()
        {
            SplitJoinTest(@"<div lang='en'>outside<div its-locale-filter-list='en'>all</div><div its-locale-filter-list='en-GB'>gb</div><div its-locale-filter-list='en-CA'>ca</div></div>",
                          @"<div lang='en'><tag>outside</tag><div its-locale-filter-list='en'><tag>all</tag></div><div its-locale-filter-list='en-GB'>gb</div><div its-locale-filter-list='en-CA'>ca</div></div>",
                          new[] { new[] { "outside" }, new[] { "all" } },
                          new[] { new[] { "<tag>outside</tag>" }, new[] { "<tag>all</tag>" } },
                          "en");
        }

        [TestMethod]
        public void Chunker_LocaleFilter_Sublocale_LangDialect_Lang()
        {
            SplitJoinTest(@"<div lang='en-US'>outside<div its-locale-filter-list='en'>all</div><div its-locale-filter-list='en-GB'>gb</div><div its-locale-filter-list='en-CA, en-US'>america</div></div>",
                          @"<div lang='en-US'><tag>outside</tag><div its-locale-filter-list='en'><tag>all</tag></div><div its-locale-filter-list='en-GB'>gb</div><div its-locale-filter-list='en-CA, en-US'><tag>america</tag></div></div>",
                          new[] { new[] { "outside" }, new[] { "all" }, new[] { "america" } },
                          new[] { new[] { "<tag>outside</tag>" }, new[] { "<tag>all</tag>" }, new[] { "<tag>america</tag>" } },
                          "en-US");
        }

        [TestMethod]
        public void Chunker_LocaleFilter_Example61_WildcardRange_IncludeExclude()
        {
            string text = @"<div its-locale-filter-list='*-Ca'>Text for Canadian locales.</div><div its-locale-filter-list='*-cA' its-locale-filter-type='exclude'>Text for non-Canadian locales.</div>";
            var both = new[] { new[] { "Text for Canadian locales." }, new[] { "Text for non-Canadian locales." } };
            var ca = new[] { new[] { "Text for Canadian locales." } };
            var noca = new[] { new[] { "Text for non-Canadian locales." } };

            SplitJoinTest("<div lang='en'>" + text + "</div>", "<div lang='en'>" + text + "</div>", noca, noca, "en");
            SplitJoinTest("<div lang='en-CA'>" + text + "</div>", "<div lang='en-CA'>" + text + "</div>", ca, ca, "en-CA");
            SplitJoinTest("<div lang='fr-CA'>" + text + "</div>", "<div lang='fr-CA'>" + text + "</div>", ca, ca, "fr-CA");
            SplitJoinTest("<div lang='en-US'>" + text + "</div>", "<div lang='en-US'>" + text + "</div>", noca, noca, "en-US");
        }
        #endregion

        private void SplitJoinTest(string input, string output, string[][] expected, string[][] tagged, string language = null, string domain = null)
        {
            ItsHtmlDocument doc = new ItsHtmlDocument(input, null);
            doc.AnnotateAll();
            ItsHtmlDocument doc2 = new ItsHtmlDocument(output, null);

            TestableHtml5Chunker chunker = new TestableHtml5Chunker(doc);
            Chunker.ChunkCollection chunks = chunker.Split(doc.Document.Root.Element(ItsHtmlDocument.XhtmlNamespace + "body"));
            Chunker.LanguageDomainGroup group = new Chunker.LanguageDomainGroup(language, domain);

            string[][] actual = chunks.Export(group);

            Assert.AreEqual(expected.Length, actual.Count());
            Assert.AreEqual(expected.Length, tagged.Length);

            for (int i = 0; i < actual.Length; i++)
            {
                string[] expectedTexts = expected[i];
                string[] actualTexts = actual[i];

                Assert.AreEqual(expectedTexts.Length, actualTexts.Length);

                for (int j = 0; j < expectedTexts.Length; j++)
                {
                    Assert.AreEqual(expectedTexts[j], actualTexts[j]);
                }
            }

            chunks.Import(group, tagged);
            chunker.Join(chunks);

            Assert.AreEqual(XDocumentToString(doc2), XDocumentToString(doc));
        }

        private string XDocumentToString(ItsDocument doc)
        {
            return XDocument.Parse(doc.Document.ToString(SaveOptions.DisableFormatting), LoadOptions.None).ToString(SaveOptions.DisableFormatting)
                    .Replace(" xmlns=\"\"", "");
        }
        #endregion

        #region Split
        [TestMethod]
        public void Chunker_Split_Languages()
        {
            string input = "<div lang='en'>" + 
                                "1<strong>2</strong><strong>3<em>4</em></strong>" +
                                    "<div>5<strong>6<em>7</em></strong></div>" +
                                "8<em>9" + 
                                    "<div>A<em>B</em></div>" +
                                "C</em>D" +
                                "<span lang='it'>F" +
                                    "<em lang='en'>" +
                                        "G" +
                                        "<span lang='it'>H</span>" +
                                        "J" +
                                        "<div>I</div>" +
                                        "<div lang='en'>J<em>K</em>L</div>" +
                                        "M" +
                                    "</em>N" +
                                "</span>" +
                                "O</div>";

            SplitTest(input, new[] {
                new[] { "1234", "89", "CD", "O" },  // p lang=en
                new[] { "567" }, // p
                new[] { "AB" }, // p
                new[] { "G", "J", "M" }, // em lang=it
                new[] { "I" }, // p
                new[] { "JKL" } // p lang=en
            }, "en");

            SplitTest(input, new[] {
                new[] { "F", "N" }, // span lang=it
                new[] { "H" } // em, span lang=it
            }, "it");
        }

        [TestMethod]
        public void Chunker_Split_Domains()
        {
            const string rules = @"
                <script type='application/its+xml'>
                <its:rules version='2.0' xmlns:its='http://www.w3.org/2005/11/its' xmlns:h='http://www.w3.org/1999/xhtml'>
                    <its:domainRule selector='//h:*' domainPointer='@domains'/>
                </its:rules>
                </script>";

            string input = rules + "<p lang='en'><span domains='it'>IT1</span><span domains='law'>Law1</span><span domains='it, law'>ITLAW</span></p>";

            SplitTest(input, new[] { new[] { "IT1", "ITLAW" } }, "en", "it");
            SplitTest(input, new[] { new[] { "Law1ITLAW" } }, "en", "law");
        }

        [TestMethod]
        public void Chunker_Split_Nested()
        {
            string input = @"
                <html>
                <head>
                    <meta charset=utf-8> 
                    <title>Elements within Text defaults for HTML5</title>
                </head>
                <body>" +
                    @"<p>The element p is not within text. But <em>the element em is</em>.</p>" +
                    @"<p>A button <button onclick='display()'>Click Here</button> is also within text. But <textarea>The content of textarea</textarea> is nested.</p>" +
                    @"Some additional text..." +
                    @"<script>The script element is nested.</script>" +
                    @"<noscript>The noscript element is nested.</noscript>" +
                @"</body>" +
                @"</html>";

            SplitTest(input, new[] {
                new[] { "Some additional text..." },
                //new[] { "Elements within Text defaults for HTML5" },
                new[] { "The element p is not within text. But the element em is." },
                new[] { "A button Click Here is also within text. But  is nested." },
                new[] { "The content of textarea" },
                //new[] { "The script element is nested." },
                new[] { "The noscript element is nested." }
            });
        }

        [TestMethod]
        public void Chunker_Split_Nested2()
        {
            string input = @"<p>Es eju <b its-within-text='nested'>Kas tu tāds?</b> uz mājām!</p>";

            SplitTest(input, new[] {
                new[] { "Es eju  uz mājām!" },
                new[] { "Kas tu tāds?" },
            });
        }

        private void SplitTest(string input, string[][] expected, string language = null, string domain = null)
        {
            ItsHtmlDocument doc = new ItsHtmlDocument(input, null);
            doc.AnnotateAll();

            TestableHtml5Chunker chunker = new TestableHtml5Chunker(doc);
            Chunker.ChunkCollection chunks = chunker.Split(doc.Document.Root.Element(ItsHtmlDocument.XhtmlNamespace + "body"));
            Chunker.LanguageDomainGroup group = new Chunker.LanguageDomainGroup(language, domain);

            string[][] exportsTexts = chunks.Export(group);
            Assert.AreEqual(expected.Length, exportsTexts.Length);

            for (int i = 0; i < exportsTexts.Length; i++)
            {
                string[] expectedTexts = expected[i];
                string[] actualTexts = exportsTexts[i];

                Assert.AreEqual(expectedTexts.Length, actualTexts.Length);

                for (int j = 0; j < expectedTexts.Length; j++)
                {
                    Assert.AreEqual(expectedTexts[j], actualTexts[j]);
                }
            }
        }
        #endregion

        #region
        private class TestableHtml5Chunker : Html5Chunker
        {
            public TestableHtml5Chunker(ItsHtmlDocument document)
                : base(document)
            {
            }

            protected override bool IsNewTagName(string tag)
            {
                return tag == "tename" || tag == "tag" || tag == "t" || tag == "tag2";
            }
        }
        #endregion
    }
}
