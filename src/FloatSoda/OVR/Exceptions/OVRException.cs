namespace FloatSoda.OVR.Exceptions;

// ReSharper disable once InconsistentNaming
public class OVRException<T>(string message, T errorCode) : Exception($"{message} <{errorCode}>") where T : Enum
{
    public T ErrorCode { get; } = errorCode;
}