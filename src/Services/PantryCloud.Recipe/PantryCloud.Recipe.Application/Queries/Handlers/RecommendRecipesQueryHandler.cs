using ErrorOr;
using MediatR;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Interfaces;
using PantryCloud.Recipe.Application.Queries;

namespace PantryCloud.Recipe.Application.Queries.Handlers;

public class RecommendRecipesQueryHandler(IRecipeSearchService searchService) : IRequestHandler<RecommendRecipesQuery, ErrorOr<RecommendRecipesResponseDto>>
{
    public async Task<ErrorOr<RecommendRecipesResponseDto>> Handle(RecommendRecipesQuery request, CancellationToken cancellationToken)
    {
        return await searchService.RecommendRecipesAsync(request.Request, cancellationToken);
    }
}
