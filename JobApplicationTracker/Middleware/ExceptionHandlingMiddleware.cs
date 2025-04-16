using JobApplicationTracker.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace JobApplicationTracker.Middleware
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
            _logger.LogError(exception, "An unexpected error occurred");

            var response = context.Response;
            response.ContentType = "application/json";
            
            var errorResponse = new ErrorResponse();

            switch (exception)
            {
                case ValidationException validationEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = "Validation error";
                    errorResponse.Errors = validationEx.Errors;
                    break;
                    
                case NotFoundException notFoundEx:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Message = notFoundEx.Message;
                    break;

                case DbUpdateConcurrencyException:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse.Message = "The record was modified by another user";
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = "An unexpected error occurred";
                    break;
            }

            var result = JsonSerializer.Serialize(errorResponse);
            await response.WriteAsync(result);
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; }
        public IDictionary<string, string[]> Errors { get; set; }
    }
} 