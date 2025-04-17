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
                { "Компрессорный блок общепромышленного исполнения", RedactText_OTHER }
            };

            methodsForRedact[name](block);
        }

        static private void RedactText_CLAPAN(List<MainBlock> block ) 
        {
            var block1 = block.FirstOrDefault(x => x.name == "Количество приводов на 1 клапан (шт) / Расположение привода /");
            block1.name = "Количество приводов на 1 клапан (шт)";
            string unRedactString = GetSubstringFromFirstNumber(block1.text, 3);
            block1.text = unRedactString.Trim().Replace("\n", "")[unRedactString.Trim().Replace("\n", "").Length - 1].ToString();

            var block2 = block.FirstOrDefault(x => x.name == "Номинальное напряжение (В) / Nominal Материал корпуса /");
            block2.name = "Номинальное напряжение (В)";
            unRedactString = GetSubstringFromFirstNumber(block2.text, 4);
            if (unRedactString.Contains("24"))
                block2.text = "24";
            else
                block2.text = "230";

            var block3 = block.FirstOrDefault(x => x.name == "Потребляемая мощность (Вт) / Power Периметральный обогрев /");
            block3.name = "Потребляемая мощность (Вт)";
            unRedactString = GetSubstringFromFirstNumber(block3.text, 4);
            block3.text = Regex.Replace(unRedactString, @"[^\d]", "");

            var block4 = block.FirstOrDefault(x => x.name == "Мощность обогрева (Вт) / Heating's Напряжение обогрева (В) /");
            block4.name = "Мощность обогрева (Вт)";

            var block5 = block.FirstOrDefault(x => x.name == "Возвратная пружина / Return spring");
            block5.name = "Возвратная пружина";

            if (block5.text.ToLower().Contains("есть"))
                block5.text = "Есть";
            else
                block5.text = "Нет";
        }
        static private void RedactText_FAN(List<MainBlock> block )
        {
            var block1 = block.FirstOrDefault(x => x.name == "Мощность двигателя (кВт) / Motor's Температура при нагнетании (°C) /");
            block1.name = "Мощность двигателя (кВт)";
            var unRedactString = GetSubstringFromFirstNumber(block1.text, 5);
            unRedactString = unRedactString.Trim().Split(' ')[0];
            block1.text = unRedactString.Trim();

            var block2 = block.FirstOrDefault(x => x.name == "Номинальное напряжение (В) / Nominal Падение давления в агрегате (Па) /");
            block2.name = "Параметры электропитания (В - тип)";

            unRedactString = GetSubstringFromFirstNumber(block2.text, 20).Trim();
            if (unRedactString.Contains(" 400"))
                block2.text = "400";
            else
                block2.text = "230";


            var block3 = block.FirstOrDefault(x => x.name == "Номинальный ток (А) / Nominal current (A)");
            block3.name = "Номинальный ток (А)";

            unRedactString = GetSubstringFromFirstNumber(block3.text, 4);
            unRedactString = unRedactString.Trim().Split(' ')[0];
            block3.text = unRedactString.Trim();


            var block4 = block.FirstOrDefault(x => x.name == "Эффективность вентилятора (%) / Fan");
            block4.name = "КПД электродвигателя (%)";

            unRedactString = GetSubstringFromFirstNumber(block4.text, 5);
            unRedactString = unRedactString.Trim().Split(' ')[0];
            block4.text = unRedactString.Trim();


            var block5 = block.FirstOrDefault(x => x.name == "Потреб. мощность рабочего колеса (кВт) Динамическое давление (Па) / Dynamic");
            block5.name = "Потреб. мощность рабочего колеса (кВт)";

            block5.text = block5.text.Trim();


            block.Add(new MainBlock("Светильник", "Нет"));
        }
        static private void RedactText_EK(List<MainBlock> block)
        {

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
