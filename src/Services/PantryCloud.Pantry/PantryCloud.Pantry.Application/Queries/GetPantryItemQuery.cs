using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;

namespace PantryCloud.Pantry.Application.Queries;

public record GetPantryItemQuery(GetPantryItemRequestDto Request) : IRequest<ErrorOr<GetPantryItemResponseDto>>;

