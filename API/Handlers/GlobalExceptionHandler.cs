using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FluentValidation;
using AttractionCatalog.Domain.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AttractionCatalog.API.Handlers
{
    /// Handles unhandled exceptions and converts them into Problem Details responses.
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            var (statusCode, title, detail, extensions) = exception switch
            {
                ValidationException validationException => (
                    StatusCodes.Status400BadRequest,
                    "Validation Error",
                    "One or more validation failures have occurred.",
                    GetValidationErrors(validationException)),
                
                NotFoundException notFoundException => (
                    StatusCodes.Status404NotFound,
                    "Resource Not Found",
                    notFoundException.Message,
                    null),
                
                UnauthorizedAccessException => (
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "You are not authorized to perform this action.",
                    null),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Server Error",
                    "An unexpected error occurred.",
                    null)
            };

            httpContext.Response.StatusCode = statusCode;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail,
                    Type = $"https://httpstatuses.com/{statusCode}"
                },
                Exception = exception
            });
        }

        private static Dictionary<string, object?>? GetValidationErrors(ValidationException exception)
        {
            var errors = exception.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => (object?)g.Select(e => e.ErrorMessage).ToArray()
                );

            return new Dictionary<string, object?> { ["errors"] = errors };
        }
    }
}
