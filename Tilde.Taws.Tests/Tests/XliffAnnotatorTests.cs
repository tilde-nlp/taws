using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Tilde.Taws.Models;

namespace Tilde.Taws.Tests
{
    [TestClass]
    public class XliffAnnotatorTests
    {
        [TestMethod]
        public void Xliff_HelloWorld()
        {
            #region Xliff
            string input = @"
                <xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2'>
                  <file original='hello.txt' source-language='lv' target-language='en' datatype='plaintext'>
                    <body>
                      <trans-unit id='hi'>
                        <source>Sveika pasaule</source>
                        <target>Hello world</target>
                          <alt-trans>
                            <target xml:lang='lt'>Sveikas pasaules?</target>
                          </alt-trans>
                        </trans-unit>
                    </body>
                  </file>
                </xliff>";

            string output = @"
                <xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/ns/its-xliff/' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'>
                  <file original='hello.txt' source-language='lv' target-language='en' datatype='plaintext'>
                    <body>
                      <trans-unit id='hi'>
                        <source><mrk mtype='term'>Sveika pasaule</mrk></source>
                        <target><mrk mtype='term'>Hello world</mrk></target>
                          <alt-trans>
                            <target xml:lang='lt'><mrk mtype='term'>Sveikas</mrk> <mrk mtype='term'>pasaules</mrk>?</target>
                          </alt-trans>
                        </trans-unit>
                    </body>
                  </file>
                </xliff>";
            #endregion

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk { SourceLanguage = "lv", Input = new[] { new[] { "Sveika pasaule" } }, Output = new[] { new[] { "<tename>Sveika pasaule</tename>" } } });
            chunks.Add(new Chunk { SourceLanguage = "en", Input = new[] { new[] { "Hello world" } }, Output = new[] { new[] { "<tename>Hello world</tename>" } } });
            chunks.Add(new Chunk { SourceLanguage = "lt", Input = new[] { new[] { "Sveikas pasaules?" } }, Output = new[] { new[] { "<tename>Sveikas</tename> <tename>pasaules</tename>?" } } });

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_Sources()
        {
            #region Xliff
            string xliff = @"
                  <file source-language='lv' target-language='en' original='123.txt' datatype='txt'>
                    <header>
                     <skl>
                      <external-file uid='3BB236513BB24732' href='Graphic Example.psd.skl'/>
                     </skl>
                     <phase-group>
                      <phase phase-name='extract' process-name='extraction' tool='Rainbow' date='20010926T152258Z' company-name='NeverLand Inc.' job-id='123' contact-name='Peter Pan' contact-email='ppan@example.com'>
                          <note>Make sure to use the glossary I sent you yesterday Thanks.</note>
                      </phase>
                     </phase-group>
                    </header>
                    <body>
                     <trans-unit id='1'>
                       <source>1 lv</source>
                       <target>1 en</target>
                     </trans-unit>
                     <trans-unit id='2'>
                       <source>2 lv</source>
                       <target>2 en</target>
                       <alt-trans>
                           <target xml:lang='lt'>2a lt</target>
                       </alt-trans>
                       <alt-trans>
                           <source>2b lv</source>
                           <target xml:lang='lt'>2b lt</target>
                       </alt-trans>
                     </trans-unit>
                     <trans-unit id='3'>
                       <source>3 lv</source>
                       <target>3 en</target>
                     </trans-unit>
                     <trans-unit id='4'>
                       <source>4 lv</source>
                       <seg-source><mrk mid='0' mtype='seg'>4seg lv</mrk></seg-source>
                       <target><mrk mid='0' mtype='seg'>4seg en</mrk></target>
                       <alt-trans mid='0' match-quality='100' origin='mytm.pentm'>
                        <source>4aseg lv</source>
                        <target xml:lang='lt'>4aseg lt</target>
                       </alt-trans>
                     </trans-unit>
                     <trans-unit id='5'>
                       <context-group>
                         <context context-type='paramtnoes'>ignore me</context>
                       </context-group>
                       <note>ignore me</note>
                     </trans-unit>
                     <group restype='table'>
                      <group restype='row'>
                       <trans-unit id='6a'>
                        <source>r1c1 lv</source>
                       </trans-unit>
                       <trans-unit id='6b'>
                        <source>r1c2 lv</source>
                       </trans-unit>
                      </group>
                      <group restype='row'>
                       <trans-unit id='6c'>
                        <source>r2c1 lv</source>
                       </trans-unit>
                       <trans-unit id='6d'>
                        <source>r2c2 lv</source>
                       </trans-unit>
                      </group>
                     </group>
                     <bin-unit id='999'>
                       <bin-source><internal-file>ignore me</internal-file></bin-source>
                       <bin-target><external-file href='dontcare.txt'/></bin-target>
                     </bin-unit>
                    </body>
                  </file>";
            #endregion

            string input = string.Format(XliffTemplate, xliff);
            string output = string.Format(XliffTemplateWithItsItsxAnnRef, xliff);

            string[][] lv = new[]
            {
                new[] { "1 lv" },
                new[] { "2 lv" },
                new[] { "2b lv" },
                new[] { "3 lv" },
                new[] { "4 lv" },
                new[] { "4seg lv" },
                new[] { "4aseg lv" },
                new[] { "r1c1 lv" },
                new[] { "r1c2 lv" },
                new[] { "r2c1 lv" },
                new[] { "r2c2 lv" },
            };

            string[][] en = new[]
            {
                new[] { "1 en" },
                new[] { "2 en" },
                new[] { "3 en" },
                new[] { "4seg en" },
            };

            string[][] lt = new[]
            {
                new[] { "2a lt" },
                new[] { "2b lt" },
                new[] { "4aseg lt" },
            };

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk { SourceLanguage = "lv", Input = lv, Output = lv });
            chunks.Add(new Chunk { SourceLanguage = "en", Input = en, Output = en });
            chunks.Add(new Chunk { SourceLanguage = "lt", Input = lt, Output = lt });

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_MultipleFiles()
        {
            #region Xliff
            string xliff = @"
                  <file source-language='en' target-language='lv' datatype='plaintext' original='hello.txt'>
                    <body>
                      <trans-unit id='hi'>
                        <source>Hello world</source>
                        <target>Sveika pasaule</target>
                      </trans-unit>
                    </body>
                  </file>
                  <file source-language='en' target-language='lv' datatype='plaintext' original='bye.txt'>
                    <body>
                      <trans-unit id='bye'>
                        <source>Goodbye cruel world</source>
                        <target>Paliec sveika ļaunā pasaule</target>
                      </trans-unit>
                    </body>
                  </file>";
            #endregion

            string input = string.Format(XliffTemplate, xliff);
            string output = string.Format(XliffTemplateWithItsItsxAnnRef, xliff);

            string[][] en1 = new[] { new[] { "Hello world" } };
            string[][] en2 = new[] { new[] { "Goodbye cruel world" } };

            string[][] lv1 = new[] { new[] { "Sveika pasaule" } };
            string[][] lv2 = new[] { new[] { "Paliec sveika ļaunā pasaule" } };

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk { SourceLanguage = "en", Input = en1, Output = en1 });
            chunks.Add(new Chunk { SourceLanguage = "lv", Input = lv1, Output = lv1 });
            chunks.Add(new Chunk { SourceLanguage = "en", Input = en2, Output = en2 });
            chunks.Add(new Chunk { SourceLanguage = "lv", Input = lv2, Output = lv2 });

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_Languages()
        {
            #region Xliff
            string xliff = @"
                  <file source-language='en' datatype='plaintext' original='hello.txt'>
                      <body>
                        <trans-unit id='1'><source>1en</source></trans-unit>
                        <trans-unit id='2'><source xml:lang='lv'>2lv</source></trans-unit>
                        <trans-unit id='3' xml:lang='lv'><source>3en</source></trans-unit>
                        <trans-unit id='4'>
                          <source>4en</source>
                          <alt-trans><source xml:lang='lv'>4a1lv</source></alt-trans>
                          <alt-trans xml:lang='lv'><source>4a2lv</source></alt-trans>
                          <alt-trans xml:lang='en'><source xml:lang='lv'>4a3lv</source></alt-trans>
                          <alt-trans xml:lang='en'><source xml:lang='lv'><g xml:lang='en'>4a4en</g></source></alt-trans>
                        </trans-unit>
                       </body>
                      </file>
                      <file source-language='en' target-language='lv' datatype='plaintext' original='hello2.txt'>
                       <body>
                        <trans-unit id='1'><source>s-1en</source><target>t-1lv</target></trans-unit>
                        <trans-unit id='2'><source>s-2en</source><target xml:lang='lt'>t-2lt</target></trans-unit>
                        <trans-unit id='3' xml:lang='lt'><source>s-3en</source><target>t-3lv</target></trans-unit>
                        <trans-unit id='4'>
                          <source>s-4en</source>
                          <alt-trans><source>s-4a1en</source><target xml:lang='lt'>t-4a1lt</target></alt-trans>
                          <alt-trans xml:lang='lv'><source>s-4a2lv</source><target>t-4a2lv</target></alt-trans>
                          <alt-trans xml:lang='lv'><source>s-4a3lv</source><target xml:lang='lt'>t-4a3lt</target></alt-trans>
                        </trans-unit>
                       </body>
                     </file>";
            #endregion

            string input = string.Format(XliffTemplate, xliff);
            string output = string.Format(XliffTemplateWithItsItsxAnnRef, xliff);

            string[][] en1 = new[] { new[] { "1en" }, new[] { "3en" }, new[] { "4en" }, new[] { "4a4en" } };
            string[][] en2 = new[] { new[] { "s-1en" }, new[] { "s-2en" }, new[] { "s-3en" }, new[] { "s-4en" }, new[] { "s-4a1en" } };

            string[][] lv1 = new[] { new[] { "2lv" }, new[] { "4a1lv" }, new[] { "4a2lv" }, new[] { "4a3lv" } };
            string[][] lv2 = new[] { new[] { "t-1lv" }, new[] { "t-3lv" }, new[] { "s-4a2lv" }, new[] { "t-4a2lv" }, new[] { "s-4a3lv" } };

            string[][] lt = new[] { new[] { "t-2lt" }, new[] { "t-4a1lt" }, new[] { "t-4a3lt" } };

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk { SourceLanguage = "en", Input = en1, Output = en1 });
            chunks.Add(new Chunk { SourceLanguage = "lv", Input = lv1, Output = lv1 });
            chunks.Add(new Chunk { SourceLanguage = "en", Input = en2, Output = en2 });
            chunks.Add(new Chunk { SourceLanguage = "lv", Input = lv2, Output = lv2 });
            chunks.Add(new Chunk { SourceLanguage = "lt", Input = lt, Output = lt });

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_Inline()
        {
            #region Xliff
            string input = string.Format(XliffTemplate, @"
                  <file original='hello.txt' source-language='en' target-language='lv' datatype='plaintext' xmlns:wtf='http://www.invalid-xliff.com/'>
                    <body>
                      <trans-unit id='hi'>
                        <source>
                           1
                           <g id='a'>2</g>
                           <g id='b'>3<g id='c'>4</g>5</g>
                           6<x/>7<bx/>8<ex/>9
                           1<bpt id='d'>abc</bpt>def<ept id='d'>ghi</ept>2
                           3<ph>abc</ph>4
                           5<it>abc</it>6
                           7<mrk>abc</mrk>8
                           9<mrk>abc<mrk>def</mrk>ghi<g>ijk</g></mrk>0
                           1<sub>subflow1</sub>2
                           3<ph>a<sub>subflow2</sub>b<sub>subflow3</sub>c</ph>4
                           5<g><sub>subflow<g>4</g>abc<sub>subflow5</sub>def</sub></g>6
                           <wtf:element>you don't belong here!!!</wtf:element>
                        </source>
                       </trans-unit>
                    </body>
                  </file>");

            string output = string.Format(XliffTemplateWithItsItsxAnnRef, @"
                 <file original='hello.txt' source-language='en' target-language='lv' datatype='plaintext' xmlns:wtf='http://www.invalid-xliff.com/'>
                    <body>
                      <trans-unit id='hi'>
                        <source>
                           <mrk mtype='term'>1</mrk>
                           <g id='a'>2</g>
                           <g id='b'>3<g id='c'>4</g>5</g>
                           6<x/>7<bx/>8<ex/>9
                           1<bpt id='d'>abc</bpt>def<ept id='d'>ghi</ept>2
                           3<ph>abc</ph>4
                           5<it>abc</it>6
                           7<mrk>abc</mrk>8
                           9<mrk>abc<mrk>def</mrk>ghi<g>ijk</g></mrk>0
                           1<sub><mrk mtype='term'>subflow1</mrk></sub>2
                           3<ph>a<sub>subflow2</sub><mrk mtype='term'>b</mrk><sub>subflow3</sub>c</ph>4
                           5<g><sub>subflow<g>4</g>abc<sub>subflow5</sub>def</sub></g><mrk mtype='term'>6</mrk>
                           <wtf:element>you don't <mrk mtype='term'>belong</mrk> here!!!</wtf:element>
                        </source>
                       </trans-unit>
                    </body>
                  </file>");
            #endregion

            List<Chunk> chunks = new List<Chunk>
            {
                new Chunk
                {
                    SourceLanguage = "en",
                    Input = new[]
                    { 
                        new[] { "1234567891abcdefghi23abc45abc67abc89abcdefghiijk0123abc456" },
                        new[] { "subflow1" },
                        new[] { "subflow2" },
                        new[] { "subflow3" },
                        new[] { "subflow4abcdef" },
                        new[] { "subflow5" },
                        new[] { "you don't belong here!!!" },
                    },
                    Output = new[]
                    { 
                        new[] { "<tename>1</tename>234567891abcdefghi23abc45abc67abc89abcdefghiijk01<tename>23a</tename><tename>b</tename><tename>c4</tename>5<tename>6</tename>" },
                        new[] { "<tename>subflow1</tename>" },
                        new[] { "subflow2" },
                        new[] { "subflow3" },
                        new[] { "<tename>subflow4</tename>abcdef" },
                        new[] { "subflow5" },
                        new[] { "you don't <tename>belong</tename> here!!!" },
                    },
                }
            };

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_ExistingTerms()
        {
            #region Xliff
            string input = string.Format(XliffTemplate, @"
                  <file original='pc.txt' source-language='en' target-language='lv'>
                    <body>
                      <trans-unit id='pc'>
                        <source>
                           <sub><mrk mtype='term'>Personal computer</mrk></sub>
                           <sub><mrk mtype='term'>Personal computer 2</mrk></sub>
                           <sub><mrk mtype='x-its-term-no'>Personal computer</mrk></sub>
                           <sub><mrk mtype='x-its-term-no'>Personal computer 2</mrk></sub>
                           <sub>Personal <mrk mtype='term'>computer</mrk></sub>
                           <sub>Personal <mrk mtype='x-its-term-no'>computer</mrk></sub>
                        </source>
                       </trans-unit>
                    </body>
                  </file>");

            string output = string.Format(XliffTemplateWithItsItsx, @"
                   <file original='pc.txt' source-language='en' target-language='lv'>
                    <body>
                      <trans-unit id='pc'>
                        <source>
                           <sub><mrk mtype='term'>Personal computer</mrk></sub>
                           <sub><mrk mtype='term'>Personal computer 2</mrk></sub>
                           <sub><mrk mtype='x-its-term-no'>Personal computer</mrk></sub>
                           <sub><mrk mtype='x-its-term-no'>Personal computer 2</mrk></sub>
                           <sub>Personal <mrk mtype='term'>computer</mrk></sub>
                           <sub>Personal <mrk mtype='x-its-term-no'>computer</mrk></sub>
                        </source>
                       </trans-unit>
                    </body>
                  </file>");
            #endregion

            List<Chunk> chunks = new List<Chunk>
            {
                new Chunk
                {
                    SourceLanguage = "en",
                    Input = new[]
                    { 
                        new[] { "Personal computer" },
                        new[] { "Personal computer 2" },
                        new[] { "Personal computer" },
                        new[] { "Personal computer 2" },
                        new[] { "Personal computer" },
                        new[] { "Personal computer" },
                    },
                    Output = new[]
                    { 
                        new[] { "<tename>Personal computer</tename>" },
                        new[] { "Personal computer 2" },
                        new[] { "<tename>Personal computer</tename>" },
                        new[] { "Personal computer 2" },
                        new[] { "<tename>Personal computer</tename>" },
                        new[] { "<tename>Personal computer</tename>" },
                    },
                }
            };

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_SegmentedText()
        {
            #region Xliff
            string input = string.Format(XliffTemplate, @"
                    <file original='EXAMPLE1_HTML5.html.xlf' source-language='en-US' target-language='fr-FR'>
                        <body>
                          <trans-unit id='3'>
                           <source xml:lang='en-us'>From the canyons of Arizona In 80 days You will come</source>
                           <seg-source>
                             <mrk mid='0' mtype='seg'>From the canyons of Arizona</mrk> 
                             <mrk mid='1' mtype='seg'>In 80 days</mrk> 
                             <mrk mid='2' mtype='seg'>You will come</mrk>
                           </seg-source>
                          </trans-unit>
                         </body>
                      </file>");

            string output = string.Format(XliffTemplateWithItsItsxAnnRef, @"
                     <file original='EXAMPLE1_HTML5.html.xlf' source-language='en-US' target-language='fr-FR'>
                        <body>
                          <trans-unit id='3'>
                           <source xml:lang='en-us'><mrk mtype='term'>From the canyons of Arizona In 80 days You will come</mrk></source>
                           <seg-source>
                             <mrk mid='0' mtype='seg'>From the canyons of Arizona</mrk> 
                             <mrk mid='1' mtype='seg'>In 80 days</mrk> 
                             <mrk mid='2' mtype='seg'><mrk mtype='term'>You will come</mrk></mrk>
                           </seg-source>
                          </trans-unit>
                         </body>
                      </file>");
            #endregion

            List<Chunk> chunks = new List<Chunk>
            {
                new Chunk
                {
                    SourceLanguage = "en-us",
                    Input = new[]
                    { 
                        new[] { "From the canyons of Arizona In 80 days You will come" },
                        new[] { "From the canyons of Arizona In 80 days You will come" },
                    },
                    Output = new[]
                    { 
                        new[] { "<tename>From the canyons of Arizona In 80 days You will come</tename>" },
                        new[] { "From the canyons of <tename>Arizona In 80</tename> days <tename>You will come</tename>" },
                    },
                }
            };

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_ItsRules()
        {
            #region Xliff
            string input = string.Format(XliffTemplateWithItsItsx, @"
                <its:rules version='2.0'>
                  <its:withinTextRule selector='//*[@inline=""yes""]' withinText='yes'/>
                  <its:localeFilterRule selector='//*[@hidden=""yes""]' localeFilterList=''/>
                  <its:termRule selector='//*[local-name()=""term""]' term='yes'/>
                  <its:termRule selector='//*[local-name()=""noterm""]' term='no'/>
                  <its:langRule selector='//*' langPointer='@lang'/>
                  <its:domainRule selector='//*' domainPointer='@domains'/>
                 </its:rules>
                 <file original='its.txt' source-language='en'>
                  <body>
                   <trans-unit id='global'>
                     <source>
                       <sub>1<sub inline='yes'>2</sub>3</sub>
                       <sub>a<g hidden='yes'>b</g>c</sub>
                       <sub><term>Personal computer</term> <noterm>Personal computer 2</noterm></sub>
                       <sub lang='lv'>čau</sub>
                       <sub domains='auto, finance'>cars and money</sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>
                 <file original='its.txt' source-language='en' domains='dddomain'>
                  <body>
                   <trans-unit id='local'>
                     <source>
                       <sub>1<sub its:withinText='yes'>2</sub>3</sub>
                       <sub>a<g its:localeFilterList=''>b</g>c</sub>
                       <sub><g its:term='yes'>Personal computer</g> <g its:term='no'>Personal computer 2</g></sub>
                       <sub xml:lang='lv'>čau</sub>
                     </source>
                   </trans-unit>
                  </body>
                 </file>");

            string output = string.Format(XliffTemplateWithItsItsx, @"
                 <its:rules version='2.0'>
                  <its:withinTextRule selector='//*[@inline=""yes""]' withinText='yes'/>
                  <its:localeFilterRule selector='//*[@hidden=""yes""]' localeFilterList=''/>
                  <its:termRule selector='//*[local-name()=""term""]' term='yes'/>
                  <its:termRule selector='//*[local-name()=""noterm""]' term='no'/>
                  <its:langRule selector='//*' langPointer='@lang'/>
                  <its:domainRule selector='//*' domainPointer='@domains'/>
                 </its:rules>
                <file original='its.txt' source-language='en'>
                  <body>
                   <trans-unit id='global'>
                     <source>
                       <sub>1<sub inline='yes'>2</sub>3</sub>
                       <sub>a<g hidden='yes'>b</g>c</sub>
                       <sub><term>Personal computer</term> <noterm>Personal computer 2</noterm></sub>
                       <sub lang='lv'>čau</sub>
                       <sub domains='auto, finance'><mrk mtype='term' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'>cars and money</mrk></sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>
                 <file original='its.txt' source-language='en' domains='dddomain'>
                  <body>
                   <trans-unit id='local'>
                     <source>
                       <sub>1<sub its:withinText='yes'>2</sub>3</sub>
                       <sub>a<g its:localeFilterList=''>b</g>c</sub>
                       <sub><g its:term='yes'>Personal computer</g> <g its:term='no'>Personal computer 2</g></sub>
                       <sub xml:lang='lv'>čau</sub>
                     </source>
                   </trans-unit>
                  </body>
                 </file>");
            #endregion

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Input = new[] { new[] { "123" }, new[] { "a", "c" }, new[] { "Personal computer"}, new[] { "Personal computer 2" } },
                Output = new[] { new[] { "<tename>123</tename>" }, new[] { "a", "c" }, new[] { "<tename>Personal computer</tename>"}, new[] { "<tename>Personal computer 2</tename>" } }
            });
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Domain = "auto",
                Input = new[] { new[] { "cars and money" } },
                Output = new[] { new[] { "<tename>cars and money</tename>" } }
            });
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Domain = "finance",
                Input = new[] { new[] { "cars and money" } },
                Output = new[] { new[] { "<tename>cars and money</tename>" } }
            });
            chunks.Add(new Chunk
            {
                SourceLanguage = "lv",
                Input = new[] { new[] { "čau" } },
                Output = new[] { new[] { "čau" } }
            });
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Domain = "dddomain",
                Input = new[] { new[] { "123" }, new[] { "a", "c" }, new[] { "Personal computerPersonal computer 2" } },
                Output = new[] { new[] { "<tename>123</tename>" }, new[] { "a", "c" }, new[] { "<tename>Personal computer</tename><tename>Personal computer 2</tename>" } }
            });
            chunks.Add(new Chunk
            {
                SourceLanguage = "lv",
                Domain = "dddomain",
                Input = new[] { new[] { "čau" } },
                Output = new[] { new[] { "čau" } }
            });

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_ItsMapping()
        {
            #region Xliff
            string input = string.Format(XliffTemplateWithItsItsx, @"
                 <file original='its-xliff.txt' source-language='en'>
                  <body>
                   <trans-unit id='mapping' xmlns:itsx='http://www.w3.org/ns/its-xliff/'>
                     <source>
                       <sub>123a<g>b</g>c<mrk mtype='abbrev'>d</mrk></sub>
                       <sub xml:space='preserve'><mrk mtype='term'>Personal computer</mrk> <mrk mtype='x-its-term-no'>Personal computer 2</mrk></sub>
                       <sub><mrk mtype='x-its' xml:lang='lv'>čau</mrk></sub>
                       <sub its:localeFilterList=''>hidden <mrk mtype='x-its' its:localeFilterList='*'>visible</mrk></sub>
                       <sub itsx:domains='auto, finance'>cars and money</sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>");

            string output = string.Format(XliffTemplateWithItsItsx, @"
                 <file original='its-xliff.txt' source-language='en'>
                  <body>
                   <trans-unit id='mapping' xmlns:itsx='http://www.w3.org/ns/its-xliff/'>
                     <source>
                       <sub><mrk mtype='term' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'>123</mrk>a<g>b</g>c<mrk mtype='abbrev'><mrk mtype='term' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'>d</mrk></mrk></sub>
                       <sub xml:space='preserve'><mrk mtype='term'>Personal computer</mrk> <mrk mtype='x-its-term-no'>Personal computer 2</mrk></sub>
                       <sub><mrk mtype='x-its' xml:lang='lv'><mrk mtype='term' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'>čau</mrk></mrk></sub>
                       <sub its:localeFilterList=''>hidden <mrk mtype='x-its' its:localeFilterList='*'><mrk mtype='term' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'>visible</mrk></mrk></sub>
                       <sub itsx:domains='auto, finance'><mrk mtype='term' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'>cars and money</mrk></sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>");
            #endregion

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Input = new[] { new[] { "123abcd" }, new[] { "Personal computer Personal computer 2" }, new[] { "visible" } },
                Output = new[] { new[] { "<tename>123</tename><tename>abc</tename><tename>d</tename>" }, new[] { "<tename>Personal computer</tename> <tename>Personal computer 2</tename>" }, new[] { "<tename>visible</tename>" } }
            });
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Domain = "auto",
                Input = new[] { new[] { "cars and money" } },
                Output = new[] { new[] { "<tename>cars and money</tename>" } }
            });
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Domain = "finance",
                Input = new[] { new[] { "cars and money" } },
                Output = new[] { new[] { "<tename>cars and money</tename>" } }
            });
            chunks.Add(new Chunk
            {
                SourceLanguage = "lv",
                Input = new[] { new[] { "čau" } },
                Output = new[] { new[] { "<tename>čau</tename>" } }
            });

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_Confidence()
        {
            #region Xliff
            string input = string.Format(XliffTemplateWithItsItsx, @"
                 <file original='confidence.txt' source-language='en'>
                  <body>
                   <trans-unit>
                     <source>
                       <sub>abc</sub>
                       <sub its:term='yes'>def</sub>
                       <sub its:term='no'>ghi</sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>");

            string output = string.Format(XliffTemplateWithItsItsx, @"
                 <file original='confidence.txt' source-language='en'>
                  <body>
                   <trans-unit>
                     <source>
                       <sub><mrk mtype='term' itsx:termConfidence='0.5' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'>abc</mrk></sub>
                       <sub its:term='yes'>def</sub>
                       <sub its:term='no'>ghi</sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>");
            #endregion

            List<Chunk> chunks = new List<Chunk>
            {
                new Chunk
                {
                    SourceLanguage = "en",
                    Input = new[] { new[] { "abc" }, new[] { "def" }, new[] { "ghi" } },
                    Output = new[]
                    {
                        new[] { "<tename score='0.5'>abc</tename>" },
                        new[] { "<tename score='0.5'>def</tename>" },
                        new[] { "<tename score='0.5'>ghi</tename>" },
                    }
                }
            };

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_AnnotatorsRef()
        {
            List<Chunk> chunks = new List<Chunk>();

            // no existing annotatorsRef
            Test("<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2'/>",
                 "<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/ns/its-xliff/' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'/>", chunks);

            // already has annotatorsRef
            Test("<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/ns/its-xliff/' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'/>",
                 "<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/ns/its-xliff/' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'/>", chunks);

            // already has annotatorsRef
            Test("<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/ns/its-xliff/' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service text-analysis|http://enrycher.ijs.si'/>",
                 "<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/ns/its-xliff/' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service text-analysis|http://enrycher.ijs.si'/>", chunks);

            // other annotatorsRefs
            Test("<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/ns/its-xliff/' its:annotatorsRef='text-analysis|http://enrycher.ijs.si terminology|http://www.cngl.ie/termchecker'/>",
                 "<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/ns/its-xliff/' its:annotatorsRef='text-analysis|http://enrycher.ijs.si terminology|http://www.cngl.ie/termchecker'/>", chunks);
        }

        [TestMethod]
        public void Xliff_AnnotatorsRef_Inline()
        {
            #region Xliff
            string input = string.Format(XliffTemplateWithItsItsx, @"
                 <file original='annrefs.txt' source-language='en'>
                  <body>
                   <trans-unit>
                     <source its:annotatorsRef='text-analysis|http://enrycher.ijs.si terminology|http://www.cngl.ie/termchecker'>
                       this is a term
                     </source>
                    </trans-unit>
                   </body>
                 </file>");

            string output = string.Format(XliffTemplateWithItsItsx, @"
                 <file original='annrefs.txt' source-language='en'>
                  <body>
                   <trans-unit>
                     <source its:annotatorsRef='text-analysis|http://enrycher.ijs.si terminology|http://www.cngl.ie/termchecker'>
                       this is a <mrk mtype='term' itsx:termConfidence='0.5' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'>term</mrk>
                     </source>
                    </trans-unit>
                   </body>
                 </file>");
            #endregion

            List<Chunk> chunks = new List<Chunk>
            {
                new Chunk
                {
                    SourceLanguage = "en",
                    Input = new[] { new[] { "this is a term" } },
                    Output = new[] { new[] { "this is a <tename score='0.5'>term</tename>" } }
                }
            };

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_NamespaceDeclaration()
        {
            List<Chunk> chunks = new List<Chunk>();

            // no its:
            Test("<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2'/>",
                 "<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/ns/its-xliff/' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'/>", chunks);

            // already has its:, it's ok
            Test("<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.w3.org/2005/11/its'/>",
                 "<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/ns/its-xliff/' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'/>", chunks);

            // already has its:, it's something else
            Test("<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.wtf.org'/>",
                 "<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.wtf.org' xmlns:its2='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/ns/its-xliff/' its2:annotatorsRef='terminology|http://tilde.com/term-annotation-service'/>", chunks);

            // already has itsx:
            Test("<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.wtf.org' xmlns:itsx='http://www.wtf.org/x'/>",
                 "<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.wtf.org' xmlns:itsx='http://www.wtf.org/x' xmlns:its2='http://www.w3.org/2005/11/its' xmlns:itsx2='http://www.w3.org/ns/its-xliff/' its2:annotatorsRef='terminology|http://tilde.com/term-annotation-service'/>", chunks);
        }

        [TestMethod]
        public void Xliff_Tbx()
        {
            #region Xliff
            string input = string.Format(XliffTemplateWithItsItsx, @"
                 <file original='tbx.txt' source-language='en'>
                  <header>
                   <note>...</note>
                  </header>
                  <body>
                   <trans-unit>
                     <source>
                       this is a term
                     </source>
                    </trans-unit>
                   </body>
                 </file>");

            string output = string.Format(XliffTemplateWithItsItsxAnnRef, @"
                 <file original='tbx.txt' source-language='en'>
                  <header>
                   <note>...</note>
                   <martif type='TBX' xmlns='http://www.ttt.org/oscarstandards/tbx/TBXcoreStructV02.dtd'>
                    <martifHeader>
                     <fileDesc>
                      <sourceDesc>
                       <p>Tilde Terminology Annotation Service</p>
                      </sourceDesc>
                     </fileDesc>
                     <encodingDesc>
                      <p type='XCSURI'>http://www.ttt.org/oscarstandards/tbx/TBXXCS.xcs</p>
                     </encodingDesc>
                    </martifHeader>
                    <text>
                     <body>
                      <termEntry for='etb-1' xml:id='tilde-tbx-etb-1'/>
                     </body>
                    </text>
                   </martif>
                  </header>
                  <body>
                   <trans-unit>
                     <source>
                       this is a <mrk itsx:termInfoRef='#tilde-tbx-etb-1' mtype='term'>term</mrk>
                     </source>
                    </trans-unit>
                   </body>
                 </file>");
            #endregion

            List<Chunk> chunks = new List<Chunk>
            {
                new Chunk
                {
                    SourceLanguage = "en",
                    Input = new[] { new[] { "this is a term" } },
                    Output = new[] { new[] { "this is a <tename termid='etb-1'>term</tename>" } }
                }
            };

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_Tbx_MultipleTerms()
        {
            #region Xliff
            string input = string.Format(XliffTemplateWithItsItsx, @"
                 <file original='tbx.txt' source-language='en'>
                  <header>
                   <note>...</note>
                  </header>
                  <body>
                   <trans-unit>
                     <source>
                       <sub>this is a term</sub>
                       <sub>this is another term</sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>");

            string output = string.Format(XliffTemplateWithItsItsxAnnRef, @"
                 <file original='tbx.txt' source-language='en'>
                  <header>
                   <note>...</note>
                   <martif type='TBX' xmlns='http://www.ttt.org/oscarstandards/tbx/TBXcoreStructV02.dtd'>
                    <martifHeader>
                     <fileDesc>
                      <sourceDesc>
                       <p>Tilde Terminology Annotation Service</p>
                      </sourceDesc>
                     </fileDesc>
                     <encodingDesc>
                      <p type='XCSURI'>http://www.ttt.org/oscarstandards/tbx/TBXXCS.xcs</p>
                     </encodingDesc>
                    </martifHeader>
                    <text>
                     <body>
                      <termEntry for='etb-1' xml:id='tilde-tbx-etb-1'/>
                      <termEntry for='etb-2' xml:id='tilde-tbx-etb-2'/>
                     </body>
                    </text>
                   </martif>
                  </header>
                  <body>
                   <trans-unit>
                     <source>
                       <sub>this is a <mrk itsx:termInfoRef='#tilde-tbx-etb-1' mtype='term'>term</mrk></sub>
                       <sub>this is <mrk itsx:termInfoRef='#tilde-tbx-etb-2' mtype='term'>another term</mrk></sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>");
            #endregion

            List<Chunk> chunks = new List<Chunk>
            {
                new Chunk
                {
                    SourceLanguage = "en",
                    Input = new[] { new[] { "this is a term" }, new[] { "this is another term" } },
                    Output = new[] { new[] { "this is a <tename termid='etb-1'>term</tename>" }, new[] { "this is <tename termid='etb-2'>another term</tename>" } }
                }
            };

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_Tbx_MultipleFiles_MultipleTerms()
        {
            #region Xliff
            string input = string.Format(XliffTemplateWithItsItsx, @"
                 <file original='tbx.txt' source-language='en'>
                  <header>
                   <note>...</note>
                  </header>
                  <body>
                   <trans-unit>
                     <source>
                       <sub>this is a term</sub>
                       <sub>this is another term</sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>
                 <file original='tbx2.txt' source-language='en'>
                  <header>
                   <note>...</note>
                  </header>
                  <body>
                   <trans-unit>
                     <source>
                       <sub>this is a term2</sub>
                       <sub>this is another term2</sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>");

            string output = string.Format(XliffTemplateWithItsItsxAnnRef, @"
                 <file original='tbx.txt' source-language='en'>
                  <header>
                   <note>...</note>
                   <martif type='TBX' xmlns='http://www.ttt.org/oscarstandards/tbx/TBXcoreStructV02.dtd'>
                    <martifHeader>
                     <fileDesc>
                      <sourceDesc>
                       <p>Tilde Terminology Annotation Service</p>
                      </sourceDesc>
                     </fileDesc>
                     <encodingDesc>
                      <p type='XCSURI'>http://www.ttt.org/oscarstandards/tbx/TBXXCS.xcs</p>
                     </encodingDesc>
                    </martifHeader>
                    <text>
                     <body>
                      <termEntry for='etb-1' xml:id='tilde-tbx-etb-1'/>
                      <termEntry for='etb-2' xml:id='tilde-tbx-etb-2'/>
                     </body>
                    </text>
                   </martif>
                  </header>
                  <body>
                   <trans-unit>
                     <source>
                       <sub>this is a <mrk itsx:termInfoRef='#tilde-tbx-etb-1' mtype='term'>term</mrk></sub>
                       <sub>this is <mrk itsx:termInfoRef='#tilde-tbx-etb-2' mtype='term'>another term</mrk></sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>
                 <file original='tbx2.txt' source-language='en'>
                  <header>
                   <note>...</note>
                   <martif type='TBX' xmlns='http://www.ttt.org/oscarstandards/tbx/TBXcoreStructV02.dtd'>
                    <martifHeader>
                     <fileDesc>
                      <sourceDesc>
                       <p>Tilde Terminology Annotation Service</p>
                      </sourceDesc>
                     </fileDesc>
                     <encodingDesc>
                      <p type='XCSURI'>http://www.ttt.org/oscarstandards/tbx/TBXXCS.xcs</p>
                     </encodingDesc>
                    </martifHeader>
                    <text>
                     <body>
                      <termEntry for='etb-3' xml:id='tilde-tbx-etb-3'/>
                      <termEntry for='etb-4' xml:id='tilde-tbx-etb-4'/>
                     </body>
                    </text>
                   </martif>
                  </header>
                  <body>
                   <trans-unit>
                     <source>
                       <sub>this is a <mrk itsx:termInfoRef='#tilde-tbx-etb-3' mtype='term'>term2</mrk></sub>
                       <sub>this is <mrk itsx:termInfoRef='#tilde-tbx-etb-4' mtype='term'>another term2</mrk></sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>");
            #endregion

            List<Chunk> chunks = new List<Chunk>
            {
                new Chunk
                {
                    SourceLanguage = "en",
                    Input = new[] { new[] { "this is a term" }, new[] { "this is another term" } },
                    Output = new[] { new[] { "this is a <tename termid='etb-1'>term</tename>" }, new[] { "this is <tename termid='etb-2'>another term</tename>" } }
                },
                new Chunk
                {
                    SourceLanguage = "en",
                    Input = new[] { new[] { "this is a term2" }, new[] { "this is another term2" } },
                    Output = new[] { new[] { "this is a <tename termid='etb-3'>term2</tename>" }, new[] { "this is <tename termid='etb-4'>another term2</tename>" } }
                }
            };

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_Tbx_NoHeader()
        {
            #region Xliff
            string input = string.Format(XliffTemplateWithItsItsx, @"
                 <file original='tbx.txt' source-language='en'>
                  <body>
                   <trans-unit>
                     <source>
                       this is a term
                     </source>
                    </trans-unit>
                   </body>
                 </file>");

            string output = string.Format(XliffTemplateWithItsItsxAnnRef, @"
                 <file original='tbx.txt' source-language='en'>
                  <header>
                   <martif type='TBX' xmlns='http://www.ttt.org/oscarstandards/tbx/TBXcoreStructV02.dtd'>
                    <martifHeader>
                     <fileDesc>
                      <sourceDesc>
                       <p>Tilde Terminology Annotation Service</p>
                      </sourceDesc>
                     </fileDesc>
                     <encodingDesc>
                      <p type='XCSURI'>http://www.ttt.org/oscarstandards/tbx/TBXXCS.xcs</p>
                     </encodingDesc>
                    </martifHeader>
                    <text>
                     <body>
                      <termEntry for='etb-1' xml:id='tilde-tbx-etb-1'/>
                     </body>
                    </text>
                   </martif>
                  </header>
                  <body>
                   <trans-unit>
                     <source>
                       this is a <mrk itsx:termInfoRef='#tilde-tbx-etb-1' mtype='term'>term</mrk>
                     </source>
                    </trans-unit>
                   </body>
                 </file>");
            #endregion

            List<Chunk> chunks = new List<Chunk>
            {
                new Chunk
                {
                    SourceLanguage = "en",
                    Input = new[] { new[] { "this is a term" } },
                    Output = new[] { new[] { "this is a <tename termid='etb-1'>term</tename>" } }
                }
            };

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_Tbx_MultipleFiles()
        {
            #region Xliff
            string input = string.Format(XliffTemplateWithItsItsx, @"
                 <file original='tbx.txt' source-language='en'>
                  <body>
                   <trans-unit>
                     <source>
                       this is a term
                     </source>
                    </trans-unit>
                   </body>
                 </file>
                 <file original='tbx2.txt' source-language='en'>
                  <body>
                   <trans-unit>
                     <source>
                       this is a term
                     </source>
                    </trans-unit>
                   </body>
                 </file>");

            string output = string.Format(XliffTemplateWithItsItsxAnnRef, @"
                 <file original='tbx.txt' source-language='en'>
                  <header>
                   <martif type='TBX' xmlns='http://www.ttt.org/oscarstandards/tbx/TBXcoreStructV02.dtd'>
                    <martifHeader>
                     <fileDesc>
                      <sourceDesc>
                       <p>Tilde Terminology Annotation Service</p>
                      </sourceDesc>
                     </fileDesc>
                     <encodingDesc>
                      <p type='XCSURI'>http://www.ttt.org/oscarstandards/tbx/TBXXCS.xcs</p>
                     </encodingDesc>
                    </martifHeader>
                    <text>
                     <body>
                      <termEntry for='etb-1' xml:id='tilde-tbx-etb-1'/>
                     </body>
                    </text>
                   </martif>
                  </header>
                  <body>
                   <trans-unit>
                     <source>
                       this is a <mrk itsx:termInfoRef='#tilde-tbx-etb-1' mtype='term'>term</mrk>
                     </source>
                    </trans-unit>
                   </body>
                 </file>
                 <file original='tbx2.txt' source-language='en'>
                  <header>
                   <martif type='TBX' xmlns='http://www.ttt.org/oscarstandards/tbx/TBXcoreStructV02.dtd'>
                    <martifHeader>
                     <fileDesc>
                      <sourceDesc>
                       <p>Tilde Terminology Annotation Service</p>
                      </sourceDesc>
                     </fileDesc>
                     <encodingDesc>
                      <p type='XCSURI'>http://www.ttt.org/oscarstandards/tbx/TBXXCS.xcs</p>
                     </encodingDesc>
                    </martifHeader>
                    <text>
                     <body>
                      <termEntry for='etb-2' xml:id='tilde-tbx-etb-2'/>
                     </body>
                    </text>
                   </martif>
                  </header>
                  <body>
                   <trans-unit>
                     <source>
                       this is a <mrk itsx:termInfoRef='#tilde-tbx-etb-2' mtype='term'>term</mrk>
                     </source>
                    </trans-unit>
                   </body>
                 </file>");
            #endregion

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Input = new[] { new[] { "this is a term" } },
                Output = new[] { new[] { "this is a <tename termid='etb-1'>term</tename>" } }
            });
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Input = new[] { new[] { "this is a term" } },
                Output = new[] { new[] { "this is a <tename termid='etb-2'>term</tename>" } }
            });

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_Tbx_ExistingIDs()
        {
            #region Xliff
            string input = string.Format(XliffTemplateWithItsItsx, @"
                 <file original='tbx.txt' source-language='en'>
                  <header xml:id='tilde-tbx-etb-1'>
                  </header>
                  <body>
                   <trans-unit>
                     <source>
                       this is a term
                     </source>
                    </trans-unit>
                   </body>
                 </file>");

            string output = string.Format(XliffTemplateWithItsItsxAnnRef, @"
                 <file original='tbx.txt' source-language='en'>
                  <header xml:id='tilde-tbx-etb-1'>
                   <martif type='TBX' xmlns='http://www.ttt.org/oscarstandards/tbx/TBXcoreStructV02.dtd'>
                    <martifHeader>
                     <fileDesc>
                      <sourceDesc>
                       <p>Tilde Terminology Annotation Service</p>
                      </sourceDesc>
                     </fileDesc>
                     <encodingDesc>
                      <p type='XCSURI'>http://www.ttt.org/oscarstandards/tbx/TBXXCS.xcs</p>
                     </encodingDesc>
                    </martifHeader>
                    <text>
                     <body>
                      <termEntry for='etb-1' xml:id='tilde-tbx-etb-1-1'/>
                     </body>
                    </text>
                   </martif>
                  </header>
                  <body>
                   <trans-unit>
                     <source>
                       this is a <mrk itsx:termInfoRef='#tilde-tbx-etb-1-1' mtype='term'>term</mrk>
                     </source>
                    </trans-unit>
                   </body>
                 </file>");
            #endregion

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Input = new[] { new[] { "this is a term" } },
                Output = new[] { new[] { "this is a <tename termid='etb-1'>term</tename>" } }
            });

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_Versions_10()
        {
            #region Xliff
            string input = @"
                <!DOCTYPE xliff PUBLIC ""-//XLIFF//DTD XLIFF//EN"" ""http://www.oasis-open.org/committees/xliff/documents/xliff.dtd"">
                <xliff version='1.0'>
                 <file original='its-versions.txt' source-language='en'>
                  <body>
                   <trans-unit id='versions'>
                     <source>
                       <sub>hello<g>world</g></sub>
                       <sub><mrk mtype='term'>Personal computer</mrk></sub>
                       <sub><mrk mtype='x-its-term-no'>Personal computer 2</mrk></sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>
                </xliff>";

            string output = @"
               <!DOCTYPE xliff PUBLIC ""-//XLIFF//DTD XLIFF//EN"" ""http://www.oasis-open.org/committees/xliff/documents/xliff.dtd"">
                <xliff version='1.0' xmlns:its='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/ns/its-xliff/'>
                 <file original='its-versions.txt' source-language='en'>
                  <body>
                   <trans-unit id='versions'>
                     <source>
                       <sub><mrk mtype='term' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'>hello</mrk><g>world</g></sub>
                       <sub><mrk mtype='term'>Personal computer</mrk></sub>
                       <sub><mrk mtype='x-its-term-no'>Personal computer 2</mrk></sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>
                </xliff>";
            #endregion

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Input = new[] { new[] { "helloworld" }, new[] { "Personal computer" }, new[] { "Personal computer 2" } },
                Output = new[] { new[] { "<tename>hello</tename>world" }, new[] { "<tename>Personal computer</tename>" }, new[] { "<tename>Personal computer 2</tename>" } }
            });

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_Versions_11()
        {
            #region Xliff
            string input = @"
                <xliff version='1.1' xmlns='urn:oasis:names:tc:xliff:document:1.1'>
                 <file original='its-versions.txt' source-language='en'>
                  <body>
                   <trans-unit id='versions'>
                     <source>
                       <sub>hello<g>world</g></sub>
                       <sub><mrk mtype='term'>Personal computer</mrk></sub>
                       <sub><mrk mtype='x-its-term-no'>Personal computer 2</mrk></sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>
                </xliff>";

            string output = @"
                <xliff version='1.1' xmlns='urn:oasis:names:tc:xliff:document:1.1' xmlns:its='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/ns/its-xliff/'>
                 <file original='its-versions.txt' source-language='en'>
                  <body>
                   <trans-unit id='versions'>
                     <source>
                       <sub><mrk mtype='term' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'>hello</mrk><g>world</g></sub>
                       <sub><mrk mtype='term'>Personal computer</mrk></sub>
                       <sub><mrk mtype='x-its-term-no'>Personal computer 2</mrk></sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>
                </xliff>";
            #endregion

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Input = new[] { new[] { "helloworld" }, new[] { "Personal computer" }, new[] { "Personal computer 2" } },
                Output = new[] { new[] { "<tename>hello</tename>world" }, new[] { "<tename>Personal computer</tename>" }, new[] { "<tename>Personal computer 2</tename>" } }
            });

            Test(input, output, chunks);
        }

        [TestMethod]
        public void Xliff_Versions_12()
        {
            #region Xliff
            string input = string.Format(XliffTemplate, @"
                 <file original='its-versions.txt' source-language='en'>
                  <body>
                   <trans-unit id='versions'>
                     <source>
                       <sub>hello<g>world</g></sub>
                       <sub><mrk mtype='term'>Personal computer</mrk></sub>
                       <sub><mrk mtype='x-its-term-no'>Personal computer 2</mrk></sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>");

            string output = string.Format(XliffTemplateWithItsItsx, @"
                 <file original='its-versions.txt' source-language='en'>
                  <body>
                   <trans-unit id='versions'>
                     <source>
                       <sub><mrk mtype='term' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'>hello</mrk><g>world</g></sub>
                       <sub><mrk mtype='term'>Personal computer</mrk></sub>
                       <sub><mrk mtype='x-its-term-no'>Personal computer 2</mrk></sub>
                     </source>
                    </trans-unit>
                   </body>
                 </file>");
            #endregion

            List<Chunk> chunks = new List<Chunk>();
            chunks.Add(new Chunk
            {
                SourceLanguage = "en",
                Input = new[] { new[] { "helloworld" }, new[] { "Personal computer" }, new[] { "Personal computer 2" } },
                Output = new[] { new[] { "<tename>hello</tename>world" }, new[] { "<tename>Personal computer</tename>" }, new[] { "<tename>Personal computer 2</tename>" } }
            });

            Test(input, output, chunks);
        }

        #region
        const string XliffTemplate = "<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2'>{0}</xliff>";
        const string XliffTemplateWithIts = "<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.w3.org/2005/11/its'>{0}</xliff>";
        const string XliffTemplateWithItsAnnRef = "<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.w3.org/2005/11/its' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'>{0}</xliff>";
        const string XliffTemplateWithItsItsx = "<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/ns/its-xliff/'>{0}</xliff>";
        const string XliffTemplateWithItsItsxAnnRef = "<xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:its='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/ns/its-xliff/' its:annotatorsRef='terminology|http://tilde.com/term-annotation-service'>{0}</xliff>";

        private void Test(string input, string output, List<Chunk> chunks)
        {
            ApiDocument p = new ApiDocument();
            p.Content = OneLine(input);
            p.UseStatisticalExtraction = true;
            p.UseTermBankExtraction = true;

            TestableXliffAnnotator annotator = new TestableXliffAnnotator(p);
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

            string outputDocumentString = annotator.Annotate().Result;
            XDocument expectedDocument = XDocument.Parse(OneLine(output));
            XDocument outputDocument = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(outputDocumentString)));

            Assert.AreEqual(expectedDocument.ToString(), outputDocument.ToString());
        }

        private class TestableXliffAnnotator : XliffAnnotator
        {
            public TestableXliffAnnotator(ApiDocument source)
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
                XElement termEntry = new XElement("termEntry", new XAttribute("for", id));

                return new XDocument(
                   new XDocumentType("martif", null, "http://www.ttt.org/oscarstandards/tbx/TBXcoreStructV02.dtd", null),
                   new XElement("martif", new XAttribute("type", "TBX"),
                       new XElement("martifHeader",
                           new XElement("fileDesc", new XElement("sourceDesc", new XElement("p", "Tilde Terminology Annotation Service"))),
                           new XElement("encodingDesc", new XElement("p", new XAttribute("type", "XCSURI"), "http://www.ttt.org/oscarstandards/tbx/TBXXCS.xcs"))),
                       new XElement("text",
                           new XElement("body", termEntry))));
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
