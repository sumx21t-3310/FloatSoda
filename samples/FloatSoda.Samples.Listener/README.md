# Listener

## これは何か

`Listener` は、子のヒット領域で発生した低レベルのポインターイベント(Down / Up / Move / Enter / Exit / Cancel)をそのまま通知するウィジェットです。Flutter の `Listener` に対応します。

「押して離したらタップ」のような意味付けはしません。意味付けが必要なら [GestureDetector](../FloatSoda.Samples.GestureDetector) を使い、`Listener` は生のイベントが要る場面(独自のジェスチャ判定、デバッグ、座標の記録など)に使います。

## 使い方

### イベントを受け取る

`OnPointerDown` / `OnPointerUp` などへ `Action<PointerEvent>` を渡します。`PointerEvent` からはフェーズ、ポインター ID、位置が取れます。

```csharp
new ListenerWidget
{
    OnPointerDown = e => Record("DeferToChild", e),
    OnPointerUp = e => RecordUp("DeferToChild", e),
    Child = new Center
    {
        Child = Marker(new Color(124, 205, 255))
    }
}
```

このサンプルは名前空間が `FloatSoda.Samples.Listener` で型名 `Listener` と衝突するため、`ListenerWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new Listener { ... }` と書けます。

### 反応する範囲を決める(HitTestBehaviour)

`Behaviour` の既定は `DeferToChild` で、**子がヒットした時だけ**反応します。上の例では中央の四角を押した時だけイベントが来て、枠の空白部分は素通りします。

`Opaque` にすると、子の空白を含む**領域全体**が対象になります。ボタンのように「この範囲のどこを押しても反応してほしい」場合はこちらです。

```csharp
new ListenerWidget
{
    Behaviour = HitTestBehaviour.Opaque,
    OnPointerDown = e => Record("Opaque", e),
    OnPointerUp = e => RecordUp("Opaque", e),
    Child = new Center
    {
        Child = Marker(new Color(255, 111, 97))
    }
}
```

3つ目の `Translucent` は、自身をヒット経路へ加えつつ、**子がヒットしなかった場合に限り**背後の兄弟へも探索を通します。子がヒットした場合はそこで探索が止まり、背後には届きません。

### 入力が届くウィンドウ種別

ポインター座標が届くのは、現状 `DashboardWindow` と `DesktopWindow` です。`WorldSpaceWindow` / `DeviceTrackedWindow` にはまだ入力経路がなく、イベントは発火しません(issue #182)。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| Enter / Exit | `Listener` 自身に `OnPointerEnter` / `OnPointerExit` がある | `Listener` には無く、`MouseRegion` が担当する |
| ヒットテスト指定の名前 | `Behaviour`(`HitTestBehaviour`) | `behavior`(`HitTestBehavior`、米国綴り) |
| その他のコールバック | Down / Up / Move / Enter / Exit / Cancel の6種 | 上記に加えて `onPointerHover` / `onPointerSignal`(スクロール)/ PanZoom 系がある |

`DeferToChild` / `Opaque` / `Translucent` の意味は Flutter と同等です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.Listener -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.Listener
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/Input.md](../../docs/Input.md) — 入力イベントの流れ
- [FloatSoda.Samples.GestureDetector](../FloatSoda.Samples.GestureDetector) — タップ・ドラッグの意味付けが要る場合
- [FloatSoda.Samples.PointerRegion](../FloatSoda.Samples.PointerRegion) — 押下に依存しないホバー状態
