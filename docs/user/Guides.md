# 〜するには

FloatSoda でよく持つ目的ごとに、それを果たすコードと注意点をまとめたページです。目的の見出しから、必要な節だけを読んでください。仕組みの説明はコンセプトの各ページにあり、この節からリンクします。

各節のコードは、[GettingStarted](GettingStarted.md) で作ったプロジェクト(NuGet の `FloatSoda` を参照したコンソールアプリ)に足して使う断片です。断片ごとに、必要な `using` を先頭に書いています。どの断片もビルドが通ることは確かめましたが、SteamVR 上の見た目と操作は確かめていません。※要確認

目的ごとの完成形のサンプル(層2の Cookbook 型)は、まだリポジトリにありません(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。Widget ごとの最小の使用例は、[`samples/`](../../samples/) にある `FloatSoda.Samples.*` の README にあります。

## 項目を縦に並べて表示する

`Column` の `Children` に、表示したい Widget を上から順に並べます。項目の間を空けるときは、高さだけを持つ `SizedBox` を間に挟みます。

```csharp
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

Widget list = new Column
{
    Children =
    [
        new Text("Apple"),
        new SizedBox { Height = 12 },
        new Text("Banana"),
        new SizedBox { Height = 12 },
        new Text("Cherry")
    ]
};
```

`Column` は、項目を上端と左端にそろえて並べます。そろえ方を変えるときは、`MainAxisAlignment`(縦方向)と `CrossAxisAlignment`(横方向)を指定します。

ウィンドウに収まらない項目は表示されません。スクロールする Widget(`ListView` など)は、公開 API ではまだ使えません(追跡: [#69](https://github.com/sumx21t-3310/FloatSoda/issues/69))。横に並べるときは、同じ書き方で `Row` を使います。

関連: [Layout](../concepts/Layout.md)、[`samples/FloatSoda.Samples.Flex`](../../samples/FloatSoda.Samples.Flex/)

## 角を丸める

`Container` の `Decoration` に `BoxDecoration` を渡し、`BorderRadius` で角の半径を指定します。枠線を付けるときは、同じ `BoxDecoration` の `Border` を指定します。

```csharp
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using SkiaSharp;

Widget card = new Container
{
    Width = 300,
    Height = 120,
    Decoration = new BoxDecoration
    {
        Color = SKColors.White,
        BorderRadius = BorderRadius.Circular(16),
        Border = Border.All(new BorderSide { Color = SKColors.Gray, Width = 2 })
    },
    Alignment = Alignment.Center,
    Child = new Text("Card")
};
```

背景色は `BoxDecoration.Color` に書きます。`Container` の `Color` と `Decoration` を同時に指定すると、`InvalidOperationException` が投げられます。

`BorderRadius` が丸めるのは背景と枠線だけで、子の絵は角からはみ出します。子も角に合わせて切り取るときは、`ClipRoundRect` で包み、同じ `BorderRadius` を指定します。

影を付ける API はありません。`BoxDecoration` が持つのは、背景色・角の半径・枠線の3つです(追跡: ※要確認)。

関連: [`samples/FloatSoda.Samples.DecoratedBox`](../../samples/FloatSoda.Samples.DecoratedBox/)、[`samples/FloatSoda.Samples.Container`](../../samples/FloatSoda.Samples.Container/)、[`samples/FloatSoda.Samples.Clip`](../../samples/FloatSoda.Samples.Clip/)

## チェックボックスの状態を持つ

オンとオフを覚えておく Widget を、`StatefulWidget` と `GestureDetector` で作ります。押されたら `SetState` の中で値を反転し、値に合わせて四角の色を変えます。

```csharp
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using SkiaSharp;

public record CheckBoxRow : StatefulWidget<CheckBoxRow>
{
    public required string Label { get; init; }

    public override State<CheckBoxRow> CreateState() => new CheckBoxRowState();
}

public class CheckBoxRowState : State<CheckBoxRow>
{
    private bool _isChecked;

    public override Widget Build(IBuildContext context) => new GestureDetector
    {
        OnTap = () => SetState(() => _isChecked = !_isChecked),
        Child = new Row
        {
            MainAxisSize = MainAxisSize.Min,
            CrossAxisAlignment = CrossAxisAlignment.Center,
            Children =
            [
                new Container
                {
                    Width = 32,
                    Height = 32,
                    Decoration = new BoxDecoration
                    {
                        Color = _isChecked ? SKColors.SeaGreen : SKColors.White,
                        BorderRadius = BorderRadius.Circular(4),
                        Border = Border.All(new BorderSide { Color = SKColors.Gray, Width = 2 })
                    }
                },
                new SizedBox { Width = 12 },
                new Text(Widget!.Label)
            ]
        }
    };
}
```

`State` の中から Widget のプロパティ(ここでは `Label`)を読むときは、`Widget!.Label` と書きます。`MainAxisSize.Min` を指定すると、`Row` の幅が中身の幅になり、押せる範囲も中身の幅に収まります。

組み込みのチェックボックスの Widget はありません。ボタンなどの部品を提供する UI の層も、まだ使えません(追跡: [#102](https://github.com/sumx21t-3310/FloatSoda/issues/102))。押したときに反応するのは `DashboardWindow` の中だけです(「[ワールド座標のウィンドウで操作したいとき](#ワールド座標のウィンドウで操作したいとき)」)。

関連: [State](../concepts/State.md)、[Input](../concepts/Input.md)、[`samples/FloatSoda.Samples.GestureDetector`](../../samples/FloatSoda.Samples.GestureDetector/)

## 画面全体の色をまとめて切り替える

色を `InheritedWidget` に持たせて、色を読む Widget より親の位置に置きます。子の Widget はそこから色を読みます。親の色を変えると、色を読んだ Widget がすべて作り直されます。

まず、色を配る Widget を定義します。

```csharp
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Widgets;

public sealed record AppTheme : InheritedWidget
{
    public required Color Background { get; init; }
    public required Color Foreground { get; init; }

    public static AppTheme Of(IBuildContext context) =>
        context.DependOnInheritedWidgetOfExactType<AppTheme>()
        ?? throw new InvalidOperationException("AppTheme が祖先にありません。");

    public override bool UpdateShouldNotify(InheritedWidget oldWidget) =>
        oldWidget is not AppTheme old
        || old.Background != Background
        || old.Foreground != Foreground;
}
```

`Of` は、いちばん近い祖先の `AppTheme` を探し、呼んだ Widget を「色を読んだ側」として登録します。`UpdateShouldNotify` が `true` を返すと、登録された Widget が作り直されます。

次に、色を読む Widget と、色を切り替える親の Widget を書きます。

```csharp
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;

public record ThemedLabel : StatelessWidget
{
    public required string Data { get; init; }

    public override Widget Build(IBuildContext context) =>
        new Text(Data) { Style = new TextStyle { Color = AppTheme.Of(context).Foreground } };
}

public record ThemedBackground : StatelessWidget
{
    public required Widget Child { get; init; }

    public override Widget Build(IBuildContext context) =>
        new ColoredBox { Color = AppTheme.Of(context).Background, Child = Child };
}

public record ThemeSwitcher : StatefulWidget<ThemeSwitcher>
{
    public override State<ThemeSwitcher> CreateState() => new ThemeSwitcherState();
}

public class ThemeSwitcherState : State<ThemeSwitcher>
{
    private bool _isDark;

    public override Widget Build(IBuildContext context) => new AppTheme
    {
        Background = _isDark ? new Color(32, 32, 40) : new Color(255, 255, 255),
        Foreground = _isDark ? new Color(255, 255, 255) : new Color(32, 32, 40),
        Child = new GestureDetector
        {
            OnTap = () => SetState(() => _isDark = !_isDark),
            Child = new ThemedBackground
            {
                Child = new Column
                {
                    Children =
                    [
                        new ThemedLabel { Data = "Tap to switch theme" },
                        new ThemedLabel { Data = "Second label" }
                    ]
                }
            }
        }
    };
}
```

`AppTheme.Of(context)` は、`AppTheme` より下にある Widget の `Build` から呼びます。`AppTheme` を返している `ThemeSwitcherState.Build` の `context` からは、自分が返した `AppTheme` を読めません。そのため、色を読む部分を `ThemedLabel` や `ThemedBackground` のような別の Widget に分けています。

文字の書式だけをまとめて変えるときは、組み込みの `DefaultTextStyle` を使えます。配下の `Text` は、自分で指定しなかった書式を `DefaultTextStyle` から受け継ぎます。色の型と指定のしかたは、「[16進の色コードで色を指定する](#16進の色コードで色を指定する)」にあります。

関連: [State](../concepts/State.md)

## Row の中でテキストをはみ出させない

`Row` の中の `Text` を `Expanded` で包みます。`Text` は `Row` の残りの幅を受け取り、その幅で折り返します。

```csharp
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

Widget row = new SizedBox
{
    Width = 400,
    Child = new Row
    {
        Children =
        [
            new SizedBox { Width = 48, Height = 48 },
            new Expanded
            {
                Child = new Text("A long message that would otherwise run past the right edge of the row.")
            }
        ]
    }
};
```

`Expanded` で包まない `Text` には、`Row` から幅の上限が渡りません。そのため、文字は1行のまま `Row` の右端を越えて描かれます。

`Row` 自身の幅には上限が要ります。この例では `SizedBox` で 400 にしています。ウィンドウの `Size` を指定していれば、ウィンドウの幅が上限になります。上限がないまま `Expanded` を使うと、レイアウトのときに次の例外が投げられます。

```text
System.InvalidOperationException: RenderFlex は主軸の最大幅が有限でない制約では flex 子をレイアウトできません。 Expanded/Flexible/Spacer を外すか、親から有限の主軸制約を与えてください。
```

行数を制限して末尾を「…」にする指定はありません。文字は、行数の制限なく折り返して表示されます(追跡: ※要確認)。

関連: [Layout](../concepts/Layout.md)、[`samples/FloatSoda.Samples.Expanded`](../../samples/FloatSoda.Samples.Expanded/)

## 左右に寄せて間を空ける

`Row` の `MainAxisAlignment` に `MainAxisAlignment.SpaceBetween` を指定します。最初の子が左端に、最後の子が右端に寄り、残りの幅が子の間に分けられます。

```csharp
using FloatSoda.Geometrics;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

Widget header = new SizedBox
{
    Width = 400,
    Child = new Row
    {
        MainAxisAlignment = MainAxisAlignment.SpaceBetween,
        Children = [new Text("Left"), new Text("Right")]
    }
};
```

子の間の1か所だけを空けるときは、空けたい位置に `Spacer` を置きます。`Spacer` は、余った幅をすべて受け取る空白です。

```csharp
Children = [new Text("Left"), new Spacer(), new Text("Right")]
```

どちらの書き方も、`Row` の幅に上限が要ります。上限がないと、`SpaceBetween` では間が空かず、`Spacer` では上の「[Row の中でテキストをはみ出させない](#row-の中でテキストをはみ出させない)」と同じ例外が投げられます。

関連: [Layout](../concepts/Layout.md)、[`samples/FloatSoda.Samples.Flex`](../../samples/FloatSoda.Samples.Flex/)

## ボタンの押せる範囲を広げる

見た目より広い範囲で押せるようにするには、`GestureDetector` と見た目の間に `Padding` を挟み、`Behaviour` に `HitTestBehaviour.Opaque` を指定します。

```csharp
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using SkiaSharp;

public record BigHitButton : StatelessWidget
{
    public required Action OnTap { get; init; }

    public override Widget Build(IBuildContext context) => new GestureDetector
    {
        Behaviour = HitTestBehaviour.Opaque,
        OnTap = OnTap,
        Child = new Padding
        {
            Spacing = EdgeInsets.All(24),
            Child = new Container { Width = 40, Height = 40, Color = SKColors.Tomato }
        }
    };
}
```

この例では、40 × 40 の赤い四角の周りの 24 ピクセルまでが押せる範囲になります。`Behaviour` の既定値は `HitTestBehaviour.DeferToChild` で、絵のある部分だけが押せる範囲になります。`Padding` の余白には絵がないため、`Opaque` を指定しないと余白は押せません。

`Opaque` を指定した範囲は、その後ろに重なっている Widget への入力も止めます。押せる範囲を広げすぎると、後ろの別のボタンが押せなくなります。

関連: [Input](../concepts/Input.md)、[`samples/FloatSoda.Samples.GestureDetector`](../../samples/FloatSoda.Samples.GestureDetector/)

## フェードインさせる

`AnimationController` で 0 から 1 へ進む値を作り、`FadeTransition` の `Opacity` に渡します。`AnimationController` にフレームの通知を渡すため、`State` は `State<T>` の代わりに `TickerProviderState<T>` を継承します。

```csharp
using FloatSoda.Animation;
using FloatSoda.Elements;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Animation;

public record FadeInPanel : StatefulWidget<FadeInPanel>
{
    public override State<FadeInPanel> CreateState() => new FadeInPanelState();
}

public class FadeInPanelState : TickerProviderState<FadeInPanel>
{
    private AnimationController? _controller;

    public override void InitState()
    {
        _controller = new AnimationController
        {
            Vsync = this,
            Duration = TimeSpan.FromMilliseconds(500),
            Curve = Curves.EaseIn
        };
        _controller.Forward();
    }

    public override void Dispose()
    {
        _controller?.Dispose();
        base.Dispose();
    }

    public override Widget Build(IBuildContext context) => new FadeTransition
    {
        Opacity = _controller!,
        Child = new Text("Hello")
    };
}
```

`Forward()` を呼ぶと、値が `Duration` の時間をかけて 0 から 1 へ進みます。`Curve` は値の進み方で、`Curves.EaseIn` はゆっくり始まって速く終わります。`FadeTransition` は値が変わるたびに描き直すため、`SetState` を呼ぶ必要はありません。

`AnimationController` は、`State` の `Dispose` で必ず `Dispose()` を呼んで止めます。止めないと、Widget が画面から消えたあともフレームの通知を受け続けます。フェードアウトさせるときは `Reverse()` を呼び、値を 1 から 0 へ戻します。

関連: [Animation](../concepts/Animation.md)、[`samples/FloatSoda.Samples.OverlayApp/PulseWidget.cs`](../../samples/FloatSoda.Samples.OverlayApp/PulseWidget.cs)(往復するフェード)

## 34cm の幅で表示する

ウィンドウの `Size` で中身の幅をピクセルで決め、`Dpm` で「1メートルあたりのピクセル数」を指定します。VR の中での幅は「ピクセルの幅 ÷ `Dpm`」で決まります。幅 340 ピクセルを 34cm にするには、`Dpm` を 1000 にします。

```csharp
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Widgets;
using SkiaSharp;

app.CreateWindow(new WorldSpaceWindow
{
    Title = "Notice",
    Size = new SKSize(340, 200),
    Dpm = new Dpm(1000),
    Child = root
});
```

`app` と `root` は、[GettingStarted](GettingStarted.md) の `Program.cs` で作ったものです。この例では、幅 340 ピクセル、高さ 200 ピクセルの中身が、幅 34cm、高さ 20cm で表示されます。※要確認(HMD での実寸は確かめていない)

`Dpm` を指定しないときの既定値は 4000 です。既定のままだと、幅 340 ピクセルは 8.5cm になります。`Dpm` は値が大きいほど表示が小さくなります。

`DashboardWindow` では、表示の大きさを SteamVR が決めるため、`Dpm` が効かないことがあります。※要確認(どの条件で効かないか)。メートルで直接大きさを指定する API はありません(追跡: [#151](https://github.com/sumx21t-3310/FloatSoda/issues/151))。

関連: [Window](../concepts/Window.md)

## 16進の色コードで色を指定する

`Color.Parse` に `#RRGGBB` か `#RGB` の形の文字列を渡します。先頭の `#` は省略できます。

```csharp
using FloatSoda.Geometrics;

Color orange = Color.Parse("#FF8800");
Color sameOrange = Color.Parse("F80");
```

FloatSoda の Widget の色のプロパティ(`ColoredBox.Color`、`Container.Color`、`TextStyle.Color` など)は、`FloatSoda.Geometrics` の `Color` 型です。色は、16進の色コードのほかに、次の2つの書き方でも指定できます。

- `new Color(255, 136, 0)` — 赤・緑・青を 0〜255 で書く
- `SKColors.Orange` — SkiaSharp の名前付きの色を、`Color` へ自動で変換させる

`new Color` の4つ目の引数は不透明度で、省略すると 255(不透明)になります。`Color` 型には名前付きの色の定数がないため、名前で色を選ぶときは `SKColors` を使います。

半透明にするときは、`with` で不透明度(`Alpha`)を変えます。`Color.Parse` は不透明度を含む `#RRGGBBAA` の形を読めません。

```csharp
Color halfOrange = Color.Parse("#FF8800") with { Alpha = 128 };
```

`Color.Parse` は、空の文字列を渡すと `ArgumentException` を、3桁でも6桁でもない文字列や16進以外の文字を含む文字列を渡すと `FormatException` を投げます。

関連: [Widget](../concepts/Widget.md)

## バックグラウンドで取った結果を画面に出す

時間のかかる処理を `Task<T>` で始め、`TaskBuilder<T>` に渡します。`TaskBuilder<T>` は、`Task` が終わるまでと終わったあとで、表示を切り替えます。

```csharp
using FloatSoda.Elements;
using FloatSoda.Widgets;

public record MessagePanel : StatefulWidget<MessagePanel>
{
    public override State<MessagePanel> CreateState() => new MessagePanelState();
}

public class MessagePanelState : State<MessagePanel>
{
    private readonly Task<string> _message = File.ReadAllTextAsync("message.txt");

    public override Widget Build(IBuildContext context) => new TaskBuilder<string>
    {
        Task = _message,
        Builder = (context, snapshot) =>
        {
            if (snapshot.HasError) return new Text("Failed to load");
            if (snapshot.HasData) return new Text(snapshot.Data!);
            return new Text("Loading...");
        }
    };
}
```

`Builder` は、`Task` の状態(`snapshot`)を受け取って表示する Widget を返します。読み込みの途中は `Loading...` が、読み込めたらファイルの中身が、失敗したら `Failed to load` が表示されます。`Task` が終わったあとの作り直しは、FloatSoda が画面を作るスレッドで行います。

`Task` は、この例のように `State` のフィールドで作ります。`Build` は何度も呼ばれるため、`Build` の中で作ると、呼ばれるたびに処理が始め直されます。

`TaskBuilder<T>` が扱うのは、1回で終わる処理です。OSC の受信のように何度も届く通知を、別のスレッドから `SetState` で画面に出す書き方は、画面の作り直しと競合しないことが保証されていません。`await` のあとに `SetState` を呼ぶ書き方も同じです。`await` の続きは、画面を作るスレッドに戻りません(追跡: [#281](https://github.com/sumx21t-3310/FloatSoda/issues/281))。

関連: [Threading](../concepts/Threading.md)

## ワールド座標のウィンドウで操作したいとき

操作を受け付けるのは `DashboardWindow` だけです。`WorldSpaceWindow` と `DeviceTrackedWindow` に `GestureDetector` を置くと、コンパイルも実行も通りますが、押したときの処理は呼ばれません(追跡: [#182](https://github.com/sumx21t-3310/FloatSoda/issues/182))。

操作が要る部分は `DashboardWindow` に置き、`WorldSpaceWindow` と `DeviceTrackedWindow` は表示だけに使います。ダッシュボードでの操作の結果を、別のウィンドウの表示に反映する手順は ※要確認 です。

関連: [Input](../concepts/Input.md)、[Window](../concepts/Window.md)
