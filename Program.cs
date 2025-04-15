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
            string path = "C:\\Users\\kku\\Desktop\\папка БЗ\\testPDFForSan.pdf";

            string pathSila = "";
            string pathControl = "";
            PDFAnalizer pDFAnalizer = new PDFAnalizer(path);
            ExcelGen.Generate(pDFAnalizer.GetAllElements(), pathSila, pathControl);
        }
    }
}
