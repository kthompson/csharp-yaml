using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yaml
{
    public class Document : Sequence
    {
        public Node Root
        {
            get
            {
                return this.First();
            }
        }
    }
}
