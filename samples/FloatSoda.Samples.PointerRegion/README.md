# PointerRegion

## これは何か

`PointerRegion` は、マウスや VR レーザーなどのポインターが子の領域へ出入りしたこと(Enter / Exit)を通知するウィジェットです。Flutter の `MouseRegion` に対応します。押下に依存しないホバー状態 — ボタンの「指さしたら光る」など — を作るために使います。

このサンプルは1枚の的(まと)に `PointerRegion` + `Listener` + `GestureDetector` を重ね、Enter / Exit / Down / Up / Cancel / Tap がどの順で届くかを1画面で確認できるようにしています。

## 使い方

### 出入りを受け取る

`OnPointerEnter` / `OnPointerExit` に `Action<PointerEvent>` を渡します。`Behaviour` の既定は `Opaque` で、子の空白を含む領域全体が対象です(`Listener` の既定 `DeferToChild` とは異なります)。

このサンプルの的は、`PointerRegion` の内側へ `Listener`(Down / Up / Cancel)と `GestureDetector`(Tap)を重ねています。3層それぞれのイベントが1本のポインター操作からどう届くかが見どころです。

```csharp
new PointerRegionWidget
{
    OnPointerEnter = HandleEnter,
    OnPointerExit = HandleExit,
    Child = new Listener
    {
        Behaviour = HitTestBehaviour.Opaque,
        OnPointerDown = HandleDown,
        OnPointerUp = HandleUp,
        OnPointerCancel = HandleCancel,
        Child = new GestureDetector
        {
            Behaviour = HitTestBehaviour.Opaque,
            OnTap = HandleTap,
            Child = BuildTarget()
        }
    }
}
```

このサンプルは名前空間が `FloatSoda.Samples.PointerRegion` で型名と衝突するため、`PointerRegionWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new PointerRegion { ... }` と書けます。

### ホバー状態を描画へ反映する

Enter / Exit で状態を切り替え、`SetState` で色やラベルを差し替えます。このサンプルの的は、ホバーで水色、押下でオレンジ、Cancel 後は赤へ変わります。

```csharp
    private void HandleEnter(PointerEvent pointerEvent)
    {
        Log(pointerEvent);
        SetState(() =>
        {
            _hovered = true;
            _enterCount++;
            _status = "ENTER / HOVERING";
        });
    }
```

### Cancel を観察する

トリガーを引いたままレーザーを領域の外へ出すと、押下中のポインター列は `Cancel` で中断され、タップは成立しません。ボタンの「押しかけてやめる」操作に相当します。この確認には実機のレーザー操作が必要です(手順は [checklist.md](checklist.md))。

### 入力が届くウィンドウ種別

ポインター座標が届くのは、現状 `DashboardWindow` と `DesktopWindow` です。`WorldSpaceWindow` / `DeviceTrackedWindow` では Enter / Exit も発火しません(issue #182)。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| ウィジェット名 | `PointerRegion`(VR レーザーもマウスも同じ「ポインター」として扱う) | `MouseRegion` |
| コールバック | `OnPointerEnter` / `OnPointerExit` の2種 | `onEnter` / `onExit` に加えて `onHover`(領域内の移動)がある |
| カーソル指定 | 無し | `cursor` でマウスカーソルの形状を変えられる |
| ヒットテスト指定 | `Behaviour`(`HitTestBehaviour`)。既定 `Opaque` | `opaque`(`bool`)。既定 `true`(実質同じ既定) |

Enter / Exit が押下に依存せず届く点は Flutter と同等です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.PointerRegion -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.PointerRegion
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/Input.md](../../docs/Input.md) — 入力イベントの流れ
- [FloatSoda.Samples.Listener](../FloatSoda.Samples.Listener) — Down / Up / Move も扱う場合
- [FloatSoda.Samples.GestureDetector](../FloatSoda.Samples.GestureDetector) — タップ・ドラッグの意味付け
