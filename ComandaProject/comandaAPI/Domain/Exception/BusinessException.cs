namespace comandaAPI.Domain.Exception;

public class BusinessException : System.Exception
{
    public BusinessException(string message) : base(message) { }
}