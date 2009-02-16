using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yaml
{
    public class Comment : Node
    {
        public string Text { get; private set; }
        public Comment(string text)
        {
            this.Text = text;
        }

        #region Node Members

        public string Tag { get; set; }

        #endregion
    }
}
