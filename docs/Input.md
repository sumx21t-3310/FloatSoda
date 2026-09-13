# Input — アクション入力(コントローラー)

FloatSoda.OVRのアクション入力は、Unity Input Systemに似た用語(ActionMap、Action、DefaultBinding)を用いて、VRコントローラーのボタン、トリガー、スティックを扱うAPIです。定義はすべてC#コードで完結し、アクションマニフェストJSONは自動生成されます。**ユーザーがJSONファイルを直接記述することはありません。**

> オーバーレイUI上のポインタ操作(ボタンのクリックなど)は、Widget のヒットテストが担当します。
> このページのアクション入力は、「UIの外」で発生するアプリレベルの入力(ショートカット、掴む、スクロールなど)を対象としています。

## 最小の例

```csharp
using FloatSoda.OVR.Input;

var grab = new InputAction<bool>
{
    Name = "grab",
    SuggestedPath = "/user/hand/right/input/trigger/click",
};

builder.Services.AddFloatSoda(new FloatSodaOptions
{
    AppKey = new AppKey("com.example.myoverlay"),
    InputActionMaps = [new InputActionMap { Name = "main", Actions = [grab] }],
});

grab.OnPerformed += _ => Console.WriteLine("トリガーが引かれた");
grab.OnReleased += () => Console.WriteLine("トリガーが離された");
// 毎フレームの値参照は grab.Value
```

## アクションの型

`InputAction<T>` の `T` に指定できる型は次の3つのみです。それ以外の型を指定すると、初期化時に例外が発生します。

| 型 | 用途 | イベント |
|---|---|---|
| `bool` | ボタン/クリック | `OnPerformed`(押下エッジ)/ `OnReleased` |
| `float` | トリガー引き量など1軸 | `OnPerformed`(値が変化したフレーム) |
| `System.Numerics.Vector2` | スティック/トラックパッド | `OnPerformed`(値が変化したフレーム) |

## Unity Input System との対応

| Unity Input System | FloatSoda | OpenVR (IVRInput) |
|---|---|---|
| InputActionAsset | `FloatSodaOptions.InputActionMaps` | アクションマニフェストJSON(自動生成) |
| InputActionMap | `InputActionMap` | アクションセット `/actions/{name}` |
| InputAction | `InputAction<T>` | アクション `/actions/{map}/in/{name}` |
| InputBinding | `DefaultBinding` / `SuggestedPath` | デフォルトバインディングJSON(自動生成) |
| `action.performed` | `OnPerformed` | — |
| `action.ReadValue<T>()` | `Value` | `GetDigitalActionData` / `GetAnalogActionData` |
| `map.Enable()` / `Disable()` | `InputActionMap.Enabled` | `UpdateActionState` の対象選択 |

## バインディングの決定権はSteamVRにある

Unity Input Systemと最も異なる点です。`DefaultBinding` や `SuggestedPath` はあくまで**初期割り当ての「提案」**であり、実際のバインディングはSteamVRが管理します。ユーザーはSteamVRのコントローラーバインディングUIからいつでも変更できます。そのため、次のようなAPIは**意図的に提供していません**。

- 実行時にバインディングを追加/変更する(`AddBinding` / `ApplyBindingOverride` 相当)
- アクションから現在のバインディングを列挙する(`action.bindings` 相当)
- composite binding(複数ボタンの合成)を定義する

## デフォルトバインディングの書き方

- `SuggestedPath` — 1つのOpenVR入力パスを、すべてのコントローラー種別(Index、Vive Wand、Oculus Touch)へ複製します。通常はこれで十分です。
- `DefaultBindings` — コントローラー種別ごとにパスを変更したい場合に使用します。指定した種別に対してのみ出力されます。

パスはOpenVRの正規表記です(SteamVRのバインディングUIに表示される表記と一致します)。

```
/user/hand/right/input/trigger/click   … トリガーをボタンとして(bool)
/user/hand/right/input/trigger/pull    … トリガー引き量(float)
/user/hand/right/input/thumbstick      … スティック(Vector2)
/user/hand/left/input/a/click          … 左手Aボタン(bool)
```

## 注意: オーバーレイアプリとアクションセットの競合

FloatSodaアプリはオーバーレイアプリであるため、シーンアプリ(VRChatなどのゲーム本体)と同時に動作します。同じ物理入力をシーンアプリも使用している場合、どちらが優先されるかはSteamVRのアクションセットの優先度に従います。ゲームの操作と衝突しにくい入力(使用されていないボタンや非利き手側など)をデフォルトに設定することをお勧めします。

## 生成物の場所

アクションマニフェスト一式は起動ごとに `%TEMP%/FloatSoda/{AppKey}/input/` へ生成され、`IVRInput.SetActionManifestPath` によって登録されます。デバッグ時は、生成されたJSONファイルを直接確認できます。
