using ErrorOr;
using MediatR;
using PantryCloud.Recipe.Application.Dtos;

namespace PantryCloud.Recipe.Application.Queries;

public record RecommendRecipesQuery(RecommendRecipesRequestDto Request) : IRequest<ErrorOr<RecommendRecipesResponseDto>>;
