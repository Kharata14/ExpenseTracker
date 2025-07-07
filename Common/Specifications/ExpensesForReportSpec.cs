using Ardalis.Specification;
using ExpenseTrackerApi.Common.Entities;

namespace ExpenseTrackerApi.Common.Specifications;

public class ExpensesForReportSpec : Specification<Expense, ExpenseReportDto>
{
    public ExpensesForReportSpec(Guid userId, DateTime startDate, DateTime endDate, List<Guid>? categoryIds = null)
    {
        var query = Query
        .Where(e => e.UserId == userId &&
                        e.ExpenseDate >= startDate &&
                        e.ExpenseDate <= endDate);

        if (categoryIds != null && categoryIds.Any())
        {
            query.Where(e => categoryIds.Contains(e.CategoryId));
        }

        query.Select(e => new ExpenseReportDto(
            e.Id,
            e.Description,
            e.Amount,
            e.ExpenseDate,
            e.Category.Name
        ));
    }
}

// მარტივი DTO პროექციისთვის
public record ExpenseReportDto(Guid Id, string? Description, decimal Amount, DateTime ExpenseDate, string CategoryName);