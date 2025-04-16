using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BZAnalizer.Service;
using OfficeOpenXml;

namespace BZAnalizer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ExcelPackage.License.SetNonCommercialPersonal("kku"); 
            string path = "C:\\Users\\kku\\Desktop\\папка БЗ\\testpdfgovna.pdf";

            string pathSila = "C:\\Users\\kku\\Desktop\\Проверка АГХК сила.XLSX";
            string pathControl = "";
            PDFAnalizer pDFAnalizer = new PDFAnalizer(path);
            ExcelGen.Generate(pDFAnalizer.GetAllElements(), pathSila, pathControl);
        }
    }
}
