using Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace Web.API.Middleware
{
    internal class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            var result = string.Empty;

            switch (exception)
            {
                case ValidationException ve:
                    code = HttpStatusCode.BadRequest;
                    result = JsonSerializer.Serialize(new { ve.Message });
                    break;
                case DomainException de:
                    code = HttpStatusCode.Conflict;
                    result = JsonSerializer.Serialize(new { de.Message });
                    break;
            }

            if (result == string.Empty)
            {
                List<Exception> exceptions = new List<Exception>();

                while (exception != null)
                {
                    exceptions.Add(exception);
                    if (exception.InnerException == null)
                        break;

                    exception = exception.InnerException;
                }

                var eVals = exceptions
                    .Select(e => new { e.Message, StackTrace = e.StackTrace ?? "" }).ToList();

                result = JsonSerializer.Serialize(eVals);
            }

            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)code;
            }

            return context.Response.WriteAsync(result);
        }
    }
}
