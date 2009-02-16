using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yaml
{
    public class Scalar : Node
    {
        public Scalar(string text)
        {
            this.Text = text;
        }

        public string Text { get; private set; }
        public string Tag { get; set; }
        public override string ToString()
        {
            return this.Text;
        }
    }
}
