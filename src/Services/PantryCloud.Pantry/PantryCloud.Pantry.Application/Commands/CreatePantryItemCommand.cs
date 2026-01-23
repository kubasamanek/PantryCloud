using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;

namespace PantryCloud.Pantry.Application.Commands;

public record CreatePantryItemCommand(CreatePantryItemRequestDto Request) : IRequest<ErrorOr<CreatePantryItemResponseDto>>;

