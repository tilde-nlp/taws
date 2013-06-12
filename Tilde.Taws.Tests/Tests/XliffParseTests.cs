using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Tilde.Taws.Models;

namespace Tilde.Taws.Tests
{
    [TestClass]
    public class XliffParseTests
    {
        #region Test Suite
        #region Examples
        [TestMethod]
        public void Xliff_File_TestSuite_Example1_HTML5()
        {
            Test("testsuite.Example1_HTML5.html.xlf", chunks =>
            {
                Assert.AreEqual(2 /* 1 file: en & fr */, chunks.Count);
                Assert.AreEqual(3 * 25, chunks.Sum(c => c.Value.Count));
            });
        }

        [TestMethod]
        public void Xliff_File_TestSuite_Example1_XML()
        {
            Test("testsuite.Example1_XML.xml.xlf", chunks =>
            {
                Assert.AreEqual(2 /* 1 file: en & fr */, chunks.Count);
                Assert.AreEqual(3 * 4, chunks.Sum(c => c.Value.Count));
            });
        }

        [TestMethod]
        public void Xliff_File_TestSuite_mostofits20()
        {
            Test("testsuite.mostofits20.xml.xlf", chunks =>
            {
                Assert.AreEqual(2 /* 1 file: en & fr */, chunks.Count);
                Assert.AreEqual(3 * 7, chunks.Sum(c => c.Value.Count));
            });
        }
        #endregion

        #region Roundtrip
        [TestMethod]
        public void Xliff_File_TestSuite_Roundtrip_MT_Moses()
        {
            Test("testsuite.roundtrip.EX-xliff-prov-rt-1-post-MT-moses.xlf", chunks => 
            {
            });
        }
        #endregion
        #endregion

