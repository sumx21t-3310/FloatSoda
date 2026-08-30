# GestureDetector

## これは何か

`GestureDetector` は、生のポインターイベントへ意味付けをして、タップやドラッグ(Pan)として通知するウィジェットです。Flutter の `GestureDetector` に対応します。

「押して、動かさずに離したらタップ」「押して動かしたらドラッグ」といった判定はジェスチャ認識器が行い、コールバックには成立した結果だけが届きます。生のイベントが要る場合は [Listener](../FloatSoda.Samples.Listener) を使います。

## 使い方

### タップを受け取る

`OnTap` はタップが成立した時に呼ばれます。押しただけでは呼ばれず、離した時点で成立します。空白を含む領域全体で反応させたいので `Behaviour = HitTestBehaviour.Opaque` を指定しています。

```csharp
new GestureDetectorWidget
{
    Behaviour = HitTestBehaviour.Opaque,
    OnTap = HandleTap,
    Child = new ColoredBox
    {
        Color = new Color(124, 205, 255),
        Child = new SizedBox
        {
            Width = 220,
            Height = 64,
            Child = new Center
            {
                Child = new Text($"タップ {_tapCount} 回")
                {
                    Style = new TextStyle { FontSize = 20, Color = new Color(16, 20, 31) }
                }
            }
        }
    }
}
```

このサンプルは名前空間が `FloatSoda.Samples.GestureDetector` で型名と衝突するため、`GestureDetectorWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new GestureDetector { ... }` と書けます。

### ドラッグで動かす(Pan)

`OnPanUpdate` には**前回位置からの移動量(デルタ)**が届きます。累積した値を `Positioned` の座標へ反映すると、箱がポインターに追従します。

```csharp
new GestureDetectorWidget
{
    Behaviour = HitTestBehaviour.Opaque,
    OnPanStart = _ => SetState(() => _dragging = true),
    OnPanUpdate = HandlePanUpdate,
    OnPanEnd = () => SetState(() => _dragging = false),
    Child = new ColoredBox
    {
        Color = _dragging
            ? new Color(255, 209, 102)
            : new Color(255, 111, 97),
        Child = new SizedBox { Width = BoxSize, Height = BoxSize }
    }
}
```

```csharp
private void HandlePanUpdate(Offset delta) => SetState(() =>
{
    _boxX = Math.Clamp(_boxX + delta.X, 0, FieldWidth - BoxSize);
    _boxY = Math.Clamp(_boxY + delta.Y, 0, FieldHeight - BoxSize);
});
```

タップとドラッグの判定は排他です。押してすぐ離せば `OnTap`、動かせば Pan 系だけが呼ばれます。

### 入力が届くウィンドウ種別

ポインター座標が届くのは、現状 `DashboardWindow` と `DesktopWindow` です。`WorldSpaceWindow` / `DeviceTrackedWindow` ではコールバックが発火しません(issue #182)。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 対応ジェスチャ | タップと Pan(`OnTap` / `OnPanStart` / `OnPanUpdate` / `OnPanEnd`)のみ | ダブルタップ、長押し、水平・垂直ドラッグ、スケール等も扱う |
| コールバックの引数 | `Offset`(開始位置・移動量)または引数なし | `TapDownDetails` / `DragUpdateDetails` 等の詳細オブジェクト |
| ヒットテスト指定の名前 | `Behaviour`(`HitTestBehaviour`) | `behavior`(`HitTestBehavior`、米国綴り) |

タップが Up で成立する点、タップとドラッグの判定が競合裁定(アリーナ)で排他になる点は Flutter と同等です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.GestureDetector -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.GestureDetector
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/Input.md](../../docs/Input.md) — 入力イベントの流れとジェスチャ認識
- [FloatSoda.Samples.Listener](../FloatSoda.Samples.Listener) — 生のポインターイベントが要る場合
- [FloatSoda.Samples.PointerRegion](../FloatSoda.Samples.PointerRegion) — Enter / Exit / Cancel と Tap の組み合わせ
