namespace FloatSoda.OVR.Exceptions;

public class VRInitializeException(string message, EVRInitError errorCode)
    : OpenVRSystemException<EVRInitError>(message, errorCode)
{
    public static void ThrowIfError(EVRInitError error)
    {
        if (error == EVRInitError.None) return;

        throw error switch
        {
            // --- 一般的な初期化エラー ---
            EVRInitError.Unknown => new VRInitializeException("未知の初期化エラーが発生しました。", error),
            EVRInitError.Init_InstallationNotFound => new VRInitializeException("OpenVRのインストールが見つかりません。", error),
            EVRInitError.Init_InstallationCorrupt => new VRInitializeException("OpenVRのインストールが壊れています。", error),
            EVRInitError.Init_VRClientDLLNotFound => new VRInitializeException("vrclient.dllが見つかりません。", error),
            EVRInitError.Init_FileNotFound => new VRInitializeException("必要な構成ファイルが見つかりません。", error),
            EVRInitError.Init_FactoryNotFound => new VRInitializeException("ファクトリインターフェースの取得に失敗しました。", error),
            EVRInitError.Init_InterfaceNotFound => new VRInitializeException("要求されたインターフェースが見つかりません。", error),
            EVRInitError.Init_InvalidInterface => new VRInitializeException("インターフェースが無効です。", error),
            EVRInitError.Init_UserConfigDirectoryInvalid => new VRInitializeException("ユーザー設定ディレクトリが無効です。", error),
            EVRInitError.Init_HmdNotFound => new VRInitializeException("HMDが見つからないか、接続されていません。", error),
            EVRInitError.Init_NotInitialized => new VRInitializeException("OpenVRが初期化されていません。", error),
            EVRInitError.Init_PathRegistryNotFound => new VRInitializeException("パスレジストリが見つかりません。", error),
            EVRInitError.Init_NoConfigPath => new VRInitializeException("構成パスが設定されていません。", error),
            EVRInitError.Init_NoLogPath => new VRInitializeException("ログパスが設定されていません。", error),
            EVRInitError.Init_PathRegistryNotWritable => new VRInitializeException("パスレジストリに書き込めません。", error),
            EVRInitError.Init_AppInfoInitFailed => new VRInitializeException("アプリケーション情報の初期化に失敗しました。", error),
            EVRInitError.Init_Retry => new VRInitializeException("再試行が必要です。", error),
            EVRInitError.Init_InitCanceledByUser => new VRInitializeException("ユーザーによって初期化がキャンセルされました。", error),
            EVRInitError.Init_AnotherAppLaunching => new VRInitializeException("別のアプリケーションが起動中です。", error),
            EVRInitError.Init_SettingsInitFailed => new VRInitializeException("設定の初期化に失敗しました。", error),
            EVRInitError.Init_ShuttingDown => new VRInitializeException("システムが終了処理中です。", error),
            EVRInitError.Init_TooManyObjects => new VRInitializeException("オブジェクトが多すぎます。", error),
            EVRInitError.Init_NoServerForBackgroundApp => new VRInitializeException("バックグラウンドアプリ用のサーバーがありません。", error),
            EVRInitError.Init_NotSupportedWithCompositor => new VRInitializeException("コンポジター使用時はサポートされません。", error),
            EVRInitError.Init_NotAvailableToUtilityApps => new VRInitializeException("ユーティリティアプリでは利用できません。", error),
            EVRInitError.Init_Internal => new VRInitializeException("内部エラーが発生しました。", error),
            EVRInitError.Init_HmdDriverIdIsNone => new VRInitializeException("HMDドライバIDがNoneです。", error),
            EVRInitError.Init_HmdNotFoundPresenceFailed => new VRInitializeException("HMDの存在確認に失敗しました。", error),
            EVRInitError.Init_VRMonitorNotFound => new VRInitializeException("VRモニターが見つかりません。", error),
            EVRInitError.Init_VRMonitorStartupFailed => new VRInitializeException("VRモニターの起動に失敗しました。", error),
            EVRInitError.Init_LowPowerWatchdogNotSupported => new VRInitializeException("低電力ウォッチドッグはサポートされていません。",
                error),
            EVRInitError.Init_InvalidApplicationType => new VRInitializeException("アプリケーションタイプが無効です。", error),
            EVRInitError.Init_NotAvailableToWatchdogApps => new VRInitializeException("ウォッチドッグアプリでは利用できません。", error),
            EVRInitError.Init_WatchdogDisabledInSettings => new VRInitializeException("設定でウォッチドッグが無効になっています。", error),
            EVRInitError.Init_VRDashboardNotFound => new VRInitializeException("VRダッシュボードが見つかりません。", error),
            EVRInitError.Init_VRDashboardStartupFailed => new VRInitializeException("VRダッシュボードの起動に失敗しました。", error),
            EVRInitError.Init_VRHomeNotFound => new VRInitializeException("VR Homeが見つかりません。", error),
            EVRInitError.Init_VRHomeStartupFailed => new VRInitializeException("VR Homeの起動に失敗しました。", error),
            EVRInitError.Init_RebootingBusy => new VRInitializeException("再起動処理中のためビジーです。", error),
            EVRInitError.Init_FirmwareUpdateBusy => new VRInitializeException("ファームウェア更新中のためビジーです。", error),
            EVRInitError.Init_FirmwareRecoveryBusy => new VRInitializeException("ファームウェア復旧中のためビジーです。", error),
            EVRInitError.Init_USBServiceBusy => new VRInitializeException("USBサービスがビジーです。", error),
            EVRInitError.Init_VRWebHelperStartupFailed => new VRInitializeException("VR Web Helperの起動に失敗しました。", error),
            EVRInitError.Init_TrackerManagerInitFailed => new VRInitializeException("トラッカーマネージャーの初期化に失敗しました。", error),
            EVRInitError.Init_AlreadyRunning => new VRInitializeException("既に実行中です。", error),
            EVRInitError.Init_FailedForVrMonitor => new VRInitializeException("VRモニター用プロセスの初期化に失敗しました。", error),
            EVRInitError.Init_PropertyManagerInitFailed => new VRInitializeException("プロパティマネージャーの初期化に失敗しました。", error),
            EVRInitError.Init_WebServerFailed => new VRInitializeException("Webサーバーの起動に失敗しました。", error),
            EVRInitError.Init_IllegalTypeTransition => new VRInitializeException("不正なタイプ遷移です。", error),
            EVRInitError.Init_MismatchedRuntimes => new VRInitializeException("ランタイムのバージョンが一致しません。", error),
            EVRInitError.Init_InvalidProcessId => new VRInitializeException("プロセスIDが無効です。", error),
            EVRInitError.Init_VRServiceStartupFailed => new VRInitializeException("VRサービスの起動に失敗しました。", error),
            EVRInitError.Init_PrismNeedsNewDrivers => new VRInitializeException("Prismドライバの更新が必要です。", error),
            EVRInitError.Init_PrismStartupTimedOut => new VRInitializeException("Prismの起動がタイムアウトしました。", error),
            EVRInitError.Init_CouldNotStartPrism => new VRInitializeException("Prismを起動できませんでした。", error),
            // EVRInitError.Init_CreateDriverDirectDeviceFailed => new VRInitializeException("ドライバダイレクトデバイスの作成に失敗しました。",
                // error),
            EVRInitError.Init_PrismExitedUnexpectedly => new VRInitializeException("Prismが予期せず終了しました。", error),

            // --- ドライバ関連 ---
            EVRInitError.Driver_Failed => new Driver("ドライバのエラーです。", error),
            EVRInitError.Driver_Unknown => new Driver("未知のドライバエラーです。", error),
            EVRInitError.Driver_HmdUnknown => new Driver("未知のHMDドライバです。", error),
            EVRInitError.Driver_NotLoaded => new Driver("ドライバがロードされていません。", error),
            EVRInitError.Driver_RuntimeOutOfDate => new Driver("ドライバのランタイムが古すぎます。", error),
            EVRInitError.Driver_HmdInUse => new Driver("HMDが別のプロセスで使用中です。", error),
            EVRInitError.Driver_NotCalibrated => new Driver("ドライバが校正されていません。", error),
            EVRInitError.Driver_CalibrationInvalid => new Driver("ドライバの校正データが無効です。", error),
            EVRInitError.Driver_HmdDisplayNotFound => new Driver("HMDのディスプレイが見つかりません。", error),
            EVRInitError.Driver_TrackedDeviceInterfaceUnknown => new Driver(
                "トラッキングデバイスインターフェースが不明です。", error),
            EVRInitError.Driver_HmdDriverIdOutOfBounds => new Driver("HMDドライバIDが範囲外です。", error),
            EVRInitError.Driver_HmdDisplayMirrored => new Driver("HMDディスプレイがミラーリング設定になっています。",
                error),
            EVRInitError.Driver_HmdDisplayNotFoundLaptop => new Driver("ラップトップ用HMDディスプレイが見つかりません。", error),

            // --- IPC (プロセス間通信) 関連 ---
            EVRInitError.IPC_ServerInitFailed => new IPC("IPCサーバーの初期化に失敗しました。", error),
            EVRInitError.IPC_ConnectFailed => new IPC("IPC接続に失敗しました。", error),
            EVRInitError.IPC_SharedStateInitFailed => new IPC("共有ステートの初期化に失敗しました。", error),
            EVRInitError.IPC_CompositorInitFailed => new IPC("コンポジターの初期化に失敗しました。", error),
            EVRInitError.IPC_MutexInitFailed => new IPC("Mutexの初期化に失敗しました。", error),
            EVRInitError.IPC_Failed => new IPC("IPC接続エラーが発生しました。", error),
            EVRInitError.IPC_CompositorConnectFailed => new IPC("コンポジターへの接続に失敗しました。", error),
            EVRInitError.IPC_CompositorInvalidConnectResponse => new IPC("コンポジターからの接続レスポンスが無効です。",
                error),
            EVRInitError.IPC_ConnectFailedAfterMultipleAttempts => new IPC("複数回の試行後、IPC接続に失敗しました。",
                error),
            EVRInitError.IPC_ConnectFailedAfterTargetExited => new IPC("ターゲット終了後、IPC接続に失敗しました。",
                error),
            EVRInitError.IPC_NamespaceUnavailable => new IPC("IPCネームスペースを利用できません。", error),

            // --- コンポジター関連 ---
            EVRInitError.Compositor_Failed => new Compositor("コンポジターが失敗しました。", error),
            EVRInitError.Compositor_D3D11HardwareRequired => new Compositor(
                "Direct3D11ハードウェアが必要です。", error),
            EVRInitError.Compositor_FirmwareRequiresUpdate => new Compositor(
                "コンポジター用にファームウェアの更新が必要です。", error),
            EVRInitError.Compositor_OverlayInitFailed => new Compositor("コンポジターオーバーレイの初期化に失敗しました。",
                error),
            EVRInitError.Compositor_ScreenshotsInitFailed => new Compositor(
                "コンポジタースクリーンショットの初期化に失敗しました。", error),
            EVRInitError.Compositor_UnableToCreateDevice => new Compositor(
                "グラフィックスデバイスを作成できませんでした。", error),
            EVRInitError.Compositor_SharedStateIsNull => new Compositor("共有ステートがnullです。", error),
            EVRInitError.Compositor_NotificationManagerIsNull => new Compositor("通知マネージャーがnullです。",
                error),
            EVRInitError.Compositor_ResourceManagerClientIsNull => new Compositor(
                "リソースマネージャークライアントがnullです。", error),
            EVRInitError.Compositor_MessageOverlaySharedStateInitFailure => new Compositor(
                "メッセージオーバーレイ共有ステートの初期化に失敗しました。", error),
            EVRInitError.Compositor_PropertiesInterfaceIsNull => new Compositor(
                "プロパティインターフェースがnullです。", error),
            EVRInitError.Compositor_CreateFullscreenWindowFailed => new Compositor(
                "全画面ウィンドウの作成に失敗しました。", error),
            EVRInitError.Compositor_SettingsInterfaceIsNull => new Compositor("設定インターフェースがnullです。",
                error),
            EVRInitError.Compositor_FailedToShowWindow =>
                new Compositor("ウィンドウの表示に失敗しました。", error),
            EVRInitError.Compositor_DistortInterfaceIsNull => new Compositor(
                "歪み補正インターフェースがnullです。", error),
            EVRInitError.Compositor_DisplayFrequencyFailure => new Compositor(
                "ディスプレイ周波数の取得に失敗しました。", error),
            EVRInitError.Compositor_RendererInitializationFailed => new Compositor(
                "レンダラーの初期化に失敗しました。", error),
            EVRInitError.Compositor_DXGIFactoryInterfaceIsNull => new Compositor(
                "DXGIファクトリインターフェースがnullです。", error),
            EVRInitError.Compositor_DXGIFactoryCreateFailed => new Compositor(
                "DXGIファクトリの作成に失敗しました。", error),
            EVRInitError.Compositor_DXGIFactoryQueryFailed => new Compositor(
                "DXGIファクトリのクエリに失敗しました。", error),
            EVRInitError.Compositor_InvalidAdapterDesktop => new Compositor("デスクトップアダプターが無効です。",
                error),
            EVRInitError.Compositor_InvalidHmdAttachment => new Compositor("HMDの接続が無効です。", error),
            EVRInitError.Compositor_InvalidOutputDesktop =>
                new Compositor("出力デスクトップが無効です。", error),
            EVRInitError.Compositor_InvalidDeviceProvided => new Compositor("提供されたデバイスが無効です。",
                error),
            EVRInitError.Compositor_D3D11RendererInitializationFailed => new Compositor(
                "D3D11レンダラーの初期化に失敗しました。", error),
            EVRInitError.Compositor_FailedToFindDisplayMode => new Compositor(
                "ディスプレイモードが見つかりませんでした。", error),
            EVRInitError.Compositor_FailedToCreateSwapChain => new Compositor(
                "スワップチェーンの作成に失敗しました。", error),
            EVRInitError.Compositor_FailedToGetBackBuffer => new Compositor("バックバッファの取得に失敗しました。",
                error),
            EVRInitError.Compositor_FailedToCreateRenderTarget => new Compositor(
                "レンダーターゲットの作成に失敗しました。", error),
            EVRInitError.Compositor_FailedToCreateDXGI2SwapChain => new Compositor(
                "DXGI2スワップチェーンの作成に失敗しました。", error),
            EVRInitError.Compositor_FailedtoGetDXGI2BackBuffer => new Compositor(
                "DXGI2バックバッファの取得に失敗しました。", error),
            EVRInitError.Compositor_FailedToCreateDXGI2RenderTarget => new Compositor(
                "DXGI2レンダーターゲットの作成に失敗しました。", error),
            EVRInitError.Compositor_FailedToGetDXGIDeviceInterface => new Compositor(
                "DXGIデバイスインターフェースの取得に失敗しました。", error),
            EVRInitError.Compositor_SelectDisplayMode => new Compositor("ディスプレイモードの選択に失敗しました。",
                error),
            EVRInitError.Compositor_FailedToCreateNvAPIRenderTargets => new Compositor(
                "NvAPIレンダーターゲットの作成に失敗しました。", error),
            EVRInitError.Compositor_NvAPISetDisplayMode => new Compositor(
                "NvAPIディスプレイモード設定に失敗しました。", error),
            EVRInitError.Compositor_FailedToCreateDirectModeDisplay => new Compositor(
                "ダイレクトモードディスプレイの作成に失敗しました。", error),
            EVRInitError.Compositor_InvalidHmdPropertyContainer => new Compositor(
                "HMDプロパティコンテナが無効です。", error),
            EVRInitError.Compositor_UpdateDisplayFrequency => new Compositor(
                "ディスプレイ周波数の更新に失敗しました。", error),
            EVRInitError.Compositor_CreateRasterizerState => new Compositor(
                "ラスタライザステートの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateWireframeRasterizerState => new Compositor(
                "ワイヤーフレームラスタライザステートの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateSamplerState => new Compositor("サンプラーステートの作成に失敗しました。",
                error),
            EVRInitError.Compositor_CreateClampToBorderSamplerState => new Compositor(
                "ClampToBorderサンプラーステートの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateAnisoSamplerState => new Compositor(
                "異方性サンプラーステートの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateOverlaySamplerState => new Compositor(
                "オーバーレイサンプラーステートの作成に失敗しました。", error),
            EVRInitError.Compositor_CreatePanoramaSamplerState => new Compositor(
                "パノラマサンプラーステートの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateFontSamplerState => new Compositor(
                "フォントサンプラーステートの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateNoBlendState => new Compositor("NoBlendステートの作成に失敗しました。",
                error),
            EVRInitError.Compositor_CreateBlendState => new Compositor("Blendステートの作成に失敗しました。",
                error),
            EVRInitError.Compositor_CreateAlphaBlendState => new Compositor(
                "AlphaBlendステートの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateBlendStateMaskR => new Compositor(
                "BlendStateMaskRの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateBlendStateMaskG => new Compositor(
                "BlendStateMaskGの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateBlendStateMaskB => new Compositor(
                "BlendStateMaskBの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateDepthStencilState => new Compositor(
                "深度ステンシルステートの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateDepthStencilStateNoWrite => new Compositor(
                "NoWrite深度ステンシルステートの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateDepthStencilStateNoDepth => new Compositor(
                "NoDepth深度ステンシルステートの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateFlushTexture => new Compositor("Flushテクスチャの作成に失敗しました。",
                error),
            EVRInitError.Compositor_CreateDistortionSurfaces => new Compositor(
                "歪み補正サーフェスの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateConstantBuffer => new Compositor("定数バッファの作成に失敗しました。",
                error),
            EVRInitError.Compositor_CreateHmdPoseConstantBuffer => new Compositor(
                "HMDポーズ定数バッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateHmdPoseStagingConstantBuffer => new Compositor(
                "HMDポーズステージングバッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateSharedFrameInfoConstantBuffer => new Compositor(
                "フレーム情報共有バッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateOverlayConstantBuffer => new Compositor(
                "オーバーレイ定数バッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateSceneTextureIndexConstantBuffer => new Compositor(
                "シーンテクスチャインデックスバッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateReadableSceneTextureIndexConstantBuffer =>
                new Compositor("読み取り可能シーンテクスチャインデックスバッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateLayerGraphicsTextureIndexConstantBuffer =>
                new Compositor("レイヤーグラフィックスインデックスバッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateLayerComputeTextureIndexConstantBuffer => new Compositor(
                "レイヤー演算インデックスバッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateLayerComputeSceneTextureIndexConstantBuffer =>
                new Compositor("レイヤー演算シーンインデックスバッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateComputeHmdPoseConstantBuffer => new Compositor(
                "演算用HMDポーズバッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateGeomConstantBuffer => new Compositor(
                "幾何定数バッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreatePanelMaskConstantBuffer => new Compositor(
                "パネルマスクバッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreatePixelSimUBO => new Compositor("PixelSimUBOの作成に失敗しました。",
                error),
            EVRInitError.Compositor_CreateMSAARenderTextures => new Compositor(
                "MSAAレンダーテクスチャの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateResolveRenderTextures => new Compositor(
                "Resolveレンダーテクスチャの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateComputeResolveRenderTextures => new Compositor(
                "演算用Resolveレンダーテクスチャの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateDriverDirectModeResolveTextures => new Compositor(
                "ドライバダイレクトResolveテクスチャの作成に失敗しました。", error),
            EVRInitError.Compositor_OpenDriverDirectModeResolveTextures => new Compositor(
                "ドライバダイレクトResolveテクスチャを開けませんでした。", error),
            EVRInitError.Compositor_CreateFallbackSyncTexture => new Compositor(
                "フォールバック同期テクスチャの作成に失敗しました。", error),
            EVRInitError.Compositor_ShareFallbackSyncTexture => new Compositor(
                "フォールバック同期テクスチャの共有に失敗しました。", error),
            EVRInitError.Compositor_CreateOverlayIndexBuffer => new Compositor(
                "オーバーレイインデックスバッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateOverlayVertexBuffer => new Compositor(
                "オーバーレイ頂点バッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateTextVertexBuffer => new Compositor(
                "テキスト頂点バッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateTextIndexBuffer => new Compositor(
                "テキストインデックスバッファの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateMirrorTextures => new Compositor("ミラーテクスチャの作成に失敗しました。",
                error),
            EVRInitError.Compositor_CreateLastFrameRenderTexture => new Compositor(
                "直前フレームレンダリングテクスチャの作成に失敗しました。", error),
            EVRInitError.Compositor_CreateMirrorOverlay => new Compositor("ミラーオーバーレイの作成に失敗しました。",
                error),
            EVRInitError.Compositor_FailedToCreateVirtualDisplayBackbuffer => new Compositor(
                "仮想ディスプレイバックバッファの作成に失敗しました。", error),
            EVRInitError.Compositor_DisplayModeNotSupported => new Compositor(
                "ディスプレイモードがサポートされていません。", error),
            EVRInitError.Compositor_CreateOverlayInvalidCall => new Compositor(
                "オーバーレイ作成呼び出しが無効です。", error),
            EVRInitError.Compositor_CreateOverlayAlreadyInitialized => new Compositor(
                "オーバーレイは既に初期化されています。", error),
            EVRInitError.Compositor_FailedToCreateMailbox => new Compositor("メールボックスの作成に失敗しました。",
                error),
            EVRInitError.Compositor_WindowInterfaceIsNull => new Compositor(
                "ウィンドウインターフェースがnullです。", error),
            EVRInitError.Compositor_SystemLayerCreateInstance => new Compositor(
                "システムレイヤーインスタンスの作成に失敗しました。", error),
            EVRInitError.Compositor_SystemLayerCreateSession => new Compositor(
                "システムレイヤーセッションの作成に失敗しました。", error),

            // --- ベンダー固有エラー (Oculus/Meta等) ---
            EVRInitError.VendorSpecific_UnableToConnectToOculusRuntime => new VendorSpecific(
                "Oculusランタイムに接続できません。", error),
            EVRInitError.VendorSpecific_WindowsNotInDevMode => new VendorSpecific(
                "Windowsが開発者モードではありません。", error),
            EVRInitError.VendorSpecific_HmdFound_CantOpenDevice => new VendorSpecific(
                "HMDデバイスを開けませんでした。", error),
            EVRInitError.VendorSpecific_HmdFound_UnableToRequestConfigStart => new VendorSpecific(
                "HMD構成開始の要求に失敗しました。", error),
            EVRInitError.VendorSpecific_HmdFound_NoStoredConfig => new VendorSpecific(
                "HMD内に保存された構成がありません。", error),
            EVRInitError.VendorSpecific_HmdFound_ConfigTooBig => new VendorSpecific(
                "HMD構成データが大きすぎます。", error),
            EVRInitError.VendorSpecific_HmdFound_ConfigTooSmall => new VendorSpecific(
                "HMD構成データが小さすぎます。", error),
            EVRInitError.VendorSpecific_HmdFound_UnableToInitZLib => new VendorSpecific(
                "zlibの初期化に失敗しました。", error),
            EVRInitError.VendorSpecific_HmdFound_CantReadFirmwareVersion => new VendorSpecific(
                "ファームウェアバージョンの読み取りに失敗しました。", error),
            EVRInitError.VendorSpecific_HmdFound_UnableToSendUserDataStart => new VendorSpecific(
                "ユーザーデータ送信開始に失敗しました。", error),
            EVRInitError.VendorSpecific_HmdFound_UnableToGetUserDataStart => new VendorSpecific(
                "ユーザーデータ取得開始に失敗しました。", error),
            EVRInitError.VendorSpecific_HmdFound_UnableToGetUserDataNext => new VendorSpecific(
                "ユーザーデータ次フレーム取得に失敗しました。", error),
            EVRInitError.VendorSpecific_HmdFound_UserDataAddressRange => new VendorSpecific(
                "ユーザーデータのアドレス範囲エラーです。", error),
            EVRInitError.VendorSpecific_HmdFound_UserDataError => new VendorSpecific(
                "ユーザーデータエラーが発生しました。", error),
            EVRInitError.VendorSpecific_HmdFound_ConfigFailedSanityCheck => new VendorSpecific(
                "HMD構成データの整合性チェックに失敗しました。", error),
            EVRInitError.VendorSpecific_OculusRuntimeBadInstall => new VendorSpecific(
                "Oculusランタイムのインストールが不完全です。", error),

            // --- その他 ---
            EVRInitError.Steam_SteamInstallationNotFound => new VRInitializeException("Steamのインストールが見つかりません。", error),
            _ => throw new VRInitializeException($"予期しない初期化エラーが発生しました: {error}", error)
        };
    }

    public sealed class Driver(string message, EVRInitError errorCode) : VRInitializeException(message, errorCode);

    public sealed class IPC(string message, EVRInitError errorCode) : VRInitializeException(message, errorCode);

    public sealed class Compositor(string message, EVRInitError errorCode) : VRInitializeException(message, errorCode);

    public sealed class VendorSpecific(string message, EVRInitError errorCode) : VRInitializeException(message, errorCode);
}