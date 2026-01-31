using ErrorOr;
using MediatR;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Interfaces;
using PantryCloud.Recipe.Application.Queries;

namespace PantryCloud.Recipe.Application.Queries.Handlers;

public class GetRecipeQueryHandler(IRecipeRepository repository)
    : IRequestHandler<GetRecipeQuery, ErrorOr<GetRecipeResponseDto>>
{
    public async Task<ErrorOr<GetRecipeResponseDto>> Handle(
        GetRecipeQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetByIdAsync(request.Request.Id, cancellationToken);
        
        if (result.IsError)
        {
            return result.Errors;
        }

        return new GetRecipeResponseDto(result.Value);
    }
}

