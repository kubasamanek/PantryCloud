using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;

namespace PantryCloud.Pantry.Application.Commands;

public record UpdatePantryItemCommand(Guid Id, UpdatePantryItemRequestDto Request) : IRequest<ErrorOr<UpdatePantryItemResponseDto>>;

