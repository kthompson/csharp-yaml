using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Yaml;

namespace YamlTester
{
    static class Program
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
                var reader = YamlReader.OpenText(item.FullName);
                Console.WriteLine(item.FullName);
                try
                {
                    reader.TestRead();
                }
                catch
                {
                    Console.WriteLine("ERROR: " + item.Name);
                }
                Console.Read();
            }
        }
    }
}
