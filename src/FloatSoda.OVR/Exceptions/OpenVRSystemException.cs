namespace FloatSoda.OVR.Exceptions;

/// <summary>Provides a common base for exceptions backed by an OpenVR error enumeration.</summary>
/// <typeparam name="TError">The OpenVR error enumeration type.</typeparam>
/// <param name="message">A human-readable description of the error.</param>
/// <param name="errorCode">The error code returned by OpenVR.</param>
public class OpenVRSystemException<TError>(string message, TError errorCode)
    : Exception($"{message} ({errorCode})") where TError : struct, Enum
{
    /// <summary>Gets the error code returned by OpenVR.</summary>
    public TError ErrorCode { get; } = errorCode;
}
