using Microsoft.AspNetCore.Http;

namespace CCP.Shared.ResultAbstraction
{
    public static class ResultExtensions
    {
        public static IResult ToProblemDetails(this Result result)
        {
            if (result.IsSuccess)
            {
                throw new InvalidOperationException("Cannot convert a successful result to ProblemDetails.");
            }

            return Results.Problem(
                statusCode: GetStatusCode(result.Error.Type),
                title: GetErrorTitle(result.Error.Type),
                type: GetType(result.Error.Type),
                detail: result.Error.Description,
                extensions: new Dictionary<string, object?>
                {
                    { "errors", new [] { result.Error} }
                });


            static int GetStatusCode(ErrorType errorType) => errorType switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };


            static string GetErrorTitle(ErrorType errorType) => errorType switch
            {
                ErrorType.Validation => "Bad Request",
                ErrorType.NotFound => "Not Found",
                ErrorType.Conflict => "Conflict",
                _ => "Internal Server Error"
            };


            static string GetType(ErrorType errorType) => errorType switch
            {
                ErrorType.Validation => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                ErrorType.NotFound => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
                ErrorType.Conflict => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
                _ => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
            };
        }

        public static Error FromException(string code, string description, int statusCode)
        {
            switch (statusCode)
            {
                case StatusCodes.Status400BadRequest:
                    return Error.Validation(code, description);
                case StatusCodes.Status404NotFound:
                    return Error.NotFound(code, description);
                case StatusCodes.Status409Conflict:
                    return Error.Conflict(code, description);
                default:
                    return Error.Failure(code, description);
            }
        }
    }
}
