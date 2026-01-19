using ErrorOr;
using PantryCloud.Pantry.Application.Dtos;

namespace PantryCloud.Pantry.Application;

public interface IPantryManagementService
{
    Task<ErrorOr<CreatePantryItemResponseDto>> CreatePantryItemAsync(CreatePantryItemRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<UpdatePantryItemResponseDto>> UpdatePantryItemAsync(Guid id, UpdatePantryItemRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<DeletePantryItemResponseDto>> DeletePantryItemAsync(DeletePantryItemRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<GetPantryItemResponseDto>> GetPantryItemAsync(GetPantryItemRequestDto request, CancellationToken cancellationToken);
    Task<ErrorOr<ListPantryItemsResponseDto>> ListPantryItemsAsync(ListPantryItemsRequestDto request, CancellationToken cancellationToken);
}