        #region Examples
        [TestMethod]
        public void Xliff_Example_MechanicalEngineering()
        {
            string xliff = @"<?xml version='1.0' encoding='UTF-8'?>
                <xliff version='1.2' xmlns='urn:oasis:names:tc:xliff:document:1.2' xmlns:okp='okapi-framework:xliff-extensions' xmlns:its='http://www.w3.org/2005/11/its' xmlns:itsx='http://www.w3.org/2008/12/its-extensions' its:version='2.0'>
                 <file original='Text_example_EN_1.txt' source-language='en-us' target-language='lv-lv' datatype='x-text/plain'>
                  <body>
                   <trans-unit id='1'>
                    <source xml:lang='en-us'>
                      Mechanical engineering is a discipline of engineering that applies the principles of physics and materials science for analysis, design, manufacturing, and maintenance of mechanical systems. 
                      It is the branch of engineering that involves the production and usage of heat and mechanical power for the design, production, and operation of machines and tools. 
                      It is one of the oldest and broadest engineering disciplines. 
                      The engineering field requires an understanding of core concepts including mechanics, kinematics, thermodynamics, materials science, structural analysis, and electricity. 
                      Mechanical engineers use these core principles along with tools like computer-aided engineering and product lifecycle management to design and analyze manufacturing plants, industrial equipment and machinery, heating and cooling systems, transport systems, aircraft, watercraft, robotics, medical devices, and others. 
                      Mechanical engineering emerged as a field during the industrial revolution in Europe in the 18th century; however, its development can be traced back several thousand years around the world. 
                      Mechanical engineering science emerged in the 19th century as a result of developments in the field of physics. 
                      The field has continually evolved to incorporate advancements in technology, and mechanical engineers today are pursuing developments in such fields as composites, mechatronics, and nanotechnology. 
                      Mechanical engineering overlaps with aerospace engineering, building services engineering, metallurgical engineering, marine engineering, civil engineering, electrical engineering, petroleum engineering, manufacturing engineering, and chemical engineering to varying amounts. 
                      Mechanical engineers also work in the field of Biomedical engineering, specifically with biomechanics, transport phenomena, biomechatronics, bionanotechnology and modeling of biological systems, like soft tissue mechanics.
                    </source>
                    <seg-source>
                      <mrk mid='0' mtype='seg'>Mechanical engineering is a discipline of engineering that applies the principles of physics and materials science for analysis, design, manufacturing, and maintenance of mechanical systems. </mrk>
                      <mrk mid='1' mtype='seg'>It is the branch of engineering that involves the production and usage of heat and mechanical power for the design, production, and operation of machines and tools. </mrk>
                      <mrk mid='2' mtype='seg'>It is one of the oldest and broadest engineering disciplines. </mrk>
                      <mrk mid='3' mtype='seg'>The engineering field requires an understanding of core concepts including mechanics, kinematics, thermodynamics, materials science, structural analysis, and electricity. </mrk>
                      <mrk mid='4' mtype='seg'>Mechanical engineers use these core principles along with tools like computer-aided engineering and product lifecycle management to design and analyze manufacturing plants, industrial equipment and machinery, heating and cooling systems, transport systems, aircraft, watercraft, robotics, medical devices, and others. </mrk>
                      <mrk mid='5' mtype='seg'>Mechanical engineering emerged as a field during the industrial revolution in Europe in the 18th century; however, its development can be traced back several thousand years around the world. </mrk>
                      <mrk mid='6' mtype='seg'>Mechanical engineering science emerged in the 19th century as a result of developments in the field of physics. </mrk>
                      <mrk mid='7' mtype='seg'>The field has continually evolved to incorporate advancements in technology, and mechanical engineers today are pursuing developments in such fields as composites, mechatronics, and nanotechnology. </mrk>
                      <mrk mid='8' mtype='seg'>Mechanical engineering overlaps with aerospace engineering, building services engineering, metallurgical engineering, marine engineering, civil engineering, electrical engineering, petroleum engineering, manufacturing engineering, and chemical engineering to varying amounts. </mrk>
                      <mrk mid='9' mtype='seg'>Mechanical engineers also work in the field of Biomedical engineering, specifically with biomechanics, transport phenomena, biomechatronics, bionanotechnology and modeling of biological systems, like soft tissue mechanics.</mrk>
                    </seg-source>
                    <target xml:lang='lv-lv'></target>
                   </trans-unit>
                  </body>
                 </file>
                </xliff>";

            ApiDocument doc = new ApiDocument();
            doc.Content = xliff;
            doc.UseTermBankExtraction = true;
            doc.UseStatisticalExtraction = true;
            doc.Language = "en";
            Test(doc, (chunks) =>
            {
                Assert.AreEqual(1, chunks.Count);
            });
        }
        #endregion

        private string Test(ApiDocument doc, Action<Chunker.ChunkCollection> annotation = null)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            TestableXliffAnnotator annotator = new TestableXliffAnnotator(doc);
            watch.Stop();
            Debug.WriteLine("ITS: " + watch.Elapsed);
            annotator.Annotation = annotation;
            watch.Restart();
            string result = annotator.Annotate().Result;
            watch.Stop();
            Debug.WriteLine("XLIFF Annotator: " + watch.Elapsed);
            return result;
        }

        private string Test(string filename, Action<Chunker.ChunkCollection> annotation = null)
        {
            ApiDocument doc = new ApiDocument();
            doc.UseStatisticalExtraction = true;
            doc.UseTermBankExtraction = true;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string path = assembly.GetName().Name + ".TestData.xliff." + filename;
            Stream stream = assembly.GetManifestResourceStream(path);
            if (stream == null)
                throw new Exception("File not found " + filename);

            using (StreamReader reader = new StreamReader(stream))
                doc.Content = reader.ReadToEnd();

            return Test(doc, annotation);
        }

        private class TestableXliffAnnotator : XliffAnnotator
        {
            public TestableXliffAnnotator(ApiDocument source)
                : base(source)
            {
            }

            public Action<Chunker.ChunkCollection> Annotation
            {
                get;
                set;
            }

            protected override async Task Annotate(Chunker.ChunkCollection chunks)
            {
                Assert.IsTrue(chunks.Count >= Root.Elements(Root.Name.Namespace + "file").Count());

                if (Annotation != null)
                    Annotation(chunks);
            }
        }
    }
}
