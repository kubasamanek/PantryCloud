using ErrorOr;
using MediatR;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Interfaces;
using PantryCloud.Recipe.Application.Queries;

namespace PantryCloud.Recipe.Application.Queries.Handlers;

public class SearchRecipesQueryHandler : IRequestHandler<SearchRecipesQuery, ErrorOr<SearchRecipesResponseDto>>
{
    private readonly IRecipeSearchService _searchService;

    public SearchRecipesQueryHandler(IRecipeSearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<ErrorOr<SearchRecipesResponseDto>> Handle(
        SearchRecipesQuery request,
        CancellationToken cancellationToken)
    {
        return await _searchService.SearchRecipesAsync(request.Request, cancellationToken);
    }
}


