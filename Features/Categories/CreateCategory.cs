using MediatR;
using ExpenseTrackerApi.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Hybrid;
using ExpenseTrackerApi.Common.Entities;

namespace ExpenseTrackerApi.Features.Categories;

public static class CreateCategory
{
    public class Endpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/", async (CreateCategoryCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Created($"/api/categories/{result.Id}", result);
            })
      .WithName("CreateCategory")
      .Produces<CategoryDto>(StatusCodes.Status201Created);
        }
    }

    public record CreateCategoryCommand(string Name, string? Icon, string? ColorHex, decimal? MonthlyBudget, Guid UserId) : IRequest<CategoryDto>;

    public record CategoryDto(Guid Id, string Name, string? Icon, string? ColorHex, decimal? MonthlyBudget);

    public class Handler : IRequestHandler<CreateCategoryCommand, CategoryDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HybridCache _cache;

        public Handler(IUnitOfWork unitOfWork, HybridCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Icon = request.Icon,
                ColorHex = request.ColorHex,
                MonthlyBudget = request.MonthlyBudget,
                UserId = request.UserId,
                IsActive = true
            };

            await _unitOfWork.Repository<Category>().AddAsync(category, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            await _cache.RemoveAsync($"categories:{request.UserId}", cancellationToken);

            return new CategoryDto(category.Id, category.Name, category.Icon, category.ColorHex, category.MonthlyBudget);
        }
    }
}