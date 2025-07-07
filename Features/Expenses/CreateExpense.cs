using MediatR;
using ExpenseTrackerApi.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ExpenseTrackerApi.Common.Entities;

namespace ExpenseTrackerApi.Features.Expenses;

public static class CreateExpense
{
    public class Endpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/", async (CreateExpenseCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Created($"/api/expenses/{result.Id}", result);
            })
          .WithName("CreateExpense")
          .Produces<ExpenseDto>(StatusCodes.Status201Created)
          .Produces(StatusCodes.Status400BadRequest);
        }
    }
    public record CreateExpenseCommand(decimal Amount, Guid CategoryId, string? Description, DateTime ExpenseDate, Guid UserId) : IRequest<ExpenseDto>;
    public record ExpenseDto(Guid Id, decimal Amount, string? Description, DateTime ExpenseDate, Guid CategoryId);

    public class Handler : IRequestHandler<CreateExpenseCommand, ExpenseDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<ExpenseDto> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
        {
            var expense = new Expense
            {
                Id = Guid.NewGuid(),
                Amount = request.Amount,
                CategoryId = request.CategoryId,
                Description = request.Description,
                ExpenseDate = request.ExpenseDate,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Expense>().AddAsync(expense, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            return new ExpenseDto(expense.Id, expense.Amount, expense.Description, expense.ExpenseDate, expense.CategoryId);
        }
    }
}