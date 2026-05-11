using System;

namespace FloatSoda.OVR.Exceptions;

public class OpenVRSystemException<TError> : Exception where TError : Enum
{
    public readonly TError? Error;

    public OpenVRSystemException(string message) : base(message)
    {
    }

    public OpenVRSystemException(string message, Exception inner) : base(message, inner)
    {
    }

    public OpenVRSystemException(string message, TError error) : this($"{message} ({error})") => Error = error;
}