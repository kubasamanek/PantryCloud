using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Queries;

public record GetMyProfileQuery(GetMyProfileRequestDto Request) : IRequest<ErrorOr<GetMyProfileResponseDto>>;
