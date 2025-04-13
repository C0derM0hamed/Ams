namespace AmsApi.Errors;

public class ApiException : Exception
{
    public int StatusCode { get; set; }
    public object? Details { get; set; }

    public ApiException(int statusCode, string message, object? details = null) : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }
}
