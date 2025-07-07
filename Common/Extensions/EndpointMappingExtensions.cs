using ExpenseTrackerApi.Features.Analytics;
using ExpenseTrackerApi.Features.Categories;
using ExpenseTrackerApi.Features.Expenses;
using ExpenseTrackerApi.Features.Reports;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;

namespace ExpenseTrackerApi.Common.Extensions;

public static class EndpointMappingExtensions
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        // ფუნქციონალი: ხარჯები
        var expenses = app.MapGroup("/api/expenses").WithTags("Expenses");
        CreateExpense.Endpoint.Map(expenses);
        GetExpenses.Endpoint.Map(expenses);
        UpdateExpense.Endpoint.Map(expenses);
        DeleteExpense.Endpoint.Map(expenses);
        ImportExpenses.Endpoint.Map(expenses);

        var categories = app.MapGroup("/api/categories").WithTags("Categories");
        CreateCategory.Endpoint.Map(categories);
        GetCategories.Endpoint.Map(categories);
        UpdateBudget.Endpoint.Map(categories);

        var reports = app.MapGroup("/api/reports").WithTags("Reports");
        GetMonthlyReport.Endpoint.Map(reports);
        GetYearlyTrends.Endpoint.Map(reports);
        GenerateExcelReport.Endpoint.Map(reports);

        var analytics = app.MapGroup("/api/analytics").WithTags("Analytics");
        GetCategoryBreakdown.Endpoint.Map(analytics);
    }
}