using FloatSoda.OVR.Exceptions.Resources;

namespace FloatSoda.OVR.Exceptions;

/// <summary>
/// Thrown when OpenVR fails to initialize (<see cref="EVRInitError"/>). Subclasses categorize the failure
/// by subsystem (<see cref="Driver"/>, <see cref="IPC"/>, <see cref="Compositor"/>, <see cref="VendorSpecific"/>).
/// </summary>
public class VRInitializeException(string message, EVRInitError errorCode)
    : OpenVRSystemException<EVRInitError>(message, errorCode)
{
    /// <summary>Throws a categorized <see cref="VRInitializeException"/> if <paramref name="error"/> indicates a failure.</summary>
    public static void ThrowIfError(EVRInitError error)
    {
        if (error == EVRInitError.None) return;

        throw error switch
        {
            // --- 一般的な初期化エラー ---
            EVRInitError.Unknown => new VRInitializeException(ExceptionMessages.VRInitializeException_Unknown, error),
            EVRInitError.Init_InstallationNotFound => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_InstallationNotFound, error),
            EVRInitError.Init_InstallationCorrupt => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_InstallationCorrupt, error),
            EVRInitError.Init_VRClientDLLNotFound => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_VRClientDLLNotFound, error),
            EVRInitError.Init_FileNotFound => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_FileNotFound, error),
            EVRInitError.Init_FactoryNotFound => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_FactoryNotFound, error),
            EVRInitError.Init_InterfaceNotFound => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_InterfaceNotFound, error),
            EVRInitError.Init_InvalidInterface => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_InvalidInterface, error),
            EVRInitError.Init_UserConfigDirectoryInvalid => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_UserConfigDirectoryInvalid, error),
            EVRInitError.Init_HmdNotFound => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_HmdNotFound, error),
            EVRInitError.Init_NotInitialized => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_NotInitialized, error),
            EVRInitError.Init_PathRegistryNotFound => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_PathRegistryNotFound, error),
            EVRInitError.Init_NoConfigPath => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_NoConfigPath, error),
            EVRInitError.Init_NoLogPath => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_NoLogPath, error),
            EVRInitError.Init_PathRegistryNotWritable => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_PathRegistryNotWritable, error),
            EVRInitError.Init_AppInfoInitFailed => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_AppInfoInitFailed, error),
            EVRInitError.Init_Retry => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_Retry, error),
            EVRInitError.Init_InitCanceledByUser => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_InitCanceledByUser, error),
            EVRInitError.Init_AnotherAppLaunching => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_AnotherAppLaunching, error),
            EVRInitError.Init_SettingsInitFailed => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_SettingsInitFailed, error),
            EVRInitError.Init_ShuttingDown => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_ShuttingDown, error),
            EVRInitError.Init_TooManyObjects => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_TooManyObjects, error),
            EVRInitError.Init_NoServerForBackgroundApp => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_NoServerForBackgroundApp, error),
            EVRInitError.Init_NotSupportedWithCompositor => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_NotSupportedWithCompositor, error),
            EVRInitError.Init_NotAvailableToUtilityApps => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_NotAvailableToUtilityApps, error),
            EVRInitError.Init_Internal => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_Internal, error),
            EVRInitError.Init_HmdDriverIdIsNone => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_HmdDriverIdIsNone, error),
            EVRInitError.Init_HmdNotFoundPresenceFailed => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_HmdNotFoundPresenceFailed, error),
            EVRInitError.Init_VRMonitorNotFound => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_VRMonitorNotFound, error),
            EVRInitError.Init_VRMonitorStartupFailed => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_VRMonitorStartupFailed, error),
            EVRInitError.Init_LowPowerWatchdogNotSupported => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_LowPowerWatchdogNotSupported,
                error),
            EVRInitError.Init_InvalidApplicationType => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_InvalidApplicationType, error),
            EVRInitError.Init_NotAvailableToWatchdogApps => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_NotAvailableToWatchdogApps, error),
            EVRInitError.Init_WatchdogDisabledInSettings => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_WatchdogDisabledInSettings, error),
            EVRInitError.Init_VRDashboardNotFound => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_VRDashboardNotFound, error),
            EVRInitError.Init_VRDashboardStartupFailed => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_VRDashboardStartupFailed, error),
            EVRInitError.Init_VRHomeNotFound => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_VRHomeNotFound, error),
            EVRInitError.Init_VRHomeStartupFailed => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_VRHomeStartupFailed, error),
            EVRInitError.Init_RebootingBusy => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_RebootingBusy, error),
            EVRInitError.Init_FirmwareUpdateBusy => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_FirmwareUpdateBusy, error),
            EVRInitError.Init_FirmwareRecoveryBusy => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_FirmwareRecoveryBusy, error),
            EVRInitError.Init_USBServiceBusy => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_USBServiceBusy, error),
            EVRInitError.Init_VRWebHelperStartupFailed => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_VRWebHelperStartupFailed, error),
            EVRInitError.Init_TrackerManagerInitFailed => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_TrackerManagerInitFailed, error),
            EVRInitError.Init_AlreadyRunning => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_AlreadyRunning, error),
            EVRInitError.Init_FailedForVrMonitor => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_FailedForVrMonitor, error),
            EVRInitError.Init_PropertyManagerInitFailed => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_PropertyManagerInitFailed, error),
            EVRInitError.Init_WebServerFailed => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_WebServerFailed, error),
            EVRInitError.Init_IllegalTypeTransition => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_IllegalTypeTransition, error),
            EVRInitError.Init_MismatchedRuntimes => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_MismatchedRuntimes, error),
            EVRInitError.Init_InvalidProcessId => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_InvalidProcessId, error),
            EVRInitError.Init_VRServiceStartupFailed => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_VRServiceStartupFailed, error),
            EVRInitError.Init_PrismNeedsNewDrivers => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_PrismNeedsNewDrivers, error),
            EVRInitError.Init_PrismStartupTimedOut => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_PrismStartupTimedOut, error),
            EVRInitError.Init_CouldNotStartPrism => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_CouldNotStartPrism, error),
            // EVRInitError.Init_CreateDriverDirectDeviceFailed => new VRInitializeException("ドライバダイレクトデバイスの作成に失敗しました。",
                // error),
            EVRInitError.Init_PrismExitedUnexpectedly => new VRInitializeException(ExceptionMessages.VRInitializeException_Init_PrismExitedUnexpectedly, error),

            // --- ドライバ関連 ---
            EVRInitError.Driver_Failed => new Driver(ExceptionMessages.VRInitializeException_Driver_Failed, error),
            EVRInitError.Driver_Unknown => new Driver(ExceptionMessages.VRInitializeException_Driver_Unknown, error),
            EVRInitError.Driver_HmdUnknown => new Driver(ExceptionMessages.VRInitializeException_Driver_HmdUnknown, error),
            EVRInitError.Driver_NotLoaded => new Driver(ExceptionMessages.VRInitializeException_Driver_NotLoaded, error),
            EVRInitError.Driver_RuntimeOutOfDate => new Driver(ExceptionMessages.VRInitializeException_Driver_RuntimeOutOfDate, error),
            EVRInitError.Driver_HmdInUse => new Driver(ExceptionMessages.VRInitializeException_Driver_HmdInUse, error),
            EVRInitError.Driver_NotCalibrated => new Driver(ExceptionMessages.VRInitializeException_Driver_NotCalibrated, error),
            EVRInitError.Driver_CalibrationInvalid => new Driver(ExceptionMessages.VRInitializeException_Driver_CalibrationInvalid, error),
            EVRInitError.Driver_HmdDisplayNotFound => new Driver(ExceptionMessages.VRInitializeException_Driver_HmdDisplayNotFound, error),
            EVRInitError.Driver_TrackedDeviceInterfaceUnknown => new Driver(
                ExceptionMessages.VRInitializeException_Driver_TrackedDeviceInterfaceUnknown, error),
            EVRInitError.Driver_HmdDriverIdOutOfBounds => new Driver(ExceptionMessages.VRInitializeException_Driver_HmdDriverIdOutOfBounds, error),
            EVRInitError.Driver_HmdDisplayMirrored => new Driver(ExceptionMessages.VRInitializeException_Driver_HmdDisplayMirrored,
                error),
            EVRInitError.Driver_HmdDisplayNotFoundLaptop => new Driver(ExceptionMessages.VRInitializeException_Driver_HmdDisplayNotFoundLaptop, error),

            // --- IPC (プロセス間通信) 関連 ---
            EVRInitError.IPC_ServerInitFailed => new IPC(ExceptionMessages.VRInitializeException_IPC_ServerInitFailed, error),
            EVRInitError.IPC_ConnectFailed => new IPC(ExceptionMessages.VRInitializeException_IPC_ConnectFailed, error),
            EVRInitError.IPC_SharedStateInitFailed => new IPC(ExceptionMessages.VRInitializeException_IPC_SharedStateInitFailed, error),
            EVRInitError.IPC_CompositorInitFailed => new IPC(ExceptionMessages.VRInitializeException_IPC_CompositorInitFailed, error),
            EVRInitError.IPC_MutexInitFailed => new IPC(ExceptionMessages.VRInitializeException_IPC_MutexInitFailed, error),
            EVRInitError.IPC_Failed => new IPC(ExceptionMessages.VRInitializeException_IPC_Failed, error),
            EVRInitError.IPC_CompositorConnectFailed => new IPC(ExceptionMessages.VRInitializeException_IPC_CompositorConnectFailed, error),
            EVRInitError.IPC_CompositorInvalidConnectResponse => new IPC(ExceptionMessages.VRInitializeException_IPC_CompositorInvalidConnectResponse,
                error),
            EVRInitError.IPC_ConnectFailedAfterMultipleAttempts => new IPC(ExceptionMessages.VRInitializeException_IPC_ConnectFailedAfterMultipleAttempts,
                error),
            EVRInitError.IPC_ConnectFailedAfterTargetExited => new IPC(ExceptionMessages.VRInitializeException_IPC_ConnectFailedAfterTargetExited,
                error),
            EVRInitError.IPC_NamespaceUnavailable => new IPC(ExceptionMessages.VRInitializeException_IPC_NamespaceUnavailable, error),

            // --- コンポジター関連 ---
            EVRInitError.Compositor_Failed => new Compositor(ExceptionMessages.VRInitializeException_Compositor_Failed, error),
            EVRInitError.Compositor_D3D11HardwareRequired => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_D3D11HardwareRequired, error),
            EVRInitError.Compositor_FirmwareRequiresUpdate => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_FirmwareRequiresUpdate, error),
            EVRInitError.Compositor_OverlayInitFailed => new Compositor(ExceptionMessages.VRInitializeException_Compositor_OverlayInitFailed,
                error),
            EVRInitError.Compositor_ScreenshotsInitFailed => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_ScreenshotsInitFailed, error),
            EVRInitError.Compositor_UnableToCreateDevice => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_UnableToCreateDevice, error),
            EVRInitError.Compositor_SharedStateIsNull => new Compositor(ExceptionMessages.VRInitializeException_Compositor_SharedStateIsNull, error),
            EVRInitError.Compositor_NotificationManagerIsNull => new Compositor(ExceptionMessages.VRInitializeException_Compositor_NotificationManagerIsNull,
                error),
            EVRInitError.Compositor_ResourceManagerClientIsNull => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_ResourceManagerClientIsNull, error),
            EVRInitError.Compositor_MessageOverlaySharedStateInitFailure => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_MessageOverlaySharedStateInitFailure, error),
            EVRInitError.Compositor_PropertiesInterfaceIsNull => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_PropertiesInterfaceIsNull, error),
            EVRInitError.Compositor_CreateFullscreenWindowFailed => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateFullscreenWindowFailed, error),
            EVRInitError.Compositor_SettingsInterfaceIsNull => new Compositor(ExceptionMessages.VRInitializeException_Compositor_SettingsInterfaceIsNull,
                error),
            EVRInitError.Compositor_FailedToShowWindow =>
                new Compositor(ExceptionMessages.VRInitializeException_Compositor_FailedToShowWindow, error),
            EVRInitError.Compositor_DistortInterfaceIsNull => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_DistortInterfaceIsNull, error),
            EVRInitError.Compositor_DisplayFrequencyFailure => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_DisplayFrequencyFailure, error),
            EVRInitError.Compositor_RendererInitializationFailed => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_RendererInitializationFailed, error),
            EVRInitError.Compositor_DXGIFactoryInterfaceIsNull => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_DXGIFactoryInterfaceIsNull, error),
            EVRInitError.Compositor_DXGIFactoryCreateFailed => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_DXGIFactoryCreateFailed, error),
            EVRInitError.Compositor_DXGIFactoryQueryFailed => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_DXGIFactoryQueryFailed, error),
            EVRInitError.Compositor_InvalidAdapterDesktop => new Compositor(ExceptionMessages.VRInitializeException_Compositor_InvalidAdapterDesktop,
                error),
            EVRInitError.Compositor_InvalidHmdAttachment => new Compositor(ExceptionMessages.VRInitializeException_Compositor_InvalidHmdAttachment, error),
            EVRInitError.Compositor_InvalidOutputDesktop =>
                new Compositor(ExceptionMessages.VRInitializeException_Compositor_InvalidOutputDesktop, error),
            EVRInitError.Compositor_InvalidDeviceProvided => new Compositor(ExceptionMessages.VRInitializeException_Compositor_InvalidDeviceProvided,
                error),
            EVRInitError.Compositor_D3D11RendererInitializationFailed => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_D3D11RendererInitializationFailed, error),
            EVRInitError.Compositor_FailedToFindDisplayMode => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_FailedToFindDisplayMode, error),
            EVRInitError.Compositor_FailedToCreateSwapChain => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_FailedToCreateSwapChain, error),
            EVRInitError.Compositor_FailedToGetBackBuffer => new Compositor(ExceptionMessages.VRInitializeException_Compositor_FailedToGetBackBuffer,
                error),
            EVRInitError.Compositor_FailedToCreateRenderTarget => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_FailedToCreateRenderTarget, error),
            EVRInitError.Compositor_FailedToCreateDXGI2SwapChain => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_FailedToCreateDXGI2SwapChain, error),
            EVRInitError.Compositor_FailedtoGetDXGI2BackBuffer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_FailedtoGetDXGI2BackBuffer, error),
            EVRInitError.Compositor_FailedToCreateDXGI2RenderTarget => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_FailedToCreateDXGI2RenderTarget, error),
            EVRInitError.Compositor_FailedToGetDXGIDeviceInterface => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_FailedToGetDXGIDeviceInterface, error),
            EVRInitError.Compositor_SelectDisplayMode => new Compositor(ExceptionMessages.VRInitializeException_Compositor_SelectDisplayMode,
                error),
            EVRInitError.Compositor_FailedToCreateNvAPIRenderTargets => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_FailedToCreateNvAPIRenderTargets, error),
            EVRInitError.Compositor_NvAPISetDisplayMode => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_NvAPISetDisplayMode, error),
            EVRInitError.Compositor_FailedToCreateDirectModeDisplay => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_FailedToCreateDirectModeDisplay, error),
            EVRInitError.Compositor_InvalidHmdPropertyContainer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_InvalidHmdPropertyContainer, error),
            EVRInitError.Compositor_UpdateDisplayFrequency => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_UpdateDisplayFrequency, error),
            EVRInitError.Compositor_CreateRasterizerState => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateRasterizerState, error),
            EVRInitError.Compositor_CreateWireframeRasterizerState => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateWireframeRasterizerState, error),
            EVRInitError.Compositor_CreateSamplerState => new Compositor(ExceptionMessages.VRInitializeException_Compositor_CreateSamplerState,
                error),
            EVRInitError.Compositor_CreateClampToBorderSamplerState => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateClampToBorderSamplerState, error),
            EVRInitError.Compositor_CreateAnisoSamplerState => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateAnisoSamplerState, error),
            EVRInitError.Compositor_CreateOverlaySamplerState => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateOverlaySamplerState, error),
            EVRInitError.Compositor_CreatePanoramaSamplerState => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreatePanoramaSamplerState, error),
            EVRInitError.Compositor_CreateFontSamplerState => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateFontSamplerState, error),
            EVRInitError.Compositor_CreateNoBlendState => new Compositor(ExceptionMessages.VRInitializeException_Compositor_CreateNoBlendState,
                error),
            EVRInitError.Compositor_CreateBlendState => new Compositor(ExceptionMessages.VRInitializeException_Compositor_CreateBlendState,
                error),
            EVRInitError.Compositor_CreateAlphaBlendState => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateAlphaBlendState, error),
            EVRInitError.Compositor_CreateBlendStateMaskR => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateBlendStateMaskR, error),
            EVRInitError.Compositor_CreateBlendStateMaskG => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateBlendStateMaskG, error),
            EVRInitError.Compositor_CreateBlendStateMaskB => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateBlendStateMaskB, error),
            EVRInitError.Compositor_CreateDepthStencilState => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateDepthStencilState, error),
            EVRInitError.Compositor_CreateDepthStencilStateNoWrite => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateDepthStencilStateNoWrite, error),
            EVRInitError.Compositor_CreateDepthStencilStateNoDepth => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateDepthStencilStateNoDepth, error),
            EVRInitError.Compositor_CreateFlushTexture => new Compositor(ExceptionMessages.VRInitializeException_Compositor_CreateFlushTexture,
                error),
            EVRInitError.Compositor_CreateDistortionSurfaces => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateDistortionSurfaces, error),
            EVRInitError.Compositor_CreateConstantBuffer => new Compositor(ExceptionMessages.VRInitializeException_Compositor_CreateConstantBuffer,
                error),
            EVRInitError.Compositor_CreateHmdPoseConstantBuffer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateHmdPoseConstantBuffer, error),
            EVRInitError.Compositor_CreateHmdPoseStagingConstantBuffer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateHmdPoseStagingConstantBuffer, error),
            EVRInitError.Compositor_CreateSharedFrameInfoConstantBuffer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateSharedFrameInfoConstantBuffer, error),
            EVRInitError.Compositor_CreateOverlayConstantBuffer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateOverlayConstantBuffer, error),
            EVRInitError.Compositor_CreateSceneTextureIndexConstantBuffer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateSceneTextureIndexConstantBuffer, error),
            EVRInitError.Compositor_CreateReadableSceneTextureIndexConstantBuffer =>
                new Compositor(ExceptionMessages.VRInitializeException_Compositor_CreateReadableSceneTextureIndexConstantBuffer, error),
            EVRInitError.Compositor_CreateLayerGraphicsTextureIndexConstantBuffer =>
                new Compositor(ExceptionMessages.VRInitializeException_Compositor_CreateLayerGraphicsTextureIndexConstantBuffer, error),
            EVRInitError.Compositor_CreateLayerComputeTextureIndexConstantBuffer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateLayerComputeTextureIndexConstantBuffer, error),
            EVRInitError.Compositor_CreateLayerComputeSceneTextureIndexConstantBuffer =>
                new Compositor(ExceptionMessages.VRInitializeException_Compositor_CreateLayerComputeSceneTextureIndexConstantBuffer, error),
            EVRInitError.Compositor_CreateComputeHmdPoseConstantBuffer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateComputeHmdPoseConstantBuffer, error),
            EVRInitError.Compositor_CreateGeomConstantBuffer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateGeomConstantBuffer, error),
            EVRInitError.Compositor_CreatePanelMaskConstantBuffer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreatePanelMaskConstantBuffer, error),
            EVRInitError.Compositor_CreatePixelSimUBO => new Compositor(ExceptionMessages.VRInitializeException_Compositor_CreatePixelSimUBO,
                error),
            EVRInitError.Compositor_CreateMSAARenderTextures => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateMSAARenderTextures, error),
            EVRInitError.Compositor_CreateResolveRenderTextures => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateResolveRenderTextures, error),
            EVRInitError.Compositor_CreateComputeResolveRenderTextures => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateComputeResolveRenderTextures, error),
            EVRInitError.Compositor_CreateDriverDirectModeResolveTextures => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateDriverDirectModeResolveTextures, error),
            EVRInitError.Compositor_OpenDriverDirectModeResolveTextures => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_OpenDriverDirectModeResolveTextures, error),
            EVRInitError.Compositor_CreateFallbackSyncTexture => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateFallbackSyncTexture, error),
            EVRInitError.Compositor_ShareFallbackSyncTexture => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_ShareFallbackSyncTexture, error),
            EVRInitError.Compositor_CreateOverlayIndexBuffer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateOverlayIndexBuffer, error),
            EVRInitError.Compositor_CreateOverlayVertexBuffer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateOverlayVertexBuffer, error),
            EVRInitError.Compositor_CreateTextVertexBuffer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateTextVertexBuffer, error),
            EVRInitError.Compositor_CreateTextIndexBuffer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateTextIndexBuffer, error),
            EVRInitError.Compositor_CreateMirrorTextures => new Compositor(ExceptionMessages.VRInitializeException_Compositor_CreateMirrorTextures,
                error),
            EVRInitError.Compositor_CreateLastFrameRenderTexture => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateLastFrameRenderTexture, error),
            EVRInitError.Compositor_CreateMirrorOverlay => new Compositor(ExceptionMessages.VRInitializeException_Compositor_CreateMirrorOverlay,
                error),
            EVRInitError.Compositor_FailedToCreateVirtualDisplayBackbuffer => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_FailedToCreateVirtualDisplayBackbuffer, error),
            EVRInitError.Compositor_DisplayModeNotSupported => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_DisplayModeNotSupported, error),
            EVRInitError.Compositor_CreateOverlayInvalidCall => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateOverlayInvalidCall, error),
            EVRInitError.Compositor_CreateOverlayAlreadyInitialized => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_CreateOverlayAlreadyInitialized, error),
            EVRInitError.Compositor_FailedToCreateMailbox => new Compositor(ExceptionMessages.VRInitializeException_Compositor_FailedToCreateMailbox,
                error),
            EVRInitError.Compositor_WindowInterfaceIsNull => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_WindowInterfaceIsNull, error),
            EVRInitError.Compositor_SystemLayerCreateInstance => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_SystemLayerCreateInstance, error),
            EVRInitError.Compositor_SystemLayerCreateSession => new Compositor(
                ExceptionMessages.VRInitializeException_Compositor_SystemLayerCreateSession, error),

            // --- ベンダー固有エラー (Oculus/Meta等) ---
            EVRInitError.VendorSpecific_UnableToConnectToOculusRuntime => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_UnableToConnectToOculusRuntime, error),
            EVRInitError.VendorSpecific_WindowsNotInDevMode => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_WindowsNotInDevMode, error),
            EVRInitError.VendorSpecific_HmdFound_CantOpenDevice => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_HmdFound_CantOpenDevice, error),
            EVRInitError.VendorSpecific_HmdFound_UnableToRequestConfigStart => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_HmdFound_UnableToRequestConfigStart, error),
            EVRInitError.VendorSpecific_HmdFound_NoStoredConfig => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_HmdFound_NoStoredConfig, error),
            EVRInitError.VendorSpecific_HmdFound_ConfigTooBig => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_HmdFound_ConfigTooBig, error),
            EVRInitError.VendorSpecific_HmdFound_ConfigTooSmall => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_HmdFound_ConfigTooSmall, error),
            EVRInitError.VendorSpecific_HmdFound_UnableToInitZLib => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_HmdFound_UnableToInitZLib, error),
            EVRInitError.VendorSpecific_HmdFound_CantReadFirmwareVersion => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_HmdFound_CantReadFirmwareVersion, error),
            EVRInitError.VendorSpecific_HmdFound_UnableToSendUserDataStart => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_HmdFound_UnableToSendUserDataStart, error),
            EVRInitError.VendorSpecific_HmdFound_UnableToGetUserDataStart => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_HmdFound_UnableToGetUserDataStart, error),
            EVRInitError.VendorSpecific_HmdFound_UnableToGetUserDataNext => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_HmdFound_UnableToGetUserDataNext, error),
            EVRInitError.VendorSpecific_HmdFound_UserDataAddressRange => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_HmdFound_UserDataAddressRange, error),
            EVRInitError.VendorSpecific_HmdFound_UserDataError => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_HmdFound_UserDataError, error),
            EVRInitError.VendorSpecific_HmdFound_ConfigFailedSanityCheck => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_HmdFound_ConfigFailedSanityCheck, error),
            EVRInitError.VendorSpecific_OculusRuntimeBadInstall => new VendorSpecific(
                ExceptionMessages.VRInitializeException_VendorSpecific_OculusRuntimeBadInstall, error),

            // --- その他 ---
            EVRInitError.Steam_SteamInstallationNotFound => new VRInitializeException(ExceptionMessages.VRInitializeException_Steam_SteamInstallationNotFound, error),
            _ => throw new VRInitializeException(string.Format(ExceptionMessages.VRInitializeException_UnexpectedError, error), error)
        };
    }

    /// <summary>Initialization failure originating from a device driver.</summary>
    public sealed class Driver(string message, EVRInitError errorCode) : VRInitializeException(message, errorCode);

    /// <summary>Initialization failure in inter-process communication with the OpenVR runtime.</summary>
    public sealed class IPC(string message, EVRInitError errorCode) : VRInitializeException(message, errorCode);

    /// <summary>Initialization failure originating from the compositor.</summary>
    public sealed class Compositor(string message, EVRInitError errorCode) : VRInitializeException(message, errorCode);

    /// <summary>Initialization failure specific to a hardware vendor.</summary>
    public sealed class VendorSpecific(string message, EVRInitError errorCode) : VRInitializeException(message, errorCode);
}
