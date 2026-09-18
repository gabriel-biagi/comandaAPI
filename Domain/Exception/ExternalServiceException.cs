using System.Net;


namespace comandaAPI.Domain.Exception;

public class ExternalServiceException : Exception
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