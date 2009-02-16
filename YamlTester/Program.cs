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
                YamlReader reader;

                
                reader = YamlReader.LoadFile(item.FullName);
                if (reader.Token != YaTools.Yaml.Token.EOS)
                    Console.WriteLine("INCOMPLETE: " + item.Name);
                
            }
        }

        [Test]
        public void Example_201()
        {
            var reader = YamlReader.LoadFile(@"Tests\Example_2.01.yaml");
            var docs = reader.Documents;
            Assert.AreEqual(1, docs.Length);
            var doc = docs[0];
            Assert.IsTrue(doc.First() is Sequence);
        }
    }
}
