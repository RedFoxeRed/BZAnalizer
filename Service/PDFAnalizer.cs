using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BZAnalizer.Models;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;


namespace BZAnalizer.Service
{
    public class PDFAnalizer
    {
        string filePath;
        string fullText;
        List<MainBlock> mainBlocks = new List<MainBlock>();
        List<MainBlock> workBlocks = new List<MainBlock>();
        List<MainBlock> reserveBlocks = new List<MainBlock>();
        List<WorkElement> elements = new List<WorkElement>();
        public PDFAnalizer(string path)
        {
            filePath = path;
            fullText = ExtractTextFromPdf();

            SeparateText(mainBlocks, DataFields.mainBLockNames, fullText);


            if (mainBlocks.FirstOrDefault(x => x.name == "Рабочая установка") != null)
            {
                SeparateText(workBlocks, DataFields.elementBlockNames, mainBlocks.FirstOrDefault(x => x.name == "Рабочая установка").text, true);
                SeparateText(reserveBlocks, DataFields.elementBlockNames, mainBlocks.FirstOrDefault(x => x.name == "Резервная установка").text, true);
            }
            else
            {
                SeparateText(workBlocks, DataFields.elementBlockNames, mainBlocks.FirstOrDefault(x => x.name == "Габаритные размеры установки").text, true);
                SeparateText(reserveBlocks, DataFields.elementBlockNames, mainBlocks.FirstOrDefault(x => x.name == "Габаритные размеры установки").text, true);
            }

            if (mainBlocks.FirstOrDefault(x => x.name == "Узел водосмесительный") != null)
            {
                List<MainBlock> waterSector = new List<MainBlock>();

                SeparateText(waterSector, DataFields.BlockParameters["Узел водосмесительный"], mainBlocks.FirstOrDefault(x => x.name == "Узел водосмесительный").text, true);

                List<MainBlock> nasosSector = new List<MainBlock>();
                List<MainBlock> UVSSector = new List<MainBlock>();

                SeparateText(nasosSector, DataFields.BlockParameters["Насос циркуляционный"], waterSector.FirstOrDefault(x => x.name == "Насос циркуляционный").text, true);
                SeparateText(UVSSector, DataFields.BlockParameters["Электропривод клапана"], waterSector.FirstOrDefault(x => x.name == "Электропривод клапана").text, true);

                elements.Add(new WorkElement("Насос циркуляционный", waterSector.FirstOrDefault(x => x.name == "Насос циркуляционный").text, nasosSector, true));
                elements.Add(new WorkElement("Насос циркуляционный", waterSector.FirstOrDefault(x => x.name == "Насос циркуляционный").text, nasosSector, false));
                elements.Add(new WorkElement("Электропривод клапана", waterSector.FirstOrDefault(x => x.name == "Электропривод клапана").text, UVSSector, true));
                elements.Add(new WorkElement("Электропривод клапана", waterSector.FirstOrDefault(x => x.name == "Электропривод клапана").text, UVSSector, false));
            }



            foreach (var block in workBlocks) 
            {
                List<MainBlock> parameters = new List<MainBlock>();
                SeparateText(parameters, DataFields.BlockParameters[block.name], block.text, true);
                if (block.name != "Нагреватель электрический")
                {
                    
                    if(block.name.ToLower().Contains("вентилятор"))
                    {
                        int count = 1;
                        string pattern = @"(\d+)\s*шт";

                        // Поиск совпадений
                        Match match = Regex.Match(block.text, pattern);

                        if (match.Success)
                        {
                            // Извлечение числа из найденного совпадения
                            count = int.Parse(match.Groups[1].Value);
                        }
                        
                        for(int i = 0; i < count; i++)
                            elements.Add(new WorkElement(block.name, block.text, parameters, true));
                    }
                    else
                        elements.Add(new WorkElement(block.name, block.text, parameters, true));
                }
                else
                {
                    AnalizeEK(parameters, true);
                }
            }
            foreach (var block in reserveBlocks)
            {
                List<MainBlock> parameters = new List<MainBlock>();
                SeparateText(parameters, DataFields.BlockParameters[block.name], block.text, true);
                if (block.name != "Нагреватель электрический")
                {
                    if (block.name.ToLower().Contains("вентилятор"))
                    {
                        int count = 1;
                        string pattern = @"(\d+)\s*шт";

                        // Поиск совпадений
                        Match match = Regex.Match(block.text, pattern);

                        if (match.Success)
                        {
                            // Извлечение числа из найденного совпадения
                            count = int.Parse(match.Groups[1].Value);
                        }

                        for (int i = 0; i < count; i++)
                            elements.Add(new WorkElement(block.name, block.text, parameters, false));
                    }
                    else
                        elements.Add(new WorkElement(block.name, block.text, parameters, false));
                }
                else
                {
                    AnalizeEK(parameters, false);
                }
            }

            List<MainBlock> parametersDevices = new List<MainBlock>();
            SeparateText(parametersDevices, DataFields.BlockParameters["Контрольно-измерительные приборы и элементы автоматики"], mainBlocks.FirstOrDefault(x => x.name == "Контрольно-измерительные приборы и элементы автоматики").text, true);
            
            foreach(var device in parametersDevices)
            {
                elements.Add(new WorkElement(device.name, device.text));
            }
            ElementHandler.CheckCHPVent(elements);

        }
        private string ExtractTextFromPdf()
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Указанный PDF-файл не найден.", filePath);
            }

