using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yaml
{
    public class Mapping : Dictionary<Node,Node>, Node
    {
        public Node this[string index]
        {
            get
            {
                var key = this.Where(item => item.Key.ToString() == index).Select(item => item.Key).First();
                if (key == null)
                    return null;

                return this[key];
            }
        }
    }
}
