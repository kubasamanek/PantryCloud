using AutoMapper;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PantryCloud.SharedKernel.Controllers;

[ApiController]
public abstract class ApiControllerBase(IMediator mediator, IMapper mapper) : ControllerBase
{
    protected readonly IMediator Mediator = mediator;
    protected readonly IMapper Mapper = mapper;

    private IActionResult Problem(List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Problem();
        }

        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            var modelState = new ModelStateDictionary();
            foreach (var error in errors)
            {
                modelState.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(modelState);
        }

        var firstError = errors[0];

        var statusCode = firstError.Type switch
        {
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest,
        };

        return Problem(statusCode: statusCode, 
            title: firstError.GetType().Name, 
            detail: firstError.Description);
    }

    protected IActionResult FromResult<T>(ErrorOr<T> result, int successStatusCode)
    {
        return result.Match<IActionResult>(
            value => StatusCode(successStatusCode, value),
            Problem
        );
    }
}

