using ExpenseTrackerApi.Common.Entities;
using ExpenseTrackerApi.Common.Specifications;
using ExpenseTrackerApi.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Hybrid;

namespace ExpenseTrackerApi.Features.Reports;

public static class GetMonthlyReport
{
    // 1. ენდფოინთის განსაზღვრება
    public class Endpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/monthly/{Year}/{Month}", async (Guid userId, int year, int month, ISender sender) =>
            {
                var query = new GetMonthlyReportQuery(userId, year, month);
                return Results.Ok(await sender.Send(query));
            })
          .WithName("GetMonthlyReport")
          .Produces<MonthlyReportDto>();
        }
    }

    // 2. მოთხოვნა
    public record GetMonthlyReportQuery(Guid UserId, int Year, int Month) : IRequest<MonthlyReportDto>;

    // 3. DTO
    public record MonthlyReportDto(decimal TotalExpenses, int NumberOfTransactions, DateTime ReportMonth);

    // 4. დამმუშავებელი
    public class Handler : IRequestHandler<GetMonthlyReportQuery, MonthlyReportDto>
    {
        private readonly IRepository<Expense> _expenseRepository;
        private readonly HybridCache _cache;

        public Handler(IRepository<Expense> expenseRepository, HybridCache cache)
        {
            _expenseRepository = expenseRepository;
            _cache = cache;
        }

        public async Task<MonthlyReportDto> Handle(GetMonthlyReportQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"report:monthly:{request.UserId}:{request.Year}-{request.Month}";

            return await _cache.GetOrCreateAsync(cacheKey, async token =>
            {
                var startDate = new DateTime(request.Year, request.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var spec = new ExpensesByDateRangeSpec(request.UserId, startDate, endDate);
                var expenses = await _expenseRepository.ListAsync(spec, token);

                var total = expenses.Sum(e => e.Amount);
                var count = expenses.Count;

                return new MonthlyReportDto(total, count, startDate);

            }, new HybridCacheEntryOptions { Expiration = TimeSpan.FromDays(1) }, cancellationToken: cancellationToken);
        }
    }
}