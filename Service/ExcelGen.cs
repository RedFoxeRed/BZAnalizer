using BZAnalizer.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BZAnalizer.Service
{
    public static class ExcelGen
    {
        public static void Generate(List<WorkElement> elements, string powerPath, string controlPath)
        {
            if (!String.IsNullOrEmpty(powerPath))
                PowerExcelGen.Generate(elements.Where(x => x.printForSila).ToList(), powerPath);
            if (!String.IsNullOrEmpty(controlPath)) 
                ControlExcelGen.Generate(elements, powerPath);
        }

        public static Dictionary<string, int> GetColumnIndices(List<string> requiredHeaders, ExcelWorksheet worksheet)
        {
            var columnIndices = new Dictionary<string, int>();

            // Перебираем все ячейки в первой строке
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                var header = worksheet.Cells[1, col].Text.Trim();
                if (requiredHeaders.Contains(header))
                {
                    columnIndices[header] = col;
                }
            }

            // Проверяем, что все необходимые заголовки найдены
            foreach (var header in requiredHeaders)
            {
                if (!columnIndices.ContainsKey(header))
                {
                    throw new InvalidOperationException($"Заголовок '{header}' не найден в шапке таблицы.");
                }
            }

            return columnIndices;
        }

        public static int FindFirstEmptyRowInColumn(ExcelWorksheet worksheet, int columnIndex)
        {
            int row = 2; // Начинаем со второй строки, так как первая - это заголовки

            while (!string.IsNullOrEmpty(worksheet.Cells[row, columnIndex].Text))
            {
                row++;
            }

            return row;
        }
    }
}
