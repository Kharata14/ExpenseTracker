using Ardalis.Specification;
using ExpenseTrackerApi.Common.Entities;

namespace ExpenseTrackerApi.Common.Specifications;

public class ExpensesByDateRangeSpec : Specification<Expense>
{
    public ExpensesByDateRangeSpec(Guid userId, DateTime startDate, DateTime endDate)
    {
        Query
          .Where(e => e.UserId == userId &&
                        e.ExpenseDate >= startDate &&
                        e.ExpenseDate <= endDate)
          .Include(e => e.Category)
          .OrderByDescending(e => e.ExpenseDate);
    }
}