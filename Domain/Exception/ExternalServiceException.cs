using System.Net;


namespace comandaAPI.Domain.Exception;

public class ExternalServiceException : System.Exception
{
    public HttpStatusCode UpstreamStatusCode { get; }

    public ExternalServiceException(
        string message,
        HttpStatusCode upstreamStatusCode)
        : base(message)
    {
        UpstreamStatusCode = upstreamStatusCode;
    }
}