namespace FloatSoda.OVR.Exceptions;

public class OpenVRSystemException<TError>(string message, TError errorCode)
    : Exception($"{message} ({errorCode})") where TError : struct, Enum
{
    public TError ErrorCode { get; } = errorCode;
}