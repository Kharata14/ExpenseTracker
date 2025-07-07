using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using ExpenseTrackerApi.Infrastructure.Repositories;
using Ardalis.Specification;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using static ExpenseTrackerApi.Features.Categories.CreateCategory;
using ExpenseTrackerApi.Common.Entities;

namespace ExpenseTrackerApi.Features.Categories;

public static class GetCategories
{
    public class Endpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/", async ([AsParameters] GetCategoriesQuery query, ISender sender) =>
            {
                return Results.Ok(await sender.Send(query));
            })
         .WithName("GetCategories")
         .Produces<List<CategoryDto>>();
        }
    }

    public record GetCategoriesQuery(Guid UserId) : IRequest<List<CategoryDto>>;

    public class Handler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly HybridCache _cache;

        public Handler(IRepository<Category> categoryRepository, HybridCache cache)
        {
            _categoryRepository = categoryRepository;
            _cache = cache;
        }

        public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"categories:{request.UserId}";

            return await _cache.GetOrCreateAsync(cacheKey, async token =>
            {
                var spec = new CategoriesByUserSpec(request.UserId);
                var categories = await _categoryRepository.ListAsync(spec, token);
                return categories.Select(c => new CategoryDto(c.Id, c.Name, c.Icon, c.ColorHex, c.MonthlyBudget)).ToList();
            }, new HybridCacheEntryOptions { Expiration = TimeSpan.FromHours(1) }, cancellationToken: cancellationToken);
        }
    }

    private class CategoriesByUserSpec : Specification<Category>
    {
        public CategoriesByUserSpec(Guid userId)
        {
            Query.Where(c => c.UserId == userId && c.IsActive);
        }
    }
}