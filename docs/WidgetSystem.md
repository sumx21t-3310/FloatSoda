← [Home](Home.md)

# ウィジェット/エレメントシステム

> **実装状況** — `✓` 使用可能 / `△` 使えるが一部の機能が未完成 / `✗` `internal` で公開 API から使えない
> - **✓** Widget / Element / State の基盤(`StatelessWidget` / `StatefulWidget` / `InheritedWidget` / `ParentDataWidget<T>`、`BuildOwner` による差分ビルド、`Key` 対応の子リスト差分)
> - **✓** ツリー補助の `Builder` / `KeyedSubtree` / `RepaintBoundary` / `ListenableBuilder`
> - **✓** レイアウト系・描画系・入力系のウィジェット。**下の[一覧](#組み込みウィジェット一覧)で `✓` が付いているものが使えます**
> - **△** `FloatSoda.Hooks`(ビルドループと未統合)
> - **✗** スクロール系の `ListView` / `GridView` / `SingleChildScrollView`
> - **予定** `Button` / `Icon` を担う UI3層構成(`FloatSoda.UI` / `Cream` / `FizzyPop`)は Phase 5 の予定で、まだ提供していません。ボタンは `GestureDetector` で組み立ててください(→ [押せるボタンを作る](#押せるボタンを作る))

## 三ツリーの役割

```
Widget (immutable record)
  │  CreateElement()
  ▼
Element (mutable)          ← 状態・ライフサイクル管理、BuildOwner が差分ビルド
  │  CreateRenderObject() / UpdateRenderObject()
  ▼
RenderObject               ← レイアウト・描画(dirty フラグで差分更新)
```

- **Widget** — UI の設計図。`abstract record` で不変。フレームごとに再生成されても `==` で差分を検知できる。
- **Element** — Widget と RenderObject を橋渡しする永続ノード。ウィジェットが更新されても Element は再利用される。再ビルドの仕組みは [BuildPipeline](BuildPipeline.md) を参照。
- **RenderObject** — `PerformLayout` と `Paint` を実装する描画エンジン。詳細は [RenderObjects](RenderObjects.md) を参照。

## Widget の階層

| 基底クラス | 役割 | 対応する Element |
|---|---|---|
| `Widget` | すべてのウィジェットの基底。`CreateElement()` を宣言 | — |
| `StatelessWidget` | `Build(IBuildContext)` で子ツリーを返す純粋関数コンポーネント | `StatelessElement` ✓ |
| `StatefulWidget<T>` | `CreateState()` で `State<T>` を分離 | `StatefulElement` ✓ |
| `InheritedWidget` | ツリー下方へのコンテキスト伝播 | `InheritedElement` ✓ |
| `ProxyWidget` | RenderObjectを作らず、単一の `Child` へ構成を委譲 | `ProxyElement` ✓ |
| `ParentDataWidget<T>` | 親RenderObjectが子ごとに持つレイアウト情報を設定 | `ParentDataElement<T>` ✓ |
| `RenderObjectWidget<T>` | `CreateRenderObject()` / `UpdateRenderObject(T)` で RenderObject を所有 | `RenderObjectElement<T>` ✓ |
| `SingleChildRenderObjectWidget<T>` | 単一の `Child` を持つ RenderObjectWidget | `SingleChildRenderObjectElement<T>` ✓ |
| `MultiChildRenderObjectWidget<T>` | `Children`(`List<Widget>`)を持つ RenderObjectWidget | `MultiChildRenderObjectElement<T>` ✓(`Key` 対応の子リスト差分) |
| `RenderObjectToWidgetAdapter` | Widget ツリーのルートを `RenderView` に接続 | `RenderObjectToWidgetElement<RenderView>` ✓ |

## ParentDataWidget

`ParentDataWidget<T>` は、自身ではRenderObjectを作らず、子RenderObjectの `ParentData` を更新します。
親RenderObjectは `SetupParentData` で `T` を用意します。派生Widgetは `ApplyParentData(T)` で値を比較・更新し、変更した場合だけ `true` を返します。
変更時の `MarkNeedsLayout()` は、基底クラスが親RenderObjectへ伝播させます。

`Flexible` や `Positioned` のように「親レイアウトだけが解釈する子ごとの情報」を宣言的なWidget APIとして表現するための基盤です。
対応するParentDataを用意しない親の下で使用すると `InvalidOperationException` になります。

## StatelessWidget

状態を持たない純粋関数コンポーネントです。`Build(IBuildContext)` でウィジェットツリーを返します。

```csharp
using FloatSoda.Elements;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

public record MyWidget : StatelessWidget
{
    public required string Title { get; init; }

    public override Widget Build(IBuildContext context)
    {
        return new Center
        {
            Child = new Text(Title)
        };
    }
}
```

`Build()` はマウント時と、`MarkNeedsBuild()` でスケジュールされた再ビルド時に `BuildOwner` から呼ばれます。

## StatefulWidget / State

`StatefulWidget<T>` は Widget から `State<T>` を分離するパターンです。`State.SetState(Action)` は状態を書き換えたうえで `Element.MarkNeedsBuild()` を呼びます。これにより、次フレームの `BuildScope()` で再ビルドされます。

```csharp
public record WatchWidget : StatefulWidget<WatchWidget>
{
    public override State<WatchWidget> CreateState() => new WatchState();
}

public class WatchState : State<WatchWidget>
{
    private Timer? _timer;
    private string _time = "00:00:00";

    public override void InitState()
    {
        _timer = new Timer(_ => SetState(() => _time = DateTime.Now.ToString("HH:mm:ss")),
            null, dueTime: 0, period: 1000);
    }

    public override Widget Build(IBuildContext context) => new Text(_time);
}
```

(このサンプルの全体は `samples/FloatSoda.Samples.OverlayApp/WatchWidget.cs` にあります)

`State<T>` のライフサイクルメソッド: `InitState()` / `SetState(Action)` / `DidUpdateWidget(T oldWidget)` / `DidChangeDependencies()`。

## InheritedWidget

ツリーの下方にコンテキスト(テーマなど)を伝播させるためのウィジェットです。`InheritedElement` が依存する子孫を追跡し、`UpdateShouldNotify(InheritedWidget oldWidget)` が `true` を返したときに依存側を再ビルド対象にします。

現在位置から最も近いスコープを読み、その更新通知を購読するには、`IBuildContext.DependOnInheritedWidgetOfExactType<T>()` を使います。テーマ側に `Of(IBuildContext)` を用意すると、利用側が照会方法を毎回書かずに済みます。

### 組み込みの `InheritedWidget`

自分で `InheritedWidget` を定義しなくても、フレームワークが2つ提供しています。
どちらも `Of(IBuildContext)` で最も近い祖先を取得し、同時に依存として登録します。

#### ServiceProvider — DI コンテナへ到達する

`ServiceProvider` は `IServiceProvider` をウィジェットツリーへ公開します。
`Widget` は `record` でコンストラクタ注入ができないため、**これがビルド中にサービスを解決する方法です。**

```csharp
using FloatSoda.Elements;
using FloatSoda.Widgets;
using Microsoft.Extensions.DependencyInjection;

public record StatusLabel : StatelessWidget
{
    public override Widget Build(IBuildContext context)
    {
        var services = ServiceProvider.Of(context);
        // IOscClient は FloatSoda が提供する型ではなく、利用側が Host へ登録した自前のサービス。
        var osc = services.GetRequiredService<IOscClient>();

        return new Text(osc.IsConnected ? "接続中" : "切断");
    }
}
```

祖先に `ServiceProvider` が無い場合、`Of` は `InvalidOperationException` を投げます。
次のように、ツリーの上位へ配置してください。

```csharp
Widget root = new ServiceProvider
{
    Services = host.Services,
    Child = new StatusLabel()
};
```

#### WindowWidget — 自分が載っているウィンドウを知る

`WindowWidget`(と派生の `DashboardWindow` / `WorldSpaceWindow` / `DeviceTrackedWindow`)も `InheritedWidget` です。
`app.CreateWindow(...)` に渡した時点でウィジェットツリーのルートになるため、どのウィジェットからでも `WindowWidget.Of(context)` で `Title` や `Size` を読めます。

```csharp
var window = WindowWidget.Of(context);
Widget caption = new Text(window.Title);
```

`WindowWidget` は `ScopeType` を基底型に固定しているため、派生型で `CreateWindow` していても `WindowWidget.Of` で取得できます。
オーバーレイ種別で表示を変えたい場合は型で分岐してください。

```csharp
Widget hint = WindowWidget.Of(context) is DashboardWindow
    ? new Text("レーザーポインターで操作できます")
    : new Text("このオーバーレイは表示専用です");
```

**種別の判定には、上のようにパターンマッチを使ってください。**
`DashboardWindow.Of(context)` のような書き方は種別の検証になりません。
`Of` を独自に持つのは `WindowWidget` / `OverlayWindow` / `DesktopWindow` の3つだけで、`DashboardWindow` / `WorldSpaceWindow` / `DeviceTrackedWindow` は自前の `Of` を持たないためです。
`DashboardWindow.Of(context)` と書いても、実際に呼ばれるのは継承した `OverlayWindow.Of` で、戻り値の型も `OverlayWindow` になります。ルートが `WorldSpaceWindow` でも例外にはなりません。

`OverlayWindow.Of(context)` は、ルートがオーバーレイ以外(`DesktopWindow`)のときだけ `InvalidOperationException` を投げます。

### Builder

`Builder` は新しい `IBuildContext` を1段挟み、`ChildBuilder` で子を構築します。同じ `Build()` 内で作成した `InheritedWidget` を、その子側のコンテキストから解決したい場合に使います。

```csharp
using FloatSoda.Elements;
using FloatSoda.Widgets;

public sealed record AlbumTheme : InheritedWidget
{
    public required string Title { get; init; }

    public static AlbumTheme? Of(IBuildContext context) =>
        context.DependOnInheritedWidgetOfExactType<AlbumTheme>();

    public override bool UpdateShouldNotify(InheritedWidget oldWidget) =>
        oldWidget is AlbumTheme oldTheme && oldTheme.Title != Title;
}

Widget album = new AlbumTheme
{
    Title = "VRChat photos",
    Child = new Builder
    {
        ChildBuilder = context =>
            new Text(AlbumTheme.Of(context)?.Title ?? "No title")
    }
};
```

Issue 記載時の `BuildContext` ではなく、FloatSoda の公開コンテキスト契約である `IBuildContext` を受け取ります。

### KeyedSubtree

`KeyedSubtree` は子の内容を変えず、ラッパーに指定した `Key` でサブツリーの同一性を制御します。同じ位置・同じキーなら子の Element / State を保持し、キーを変えるとサブツリーを差し替えます。

```csharp
new KeyedSubtree
{
    Key = new ValueKey<string>(albumId),
    Child = BuildAlbum(albumId)
};
```

### RepaintBoundary

`RepaintBoundary` は子を独立した合成レイヤーへ記録します。境界内の `MarkNeedsPaint()` は `RenderRepaintBoundary` で止まり、変更されていない祖先を再描画しません。

```csharp
using FloatSoda.Widgets.Paint;

new RepaintBoundary
{
    Child = BuildFrequentlyChangingWidget()
};
```

### ListenableBuilder

`ListenableBuilder` はBCLの `INotifyPropertyChanged` を購読し、通知が届いたときに `ChildBuilder` 配下だけを再構築します。ViewModel全体をStatefulWidgetへ写し替えず、変更される表示領域を局所化したい場合に使います。プロパティ名によるフィルタは行わないため、`PropertyChanged` のどの通知でも再構築します。

```csharp
using System.ComponentModel;
using FloatSoda.Widgets;

sealed class CounterState : INotifyPropertyChanged
{
    private int _count;

    public int Count
    {
        get => _count;
        set
        {
            if (_count == value) return;
            _count = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

var counter = new CounterState();

Widget counterLabel = new ListenableBuilder
{
    Listenable = counter,
    ChildBuilder = _ => new Text($"Count: {counter.Count}")
};
```

`Listenable` を同じ位置の新しい `ListenableBuilder` で差し替えると、古いオブジェクトの購読を解除して新しいオブジェクトへ付け替えます。ツリーから外れたときも購読を解除します。

> **スレッド契約:** `PropertyChanged` は `ListenableBuilder` がマウントされたスレッド（通常はFloatSodaのメインループ）から発火してください。OSC受信やネットワーク処理などのバックグラウンドスレッドから直接通知すると `InvalidOperationException` を投げます。現時点では任意スレッドの通知をメインループへ自動マーシャリングする公開APIはありません。状態の変更と通知を呼び出し側でメインループへ移してから発火してください。

## Hooks(FloatSoda.Hooks)

> **△ 部分実装:** `FloatSoda.Hooks` プロジェクトに R3 ベースの `HookWidget` / `HookElement` がありますが、フレームワークのビルドループとは未統合です。`HookExtension` の `UseState` / `UseEffect` / `Depends` / `UseMemo` / `UseAction` は `NotImplementedException` を投げます。Phase 4 で統合します。

`HookWidget.Build()` 内で `UseState(initialValue)` を呼ぶと `ReactiveProperty<T>` が返ります。値の変更が再ビルドをトリガーする、React フック風の API を目指しています。

```csharp
// 構想中の API(未動作。Button は FloatSoda.UI.Cream などのデザインシステム層のもの)
public override Widget Build(IBuildContext context)
{
    var count = UseState(0);

    return new Button
    {
        Child = new Text($"Count: {count.Value}"),
        OnPressed = () => count.Value++,
    };
}
```

## 組み込みウィジェット一覧

### Layout

| ウィジェット | 実装状況 | 説明 | 主なプロパティ |
|---|---|---|---|
| `Center` | ✓ | 子を中央に配置(`Align` に委譲) | `Child` |
| `Align` | ✓ | 子を指定の `Alignment` で配置 | `Alignment`, `WidthFactor`, `HeightFactor`, `Child` |
| `Column` | ✓ | 垂直方向に並べる(`Flex` に委譲) | `Children`, `MainAxisAlignment`, `CrossAxisAlignment`, `MainAxisSize` |
| `Row` | ✓ | 水平方向に並べる(`Flex` に委譲) | `Children`, `MainAxisAlignment`, `CrossAxisAlignment`, `MainAxisSize` |
| `Flex` | ✓ | 方向指定のフレックスレイアウト。`UpdateRenderObject` と `Key` 対応の子リスト差分に対応 | `Direction`, `Children`, `MainAxisAlignment`, `CrossAxisAlignment`, `VerticalDirection` |
| `Wrap` | ✓ | 主軸の利用可能領域で子を `run` へ折り返して配置 | `Direction`, `Children`, `Spacing`, `RunSpacing`, `Alignment`, `RunAlignment`, `CrossAxisAlignment`, `VerticalDirection` |
| `Flexible` | ✓ | `Flex`系の余剰主軸領域を比率で受け取り、子が割当量以下の大きさを選択可能 | `Flex`, `Fit`, `Child` (必須) |
| `Expanded` | ✓ | `Flex`系の余剰主軸領域を比率で受け取り、子を割当量いっぱいに拡張 | `Flex`, `Child` (必須) |
| `Spacer` | ✓ | `Flex`系へ比率指定できる空白を挿入 | `Flex` |
| `SizedBox` | ✓ | 固定サイズのボックス | `Width`, `Height`, `Child` |
| `ConstrainedBox` | ✓ | 親の制約と交差する追加制約を子へ適用 | `AdditionalConstraints` (`BoxConstraints`, 必須), `Child` |
| `AspectRatio` | ✓ | 親制約内で幅対高さの比率を維持して子へ固定寸法を適用 | `Ratio` (正の有限値、必須), `Child` |
| `FittedBox` | ✓ | 子を自然サイズでレイアウトし、`BoxFit`と`Alignment`に従って拡大縮小・配置 | `Fit`, `Alignment`, `ClipBehavior`, `Child` |
| `LimitedBox` | ✓ | 親の上限が無限の軸だけ、子へ最大寸法を適用 | `MaxWidth`, `MaxHeight`, `Child` |
| `ConstraintsTransformBox` | ✓ | 親制約を任意の `BoxConstraintsTransform` で変換し、子を配置 | `ConstraintsTransform` (必須), `Alignment`, `ClipBehavior`, `Child` |
| `UnconstrainedBox` | ✓ | 両軸または指定軸以外の制約を外して子を自然サイズで配置 | `ConstrainedAxis`, `Alignment`, `ClipBehavior`, `Child` |
| `IntrinsicWidth` | ✓ | 子の最大intrinsic幅へ収縮し、任意のstep単位で切り上げ | `StepWidth`, `Child` |
| `IntrinsicHeight` | ✓ | 子の最大intrinsic高さへ収縮し、任意のstep単位で切り上げ | `StepHeight`, `Child` |
| `Padding` | ✓ | 子の制約を余白分だけ縮小し、子を余白の左上位置へ配置 | `Spacing` (`EdgeInsets`, 必須), `Child` |
| `Stack` | ✓ | 複数の子を重ね、非Positioned子を`Alignment`と`Fit`で配置 | `Children`, `Alignment`, `Fit` |
| `Positioned` | ✓ | `Stack`の子を辺からの距離または固定寸法で絶対配置 | `Left`, `Top`, `Right`, `Bottom`, `Width`, `Height`, `Child` |
| `IndexedStack` | ✓ | 全子をレイアウトし、`Index`で選んだ1子だけを描画・ヒットテスト。`null`なら全子を非表示 | `Children`, `Index`, `Alignment`, `Fit` |
| `Offstage` | ✓ | 子をレイアウトしたまま描画・ヒットテストから除外 | `IsOffstage`, `Child` |
| `Visibility` | ✓ | `Visible`に応じて必須の`Child`と`Replacement`を切り替え。非表示子の状態保持は行わない | `Visible`, `Child` (必須), `Replacement` |
| `RotatedBox` | ✓ | 90度単位でレイアウト寸法ごと時計回りに回転。負値・4以上は4を法として正規化 | `QuarterTurns`, `Child` |
| `FractionallySizedBox` | ✓ | 親の最大寸法に対する割合を子へtight制約として適用し、子を配置 | `WidthFactor`, `HeightFactor`, `Alignment`, `Child` |
| `OverflowBox` | ✓ | 親とは異なる制約を子へ渡し、自身の領域外への描画を許可 | `MinWidth`, `MaxWidth`, `MinHeight`, `MaxHeight`, `Fit`, `Alignment`, `Child` |
| `SizedOverflowBox` | ✓ | 自身は指定サイズを採り、子へ親の元の制約を渡して配置 | `Size` (`Size`, 必須), `Alignment`, `Child` |
| `Container` | ✓ | 配置・余白・装飾・寸法・変換を1つのウィジェットで合成 | `Alignment`, `Padding`, `Color`, `Decoration`, `Width`, `Height`, `Transform`, `TransformAlignment`, `Child` |
| `ListView` | ✗ 未実装(`internal`) | スクロール可能なリスト | — |
| `GridView` | ✗ 未実装(`internal`) | グリッドレイアウト | — |
| `SingleChildScrollView` | ✗ 未実装(`internal`) | 単一子をスクロール | — |

`Container` は、`Align`、`Padding`、`DecoratedBox`、`SizedBox`、`Transform` を1つにまとめた合成ウィジェットです。
指定したプロパティに対応するウィジェットだけを使い、内側から配置・余白・装飾・寸法・変換の順に重ねます。
`Padding` は装飾の内側に入るため、子の領域は余白の分だけ装飾より小さくなります。
`Color` と `Decoration` を同時に指定すると `InvalidOperationException` になります。背景色と角丸を両方使う場合は `BoxDecoration.Color` へまとめてください。

```csharp
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

Widget card = new Container
{
    Width = 320,
    Padding = EdgeInsets.All(16),
    Decoration = new BoxDecoration
    {
        Color = new Color(32, 32, 40),
        BorderRadius = BorderRadius.Circular(12)
    },
    Child = new Text("VRChat: Online")
};
```

`ConstrainedBox` は、親から渡された制約の範囲内で、追加の最小・最大サイズを子へ適用します。

`AspectRatio.Ratio` は幅を高さで割った値です。両軸が可変の場合は幅の上限を優先し、収まらないときは高さの上限から幅を再計算します。幅と高さの両方に上限がない場所ではサイズを決められません。親の `SizedBox` や `ConstrainedBox` で少なくとも一方の上限を与えてください。

`FittedBox` は、子を制約なしの自然サイズでレイアウトし、描画時に変換します。`BoxFit` には `Fill`、`Contain`、`Cover`、`FitWidth`、`FitHeight`、`None`、`ScaleDown` があります。`Cover` などで領域からはみ出す部分を切り抜く場合は、`ClipBehavior` を指定します。

`LimitedBox` は、親から受け取った最大幅または最大高さが正の無限大の場合にのみ、対応する上限を適用します。有限の親制約がある場合、`MaxWidth` と `MaxHeight` は適用されません。

```csharp
using FloatSoda.Geometrics;
using FloatSoda.Rendering.Layers;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

Widget thumbnail = new AspectRatio
{
    Ratio = 16.0 / 9.0,
    Child = new FittedBox
    {
        Fit = BoxFit.Cover,
        ClipBehavior = Clip.HardEdge,
        Child = new SizedBox { Width = 1920, Height = 1080 }
    }
};
```

`IndexedStack.Index` は0始まりです。`null` を指定すると、すべての子をレイアウトしたまま非表示にします。負の値や `Children` の範囲外を指定すると `ArgumentOutOfRangeException` を投げます。
`RotatedBox` は、回転後の幅と高さをレイアウトに反映します。レイアウト寸法を変えずに描画だけを任意の角度で変形する `Transform` とは用途が異なります。

#### 表示・非表示の3つのウィジェットの使い分け

`Visibility`、`Offstage`、`IndexedStack` はどれも表示を切り替えますが、**非表示にした子の状態(`State`)を保つかどうか**と、**非表示の間もレイアウトを計算するか**が異なります。

| ウィジェット | 非表示の子の `State` | 非表示の子のレイアウト | 向いている用途 |
|---|---|---|---|
| `Visibility` | 通常は失われる(下記) | 計算しない | 状態を持たない表示切り替え |
| `Offstage` | 保たれる | 再レイアウト時に計算する | 戻したときに元の状態でいてほしい単一の子 |
| `IndexedStack` | 保たれる | 再レイアウト時に全子ぶん計算する | タブのように複数の候補から1つを選ぶ |

`Visibility` は `Visible = false` のとき、`Child` の代わりに `Replacement`(省略時は空の `SizedBox`)をツリーへ置きます。`Child` と `Replacement` の実行時型が異なる場合、`Widget.CanUpdate` が `false` となり、`Child` の Element と `State` は破棄されます。既定の `Replacement` を使う通常のケースがこれに該当します。

ただし、**状態が必ず破棄されるわけではありません**。`Child` と `Replacement` が同じ実行時型で `Key` も等しい場合(どちらも `Key` を指定していない場合を含む)、`Element.UpdateChild` が既存の Element を再利用するため状態が残ります。非表示を状態のリセット手段として使う場合は、型か `Key` を変えて破棄を確実にしてください。

`Offstage` と `IndexedStack` は、非表示の子もツリーに残すため状態を保ちます。しかし、**再レイアウト時には非表示の子の分も計算します**。候補が多い場合や子のレイアウトが重い場合は、計算コストが増加します。

このコストは毎フレーム発生するわけではありません。ウィジェットと RenderObject の両方に変更がないフレームでは、レイアウト自体がスキップされます(→ [BuildPipeline](BuildPipeline.md))。`IndexedStack` の `Index` を変えたときも `MarkNeedsPaint()` だけが実行されるため、タブの切り替えで再レイアウトは発生しません。

```csharp
// 状態を保ちたい: 開閉してもスクロール位置を維持する
new Offstage { IsOffstage = !isExpanded, Child = BuildDetails() }

// 状態を保たなくてよい: 通知の有無で出し分けるだけ
new Visibility { Visible = hasNotification, Child = new Text("新着あり") }

// 複数候補から1つ: タブごとの入力内容を保つ
new IndexedStack { Index = selectedTab, Children = [BuildHome(), BuildSettings()] }
```

`Expanded`、`Flexible`、`Spacer` は、`Row`、`Column`、`Flex` の直接の子として使用します。`Flex` には1以上の整数を指定します。たとえば `Flex = 2` は、`Flex = 1` の子の2倍の余剰領域を受け取ります。`Expanded` は `FlexFit.Tight` 固定、`Flexible` は既定で `FlexFit.Loose` です。
主軸の最大制約が無限の場合、余剰領域を決定できません。そのため、flex子を含む `Flex` は `InvalidOperationException` を投げます。親の `SizedBox` や `ConstrainedBox` で、有限の幅(`Row`)または高さ(`Column`)を与えてください。

```csharp
using FloatSoda.Geometrics;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

Widget toolbar = new SizedBox
{
    Width = 600,
    Height = 64,
    Child = new Row
    {
        CrossAxisAlignment = CrossAxisAlignment.Stretch,
        Children =
        [
            new Expanded
            {
                Flex = 2,
                Child = new Text("VRChat status")
            },
            new Spacer { Flex = 1 },
            new Flexible
            {
                Flex = 1,
                Fit = FlexFit.Loose,
                Child = new Text("Settings")
            }
        ]
    }
};
```

```csharp
Widget panel = new ConstrainedBox
{
    AdditionalConstraints = new BoxConstraints(
        MinWidth: 240,
        MaxWidth: 400,
        MinHeight: 120,
        MaxHeight: 240),
    Child = new SizedBox { Width = 320, Height = 180 }
};
```

`ConstraintsTransformBox` は、親の制約を `BoxConstraintsTransform` delegate で変換してから子へ渡します。子の自然サイズが親の制約を超えた場合、自身は親の制約内のサイズを採用し、`Alignment` に従って子を配置します。`ClipBehavior = Clip.None` でははみ出した部分も描画し、`Clip.HardEdge` や `Clip.Antialias` では自身の矩形で切り抜きます。`Clip` は `FloatSoda.Rendering.Layers` 名前空間の型であるため、使用するファイルにこの名前空間を import してください。

```csharp
using FloatSoda.Rendering.Layers;

Widget wideLogRow = new ConstraintsTransformBox
{
    // 最小幅は維持し、最大幅だけを外す。
    ConstraintsTransform = ConstraintsTransformBox.MaxWidthUnconstrained,
    Alignment = Alignment.CenterRight,
    ClipBehavior = Clip.HardEdge,
    Child = new Text("VRChatの長いログメッセージ")
};
```

定型的な変換には次のstaticメソッドをそのままdelegateとして指定できます。

- `Unmodified`: 制約を変更しない
- `Unconstrained`: 両軸の最小・最大制約を外す
- `WidthUnconstrained` / `HeightUnconstrained`: 指定軸の最小・最大制約を外す
- `MaxWidthUnconstrained` / `MaxHeightUnconstrained`: 指定軸の最大制約だけを外す
- `MaxUnconstrained`: 両軸の最大制約だけを外す

独自変換の戻り値では、最小値が0以上の有限値であり、かつ最大値が対応する最小値以上または正の無限大である必要があります。NaN、負の値、負の無限大、または最小値が最大値を超える制約を返した場合、子のレイアウト前に `ArgumentException` を投げます。

`UnconstrainedBox` は、`ConstraintsTransformBox` を組み合わせた簡易ウィジェットです。
`ConstrainedAxis = null`(既定値)では両軸の制約を外します。`Axis.Horizontal` では横軸の制約のみ、`Axis.Vertical` では縦軸の制約のみを維持します。

```csharp
Widget naturalWidthRow = new UnconstrainedBox
{
    ConstrainedAxis = Axis.Vertical,
    Alignment = Alignment.CenterLeft,
    Child = new Row
    {
        Children = [new Text("自然な幅で並べるログ行")]
    }
};
```

`FractionallySizedBox` は、`WidthFactor` や `HeightFactor` を指定した軸において、親の最大寸法に factor を乗算し、その寸法を子へ tight 制約として渡します。`null` を指定した軸は親の制約を変更しません。factor を使用する軸には、有限の最大制約が必要です。

`OverflowBox` は、`MinWidth`、`MaxWidth`、`MinHeight`、`MaxHeight` のうち、指定した境界のみを親の制約から上書きします。`Fit = OverflowBoxFit.Max` は有限の親領域を最大まで使用し、`OverflowBoxFit.DeferToChild` は親の制約内で子のサイズに従います。領域からはみ出した部分は切り抜かず、そのまま描画します。

`SizedOverflowBox` は、自身のサイズを `FloatSoda.Geometrics.Size` で指定し、子へは親から受け取った元の制約をそのまま渡します。自身と子を異なるサイズでレイアウトしたい場合に使用します。

```csharp
Widget overflowPreview = new SizedOverflowBox
{
    Size = new Size(240, 120),
    Alignment = Alignment.Center,
    Child = new OverflowBox
    {
        MinWidth = 320,
        MaxWidth = 320,
        Fit = OverflowBoxFit.Max,
        Child = new FractionallySizedBox
        {
            HeightFactor = 0.5,
            Child = new Text("VRChat preview")
        }
    }
};
```

`IntrinsicWidth` と `IntrinsicHeight` は、通常のレイアウト前に子へ自然な寸法を問い合わせます。`StepWidth` や `StepHeight` を指定すると、計測値をその正の有限値の倍数へ切り上げます。たとえば、内容量が異なるカードを一定の step 幅へ揃える場合に使用します。

```csharp
Widget statusCard = new IntrinsicWidth
{
    StepWidth = 40,
    Child = new Text("VRChat: Online")
};
```

intrinsic 測定は追加のツリー走査を必要とするため、入れ子になると最悪で O(N²) の計算量になります。スクロール領域や大量の項目を持つツリーでは使用せず、寸法が分かっている場合は `SizedBox` や `ConstrainedBox` を優先してください。

### Painting

| ウィジェット | 実装状況 | 説明 | 主なプロパティ |
|---|---|---|---|
| `ColoredBox` | ✓ | 単色背景 | `Color` (`Color`), `Child` |
| `DecoratedBox` | ✓ | `BoxDecoration` の背景色・角丸・ボーダーを子の前面または背面へ描画 | `Decoration`, `Position`, `Child` |
| `ClipRect` | ✓ | 矩形クリップ | `Clipper`, `ClipBehavior`, `Child` |
| `ClipRoundRect` | ✓ | 角丸矩形クリップ | `BorderRadius`, `Clipper`, `ClipBehavior`, `Child` |
| `ClipOval` | ✓ | 楕円クリップ | `CustomClipper`, `ClipBehavior`, `Child` |
| `ClipCustomPath` | ✓ | カスタムパスクリップ | `Clipper`, `ClipBehavior`, `Child` |
| `Opacity` | ✓ | 0から1までの固定不透明度を合成レイヤーで適用 | `Value`, `Child` |
| `Transform` | ✓ | レイアウト後に `Matrix3x2` の2次元変換を適用 | `Matrix`, `Origin`, `Alignment`, `TransformHitTests`, `Child` |
| `RepaintBoundary` | ✓ | 子の再描画を独立した合成レイヤー内に限定 | `Child` |

### Animation

| ウィジェット | 実装状況 | 説明 | 主なプロパティ |
|---|---|---|---|
| `FadeTransition` | ✓ | `IAnimation<double>` で子の不透明度を駆動(リビルド不要、ペイントのみ)→ [Animation](Animation.md) | `Opacity` (`IAnimation<double>`), `Child` |

### Text / Paint

| ウィジェット | 実装状況 | 説明 | 主なプロパティ |
|---|---|---|---|
| `RichText` | ✓ | `TextSpan` でスタイル付きテキストを表示 | `Text` (`TextSpan`) |
| `Text` | ✓ | 単一書式のテキスト表示(`RichText` / `TextSpan` に委譲) | `Data` (string), `Style` (`TextStyle?`) |
| `DefaultTextStyle` | ✓ | 配下の `Text` へ既定のテキスト書式を伝播する `InheritedWidget` | `Style` (`TextStyle`), `Child` |
| `Paint.Image` | ✓ | `ImageProvider`から読み込んだ画像を`Fit`に従って表示。読み込み中と失敗時は`Child`のみを描画し、失敗は`OnError`で通知 | `Provider`, `Fit`, `Alignment`, `Child`, `OnError` |
| `Paint.Icon` | ✓ | `IconData`と`FontProvider`で指定したアイコンフォントのグリフを表示 | `Data`, `Size`, `Color` |

`Paint.Image` の `Fit` には、`FittedBox` と同じ `BoxFit` を指定します。既定値は `Contain`(縦横比を維持して領域内へ収める)です。領域内の配置位置は `Alignment` で指定します(既定値は `Alignment.Center`)。`Cover` のように画像の一部だけを使う場合は、**描画元の矩形を切り取って**描画します。そのため、**どの `Fit` を指定しても領域外へはみ出しません**。画像のどの部分を残すかは `Alignment` によって決まります。

`FittedBox` が `ClipBehavior` を持ち、`Paint.Image` が持たない理由はここにあります。`FittedBox` は描画元を切り取れない子ウィジェットを拡大縮小するため、切り抜きの指定が必要です。一方、`Paint.Image` は描画元の矩形自体を狭めることができます。

`Text` は表示する文字列を単一値コンストラクタで受け取り、書式は `init` プロパティで指定します。空文字列も有効です。

書式は Flutter と同じ優先順位で解決されます。優先順位は **明示指定 > 祖先の `DefaultTextStyle` > フレームワークの既定値**(フォントサイズ30、Arial、黒、ウェイト400)です。`TextStyle` の各プロパティは `null` を未指定として扱い、未指定のプロパティのみが継承によって埋まります。

```csharp
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Core.Providers;
using FloatSoda.Widgets;

new Text("Hello, VR!")
{
    Style = new TextStyle
    {
        FontSize = 36,
        Color = new Color(255, 255, 255),
        Font = new SystemFontProvider("Arial"),
        FontWeight = 700,
        IsItalic = false
    }
}
```

複数の `Text` に同じ書式を適用する場合は、`DefaultTextStyle` を使って祖先から伝播させます。配下の `Text` は明示したプロパティのみを上書きし、残りを継承します。`DefaultTextStyle` の `Style` を変更すると、依存する `Text` は自動的に再ビルドされます。

```csharp
new DefaultTextStyle
{
    Style = new TextStyle { FontSize = 36, Color = new Color(255, 255, 255) },
    Child = new Column
    {
        Children =
        [
            new Text("タイトル"),                                                  // 36px・白
            new Text("強調") { Style = new TextStyle { Color = new Color(255, 200, 0) } } // 36px・黄
        ]
    }
}
```

書式を継承させたくない `Text` には、`Inherit = false` を設定した `TextStyle` を指定します。この場合、未指定のプロパティにはフレームワークの既定値が適用されます。

システムにないフォントは `FileFontProvider` で指定します。同じ値の Provider は内部で共有されるため、複数の `Text` や `Icon` から使用しても、フォントリソースは1度だけ読み込まれます。

```csharp
using FloatSoda.Core;
using FloatSoda.Core.Providers;
using FloatSoda.Geometrics;
using FloatSoda.Widgets.Paint;

var materialIcons = new FileFontProvider("Assets/MaterialIcons-Regular.otf");

new Icon(new IconData(0xe88a, materialIcons))
{
    Size = 24,
    Color = new Color(255, 255, 255)
}
```

`Button` や `IconButton` は、コアではなくデザインシステム層(`FloatSoda.UI.Cream`、`FloatSoda.UI.FizzyPop`)が担う設計です。振る舞いを担うヘッドレスウィジェット(`ButtonBase` など)は `FloatSoda.UI` に置かれます。**これらの層はまだ提供されていません**(→ [UILayering](UILayering.md#実装状況))。現在のところ、必要なボタンは [押せるボタンを作る](#押せるボタンを作る) の方法で組み立ててください。

### Gesture

| ウィジェット | 実装状況 | 説明 | 主なプロパティ |
|---|---|---|---|
| `GestureDetector` | ✓ | タップとパン(ドラッグ)を検知する | `OnTap`, `OnPanStart`, `OnPanUpdate`, `OnPanEnd`, `Behaviour`, `Child` (必須) |
| `RawGestureDetector` | ✓ | 独自の `GestureRecognizer` を登録して認識器の組み合わせを自分で決める | `Gestures`, `Behaviour`, `Child` (必須) |
| `Listener` | ✓ | 意味付けされていない生のポインターイベントを受け取る | `OnPointerDown`, `OnPointerUp`, `OnPointerMove`, `OnPointerEnter`, `OnPointerExit`, `OnPointerCancel`, `Behaviour`, `Child` |
| `PointerRegion` | ✓ | 押下に依存しないホバー(領域への出入り)だけを受け取る | `OnPointerEnter`, `OnPointerExit`, `Behaviour`, `Child` |
| `AbsorbPointer` | ✓ | 自身をヒットさせたうえで、子へのヒットテストを止める | `Absorbing` (既定 `true`), `Child` |
| `IgnorePointer` | ✓ | 自身と子をヒットテストの対象から外し、背後の兄弟へ通す | `Ignoring` (既定 `false`), `Child` |

## ジェスチャとヒットテスト

ヒットテストは「ポインタ座標からそこにある RenderObject を特定する」仕組みであり、ジェスチャ認識は「特定した対象へのポインターイベント列をタップやパンとして解釈する」仕組みです。
FloatSoda では**どちらも実装済み**です。

```csharp
using FloatSoda.Geometrics;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;

Widget tappable = new GestureDetector
{
    OnTap = () => Console.WriteLine("押された"),
    Child = new Container
    {
        Width = 200,
        Height = 60,
        Color = new Color(80, 120, 200),
        Alignment = Alignment.Center,
        Child = new Text("Tap me")
    }
};
```

### ポインタ入力が届く範囲

**現時点でポインタ座標が届くのは、ダッシュボードオーバーレイ(`DashboardWindow`)だけです。**
SteamVR がダッシュボード上のレーザーポインターをマウスイベントとして送信するため、
FloatSoda はこれを `IRawPointerSource` として受け取っています。
`WorldSpaceWindow` と `DeviceTrackedWindow` はコントローラーレイからの入力が未接続です。
ヒットテスト自体は機能しますが、判定用の座標が供給されません。この接続は Phase 1 の残件です。

したがって、`WorldSpaceWindow` で `GestureDetector` を使用しても、
コンパイルや実行は成功しますがコールバックは呼ばれません。

### ヒットテストの振る舞い

`Behaviour`(`HitTestBehaviour`)は、ウィジェット自身をヒットテストの対象に含めるかを制御します。

| 値 | 意味 |
|---|---|
| `DeferToChild` | 子がヒットしたときだけ自身もヒットする |
| `Opaque` | 自身の領域全体をヒットとして扱い、背後の兄弟への探索を止める |
| `Translucent` | 自身をヒットパスへ加えたうえで、背後の兄弟への探索も続ける |

`SizedBox` などの描画内容を持たない領域をタップ可能にするには、`Behaviour = HitTestBehaviour.Opaque` を指定します。
`DeferToChild` の場合、描画内容を持たない子はヒット対象になりません。

**`Behaviour` の既定値はウィジェットごとに異なります。**

| ウィジェット | `Behaviour` の既定値 | 理由 |
|---|---|---|
| `GestureDetector` / `RawGestureDetector` / `Listener` | `DeferToChild` | 押下対象は子の描画領域と一致させるのが一般的であり、余白まで判定すると背後のウィジェットを意図せず塞ぐため |
| `PointerRegion` | `Opaque` | ホバー領域は子の隙間を含む矩形全体として扱うため。隙間で判定が切れると状態が細かく点滅するため |

`PointerRegion` で子の描画領域のみをホバー対象にする場合は、`Behaviour = HitTestBehaviour.DeferToChild` を明示します。

### 押せるボタンを作る

`GestureDetector` と `StatefulWidget` を組み合わせることで、タップで表示が変わるボタンを作成できます。
`OnTap` 内で `SetState` を呼ぶと、状態が更新され再ビルドがスケジュールされます。

```csharp
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;

public record CounterPanel : StatefulWidget<CounterPanel>
{
    public override State<CounterPanel> CreateState() => new CounterPanelState();
}

public class CounterPanelState : State<CounterPanel>
{
    private int _count;

    public override Widget Build(IBuildContext context) => new GestureDetector
    {
        OnTap = () => SetState(() => _count++),
        Child = new Container
        {
            Width = 200,
            Height = 60,
            Color = new Color(80, 120, 200),
            Alignment = Alignment.Center,
            Child = new Text($"押した回数: {_count}")
        }
    };
}
```

`State<T>` の派生型は **`class` で宣言します**(`record` は `record` 以外のクラスを継承できないため)。

**押下した瞬間に色を変えたい場合、`GestureDetector` のみでは実現できません。**
`GestureDetector.OnTap` は指を離した後に一度だけ呼ばれ、押下開始を検知する手段がないためです。
押下中の見た目を変更するには、以下のいずれかを使用します。

| やりたいこと | 使うもの |
|---|---|
| 押し下げ・離す・取り消しを個別に扱う | `RawGestureDetector` + `TapGestureRecognizer` の `OnTapDown` / `OnTapUp` / `OnTapCancel`(下の「認識器を自分で組む」) |
| ホバー(領域への出入り)で見た目を変える | `PointerRegion`(`OnPointerEnter` / `OnPointerExit`) |

動作するコードは以下のサンプルを参照してください。

- `samples/FloatSoda.Samples.OverlayApp/CounterWidget.cs` — `GestureDetector` + `SetState` のカウンター
- `samples/FloatSoda.Samples.PointerRegion/PointerRegionDemo.cs` — ホバー・押下・取り消しの状態をすべて表示するデモ

### 用意された `Button` はまだありません

`Button` は UI3層構成(`FloatSoda.UI` と `Cream` / `FizzyPop`)で提供予定ですが、**Phase 5 の対象であり現在は使用できません。**
リポジトリに `ButtonBase` / `Button` / `ButtonStyle` の型は存在しますが、
`ButtonBase` が `GestureDetector` に接続されておらず状態が更新されません。
そのため、これらのプロジェクトは NuGet で配布していません(→ [UILayering](UILayering.md#実装状況))。

**足りないのはフレームワークのジェスチャ基盤ではなく、その上に乗せる層です。**
ボタンを作成する場合は、前述の[押せるボタンを作る](#押せるボタンを作る)の方法で構築してください。

### 認識器を自分で組む(`RawGestureDetector`)

`GestureDetector` は、タップとパンのみを扱う既製のウィジェットです。
独自の解釈が必要な場合は、`RawGestureDetector` を使用して `GestureRecognizer` を手動で登録します。

組み込みの認識器は以下の2つです。

| 認識器 | コールバック |
|---|---|
| `TapGestureRecognizer` | `OnTap`, `OnTapDown`, `OnTapUp`, `OnTapCancel` |
| `PanGestureRecognizer` | `OnPanStart`, `OnPanUpdate`, `OnPanEnd` |

`Gestures` は `Dictionary<Type, GestureRecognizerFactory>` です。
キーに認識器の型、値に「生成用デリゲート」と「コールバック設定用デリゲート」の組を指定します。

```csharp
using FloatSoda.Gesture;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;

Widget tapOnly = new RawGestureDetector
{
    Gestures = new Dictionary<Type, GestureRecognizerFactory>
    {
        [typeof(TapGestureRecognizer)] = new GestureRecognizerFactory<TapGestureRecognizer>(
            // 1つ目: 認識器を生成する。再構築のたびには呼ばれない。
            () => new TapGestureRecognizer(),
            // 2つ目: 再構築のたびに呼ばれ、最新のコールバックを差し込む。
            recognizer =>
            {
                recognizer.OnTapDown = position => Console.WriteLine($"押下: {position}");
                recognizer.OnTap = () => Console.WriteLine("確定");
                recognizer.OnTapCancel = () => Console.WriteLine("取り消し");
            })
    },
    Child = BuildSurface()
};
```

**デリゲートを生成用と設定用に分けているのは、ウィジェットの再構築をまたいで認識器のインスタンスを保持するためです。**
`Widget` は `record` であり毎回作り直されますが、認識器はジェスチャの途中状態を保持します。
認識器まで毎回作り直すと、押下中に再構築が発生した時点でジェスチャが途切れます。

複数の認識器を登録した場合、どれが採用されるかは `GestureArenaManager` が判定します。決着のつき方は2通りです。

1. **いずれかが勝利を宣言した時点で確定する。** たとえばパンは、指が一定距離を越えて動いた時点で
   自身のジェスチャだと宣言します。このとき他の認識器は `RejectGesture` を受けて脱落します。
2. **誰も宣言しないままポインタが上がったら、最初に登録された認識器が勝つ。**
   `Dictionary` の列挙順に依存するため、優先したい認識器を先に登録してください。

**組み込みの認識器は2つとも自身で勝利を宣言するため、通常は1の条件で確定します。**
タップとパンを両方登録した場合、指をほとんど動かさずに離せばタップが、動かせばパンが採用されます。
2の条件は、勝利も辞退も宣言しない独自の認識器を作成した際のフォールバックです。

## Key

`IKey` / `ValueKey<T>` / `UniqueKey` が定義されており、`Widget.Key` プロパティと差分判定に組み込まれています。`Widget.CanUpdate(old, new)` は、実行時型と `Key` が等しい場合に既存の Element を再利用します(Flutter と同じ「型 + Key」判定)。`Element.UpdateChild` は、まず record 等値の高速パスで同一 Widget をスキップし、その後 `CanUpdate` で更新可否を判断します。`MultiChildRenderObjectElement` の子リストの差分検出でも、`Key` を使用して要素の同一性を追跡します(詳細は [BuildPipeline](BuildPipeline.md) を参照)。

既存の子ウィジェット自体を変更せずにキーを付けたい場合は、`KeyedSubtree` の `Key` と `Child` を指定します。

## 関連ページ

- [BuildPipeline](BuildPipeline.md) — BuildOwner / dirty list / UpdateChild の詳細
- [RenderObjects](RenderObjects.md) — Widget が生成する RenderObject のリファレンス
- [GettingStarted](GettingStarted.md) — Widget を使った最初のアプリ
- [UILayering](UILayering.md) — ヘッドレスUI層とデザインシステム層の構成
- [APIDesign](APIDesign.md) — ウィジェット API の設計規約
