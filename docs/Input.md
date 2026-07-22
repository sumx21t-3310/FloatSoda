# Input — アクション入力(コントローラー)

FloatSoda.OVR のアクション入力は、Unity Input System に似た語彙(ActionMap → Action → DefaultBinding)で
VRコントローラーのボタン・トリガー・スティックを扱うAPIです。定義はすべてC#コードで完結し、
アクションマニフェストJSONは自動生成されます。**ユーザーがJSONファイルを書くことはありません。**

> オーバーレイUI上のポインタ操作(ボタンのクリック等)はウィジェットのヒットテストが担当します。
> このページのアクション入力は「UIの外」のアプリレベル入力(ショートカット、掴む、スクロール等)用です。

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

`InputAction<T>` の `T` は次の3つだけです。それ以外の型は初期化時に例外になります。

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

Unityと最も違う点です。`DefaultBinding` / `SuggestedPath` は **初期割り当ての「提案」** であり、
実際のバインディングはSteamVRが管理し、ユーザーがSteamVRのコントローラーバインディングUIで
いつでも変更できます。このため次のAPIは **意図的に存在しません**。

- 実行時にバインディングを追加/変更する(`AddBinding` / `ApplyBindingOverride` 相当)
- アクションから現在のバインディングを列挙する(`action.bindings` 相当)
- composite binding(複数ボタンの合成)を定義する

## デフォルトバインディングの書き方

- `SuggestedPath` — 1本のOpenVR入力パスを全コントローラー種別(Index / Vive Wand / Oculus Touch)へ複製します。まずはこれで十分です。
- `DefaultBindings` — コントローラー種別ごとにパスを変えたいときに使います。指定した種別にだけ出力されます。

パスはOpenVR正規表記です(SteamVRのバインディングUIに表示される表記と一致します)。

```
/user/hand/right/input/trigger/click   … トリガーをボタンとして(bool)
/user/hand/right/input/trigger/pull    … トリガー引き量(float)
/user/hand/right/input/thumbstick      … スティック(Vector2)
/user/hand/left/input/a/click          … 左手Aボタン(bool)
```

## 注意: オーバーレイアプリとアクションセットの競合

FloatSodaアプリはオーバーレイアプリなので、シーンアプリ(VRChat等のゲーム本体)と同時に動きます。
同じ物理入力をシーンアプリも使っている場合の優先はSteamVRのアクションセット優先度に従います。
ゲームの操作と衝突しにくい入力(使われていないボタン、非利き手側など)をデフォルトに選ぶことを推奨します。

## 生成物の場所

アクションマニフェスト一式は `%TEMP%/FloatSoda/{AppKey}/input/` に毎起動時に生成され、
`IVRInput.SetActionManifestPath` で登録されます。デバッグ時はこのJSONを直接確認できます。
