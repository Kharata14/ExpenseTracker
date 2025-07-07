using MediatR;
using ExpenseTrackerApi.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Hybrid;
using ExpenseTrackerApi.Common.Entities;

namespace ExpenseTrackerApi.Features.Expenses;

public static class DeleteExpense
{
    public class Endpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
            {
                var userId = Guid.NewGuid(); // Placeholder
                await sender.Send(new DeleteExpenseCommand(id, userId));
                return Results.NoContent();
            })
          .WithName("DeleteExpense")
          .Produces(StatusCodes.Status204NoContent)
          .Produces(StatusCodes.Status404NotFound);
        }
    }

    public record DeleteExpenseCommand(Guid Id, Guid UserId) : IRequest;

    public class Handler : IRequestHandler<DeleteExpenseCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HybridCache _cache;

        public Handler(IUnitOfWork unitOfWork, HybridCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
        {
            var expenseRepo = _unitOfWork.Repository<Expense>();
            var expense = await expenseRepo.GetByIdAsync(request.Id, cancellationToken);

            if (expense is null || expense.UserId != request.UserId)
            {
                return;
            }

            await expenseRepo.DeleteAsync(expense, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            var cacheKey = $"report:monthly:{request.UserId}:{expense.ExpenseDate.Year}-{expense.ExpenseDate.Month}";
            await _cache.RemoveAsync(cacheKey, cancellationToken);
        }
    }
}