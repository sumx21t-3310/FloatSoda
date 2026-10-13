# GettingStarted — 最初のオーバーレイを作る

新しい C# のプロジェクトから始めて、SteamVR のダッシュボードに、ボタンを押すと数が1つ増えるオーバーレイを出します。手順は4つで、プロジェクトの作成、ウィンドウの表示、Widget の配置、状態の追加の順に進めます。

## 作るもの

完成すると、SteamVR のダッシュボードに「MyOverlay」というタブが増えます。タブを開くと、青い背景の上に `Count: 0` という文字と `+1` のボタンが表示されます。ボタンをレーザーポインターで押すたびに、数が1つ増えます。

## 前提

- Windows に .NET 10 SDK と SteamVR が入っていること
- `dotnet` コマンドでコンソールアプリを作れること
- アプリを実行する前に SteamVR を起動しておくこと

FloatSoda は Alpha 版です。公開 API は予告なく変わることがあります。

## Step 1 プロジェクトを作り、パッケージを参照する

コンソールアプリを作り、NuGet の `FloatSoda` パッケージを追加します。

```bash
dotnet new console -n MyOverlayApp -f net10.0
cd MyOverlayApp
dotnet add package FloatSoda
```

`MyOverlayApp.csproj` に `<PackageReference Include="FloatSoda" ... />` の行が増えていれば成功です。後の手順で使う `Microsoft.Extensions.Hosting` と `SkiaSharp` は、`FloatSoda` と一緒に入ります。別に追加する必要はありません。

## Step 2 最初のウィンドウを出す

`Program.cs` の中身を、次のコードに置き換えます。青い四角を1つだけ表示するオーバーレイです。

```csharp
using FloatSoda;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Paint;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFloatSoda();

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

Widget root = new ColoredBox
{
    Color = SKColors.CornflowerBlue
};

app.CreateWindow(new DashboardWindow
{
    Title = "MyOverlay",
    Size = new SKSize(600, 300),
    Child = root
});

await host.RunAsync();
```

コードの流れは次のとおりです。

1. `AddFloatSoda()` で、FloatSoda を .NET の Generic Host に登録する
2. `FloatSodaApp` を取り出し、`CreateWindow` でウィンドウを1つ作る
3. `host.RunAsync()` で、SteamVR が終わるまでアプリを動かし続ける

`DashboardWindow` は、SteamVR のダッシュボードにタブとして出るウィンドウです。`Title` はダッシュボードのタブに出る名前です。`Child` に渡した Widget が、ウィンドウの中身になります。`Size` は中身を描く大きさをピクセルで指定します。Step 4 で表示する数の桁が増えると中身の大きさが変わるため、ここで大きさを決めておきます(`Size` を省いたときの動きは [Window](../concepts/Window.md) にあります)。

SteamVR を起動してから、アプリを実行します。

```bash
dotnet run
```

SteamVR のダッシュボードに「MyOverlay」のタブが増え、開くと青い四角が表示されれば成功です。※要確認(ビルドが通ることまでは確かめた。SteamVR 上の表示は確かめていない)

アプリは、SteamVR を終了するか、ターミナルで Ctrl+C を押すと終わります。

## Step 3 Widget を置く

`root` の中身を、文字を2行並べる Widget に置き換えます。`Program.cs` の先頭に次の `using` を3行足します。

```csharp
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets.Layout;
```

続けて、`Widget root = ...;` の部分を次のコードに差し替えます。

```csharp
Widget root = new ColoredBox
{
    Color = SKColors.CornflowerBlue,
    Child = new Padding
    {
        Spacing = EdgeInsets.All(24),
        Child = new Column
        {
            Children =
            [
                new Text("Hello, FloatSoda")
                {
                    Style = new TextStyle { FontSize = 48, Color = SKColors.White }
                },
                new Text("Widgets inside widgets.")
                {
                    Style = new TextStyle { FontSize = 28, Color = SKColors.White }
                }
            ]
        }
    }
};
```

