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
                    "Артикул", "Обозначение", "Тип", "Для ЧП", "Параметры элемента", "Осн/рез", "Ввод", "Номер системы"
                };
                // Получаем заголовки из первой строки
                Dictionary<string, int> columnIndices = ExcelGen.GetColumnIndices(requiredHeaders, worksheet);

                // Находим первую пустую строку в столбце "Артикул"
                int startRow = ExcelGen.FindFirstEmptyRowInColumn(worksheet, columnIndices["Артикул"]);

                // Записываем данные из каждого элемента
                foreach (var element in elements)
                {
                    worksheet.Cells[startRow, columnIndices["Артикул"]].Value = element.article;
                    worksheet.Cells[startRow, columnIndices["Обозначение"]].Value = element.description;
                    worksheet.Cells[startRow, columnIndices["Тип"]].Value = element.fullnameElement;
                    worksheet.Cells[startRow, columnIndices["Для ЧП"]].Value = element.powerfull;
                    worksheet.Cells[startRow, columnIndices["Параметры элемента"]].Value = element.stringParameters;
                    worksheet.Cells[startRow, columnIndices["Осн/рез"]].Value = element.mainOrReserveString;
                    worksheet.Cells[startRow, columnIndices["Ввод"]].Value = element.numberVvod;
                    worksheet.Cells[startRow, columnIndices["Номер системы"]].Value = element.sysNumber;

                    startRow++; // Переходим к следующей строке
                }

                // Предлагаем пользователю выбрать путь для сохранения через диалоговое окно
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx", // Фильтр для файлов Excel
                    Title = "Сохранить файл Excel",
                    FileName = Path.GetFileNameWithoutExtension(excelFilePath) + "_updated.xlsx" // Имя файла по умолчанию
                };

                bool? result = saveFileDialog.ShowDialog(); // Показываем диалоговое окно

                if (result == true)
                {
                    string savePath = saveFileDialog.FileName;

                    // Сохраняем изменения в новый файл
                    try
                    {
                        FileInfo saveFileInfo = new FileInfo(savePath);
                        package.SaveAs(saveFileInfo);

                        Console.WriteLine($"Файл успешно сохранен: {savePath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при сохранении файла: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Сохранение отменено пользователем.");
                }
            }
        }
    }
}
