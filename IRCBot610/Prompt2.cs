using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Speech.Synthesis;

namespace IRCBot610
{
    class Prompt2 : Prompt
    {
        public String Text;

        public Prompt2(String s)
            : base(s)
        {
            this.Text = s;
        }
    }
}
