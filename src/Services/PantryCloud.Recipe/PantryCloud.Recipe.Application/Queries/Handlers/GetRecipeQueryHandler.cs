using ErrorOr;
using MediatR;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Interfaces;
using PantryCloud.Recipe.Application.Queries;

namespace PantryCloud.Recipe.Application.Queries.Handlers;

public class GetRecipeQueryHandler : IRequestHandler<GetRecipeQuery, ErrorOr<GetRecipeResponseDto>>
{
    private readonly IRecipeRepository _repository;

    public GetRecipeQueryHandler(IRecipeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<GetRecipeResponseDto>> Handle(
        GetRecipeQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _repository.GetByIdAsync(request.Request.Id, cancellationToken);
        
        if (result.IsError)
        {
            return result.Errors;
        }

        return new GetRecipeResponseDto(result.Value);
    }
}

