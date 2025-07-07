using MediatR;
using ExpenseTrackerApi.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Hybrid;
using ExpenseTrackerApi.Common.Entities;

namespace ExpenseTrackerApi.Features.Categories;

public static class UpdateBudget
{
    public class Endpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/{id:guid}/budget", async (Guid id, UpdateBudgetCommand command, ISender sender) =>
            {
                command = command with { CategoryId = id };
                await sender.Send(command);
                return Results.NoContent();
            })
      .WithName("UpdateBudget")
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status404NotFound);
        }
    }

    public record UpdateBudgetCommand(Guid CategoryId, decimal NewBudget, Guid UserId) : IRequest;

    public class Handler : IRequestHandler<UpdateBudgetCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HybridCache _cache;

        public Handler(IUnitOfWork unitOfWork, HybridCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
        {
            var categoryRepo = _unitOfWork.Repository<Category>();
            var category = await categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);

            if (category is null || category.UserId != request.UserId)
            {
                return;
            }

            category.MonthlyBudget = request.NewBudget;

            await categoryRepo.UpdateAsync(category, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            await _cache.RemoveAsync($"categories:{request.UserId}", cancellationToken);
        }
    }
}
