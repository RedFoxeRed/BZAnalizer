using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BZAnalizer.Models
{
    public class MainBlock
    {
        public string name;
        public string text;
        public MainBlock(string name, string text)
        {
            this.text = text;
            this.name = name;
        }
    }
}
