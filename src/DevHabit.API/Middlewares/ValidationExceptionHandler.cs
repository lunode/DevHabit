using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace DevHabit.API.Middlewares;

public class ValidationExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return ValueTask.FromResult(false);
        }
        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Detail = "One or more validation errors occured.",
                Status = StatusCodes.Status400BadRequest,
            }
        };
        var errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key.ToLowerInvariant(),
                        g => g.Select(e => e.ErrorMessage).ToArray());
        context.ProblemDetails.Extensions.Add("errors", errors);
        return problemDetails.TryWriteAsync(context);
    }
}