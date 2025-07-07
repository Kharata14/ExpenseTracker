using MediatR;
using ExpenseTrackerApi.Infrastructure.Repositories;
using ExpenseTrackerApi.Common.Specifications;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ExpenseTrackerApi.Common.Entities;

namespace ExpenseTrackerApi.Features.Expenses;

public static class GetExpenses
{
    public class Endpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/", async ([AsParameters] GetExpensesQuery query, ISender sender) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
         .WithName("GetExpenses")
         .Produces<List<CreateExpense.ExpenseDto>>();
        }
    }

    public record GetExpensesQuery(Guid UserId, DateTime? StartDate, DateTime? EndDate, int PageNumber = 1, int PageSize = 20) : IRequest<List<CreateExpense.ExpenseDto>>;

    public class Handler : IRequestHandler<GetExpensesQuery, List<CreateExpense.ExpenseDto>>
    {
        private readonly IRepository<Expense> _expenseRepository;

        public Handler(IRepository<Expense> expenseRepository) => _expenseRepository = expenseRepository;

        public async Task<List<CreateExpense.ExpenseDto>> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
        {
            var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = request.EndDate ?? DateTime.UtcNow;

            var spec = new ExpensesByDateRangeSpec(request.UserId, startDate, endDate);

            var expenses = await _expenseRepository.ListAsync(spec, cancellationToken);

            return expenses.Select(e => new CreateExpense.ExpenseDto(e.Id, e.Amount, e.Description, e.ExpenseDate, e.CategoryId)).ToList();
        }
    }
}