using ClosedXML.Excel;
using System.Data;

namespace ExpenseTrackerApi.Infrastructure.Services;

public class ExcelService : IExcelService
{
    public byte[] GenerateExcel<T>(IEnumerable<T> data, string sheetName)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);
        worksheet.Cell(1, 1).InsertTable(data);
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}