FloatSoda の画面は、Widget を `Child` と `Children` で入れ子にして組み立てます。この例では、`ColoredBox` が背景を塗り、`Padding` が内側に 24 ピクセルの余白を空け、`Column` が2つの `Text` を縦に並べます。Unity の Hierarchy に GameObject を並べる作業を、C# のコードで書く形です。

もう一度 `dotnet run` で実行し、青い背景の左上に白い文字が2行表示されれば成功です。※要確認(SteamVR 上の表示は確かめていない)

Widget の考え方は [Widget](../concepts/Widget.md) に、大きさと位置の決まり方は [Layout](../concepts/Layout.md) にあります。

## Step 4 状態を持たせる

押した回数を覚えておくには、状態を持つ Widget を作ります。プロジェクトに `CounterPanel.cs` を追加し、次のコードを書きます。

```csharp
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using SkiaSharp;

public record CounterPanel : StatefulWidget<CounterPanel>
{
    public override State<CounterPanel> CreateState() => new CounterPanelState();
}

public class CounterPanelState : State<CounterPanel>
{
    private int _count;

    public override Widget Build(IBuildContext context) => new Column
    {
        Children =
        [
            new Text($"Count: {_count}")
            {
                Style = new TextStyle { FontSize = 48, Color = SKColors.White }
            },
            new GestureDetector
            {
                OnTap = () => SetState(() => _count++),
                Child = new Container
                {
                    Width = 200,
                    Height = 60,
                    Color = SKColors.DarkSlateBlue,
                    Alignment = Alignment.Center,
                    Child = new Text("+1")
                    {
                        Style = new TextStyle { FontSize = 32, Color = SKColors.White }
                    }
                }
            }
        ]
    };
}
```

状態を持つ Widget は、`StatefulWidget<T>` を継承した `record` と、`State<T>` を継承した `class` の2つで書きます。`State<T>` の派生は `class` で宣言します。`record` は `record` 以外のクラスを継承できないためです。

`GestureDetector` は、子が押されたときに `OnTap` を呼びます。`OnTap` の中で `SetState` を呼ぶと、`_count` を書き換えたあとで `Build` がもう一度呼ばれ、新しい数が表示されます。uGUI のように `text.text = ...` で表示を直接書き換えるのではなく、状態を変えて画面を作り直す書き方です。

最後に、`Program.cs` の `root` を、`CounterPanel` を置く形に差し替えます。

```csharp
Widget root = new ColoredBox
{
    Color = SKColors.CornflowerBlue,
    Child = new Padding
    {
        Spacing = EdgeInsets.All(24),
        Child = new CounterPanel()
    }
};
```

`dotnet run` で実行し、ダッシュボードで `+1` のボタンを押すたびに `Count:` の数が1つ増えれば成功です。※要確認(SteamVR 上の操作は確かめていない)

ボタンを押せるのは、ダッシュボードのウィンドウ(`DashboardWindow`)だけです。ほかの種類のウィンドウにボタンを置いても、押したときの処理は呼ばれません(追跡: [#182](https://github.com/sumx21t-3310/FloatSoda/issues/182))。状態と作り直しの仕組みは [State](../concepts/State.md) にあります。

## 完成

完成したプロジェクトは、`Program.cs` と `CounterPanel.cs` の2つのファイルでできています。リポジトリには、途中までの形に近いサンプルがあります。

- [`samples/FloatSoda.Samples.GettingStarted`](../../samples/FloatSoda.Samples.GettingStarted/) — Step 2 と同じく、色の付いた四角を1つ出す最小のアプリ
- [`samples/FloatSoda.Samples.OverlayApp/CounterWidget.cs`](../../samples/FloatSoda.Samples.OverlayApp/CounterWidget.cs) — `GestureDetector` と `SetState` で数を増やす Widget

## 次に読むもの

- [FloatSoda の考え方](../concepts/Overview.md) — 今回書いたコードが、どうオーバーレイに表示されるか
- [〜するには](Guides.md) — 項目を並べる、色を切り替えるなど、目的ごとの手順
- [ミニアプリを作る](Tutorials.md) — 複数の Widget と状態と入力を組み合わせて、小さなアプリを作る
- Widget ごとの使い方 — リポジトリの [`samples/`](../../samples/) にある `FloatSoda.Samples.*` の各 README
