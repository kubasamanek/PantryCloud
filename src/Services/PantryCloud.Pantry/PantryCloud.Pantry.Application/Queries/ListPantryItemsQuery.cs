using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;

namespace PantryCloud.Pantry.Application.Queries;

public record ListPantryItemsQuery(ListPantryItemsRequestDto Request) : IRequest<ErrorOr<ListPantryItemsResponseDto>>;

