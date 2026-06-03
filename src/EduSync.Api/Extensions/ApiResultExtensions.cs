using EduSync.SharedKernel.Results;
using AppError = EduSync.SharedKernel.Results.Error;
using AppResult = EduSync.SharedKernel.Results.Result;

namespace EduSync.Api.Extensions;

public static class ApiResultExtensions
{
    public static IResult ToHttpResult(this AppResult result) =>
        result.IsSuccess
            ? Results.NoContent()
            : ToErrorResult(result.Error!);

    public static IResult ToHttpResult<T>(this EduSync.SharedKernel.Results.Result<T> result, Func<T, IResult>? onSuccess = null)
    {
        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error!);
        }

        return onSuccess?.Invoke(result.Value!) ?? Results.Ok(result.Value);
    }

    private static IResult ToErrorResult(AppError error)
    {
        var status = error.Code switch
        {
            "VALIDATION_ERROR" => StatusCodes.Status400BadRequest,
            "NOT_FOUND" => StatusCodes.Status404NotFound,
            "FORBIDDEN" => StatusCodes.Status403Forbidden,
            "AUTH_FAILED" => StatusCodes.Status401Unauthorized,
            "CONFLICT" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };

        return Results.Json(
            new { code = error.Code, message = error.Message, errors = error.FieldErrors },
            statusCode: status);
    }
}
