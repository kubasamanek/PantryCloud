using ErrorOr;
using MediatR;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Interfaces;
using PantryCloud.Recipe.Application.Queries;

namespace PantryCloud.Recipe.Application.Queries.Handlers;

public class SearchRecipesQueryHandler(IRecipeSearchService searchService)
    : IRequestHandler<SearchRecipesQuery, ErrorOr<SearchRecipesResponseDto>>
{
    public async Task<ErrorOr<SearchRecipesResponseDto>> Handle(
        SearchRecipesQuery request,
        CancellationToken cancellationToken)
    {
        return await searchService.SearchRecipesAsync(request.Request, cancellationToken);
    }
}


