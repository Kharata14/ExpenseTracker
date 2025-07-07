namespace ExpenseTrackerApi.Infrastructure.Services;

public interface IEmailService
{
    Task SendBudgetAlertAsync(string to, string categoryName, decimal budget, decimal spent);
}