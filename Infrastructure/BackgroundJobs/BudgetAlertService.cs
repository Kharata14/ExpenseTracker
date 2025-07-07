using Ardalis.Specification;
using ExpenseTrackerApi.Infrastructure.Repositories;
using ExpenseTrackerApi.Infrastructure.Services;
using ExpenseTrackerApi.Common.Specifications;
using ExpenseTrackerApi.Common.Entities;

namespace ExpenseTrackerApi.Infrastructure.BackgroundJobs;

public class BudgetAlertService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<BudgetAlertService> _logger;

    public BudgetAlertService(IUnitOfWork unitOfWork, IEmailService emailService, ILogger<BudgetAlertService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task CheckBudgetsAndSendAlerts()
    {
        _logger.LogInformation("ბიუჯეტის გაფრთხილებების შემოწმების დაწყება: {Time}", DateTime.UtcNow);

        var categoryRepo = _unitOfWork.Repository<Category>();
        var expenseRepo = _unitOfWork.Repository<Expense>();
        var alertRepo = _unitOfWork.Repository<BudgetAlert>();

        var categoriesWithBudgetSpec = new CategoriesWithBudgetSpec();
        var categories = await categoryRepo.ListAsync(categoriesWithBudgetSpec);

        foreach (var category in categories)
        {
            var today = DateTime.UtcNow;
            var startDate = new DateTime(today.Year, today.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var alertSentSpec = new AlertSentSpec(category.UserId, category.Id, startDate);
            int existingAlertsCount = await alertRepo.CountAsync(alertSentSpec);
            if (existingAlertsCount > 0)
            {
                continue;
            }

            var expensesSpec = new ExpensesForReportSpec(category.UserId, startDate, endDate, new List<Guid> { category.Id });
            var expenses = await expenseRepo.ListAsync(expensesSpec);
            var totalSpent = expenses.Sum(e => e.Amount);

            if (category.MonthlyBudget.HasValue && totalSpent >= (category.MonthlyBudget.Value * 0.8m))
            {
                _logger.LogWarning("ბიუჯეტის ლიმიტს მიახლოება: მომხმარებელი {UserId}, კატეგორია {CategoryName}", category.UserId, category.Name);

                await _emailService.SendBudgetAlertAsync(
                    to: category.User.Email,
                    categoryName: category.Name,
                    budget: category.MonthlyBudget.Value,
                    spent: totalSpent
                );

                var alert = new BudgetAlert
                {
                    Id = Guid.NewGuid(),
                    UserId = category.UserId,
                    CategoryId = category.Id,
                    Month = startDate,
                    PercentageUsed = (totalSpent / category.MonthlyBudget.Value) * 100,
                    AlertSentAt = DateTime.UtcNow
                };
                await alertRepo.AddAsync(alert);
            }
        }

        await _unitOfWork.CompleteAsync();
        _logger.LogInformation("ბიუჯეტის გაფრთხილებების შემოწმება დასრულდა: {Time}", DateTime.UtcNow);
    }
    private class CategoriesWithBudgetSpec : Specification<Category>
    {
        public CategoriesWithBudgetSpec()
        {
            Query.Where(c => c.MonthlyBudget.HasValue && c.MonthlyBudget > 0 && c.IsActive)
               .Include(c => c.User);
        }
    }

    private class AlertSentSpec : Specification<BudgetAlert>
    {
        public AlertSentSpec(Guid userId, Guid categoryId, DateTime month)
        {
            Query.Where(a => a.UserId == userId && a.CategoryId == categoryId && a.Month.Month == month.Month && a.Month.Year == month.Year);
        }
    }
}