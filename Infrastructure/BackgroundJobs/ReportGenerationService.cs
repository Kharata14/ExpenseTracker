using ExpenseTrackerApi.Common.Entities;
using ExpenseTrackerApi.Common.Specifications;
using ExpenseTrackerApi.Features.Reports;
using ExpenseTrackerApi.Infrastructure.Repositories;
using ExpenseTrackerApi.Infrastructure.Services;

namespace ExpenseTrackerApi.Infrastructure.BackgroundJobs;

public class ReportGenerationService : IReportGenerationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExcelService _excelService;
    private readonly ILogger<ReportGenerationService> _logger;

    public ReportGenerationService(IUnitOfWork unitOfWork, IExcelService excelService, ILogger<ReportGenerationService> logger)
    {
        _unitOfWork = unitOfWork;
        _excelService = excelService;
        _logger = logger;
    }

    public async Task GenerateMonthlyExcelReport(Guid userId, int year, int month)
    {
        _logger.LogInformation("Excel რეპორტის გენერაციის დაწყება: მომხმარებელი {UserId}, პერიოდი {Year}-{Month}", userId, year, month);
        try
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var spec = new ExpensesForReportSpec(userId, startDate, endDate);

            var expensesData = await _unitOfWork.Repository<Expense>().ListAsync(spec);

            if (!expensesData.Any())
            {
                _logger.LogWarning("რეპორტისთვის მონაცემები არ მოიძებნა.");
                return;
            }

            var fileContents = _excelService.GenerateExcel(expensesData, "Monthly Expenses");

            var reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedReports");
            Directory.CreateDirectory(reportsDir);
            var fileName = $"Report_{userId}_{year}_{month}_{Guid.NewGuid()}.xlsx";
            var filePath = Path.Combine(reportsDir, fileName);

            await File.WriteAllBytesAsync(filePath, fileContents);

            _logger.LogInformation("Excel რეპორტი წარმატებით დაგენერირდა და შეინახა: {FilePath}", filePath);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excel რეპორტის გენერაციისას მოხდა შეცდომა მომხმარებლისთვის {UserId}", userId);
            throw;
        }
    }
}