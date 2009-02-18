using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yaml
{
    public class Tag : Node
    {
        public Tag()
        {

        }

        public Tag(string type)
        {
            this.TypeId = type;
        }
        public string TypeId { get; set; }
        public Node Value { get; set; }
    }
}
