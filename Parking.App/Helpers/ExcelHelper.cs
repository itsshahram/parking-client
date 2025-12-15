namespace Parking.App.Helpers;

using ClosedXML.Excel;
using System.ComponentModel;
using System.Reflection;


public static class ExcelHelper
{
    public static byte[] ExportToExcel<T>(
        List<T> data,
        string sheetName = "Sheet1",
        bool? isRtl = false,
        string[]? filterRows = null)
    {
        using var workbook = new XLWorkbook();
        workbook.RightToLeft = isRtl ?? false;

        var worksheet = workbook.Worksheets.Add(sheetName);

        int currentRow = 1;
        int startCol = 1;
        int endCol = 12; 

        if (filterRows != null && filterRows.Length > 0)
        {
            for (int i = 0; i < filterRows.Length; i++)
            {
                var range = worksheet.Range(currentRow, startCol, currentRow, endCol);
                range.Merge();
                range.Value = filterRows[i];

                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                range.Style.Alignment.ReadingOrder = XLAlignmentReadingOrderValues.RightToLeft;
                range.Style.Alignment.WrapText = false;

                if (i == 0)
                {
                    range.Style.Font.Bold = true;
                    range.Style.Font.FontSize = 14;
                    range.Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                worksheet.Row(currentRow).Height = 24;
                currentRow++;
            }

            currentRow++; // space after filters
        }

        // 🔹 TABLE HEADER
        var properties = typeof(T).GetProperties();
        int col = startCol;

        foreach (var prop in properties)
        {
            var displayName =
                prop.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
                ?? prop.Name;

            worksheet.Cell(currentRow, col).Value = displayName;
            worksheet.Cell(currentRow, col).Style.Font.Bold = true;
            col++;
        }

        currentRow++;

        foreach (var item in data)
        {
            col = startCol;

            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);

                worksheet.Cell(currentRow, col).Value = value switch
                {
                    DateTime dt => dt.ToString("yyyy-MM-dd HH:mm"),
                    _ => value?.ToString() ?? string.Empty
                };

                col++;
            }

            currentRow++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
