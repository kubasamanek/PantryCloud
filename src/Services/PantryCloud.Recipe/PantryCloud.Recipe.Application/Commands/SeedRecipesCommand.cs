using ErrorOr;
using MediatR;
using PantryCloud.Recipe.Application.Dtos;

namespace PantryCloud.Recipe.Application.Commands;

public record SeedRecipesCommand(SeedRecipesRequestDto Request) : IRequest<ErrorOr<SeedRecipesResponseDto>>;


