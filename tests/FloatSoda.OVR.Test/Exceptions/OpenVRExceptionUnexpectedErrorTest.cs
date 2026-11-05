using FloatSoda.OVR.Exceptions;

namespace FloatSoda.OVR.Test.Exceptions;

/// <summary>
/// switch に腕がないエラー値でも、各クラスの型付き例外が投げられることを確かめる(#295 の回帰テスト)。
/// </summary>
public class OpenVRExceptionUnexpectedErrorTest
{
    private const int UndefinedErrorValue = 999;

    [Fact]
    public void VRSettingsException_ThrowIfError_AccessDenied_VRSettingsExceptionを投げる()
    {
        var exception = Assert.Throws<VRSettingsException>(() =>
            VRSettingsException.ThrowIfError(EVRSettingsError.AccessDenied)
        );

        Assert.Equal(EVRSettingsError.AccessDenied, exception.ErrorCode);
        Assert.NotEqual("VRSettingsException_AccessDenied", exception.Message);
    }

    [Fact]
    public void VRNotificationException_ThrowIfError_ServiceUnavailable_VRNotificationExceptionを投げる()
    {
        var exception = Assert.Throws<VRNotificationException>(() =>
            VRNotificationException.ThrowIfError(EVRNotificationError.ServiceUnavailable)
        );

        Assert.Equal(EVRNotificationError.ServiceUnavailable, exception.ErrorCode);
        Assert.NotEqual("VRNotificationException_ServiceUnavailable", exception.Message);
    }

    [Fact]
    public void VRSettingsException_ThrowIfError_未定義の値_値を含むVRSettingsExceptionを投げる()
    {
        var error = (EVRSettingsError)UndefinedErrorValue;

        var exception = Assert.Throws<VRSettingsException>(() =>
            VRSettingsException.ThrowIfError(error)
        );

        Assert.Equal(error, exception.ErrorCode);
        Assert.Contains(UndefinedErrorValue.ToString(), exception.Message);
    }

    [Fact]
    public void VRNotificationException_ThrowIfError_未定義の値_値を含むVRNotificationExceptionを投げる()
    {
        var error = (EVRNotificationError)UndefinedErrorValue;

        var exception = Assert.Throws<VRNotificationException>(() =>
            VRNotificationException.ThrowIfError(error)
        );

        Assert.Equal(error, exception.ErrorCode);
        Assert.Contains(UndefinedErrorValue.ToString(), exception.Message);
    }

    [Fact]
    public void VRScreenshotException_ThrowIfError_未定義の値_値を含むVRScreenshotExceptionを投げる()
    {
        var error = (EVRScreenshotError)UndefinedErrorValue;

        var exception = Assert.Throws<VRScreenshotException>(() =>
            VRScreenshotException.ThrowIfError(error)
        );

        Assert.Equal(error, exception.ErrorCode);
        Assert.Contains(UndefinedErrorValue.ToString(), exception.Message);
    }

    [Fact]
    public void VRCompositorException_ThrowIfError_未定義の値_値を含むVRCompositorExceptionを投げる()
    {
        var error = (EVRCompositorError)UndefinedErrorValue;

        var exception = Assert.Throws<VRCompositorException>(() =>
            VRCompositorException.ThrowIfError(error)
        );

        Assert.Equal(error, exception.ErrorCode);
        Assert.Contains(UndefinedErrorValue.ToString(), exception.Message);
    }

    [Fact]
    public void TrackedPropertyException_ThrowIfError_未定義の値_値を含むTrackedPropertyExceptionを投げる()
    {
        var error = (ETrackedPropertyError)UndefinedErrorValue;

        var exception = Assert.Throws<TrackedPropertyException>(() =>
            TrackedPropertyException.ThrowIfError(error)
        );

        Assert.Equal(error, exception.ErrorCode);
        Assert.Contains(UndefinedErrorValue.ToString(), exception.Message);
    }
}
