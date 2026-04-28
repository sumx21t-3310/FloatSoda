namespace FloatSoda.OVR;

// ReSharper disable once InconsistentNaming
public class OVRException<T>(string message, T errorCode) : Exception($"{message} <{errorCode}>")
{
    public T ErrorCode { get; } = errorCode;
}