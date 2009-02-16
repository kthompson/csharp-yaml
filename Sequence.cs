using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yaml
{
    public class Sequence : List<Node>, Node
    {
        public string Tag { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}]", string.Join(",", this.Select(node => node.ToString()).ToArray()));
        }
    }
}
