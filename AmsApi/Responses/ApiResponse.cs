namespace AmsApi.Responses;

public class ApiResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public object? Details { get; set; }

    public ApiResponse(int statusCode, string message, object? details = null)
    {
        StatusCode = statusCode;
        Message = message;
        Details = details;
    }
}
