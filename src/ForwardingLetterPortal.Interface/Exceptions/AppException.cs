namespace ForwardingLetterPortal.Interface.Exceptions
{
    /// <summary>Expected business error; the API maps it to an HTTP status + { message }.</summary>
    public class AppException(string message, int statusCode = 400) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;

        public static AppException NotFound(string message) => new(message, 404);
        public static AppException Unauthorized(string message) => new(message, 401);
        public static AppException Forbidden(string message) => new(message, 403);
        public static AppException Conflict(string message) => new(message, 409);
    }
}
