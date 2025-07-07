using MediatR;
using ExpenseTrackerApi.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Hybrid;
using Ardalis.Specification;
using ExpenseTrackerApi.Common.Entities;

namespace ExpenseTrackerApi.Features.Reports;

public static class GetYearlyTrends
{
    public class Endpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/yearly/{year}", async (int year, Guid userId, ISender sender) =>
            {
                var query = new GetYearlyTrendsQuery(userId, year);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
          .WithName("GetYearlyTrends")
          .Produces<YearlyTrendsDto>();
        }
    }

    public record GetYearlyTrendsQuery(Guid UserId, int Year) : IRequest<YearlyTrendsDto>;

    public record MonthlySummary(int Month, decimal TotalAmount);
    public record YearlyTrendsDto(int Year, List<MonthlySummary> MonthlyTotals);

    public class YearlyTrendsSpec : Specification<Expense>
    {
        public YearlyTrendsSpec(Guid userId, int year)
        {
            Query.Where(e => e.UserId == userId && e.ExpenseDate.Year == year);
        }
    }

    public class Handler : IRequestHandler<GetYearlyTrendsQuery, YearlyTrendsDto>
    {
        private readonly IRepository<Expense> _expenseRepository;
        private readonly HybridCache _cache;

        public Handler(IRepository<Expense> expenseRepository, HybridCache cache)
        {
            _expenseRepository = expenseRepository;
            _cache = cache;
        }

        public async Task<YearlyTrendsDto> Handle(GetYearlyTrendsQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"report:yearly:{request.UserId}:{request.Year}";

            return await _cache.GetOrCreateAsync(cacheKey, async token =>
            {
                var spec = new YearlyTrendsSpec(request.UserId, request.Year);
                var expenses = await _expenseRepository.ListAsync(spec, token);

                var monthlyTotals = expenses
                   .GroupBy(e => e.ExpenseDate.Month)
                   .Select(g => new MonthlySummary(g.Key, g.Sum(e => e.Amount)))
                   .OrderBy(s => s.Month)
                   .ToList();

                return new YearlyTrendsDto(request.Year, monthlyTotals);
            }, new HybridCacheEntryOptions { Expiration = TimeSpan.FromDays(1) }, cancellationToken: cancellationToken);
        }
    }
}