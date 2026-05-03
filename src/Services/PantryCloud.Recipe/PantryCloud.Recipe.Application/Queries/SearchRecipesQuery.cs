using ErrorOr;
using MediatR;
using PantryCloud.Recipe.Application.Dtos;

namespace PantryCloud.Recipe.Application.Queries;

public record SearchRecipesQuery(SearchRecipesRequestDto Request) : IRequest<ErrorOr<SearchRecipesResponseDto>>;


