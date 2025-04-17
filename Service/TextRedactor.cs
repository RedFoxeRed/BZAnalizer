using BZAnalizer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;

namespace BZAnalizer.Service
{
    static class TextRedactor
    {
        static public void Redaction(string name, List<MainBlock> block)
        {
            Dictionary<string, Action<List<MainBlock>>> methodsForRedact = new Dictionary<string, Action<List<MainBlock>>>()
            {
                { "Панель с клапаном", RedactText_CLAPAN },
                { "Клапан рециркуляционный", RedactText_CLAPAN },
                { "Блок воздухозаборный", RedactText_CLAPAN },
                { "Вентилятор", RedactText_FAN },
                { "Фильтр", RedactText_OTHER },
                { "Вытяжной вентилятор", RedactText_FAN },
                { "Блок для установки фильтрующих вставок", RedactText_OTHER },
                { "Нагреватель жидкостный", RedactText_OTHER },
                { "Испаритель", RedactText_OTHER },
                { "Приточный вентилятор", RedactText_FAN },
                { "Нагреватель электрический", RedactText_EK },
                { "Насос циркуляционный", RedactText_NASOS },
                { "Электропривод клапана", RedactText_UVS },
                { "Компрессорный блок общепромышленного исполнения", RedactText_COMPRESSOR },
                { "Конденсаторный блок взрывозащищенного исполнения", RedactText_CONDENSATOR }
            };

            methodsForRedact[name](block);
        }

        static private void RedactText_CLAPAN(List<MainBlock> block ) 
        {
            var block1 = block.FirstOrDefault(x => x.name == "Количество приводов на 1 клапан (шт) / Расположение привода /");
            block1.name = "Количество приводов на 1 клапан (шт)";
            string unRedactString = GetSubstringFromFirstNumber(block1.text, 3);
            block1.text = unRedactString.Trim().Replace("\n", "")[unRedactString.Trim().Replace("\n", "").Length - 1].ToString();

            var block2 = block.FirstOrDefault(x => x.name.Contains("Номинальное напряжение (В)"));
            block2.name = "Номинальное напряжение (В)";
            unRedactString = GetSubstringFromFirstNumber(block2.text, 4);
            if (unRedactString.Contains("24"))
                block2.text = "24";
            else
                block2.text = "230";

            var block3 = block.FirstOrDefault(x => x.name.Contains("Потребляемая мощность (Вт)"));
            block3.name = "Потребляемая мощность (Вт)";
            unRedactString = GetSubstringFromFirstNumber(block3.text, 4);
            block3.text = Regex.Replace(unRedactString, @"[^\d]", "");

            var block4 = block.FirstOrDefault(x => x.name.Contains("Мощность обогрева (Вт)"));
            if(block4.text.Length == 0)
            {
                block4 = block.FirstOrDefault(x => x.name.Contains("Heating's voltage (V)"));
                block4.text = GetSubstringFromFirstNumber(block4.text, 5).Split(' ')[0];
            }
            block4.name = "Мощность обогрева (Вт)";

            var block5 = block.FirstOrDefault(x => x.name.Contains("Возвратная пружина"));
            block5.name = "Возвратная пружина";

            if (block5.text.ToLower().Contains("есть"))
                block5.text = "Есть";
            else
                block5.text = "Нет";
        }
        static private void RedactText_FAN(List<MainBlock> block )
        {
            var block1 = block.FirstOrDefault(x => x.name.Contains("Мощность двигателя (кВт)"));
            block1.name = "Мощность двигателя (кВт)";
            var unRedactString = GetSubstringFromFirstNumber(block1.text, 5);
            unRedactString = unRedactString.Trim().Split(' ')[0];
            block1.text = unRedactString.Trim();

            var block2 = block.FirstOrDefault(x => x.name.Contains("Номинальное напряжение (В)"));
            if(block2 == null)
                block2 = block.FirstOrDefault(x => x.name.Contains("Параметры электропитания"));
            block2.name = "Параметры электропитания (В - тип)";

            unRedactString = GetSubstringFromFirstNumber(block2.text, 20).Trim();
            if (unRedactString.Contains(" 400"))
                block2.text = "400";
            else
                block2.text = "230";


            var block3 = block.FirstOrDefault(x => x.name.Contains("Номинальный ток (А)"));
            block3.name = "Номинальный ток (А)";

            unRedactString = GetSubstringFromFirstNumber(block3.text, 4);
            if (Convert.ToDouble(unRedactString.Trim().Split(' ')[0]) > 200)
                unRedactString = unRedactString.Trim().Split(' ')[1];
            else
                unRedactString = unRedactString.Trim().Split(' ')[0];

            block3.text = unRedactString.Trim();


            var block4 = block.FirstOrDefault(x => x.name.Contains("Эффективность вентилятора (%)"));
            if(block4.text.Length == 0)
                block4 = block.FirstOrDefault(x => x.name.Contains("Манометр"));

            if (block.FirstOrDefault(x => x.name.Contains("КПД электродвигателя (%)")) != null)
            {
                block.Remove(block.FirstOrDefault(x => x.name.Contains("КПД электродвигателя (%)")));
            }

            block4.name = "КПД электродвигателя (%)";

            unRedactString = GetSubstringFromFirstNumber(block4.text, 5);
            unRedactString = unRedactString.Trim().Split(' ')[0];
            block4.text = unRedactString.Trim();


            var block5 = block.FirstOrDefault(x => x.name.Contains("Потреб. мощность рабочего колеса"));
            if (block5.text.Length <= 1)
            {
                block5 = block.FirstOrDefault(x => x.name.Contains("Светильник"));
                block5.text = Regex.Replace(GetSubstringFromFirstNumber(block5.text, 7).Split(' ')[0], @"[^0-9,]", "");

            }
            if (block.FirstOrDefault(x => x.name.Contains("Потреб. мощность рабочего колеса (кВт)")) != null)
            {
                block.Remove(block.FirstOrDefault(x => x.name.Contains("Потреб. мощность рабочего колеса (кВт)")));
            }
            block5.name = "Потреб. мощность рабочего колеса (кВт)";

            block5.text = block5.text.Trim();

            if(block.FirstOrDefault(x => x.name.Contains("Светильник")) == null)
                block.Add(new MainBlock("Светильник", "Нет"));
        }
        static private void RedactText_EK(List<MainBlock> block)
        {

        }
        static private void RedactText_NASOS(List<MainBlock> block)
        {
            var block1 = block.FirstOrDefault(x => x.name == "Параметры электропитания (В) - тип / Ответные фланцы /");
            block1.name = "Параметры электропитания (В) - тип";
            if (block1.text.Contains("400"))
                block1.text = "400 - 3";
            else
                block1.text = "230";

            var block2 = block.FirstOrDefault(x => x.name == "Потребляемая мощность (Вт) / Теплоизоляция узла /");
            block2.name = "Потребляемая мощность (Вт)";
            var unRedactString = GetSubstringFromFirstNumber(block2.text, 10);
            block2.text = unRedactString.Split(' ')[1];

            var block3 = block.FirstOrDefault(x => x.name == "Сила тока (A) /");
            block3.name = "Сила тока (A)";
        }
        static private void RedactText_UVS(List<MainBlock> block)
        {
            var block1 = block.FirstOrDefault(x => x.name == "Напряжение (В) / Тип /");
            block1.name = "Напряжение питания (В)";

            var unRedactString = GetSubstringFromFirstNumber(block1.text, 10);
            block1.text = unRedactString;

            var block2 = block.FirstOrDefault(x => x.name == "Потребляемая мощность (Вт) / Условный диаметр DN (мм) / Условный диаметр DN (мм) /");
            block2.name = "Потребляемая мощность (Вт)";

            block2.text = GetSubstringFromFirstNumber(block2.text, 7).Split(' ')[0];
        }

