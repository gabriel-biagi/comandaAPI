using System.Text.Json;

namespace comandaAPI.Middlewares;

public class ErrorDetails
{
    public int StatusCode { get; private set; }
    public string? Message { get; private set; }
    public string? TraceId { get; private set; }
    public string? StackTrace { get; private set; }


    public ErrorDetails(int statusCode, string? message, string? traceId, string? stackTrace)
    {
        StatusCode = statusCode;
        Message = message;
        TraceId = traceId;
        StackTrace = stackTrace;
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}