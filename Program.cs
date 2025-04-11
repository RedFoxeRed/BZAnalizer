using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BZAnalizer.Service;

namespace BZAnalizer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string path = "C:\\Users\\kiril\\OneDrive\\Рабочий стол\\testPDFForSan.pdf";

            PDFAnalizer pDFAnalizer = new PDFAnalizer(path);
        }
    }
}