        static private void RedactText_COMPRESSOR(List<MainBlock> block)
        {
            var block1 = block.FirstOrDefault(x => x.name.Contains("Напряжение сети"));
            if (block1.text.Contains("400"))
                block1.text = "400";
            else
                block1.text = "230";

            var block2 = block.FirstOrDefault(x => x.name.Contains("Общая потребл. мощность (кВт) Потребляемый ток (1 шт., on/off, А)"));
            block2.name = "Общая потребляемая мощность (кВт)";
            block2.text = block2.text.Split(' ')[0];

            var block3 = block.FirstOrDefault(x => x.name.Contains("Общий потребл. ток (А) Макс.рабочий ток (МОС) (1 шт., А)"));
            block3.name = "Общий потребляемый ток (А)";
            block3.text = block3.text.Split(' ')[0];

            var block5 = block.FirstOrDefault(x => x.name.Contains("Потребл. мощность (кВт)")).text;
            var block6 = block.FirstOrDefault(x => x.name.Contains("Потребл. ток (А)")).text;

            var block4 = block.FirstOrDefault(x => x.name.Contains("ТЭН подогрева картера"));

            if (block4 == null)
                return;

            string patternWatt = @"(\d{1,3})\s*(Вт|W)";
            string patternCount = @"(\d+)\s*шт";

            Match wattMatch = Regex.Match(block4.text, patternWatt);
            int wattValue = wattMatch.Success ? int.Parse(wattMatch.Groups[1].Value) : 0;

            // Поиск количества (шт.)
            Match countMatch = Regex.Match(block4.text, patternCount);
            int countValue = countMatch.Success ? int.Parse(countMatch.Groups[1].Value) : 0;

            double kvt = Convert.ToDouble(block2.text) + (wattValue * countValue / 1000) + Convert.ToDouble(block5);
            double nominal = Convert.ToDouble(block3.text) + (wattValue * countValue / 230) + Convert.ToDouble(block6);

            block2.text = kvt.ToString();
            block3.text = nominal.ToString();
        }
        static private void RedactText_CONDENSATOR(List<MainBlock> block)
        {
            int tadam = 0;
            var block1 = block.FirstOrDefault(x => x.name == "Потребл. мощность (кВт)");
            block1.text = GetSubstringFromFirstNumber(block1.text.Trim(), 5).Split(' ')[0];

            var block2 = block.FirstOrDefault(x => x.name == "Мощность двигателя (кВт)");
            block2.name = "Потребл. ток (А)";
            block2.text = Math.Round((Convert.ToDouble(block1.text) * 1000 / 230.0),2).ToString();
        }
        static private void RedactText_OTHER(List<MainBlock> block)
        {

        }

        private static string GetSubstringFromFirstNumber(string input, int offset)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Входная строка не может быть null или пустой.", nameof(input));
            }

            if (offset < 0)
            {
                throw new ArgumentException("Смещение не может быть отрицательным.", nameof(offset));
            }

            // Регулярное выражение для поиска первого числа в строке
            var match = Regex.Match(input, @"\d+");
            if (!match.Success)
            {
                throw new InvalidOperationException("В строке не найдено ни одного числа.");
            }

            // Индекс начала числа
            int numberStartIndex = match.Index;

            // Вычисляем конечный индекс подстроки
            int endIndex = numberStartIndex + match.Length + offset;

            // Если endIndex выходит за пределы строки, обрезаем до конца строки
            if (endIndex > input.Length)
            {
                endIndex = input.Length;
            }

            // Возвращаем подстроку
            return input.Substring(numberStartIndex, endIndex - numberStartIndex);
        }
    }
}
