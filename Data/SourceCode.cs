using System;
using System.Collections.Generic;
using System.Text;

namespace Mapalyzer.Data 
{
    internal class SourceCode
    {
        internal string Body { get; set; }
        internal string Filename { get; set; }

        internal SourceCode(string body, string filename)
        {
            Body = body;
            Filename = filename;
        }
    }
}