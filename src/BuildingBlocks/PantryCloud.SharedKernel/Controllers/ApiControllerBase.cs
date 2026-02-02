using AutoMapper;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PantryCloud.SharedKernel.Controllers;

/// <summary>
/// Base class for API controllers that use MediatR and AutoMapper.
/// Provides helper methods for handling ErrorOr results and converting them to appropriate HTTP responses.
/// </summary>
[ApiController]
public abstract class ApiControllerBase(IMediator mediator, IMapper mapper) : ControllerBase
{
    /// <summary>
    /// Gets the MediatR mediator for sending commands and queries.
    /// </summary>
    protected readonly IMediator Mediator = mediator;
    
    /// <summary>
    /// Gets the AutoMapper mapper for object-to-object mapping.
    /// </summary>
    protected readonly IMapper Mapper = mapper;
    
    /// <summary>
    /// Converts an ErrorOr result to an IActionResult with the specified success status code.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The ErrorOr result.</param>
    /// <param name="successStatusCode">The HTTP status code to return on success.</param>
    /// <returns>An IActionResult representing the result.</returns>
    protected IActionResult FromResult<T>(ErrorOr<T> result, int successStatusCode)
    {
        return result.Match<IActionResult>(
            value => successStatusCode == StatusCodes.Status204NoContent ? NoContent() : StatusCode(successStatusCode, value),
            Problem
        );
    }
    
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
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest,
        };

        return Problem(statusCode: statusCode, 
            title: firstError.GetType().Name, 
            detail: firstError.Description);
    }

}
