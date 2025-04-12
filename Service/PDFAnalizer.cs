using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            foreach (var block in workBlocks) 
            {
                List<MainBlock> parameters = new List<MainBlock>();
                SeparateText(parameters, DataFields.BlockParameters[block.name], block.text, true);
                elements.Add(new WorkElement(block.name, block.text, parameters, true));
            }
            foreach (var block in reserveBlocks)
            {
                List<MainBlock> parameters = new List<MainBlock>();
                SeparateText(parameters, DataFields.BlockParameters[block.name], block.text, true);
                elements.Add(new WorkElement(block.name, block.text, parameters, false));
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
                if(CheckFunc)
                    Console.WriteLine($"\n\n{block.name}: {block.text}");
            }
        }
    }
}
