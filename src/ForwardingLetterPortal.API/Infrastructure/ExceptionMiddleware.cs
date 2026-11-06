using ForwardingLetterPortal.Interface.Exceptions;

namespace ForwardingLetterPortal.API.Infrastructure
{
    /// <summary>Maps AppException to its HTTP status with a { message } body; hides unexpected errors.</summary>
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await next(ctx);
            }
            catch (AppException ex)
            {
                await Write(ctx, ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception");
                await Write(ctx, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        private static Task Write(HttpContext ctx, int status, string message)
        {
            if (ctx.Response.HasStarted) return Task.CompletedTask;
            ctx.Response.Clear();
            ctx.Response.StatusCode = status;
            return ctx.Response.WriteAsJsonAsync(new { message });
        }
    }
}
