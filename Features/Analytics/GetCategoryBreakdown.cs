using MediatR;
using ExpenseTrackerApi.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Hybrid;
using Ardalis.Specification;
using ExpenseTrackerApi.Common.Entities;

namespace ExpenseTrackerApi.Features.Analytics;

public static class GetCategoryBreakdown
{
    public class Endpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/breakdown", async ([AsParameters] GetCategoryBreakdownQuery query, ISender sender) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
      .WithName("GetCategoryBreakdown")
      .Produces<CategoryBreakdownDto>();
        }
    }

    public record GetCategoryBreakdownQuery(Guid UserId, DateTime StartDate, DateTime EndDate) : IRequest<CategoryBreakdownDto>;

    public record CategorySummary(string CategoryName, decimal TotalAmount, double Percentage);
    public record CategoryBreakdownDto(decimal GrandTotal, List<CategorySummary> Breakdown);

    public class CategoryBreakdownSpec : Specification<Expense>
    {
        public CategoryBreakdownSpec(Guid userId, DateTime startDate, DateTime endDate)
        {
            Query
            .Where(e => e.UserId == userId && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .Include(e => e.Category);
        }
    }

    public class Handler : IRequestHandler<GetCategoryBreakdownQuery, CategoryBreakdownDto>
    {
        private readonly IRepository<Expense> _expenseRepository;

        public Handler(IRepository<Expense> expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        public async Task<CategoryBreakdownDto> Handle(GetCategoryBreakdownQuery request, CancellationToken cancellationToken)
        {
            var spec = new CategoryBreakdownSpec(request.UserId, request.StartDate, request.EndDate);
            var expenses = await _expenseRepository.ListAsync(spec, cancellationToken);

            var grandTotal = expenses.Sum(e => e.Amount);
            if (grandTotal == 0)
            {
                return new CategoryBreakdownDto(0, new List<CategorySummary>());
            }

            var breakdown = expenses
            .GroupBy(e => e.Category.Name)
            .Select(g => new CategorySummary(
                    g.Key,
                    g.Sum(e => e.Amount),
                    Math.Round((double)(g.Sum(e => e.Amount) / grandTotal) * 100, 2)
                ))
            .OrderByDescending(s => s.TotalAmount)
            .ToList();

            return new CategoryBreakdownDto(grandTotal, breakdown);
        }
    }
}