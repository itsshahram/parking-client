using ClosedXML.Excel;
using System.ComponentModel;

namespace Parking.App.Helpers
{
    public static class ExcelHelper
    {
        public static byte[] ExportToExcel<T>(List<T> data, string sheetName = "Sheet1", bool? isRtl = false)
        {
            using (var workbook = new XLWorkbook())
            {
                workbook.RightToLeft = isRtl ?? false;
                var worksheet = workbook.Worksheets.Add(sheetName);

                var properties = typeof(T).GetProperties();

                int col = 1;
                foreach (var prop in properties)
                {
                    var displayName = prop.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? prop.Name;
                    worksheet.Cell(1, col).Value = displayName;
                    col++;
                }
                int row = 2;
                foreach (var item in data)
                {
                    col = 1;
                    foreach (var prop in properties)
                    {
                        object value = prop.GetValue(item);

                        // تبدیل مقدار به نوع مناسب برای XLCellValue
                        worksheet.Cell(row, col).Value = value switch
                        {
                            string str => str,
                            int num => num,
                            double dbl => dbl,
                            bool boolean => boolean,
                            DateTime dt => dt.ToString("yyyy-MM-dd"),
                            _ => value?.ToString() ?? string.Empty
                        };

                        col++;
                    }
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
