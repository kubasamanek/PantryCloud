using ErrorOr;
using MediatR;
using PantryCloud.Recipe.Application.Dtos;

namespace PantryCloud.Recipe.Application.Queries;

public record GetRecipeQuery(GetRecipeRequestDto Request) : IRequest<ErrorOr<GetRecipeResponseDto>>;


