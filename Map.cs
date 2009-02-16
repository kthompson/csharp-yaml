using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yaml
{
    public class Mapping : Dictionary<Node,Node>, Node
    {
        public string Tag { get; set; }
    }
}
