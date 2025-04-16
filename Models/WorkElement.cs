using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BZAnalizer.Service;
using System.Text.RegularExpressions;

namespace BZAnalizer.Models
{
    public class WorkElement
    {

        public string name = "";
        public bool MainOrReserve = true; // true - осн, false - рез
        public string textFromPDF = "";
        public List<MainBlock> parameters = new List<MainBlock>();

        public List<WorkElement> childrenElements = new List<WorkElement>();

        #region InformationForEXCEL

        public string article; // tag
        public string description; // Обозначение
        public string fullnameElement; // Тип (Определяющее название)
        public string powerfull; // Для ЧП
        public string stringParameters; // Параметры элемента
        public string mainOrReserveString; // осн/рез
        public string numberVvod; // Ввод (1 / 2)
        public string sysNumber = "1"; // Номер системы
        public bool printForSila = false;

        #endregion
        public WorkElement(string name, string textFromPDF, List<MainBlock> parameters, bool mainOrReserve)
        {
            this.name = name;  
            this.textFromPDF = textFromPDF;
            this.parameters = parameters;
            MainOrReserve = mainOrReserve;

            mainOrReserveString = mainOrReserve ? "осн" : "рез";
            numberVvod = mainOrReserve ? "1" : "2";

            ElementHandler.MethodsForElements[name](this);
        }
        public WorkElement(string name, string textFromPDF)
        {
            this.name = name;
            this.textFromPDF = textFromPDF;
            this.parameters = parameters;

            if (name.Contains("Частотный"))
            {
                double pwr = GetPowerCHP(textFromPDF);
                string plce = textFromPDF.Contains("шкафу") ? "В шкафу" : "Снаружи";
                string countE = textFromPDF.Substring(textFromPDF.Trim().Length - 5).Trim().Replace("шт", "").Replace(" ", "");


                this.parameters.Add(new MainBlock("Мощность ЧП", pwr.ToString()));
                this.parameters.Add(new MainBlock("Положение ЧП", plce));
                this.parameters.Add(new MainBlock("Кол-во ЧП", countE));
            }
        }

        public double GetPowerCHP(string input)
        {
            string pattern = @"(\d+[.,]\d+)\s*к[вВ][тТ]";
            // Поиск совпадений
            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                // Извлекаем найденное значение (например, "5,5")
                string kWString = match.Groups[1].Value;

                // Заменяем запятую на точку для корректного преобразования в double
                kWString = kWString.Replace('.', ',');

                // Преобразуем в double
                if (double.TryParse(kWString, out double kW))
                {
                    return kW;
                }
                else
                {
                    return -2;
                }

            }
            else
            {
                return -1;
            }

        }
    }
}
