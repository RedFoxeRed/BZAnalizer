using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BZAnalizer.Models
{
    public class WorkElement
    {
        string name = "";
        bool MainOrReserve = true; // true - осн, false - рез
        string textFromPDF = "";
        List<MainBlock> parameters = new List<MainBlock>();
        public WorkElement(string name, string textFromPDF, List<MainBlock> parameters, bool mainOrReserve)
        {
            this.textFromPDF = textFromPDF;
            this.parameters = parameters;
            MainOrReserve = mainOrReserve;
        }
    }
}
