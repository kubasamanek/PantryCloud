using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;

namespace PantryCloud.Pantry.Application.Commands;

public record DeletePantryItemCommand(DeletePantryItemRequestDto Request) : IRequest<ErrorOr<DeletePantryItemResponseDto>>;

