using ErrorOr;
using MediatR;
using PantryCloud.Audit.Application.Dtos;

namespace PantryCloud.Audit.Application.Queries;

public record ListHouseholdAuditEntriesQuery(ListAuditEntriesRequestDto Request)
    : IRequest<ErrorOr<ListAuditEntriesResponseDto>>;
