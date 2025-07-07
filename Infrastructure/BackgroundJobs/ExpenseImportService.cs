using CsvHelper;
using ExpenseTrackerApi.Common.Entities;
using ExpenseTrackerApi.Features.Expenses;
using ExpenseTrackerApi.Infrastructure.Repositories;
using System.Globalization;
using static ExpenseTrackerApi.Features.Expenses.ImportExpenses;

namespace ExpenseTrackerApi.Infrastructure.BackgroundJobs;

public class ExpenseImportService : IExpenseImportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExpenseImportService> _logger;

    public ExpenseImportService(IUnitOfWork unitOfWork, ILogger<ExpenseImportService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ProcessImport(Guid userId, string filePath)
    {
        _logger.LogInformation("CSV იმპორტის დაწყება ფაილისთვის: {FilePath}", filePath);
        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<CsvImportRecord>().ToList();
            var expenseRepo = _unitOfWork.Repository<Expense>();

            foreach (var record in records)
            {
                var expense = new Expense
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CategoryId = record.CategoryId,
                    Amount = record.Amount,
                    Description = record.Description,
                    ExpenseDate = record.ExpenseDate,
                    CreatedAt = DateTime.UtcNow
                };
                await expenseRepo.AddAsync(expense);
            }

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("{Count} ხარჯი წარმატებით დაიმპორტდა.", records.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CSV იმპორტისას მოხდა შეცდომა.");
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
    private record CsvImportRecord(decimal Amount, Guid CategoryId, string? Description, DateTime ExpenseDate);
}