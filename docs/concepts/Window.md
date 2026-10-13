# Window — オーバーレイの種類と大きさ

FloatSoda の UI は、`WindowWidget` を根にした Widget ツリーとして、SteamVR のオーバーレイに表示されます。オーバーレイには、ダッシュボード・ワールド座標・デバイス追従の3種類があり、種類によって置かれる場所と届く入力が違います。VR の中での大きさは、UI のピクセル数を `Dpm` で割った長さで決まります。

## 概要

ウィンドウは、`app.CreateWindow(...)` に `DashboardWindow`、`WorldSpaceWindow`、`DeviceTrackedWindow` のどれかを渡して作ります。渡した `WindowWidget` は、そのまま Widget ツリーの根になります。1つのアプリで複数のウィンドウを作れます。

開発中は、SteamVR の代わりにデスクトップ上のウィンドウへ表示する `DesktopWindow` も使えます。HMD を被らずに見た目を確かめるための種類で、マウスで操作できます。

## いつ使うか

オーバーレイの種類は、ユーザーがその UI をいつ、どこで見るかで選びます。

| 種類 | 表示される場所 | 向いている UI | ポインタ入力 |
|---|---|---|---|
| `DashboardWindow` | SteamVR ダッシュボードのタブ。ユーザーが開いたときだけ見える | 設定画面、写真アルバムのように、腰を据えて操作する UI | 届く |
| `WorldSpaceWindow` | 部屋の決まった位置(`Position`、`Rotation`)。作った時点から見える | 部屋に置いておく時計や情報パネル | 届かない |
| `DeviceTrackedWindow` | コントローラーや HMD(`Target`)に付いて動く。作った時点から見える | 手元の通知、腕時計のような表示 | 届かない |

`WorldSpaceWindow` の `Position` は、プレイエリアの床の中央を原点とするメートル単位の座標です。既定は、中央から前方 1m、高さ 1m の位置です。

## 基本モデル

### ウィンドウの大きさ(ピクセル)

`Size` を指定すると、ウィンドウはその大きさに固定され、子にも同じ大きさの制約が渡ります。`Size` を省略すると、子をレイアウトした結果の大きさがウィンドウの大きさになります。制約の考え方は [Layout](Layout.md) にあります。

### VR の中での大きさ(メートル)

オーバーレイの VR の中での幅は、次の式で決まります。

```text
幅(メートル) = ウィンドウの幅(ピクセル) ÷ Dpm
```

`Dpm` は、1メートルに何ピクセルを並べるかを表す密度です。既定の `Dpm.Default` は 4000 で、1ピクセルが 0.25mm になります。高さは、ウィンドウの縦横の比から決まります ※要確認(SteamVR が幅だけを受け取り、高さを縦横の比で決めること)。

`Dpm` は大きいほど表示が小さくなります。たとえば、幅 340px のトーストを既定の 4000 で表示すると、幅は 8.5cm です。HMD の前方 60cm に置くと、ほとんど読めません。同じ 340px を 34cm で表示したいときは、`Dpm = new Dpm(1000)` を指定します。1ピクセルを何ミリにするかで決めたいときは、`Dpm.FromMillimetersPerPixel(1)` のようにも書けます。

```csharp
app.CreateWindow(new DeviceTrackedWindow
{
    Title = "Toast",
    Target = TrackedDevice.LeftController,
    Dpm = new Dpm(1000),   // 340px → 34cm
    Child = toast,
});
```

`DashboardWindow` では、ダッシュボードの大きさを SteamVR が管理するため、`Dpm` を変えても表示の大きさが変わらない場合があります ※要確認(どの条件で効かないか)。

### ウィンドウの情報を Widget から読む

`WindowWidget` は `InheritedWidget` でもあります。ツリーのどこからでも `WindowWidget.Of(context)` で、自分が載っているウィンドウの `Title` や `Size` を読めます。

種類で表示を変えたいときは、`WindowWidget.Of(context)` の戻り値をパターンマッチで判定します。`DashboardWindow.Of(context)` と書いても、種類の判定にはなりません。`DashboardWindow` は自分の `Of` を持たず、継承した `OverlayWindow.Of` が呼ばれるためです。

```csharp
Widget hint = WindowWidget.Of(context) is DashboardWindow
    ? new Text("レーザーポインターで操作できます")
    : new Text("このオーバーレイは表示専用です");
```

## 最小例

3種類のウィンドウを作る最小の形は、次のとおりです。

```csharp
// ダッシュボードのタブ
app.CreateWindow(new DashboardWindow { Title = "MyDashboard", Child = root });

// 部屋の決まった位置。Position を省略すると前方 1m・高さ 1m
app.CreateWindow(new WorldSpaceWindow { Title = "MyWorld", Child = root });

// 左コントローラーに追従
app.CreateWindow(new DeviceTrackedWindow { Title = "MyHand", Child = root, Target = TrackedDevice.LeftController });
```

`DesktopWindow` への切り替え方は、各サンプルの `Program.cs` にあります。たとえば [`samples/FloatSoda.Samples.Align/Program.cs`](../../samples/FloatSoda.Samples.Align/Program.cs) は、`--desktop` を付けて起動すると `DesktopWindow` に表示します。

## Unity / uGUI からの読み替え

| Unity | FloatSoda | 違う点 |
|---|---|---|
| Render Mode が World Space の Canvas | `WorldSpaceWindow` | シーンの GameObject ではなく、SteamVR のオーバーレイとして、ゲームの画面の上に重なる |
| コントローラーの子に置いた Canvas | `DeviceTrackedWindow` | 親子付けの代わりに `Target` と `Offset` で追従先を指定する |
| World Space の Canvas の `localScale`(0.001 なら 1 単位が 1mm) | `Dpm`(1000 なら 1ピクセルが 1mm) | 倍率ではなく密度で指定するため、値が大きいほど小さく表示される |
| Canvas の RectTransform の幅と高さ | `Size` | 省略すると、子のレイアウト結果に合わせる |

## 制約と未実装

`WorldSpaceWindow` と `DeviceTrackedWindow` は、今は表示専用です。ポインタ入力が届かないため、`GestureDetector` を置いてもコールバックは呼ばれません(追跡: [#182](https://github.com/sumx21t-3310/FloatSoda/issues/182))。

ほかに、次の制約があります。

- VR の中での大きさをメートルで直接指定する API はなく、`Dpm` で決める(追跡: [#151](https://github.com/sumx21t-3310/FloatSoda/issues/151))
- `Size` を省略したウィンドウは、起動後に中身が大きくなっても、SteamVR 上の表示範囲が最初の大きさのまま変わらない(追跡: [#285](https://github.com/sumx21t-3310/FloatSoda/issues/285))
- 作ったウィンドウを閉じる API はない(追跡: [#218](https://github.com/sumx21t-3310/FloatSoda/issues/218))
- 実行中にオーバーレイの種類を切り替える API はない(追跡: [#90](https://github.com/sumx21t-3310/FloatSoda/issues/90))

中身の大きさが変わるウィンドウ(項目が増えるリストなど)には、`Size` を指定します。

## 関連ページ

- [〜するには](../user/Guides.md) — 34cm の幅で表示する手順
- [Input](Input.md) — ウィンドウの種類ごとに届く入力
- [Layout](Layout.md) — `Size` から子へ渡る制約
