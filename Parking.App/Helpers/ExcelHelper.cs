using ClosedXML.Excel;
using System.ComponentModel;

namespace Parking.App.Helpers;

public static class ExcelHelper
{
    public static byte[] ExportToExcel<T>(List<T> data, string sheetName = "Sheet1", bool? isRtl = false, string filterInfo = "")
    {
        using (var workbook = new XLWorkbook())
        {
            workbook.RightToLeft = isRtl ?? false;
            var worksheet = workbook.Worksheets.Add(sheetName);

            int currentRow = 1;
            if (!string.IsNullOrWhiteSpace(filterInfo))
            {
                string filterText = $"🔍 فیلتر گزارش: {filterInfo}";

                var filterRange = worksheet.Range(currentRow, 1, currentRow + 1, 12);
                filterRange.Merge();

                filterRange.Value = filterText;

                filterRange.Style
                    .Font.SetBold()
                    .Fill.SetBackgroundColor(XLColor.LightYellow)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)   
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)       
                    .Alignment.SetWrapText(false);

                worksheet.Row(currentRow).AdjustToContents();
                worksheet.Row(currentRow + 1).Height = worksheet.Row(currentRow).Height; 

                currentRow += 3; 
            }

            var properties = typeof(T).GetProperties();

            int col = 1;
            foreach (var prop in properties)
            {
                var displayName = prop.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? prop.Name;
                worksheet.Cell(currentRow, col).Value = displayName;
                col++;
            }

            currentRow++;

            foreach (var item in data)
            {
                col = 1;
                foreach (var prop in properties)
                {
                    object value = prop.GetValue(item);

                    worksheet.Cell(currentRow, col).Value = value switch
                    {
                        string str => str,
                        int num => num,
                        double dbl => dbl,
                        bool boolean => boolean,
                        DateTime dt => dt.ToString("yyyy-MM-dd HH:mm"),
                        _ => value?.ToString() ?? string.Empty
                    };

                    col++;
                }
                currentRow++;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
}
