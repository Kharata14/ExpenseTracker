using System.Data;

namespace ExpenseTrackerApi.Infrastructure.Services;

public interface IExcelService
{
    byte[] GenerateExcel<T>(IEnumerable<T> data, string sheetName);
}