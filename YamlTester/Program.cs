using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Yaml;
using NUnit.Framework;

namespace YamlTester
{
    [TestFixture]
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var directory = new DirectoryInfo("Tests");
            foreach (var item in directory.GetFiles())
            {
                Console.WriteLine(item.FullName);
                var reader = YamlReader.LoadFile(item.FullName);
                if (reader.Token != YaTools.Yaml.Token.EOS)
                    Console.WriteLine("INCOMPLETE: " + item.Name);

                Console.Read();
            }
        }

        [Test]
        public void Example_201()
        {
            var reader = YamlReader.LoadFile(@"Tests\Example_2.01.yaml");
            var docs = reader.Documents;
            Assert.AreEqual(1, docs.Length);
            var doc = docs[0];
            
            var seq = doc.Root as Sequence;
            Assert.IsNotNull(seq);
            Assert.AreEqual(3, seq.Count);
            Assert.IsInstanceOf<Scalar>(seq[0]);
            Assert.AreEqual("Mark McGwire", seq[0].ToString());
            Assert.AreEqual("Sammy Sosa", seq[1].ToString());
            Assert.AreEqual("Ken Griffey", seq[2].ToString());
        }

        [Test]
        public void Example_207()
        {
            var reader = YamlReader.LoadFile(@"Tests\Example_2.07.yaml");
            var docs = reader.Documents;
            Assert.AreEqual(2, docs.Length);
            var doc = docs[0];
            var seq = doc.Root as Sequence;

            Assert.IsNotNull(seq);
            Assert.AreEqual("Mark McGwire", seq[0].ToString());
            Assert.AreEqual("Sammy Sosa", seq[1].ToString());
            Assert.AreEqual("Ken Griffey", seq[2].ToString());

            seq = docs[1].Root as Sequence;

            Assert.IsNotNull(seq);
            Assert.AreEqual("Chicago Cubs", seq[0].ToString());
            Assert.AreEqual("St Louis Cardinals", seq[1].ToString());
        }


        [Test]
        public void Example_208()
        {
            var reader = YamlReader.LoadFile(@"Tests\Example_2.08.yaml");
            var docs = reader.Documents;
            Assert.AreEqual(2, docs.Length);
            var doc = docs[0];
            var map = doc.Root as Mapping;


            Assert.IsNotNull(map);
            Assert.AreEqual("20:03:20", map["time"].ToString());
            Assert.AreEqual("Sammy Sosa", map["player"].ToString());
            Assert.AreEqual("strike (miss)", map["action"].ToString());

            map = docs[1].Root as Mapping;

            Assert.IsNotNull(map);
            Assert.AreEqual("20:03:47", map["time"].ToString());
            Assert.AreEqual("Sammy Sosa", map["player"].ToString());
            Assert.AreEqual("grand slam", map["action"].ToString());

            
        }

        [Test]
        public void Example_213()
        {
            var reader = YamlReader.LoadFile(@"Tests\Example_2.13.yaml");
            var docs = reader.Documents;
            Assert.AreEqual(1, docs.Length);
            var doc = docs[0];
            
            var yml = @"\//||\/||" + "\r\n" + 
                      @"// ||  ||__";
            
            Assert.AreEqual(yml, doc.Root.ToString());

        }

        [Test]
        public void Example_218()
        {
            var reader = YamlReader.LoadFile(@"Tests\Example_2.18.yaml");
            var docs = reader.Documents;
            Assert.AreEqual(1, docs.Length);
            var doc = docs[0];
            var map = doc.Root as Mapping;
            Assert.IsNotNull(map);
            Assert.AreEqual("This unquoted scalar spans many lines.", map["plain"].ToString());
            Assert.AreEqual("So does this quoted scalar.\n", map["quoted"].ToString());

        }
    }
}
