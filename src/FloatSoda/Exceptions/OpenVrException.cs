namespace FloatSoda.Exceptions;

public class OpenVrException<T>(string message, T errorCode) : Exception($"{message} <{errorCode}>")
{
    public T ErrorCode { get; } = errorCode;
}