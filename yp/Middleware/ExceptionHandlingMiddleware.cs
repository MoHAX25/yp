using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using yp.Exceptions;

namespace yp.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(exception,
                    "Исключение поймано после начала записи ответа, ProblemDetails отправить нельзя. Path: {Path}",
                    context.Request.Path);
                return;
            }

            try
            {
                var (statusCode, title) = MapException(exception);

                if (statusCode == (int)HttpStatusCode.InternalServerError)
                {
                    _logger.LogError(exception, "Необработанное исключение при обработке запроса {Path}", context.Request.Path);
                }
                else
                {
                    _logger.LogWarning(exception, "Ошибка обработки запроса {Path}: {Message}", context.Request.Path, exception.Message);
                }

                var problemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Type = $"https://httpstatuses.io/{statusCode}",
                    Instance = context.Request.Path,
                    Detail = exception.Message,
                };

                if (exception is ValidationAppException validationEx && validationEx.Errors != null)
                {
                    problemDetails.Extensions["errors"] = validationEx.Errors;
                }

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = statusCode;

                var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(json);
            }
            catch (Exception handlerEx)
            {
                _logger.LogError(handlerEx,
                    "Сам обработчик исключений упал при обработке запроса {Path}. Исходное исключение: {OriginalException}",
                    context.Request.Path, exception);

                if (context.Response.HasStarted)
                {
                    return;
                }

                context.Response.Clear();
                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                const string fallbackJson = """{"status":500,"title":"Внутренняя ошибка сервера","detail":"Произошла непредвиденная ошибка."}""";

                await context.Response.WriteAsync(fallbackJson);
            }
        }

        private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
        {
            NotFoundException => ((int)HttpStatusCode.NotFound, "Ресурс не найден"),
            ValidationAppException => ((int)HttpStatusCode.BadRequest, "Ошибка валидации"),
            ArgumentException => ((int)HttpStatusCode.BadRequest, "Некорректный запрос"),
            _ => ((int)HttpStatusCode.InternalServerError, "Внутренняя ошибка сервера")
        };
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}