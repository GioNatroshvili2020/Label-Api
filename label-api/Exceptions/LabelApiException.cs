namespace label_api.Exceptions;

public class LabelApiException : Exception
{
    public int StatusCode { get; }

    public LabelApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    public LabelApiException(int statusCode, string message, Exception innerException) : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    // Common exception factory methods
    public static LabelApiException BadRequest(string message) => new(400, message);
    public static LabelApiException Unauthorized(string message = "Unauthorized access") => new(401, message);
} 