using Microsoft.AspNetCore.Diagnostics;
using StockFlow.Api.Domain.Exceptions;

namespace StockFlow.Api.Infrastructure;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            ResourceNotFoundException => (
                StatusCodes.Status404NotFound,
                "Recurso não encontrado",
                exception.Message),
            ConflictException => (
                StatusCodes.Status409Conflict,
                "Conflito de dados",
                exception.Message),
            BusinessRuleException => (
                StatusCodes.Status422UnprocessableEntity,
                "Regra de negócio não atendida",
                exception.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno",
                "Ocorreu um erro inesperado.")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Erro não tratado durante a requisição.");
        }
        else
        {
            logger.LogWarning("Requisição rejeitada: {Message}", exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;
        await Results.Problem(
                statusCode: statusCode,
                title: title,
                detail: detail)
            .ExecuteAsync(httpContext);

        return true;
    }
}
