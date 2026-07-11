using FloatSoda.OVR.Exceptions.Resources;

namespace FloatSoda.OVR.Exceptions;

/// <summary>
/// Thrown when an OpenVR render model operation (<see cref="EVRRenderModelError"/>) fails.
/// </summary>
public class VRRenderModelException(string message, EVRRenderModelError errorCode)
    : OpenVRSystemException<EVRRenderModelError>(message, errorCode)
{
    private VRRenderModelException(EVRRenderModelError error)
        : this(GetMessage(error), error)
    {
    }

    private static string GetMessage(EVRRenderModelError error) => error switch
    {
        EVRRenderModelError.Loading => ExceptionMessages.VRRenderModelException_Loading,
        EVRRenderModelError.NotSupported => ExceptionMessages.VRRenderModelException_NotSupported,
        EVRRenderModelError.InvalidArg => ExceptionMessages.VRRenderModelException_InvalidArg,
        EVRRenderModelError.InvalidModel => ExceptionMessages.VRRenderModelException_InvalidModel,
        EVRRenderModelError.NoShapes => ExceptionMessages.VRRenderModelException_NoShapes,
        EVRRenderModelError.MultipleShapes => ExceptionMessages.VRRenderModelException_MultipleShapes,
        EVRRenderModelError.TooManyVertices => ExceptionMessages.VRRenderModelException_TooManyVertices,
        EVRRenderModelError.MultipleTextures => ExceptionMessages.VRRenderModelException_MultipleTextures,
        EVRRenderModelError.BufferTooSmall => ExceptionMessages.VRRenderModelException_BufferTooSmall,
        EVRRenderModelError.NotEnoughNormals => ExceptionMessages.VRRenderModelException_NotEnoughNormals,
        EVRRenderModelError.NotEnoughTexCoords => ExceptionMessages.VRRenderModelException_NotEnoughTexCoords,
        EVRRenderModelError.InvalidTexture => ExceptionMessages.VRRenderModelException_InvalidTexture,
        _ => string.Format(ExceptionMessages.VRRenderModelException_UnexpectedError, error)
    };

    /// <summary>Throws a <see cref="VRRenderModelException"/> if <paramref name="error"/> indicates a failure.</summary>
    public static void ThrowIfError(EVRRenderModelError error)
    {
        if (error == EVRRenderModelError.None) return;

        throw new VRRenderModelException(error);
    }
}