            using (PdfReader reader = new PdfReader(filePath))
            {
                var textBuilder = new System.Text.StringBuilder();

                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    string pageText = PdfTextExtractor.GetTextFromPage(reader, page);
                    textBuilder.AppendLine(pageText);
                }

                return textBuilder.ToString();
            }
        }
        private void SeparateText(List<MainBlock> blocks, List<string> stringList, string text, bool CheckFunc = false)
        {
            // Словарь для подсчета количества вхождений каждого ключа
            Dictionary<string, int> keyCount = new Dictionary<string, int>();

            // Начало обработки текста
            int currentIndex = 0;
            string previousKey = null;

            while (currentIndex < text.Length)
            {
                // Поиск ближайшего вхождения любой строки из списка
                int nextIndex = int.MaxValue;
                string foundKey = null;

                foreach (var key in stringList)
                {
                    int index = text.IndexOf(key, currentIndex, StringComparison.Ordinal);
                    if (index != -1 && index < nextIndex)
                    {
                        nextIndex = index;
                        foundKey = key;
                    }
                }

                // Если больше нет вхождений, записываем остаток текста
                if (foundKey == null)
                {
                    string remainingText = text.Substring(currentIndex).Trim();
                    if (!string.IsNullOrEmpty(remainingText))
                    {
                        string key = previousKey != null ? $"{previousKey}" : "Основная информация";
                        blocks.Add(new MainBlock(key, remainingText));
                    }
                    break;
                }

                // Берем текст до найденного вхождения
                string blockText = text.Substring(currentIndex, nextIndex - currentIndex).Trim();

                // Определяем ключ для записи в словарь
                if (previousKey == null)
                {
                    if (!CheckFunc)
                    {
                        // Первый блок (основная информация)
                        blocks.Add(new MainBlock("Основная информация", blockText));
                    }
                }
                else
                {
                    // Увеличиваем счетчик для предыдущего ключа
                    keyCount[previousKey]++;
                    string key = $"{previousKey}";
                    blocks.Add(new MainBlock(key, blockText));
                }

                // Обновляем текущую позицию и предыдущний ключ
                currentIndex = nextIndex + foundKey.Length;
                previousKey = foundKey;

                // Инициализируем счетчик для нового ключа, если его еще нет
                if (!keyCount.ContainsKey(previousKey))
                {
                    keyCount[previousKey] = 0;
                }
            }

            // Вывод результата

            foreach (var block in blocks)
            {
                if (CheckFunc)
                    Console.WriteLine($"\n\n{block.name}: {block.text}");
            }
        }

        private void AnalizeEK(List<MainBlock> EK1EK2, bool osnOrReserve)
        {
            List<MainBlock> paramsForEK1 = new List<MainBlock>();
            SeparateText(paramsForEK1, DataFields.BlockParameters[EK1EK2[0].name], EK1EK2[0].text, true);

            List<MainBlock> paramsForEK2 = new List<MainBlock>();
            SeparateText(paramsForEK2, DataFields.BlockParameters[EK1EK2[1].name], EK1EK2[1].text, true);

            var stepsFromEK1 = ParseStringToDoubles(paramsForEK1.FirstOrDefault(x => x.name == "Количество ступеней").text.Trim());
            var powerStepsEK1 = ParseStringToDoubles(paramsForEK1.FirstOrDefault(x => x.name == "Мощность ступени (кВт)").text.Trim().Replace(".", ","));

            int ind = 0;
            foreach(var count in stepsFromEK1)
            {
                var pow = powerStepsEK1[ind];


                for (int i = 0; i < count; i++) 
                {
                    List<MainBlock> blockForEK = new List<MainBlock>()
                    {
                        new MainBlock("Мощность ступени", pow.ToString()),
                        new MainBlock("Номер ступени", (i + 1).ToString())
                    };
                    elements.Add(new WorkElement($"ЭК", EK1EK2[0].text, blockForEK, osnOrReserve));
                }
                ind++;
            }

            var stepsFromEK2 = ParseStringToDoubles(paramsForEK2.FirstOrDefault(x => x.name == "Количество ступеней").text.Trim());
            var powerStepsEK2 = ParseStringToDoubles(paramsForEK2.FirstOrDefault(x => x.name == "Мощность ступени (кВт)").text.Trim().Replace(".", ","));

            ind = 0;
            foreach (var count in stepsFromEK2)
            {
                var pow = powerStepsEK2[ind];

                List<MainBlock> blockForEK = new List<MainBlock>()
                {
                    new MainBlock("Мощность ступени", pow.ToString())
                };

                for (int i = 0; i < count; i++)
                {
                    elements.Add(new WorkElement($"ЭК Ступень {i + 1}", EK1EK2[1].text, blockForEK, osnOrReserve));
                }
                ind++;
            }

        }

        private List<double> ParseStringToDoubles(string input)
        {
            // Создаем пустой список для хранения результатов
            List<double> result = new List<double>();

            // Разделяем строку по пробелам
            string[] parts = input.Split(' ');

            // Проверяем первую часть
            if (parts.Length > 0 && parts[0] != "-" && double.TryParse(parts[0], out double firstNumber))
            {
                result.Add(firstNumber); // Добавляем первое число в список
            }

            // Проверяем вторую часть
            if (parts.Length > 1 && parts[1] != "-" && double.TryParse(parts[1], out double secondNumber))
            {
                result.Add(secondNumber); // Добавляем второе число в список
            }

            return result;
        }
        public List<WorkElement> GetAllElements()
            { return elements; }
    }
}
