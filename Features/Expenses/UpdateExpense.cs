using MediatR;
using ExpenseTrackerApi.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Hybrid;
using ExpenseTrackerApi.Common.Entities;

namespace ExpenseTrackerApi.Features.Expenses;

public static class UpdateExpense
{
    public class Endpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/{id:guid}", async (Guid id, UpdateExpenseCommand command, ISender sender) =>
            {
                if (id != command.Id)
                {
                    return Results.BadRequest("ID-ები არ ემთხვევა.");
                }
                await sender.Send(command);
                return Results.NoContent();
            })
           .WithName("UpdateExpense")
           .Produces(StatusCodes.Status204NoContent)
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status400BadRequest);
        }
    }

    public record UpdateExpenseCommand(Guid Id, decimal Amount, Guid CategoryId, string? Description, DateTime ExpenseDate, Guid UserId) : IRequest;

    public class Handler : IRequestHandler<UpdateExpenseCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HybridCache _cache;

        public Handler(IUnitOfWork unitOfWork, HybridCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
        {
            var expenseRepo = _unitOfWork.Repository<Expense>();
            var expense = await expenseRepo.GetByIdAsync(request.Id, cancellationToken);

            if (expense is null || expense.UserId != request.UserId)
            {
                
                throw new KeyNotFoundException("ხარჯი ვერ მოიძებნა ან მომხმარებლის ID არ ემთხვევა.");
            }

            expense.Amount = request.Amount;
            expense.CategoryId = request.CategoryId;
            expense.Description = request.Description;
            expense.ExpenseDate = request.ExpenseDate;
            expense.UpdatedAt = DateTime.UtcNow;

            await expenseRepo.UpdateAsync(expense, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            var cacheKey = $"report:monthly:{request.UserId}:{expense.ExpenseDate.Year}-{expense.ExpenseDate.Month}";
            await _cache.RemoveAsync(cacheKey, cancellationToken);
        }
    }
}
