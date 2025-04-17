using BZAnalizer.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BZAnalizer.Service
{
    public static class PowerExcelGen
    {
        public static void Generate(List<WorkElement> elements, string excelFilePath)
        {
            if (elements == null || elements.Count == 0)
            {
                throw new ArgumentException("Список элементов пуст.");
            }

            if (!File.Exists(excelFilePath))
            {
                throw new FileNotFoundException("Указанный файл Excel не найден.", excelFilePath);
            }

            FileInfo fileInfo = new FileInfo(excelFilePath);

            int numberSystem = 1;
            foreach(var el in elements.Where(x => x.MainOrReserve))
            {
                if (el.sysNumber != "0")
                    continue;
                string parameters = el.stringParameters;
                string fullname = el.fullnameElement;

                var SimilarElement = elements.FirstOrDefault(x =>
                {
                    if(x.sysNumber == "0" && parameters == x.stringParameters && fullname == x.fullnameElement && !x.MainOrReserve)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });

                el.sysNumber = numberSystem.ToString();

                if (SimilarElement != null)
                {
                    SimilarElement.sysNumber = numberSystem.ToString();
                }

                numberSystem++;
            }


            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                // Открываем второй лист
                var worksheet = package.Workbook.Worksheets[1]; // Индексация начинается с 0
                if (worksheet == null)
                {
                    throw new InvalidOperationException("Второй лист в Excel-файле отсутствует.");
                }
                var requiredHeaders = new List<string>
                {
                    "Тег", "Название", "Тип", "Мощность ЧП", "Мощность", "Осн. или рез.", "Ввод", "№ системы"
                };
                // Получаем заголовки из первой строки
                Dictionary<string, int> columnIndices = ExcelGen.GetColumnIndices(requiredHeaders, worksheet);

                // Находим первую пустую строку в столбце "Артикул"
                int startRow = ExcelGen.FindFirstEmptyRowInColumn(worksheet, columnIndices["Тег"]);

                // Записываем данные из каждого элемента
                foreach (var element in elements)
                {
                    worksheet.Cells[startRow, columnIndices["Тег"]].Value = element.article;
                    worksheet.Cells[startRow, columnIndices["Название"]].Value = element.description;
                    worksheet.Cells[startRow, columnIndices["Тип"]].Value = element.fullnameElement;
                    worksheet.Cells[startRow, columnIndices["Мощность ЧП"]].Value = element.powerfull;
                    worksheet.Cells[startRow, columnIndices["Мощность"]].Value = element.stringParameters;
                    worksheet.Cells[startRow, columnIndices["Осн. или рез."]].Value = element.mainOrReserveString;
                    worksheet.Cells[startRow, columnIndices["Ввод"]].Value = element.numberVvod;
                    worksheet.Cells[startRow, columnIndices["№ системы"]].Value = element.sysNumber;

                    startRow++; // Переходим к следующей строке
                }

                // Предлагаем пользователю выбрать путь для сохранения через диалоговое окно
                //SaveFileDialog saveFileDialog = new SaveFileDialog
                //{
                //    Filter = "Excel Files|*.xlsx", // Фильтр для файлов Excel
                //    Title = "Сохранить файл Excel",
                //    FileName = Path.GetFileNameWithoutExtension(excelFilePath) + "_updated.xlsx" // Имя файла по умолчанию
                //};

                //bool? result = saveFileDialog.ShowDialog(); // Показываем диалоговое окно

                //if (result == true)
                //{
                //    string savePath = saveFileDialog.FileName;

                //    // Сохраняем изменения в новый файл
                //    try
                //    {
                //        FileInfo saveFileInfo = new FileInfo(savePath);
                //        package.SaveAs(saveFileInfo);

                //        Console.WriteLine($"Файл успешно сохранен: {savePath}");
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine($"Ошибка при сохранении файла: {ex.Message}");
                //    }
                //}
                //else
                //{
                //    Console.WriteLine("Сохранение отменено пользователем.");
                //}

                package.Save();
            }
        }
    }
}
