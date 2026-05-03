using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Commands;

public record TransferOwnershipCommand(TransferOwnershipRequestDto Request) : IRequest<ErrorOr<TransferOwnershipResponseDto>>;
