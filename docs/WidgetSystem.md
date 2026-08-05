← [Home](Home.md)

# ウィジェット/エレメントシステム

> **実装状況:**
> - **実装済み:** `StatelessWidget` / `StatefulWidget` / `InheritedWidget` とそれぞれの Element が動作します。`State.SetState()` による再ビルド、`InheritedWidget` の依存追跡・通知、`MultiChildRenderObjectElement` の `Key` 対応の子リスト差分も実装済みです。ツリー補助の `Builder` / `KeyedSubtree` / `RepaintBoundary` と、`ParentDataWidget<T>` による親固有レイアウト情報の適用にも対応しています。`SingleChildRenderObjectWidget<T>` / `MultiChildRenderObjectWidget<T>` ベースのウィジェット(`ColoredBox`, `Align`, `Flex`, `Stack`, `Offstage`, `IndexedStack`, `RotatedBox`, `Clip*`, `SizedBox`, `ConstrainedBox`, `RichText`, `Text` など)も使用可能で、`BuildOwner` による差分ビルドが動作します([BuildPipeline](BuildPipeline.md) 参照)。
> - **未実装:** `ListView`, `GridView`, `SingleChildScrollView` は `internal` で、公開 API から除外されています。`Padding`, `Container`, `DecoratedBox`, `Opacity`, `Transform` は公開 API として利用できます。入力系の `GestureDetector` / `Listener` は公開スタブです。`Button` / `Icon` はデザインシステム層(`FloatSoda.UI.Cream` / `FloatSoda.UI.FizzyPop`)へ移動しました(→ [UILayering](UILayering.md))。
> - **WIP:** `FloatSoda.Hooks`(R3 ベースの `UseState` など)はフレームワークのビルドループと未統合です。ジェスチャ・ヒットテストは未実装です。

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

- **Widget** — UI の設計図。`abstract record` で不変。フレームごとに再生成されても `==` で差分検知できる。
- **Element** — Widget と RenderObject を橋渡しする永続ノード。ウィジェットが更新されても Element は再利用される。再ビルドの仕組みは [BuildPipeline](BuildPipeline.md) を参照。
- **RenderObject** — `PerformLayout` と `Paint` を実装する描画エンジン。詳細は [RenderObjects](RenderObjects.md) を参照。

---

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

---

## ParentDataWidget

`ParentDataWidget<T>` は、自身ではRenderObjectを作らず、子RenderObjectの `ParentData` を更新します。
親RenderObjectは `SetupParentData` で `T` を用意し、派生Widgetは `ApplyParentData(T)` で値を比較・更新して、変更した場合だけ `true` を返します。
変更時の `MarkNeedsLayout()` は基底クラスが親RenderObjectへ伝播します。

`Flexible` や `Positioned` のように「親レイアウトだけが解釈する子ごとの情報」を宣言的なWidget APIとして表現するための基盤です。
対応するParentDataを用意しない親の下で使用すると `InvalidOperationException` になります。

---

## StatelessWidget

状態を持たない純粋関数コンポーネント。`Build(IBuildContext)` でウィジェットツリーを返します。

```csharp
using FloatSoda.Elements;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Components;
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

---

## StatefulWidget / State

`StatefulWidget<T>` は Widget から `State<T>` を分離するパターンです。`State.SetState(Action)` は状態を書き換えたうえで `Element.MarkNeedsBuild()` を呼び、次フレームの `BuildScope()` で再ビルドされます。

```csharp
public record WatchWidget : StatefulWidget<WatchWidget>
{
    public override State<WatchWidget> CreateState() => new WatchState();
}

public record WatchState : State<WatchWidget>
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

---

## InheritedWidget

ツリーの下方にコンテキスト(テーマなど)を伝播させるためのウィジェットです。`InheritedElement` が依存する子孫を追跡し、`UpdateShouldNotify(InheritedWidget oldWidget)` が `true` を返したときに依存側を再ビルド対象にします。

現在位置から最も近いスコープを読み、その更新通知を購読するには、`IBuildContext.DependOnInheritedWidgetOfExactType<T>()` を使います。テーマ側に `Of(IBuildContext)` を用意すると、利用側が照会方法を毎回書かずに済みます。

### Builder

`Builder` は新しい `IBuildContext` を1段挟み、`ChildBuilder` で子を構築します。同じ `Build()` 内で作成した `InheritedWidget` を、その子側のコンテキストから解決したい場合に使います。

```csharp
using FloatSoda.Elements;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Components;

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
using FloatSoda.Widgets.Components;

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

---

## Hooks(FloatSoda.Hooks)

> **WIP:** `FloatSoda.Hooks` プロジェクトに R3 ベースの `HookWidget` / `HookElement` が部分実装されていますが、フレームワークのビルドループとは未統合です。`HookExtension` の `UseState` / `UseEffect` / `Depends` / `UseMemo` / `UseAction` は `NotImplementedException` を投げます。

`HookWidget.Build()` 内で `UseState(initialValue)` を呼ぶと `ReactiveProperty<T>` が返り、値の変更が再ビルドをトリガーする、という React フック風の API を目指しています。

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

---

## 組み込みウィジェット一覧

### Layout

| ウィジェット | 実装状況 | 説明 | 主なプロパティ |
|---|---|---|---|
| `Center` | ✓ | 子を中央に配置(`Align` に委譲) | `Child` |
| `Align` | ✓ | 子を指定の `Alignment` で配置 | `Alignment`, `WidthFactor`, `HeightFactor`, `Child` |
| `Column` | ✓ | 垂直方向に並べる(`Flex` に委譲) | `Children`, `MainAxisAlignment`, `CrossAxisAlignment`, `MainAxisSize` |
| `Row` | ✓ | 水平方向に並べる(`Flex` に委譲) | `Children`, `MainAxisAlignment`, `CrossAxisAlignment`, `MainAxisSize` |
| `Flex` | ✓ | 方向指定のフレックスレイアウト。`UpdateRenderObject` と `Key` 対応の子リスト差分に対応 | `Direction`, `Children`, `MainAxisAlignment`, `CrossAxisAlignment`, `VerticalDirection` |
| `SizedBox` | ✓ | 固定サイズのボックス | `Width`, `Height`, `Child` |
| `ConstrainedBox` | ✓ | 親の制約と交差する追加制約を子へ適用 | `AdditionalConstraints` (`BoxConstraints`, 必須), `Child` |
| `IntrinsicWidth` | ✓ | 子の最大intrinsic幅へ収縮し、任意のstep単位で切り上げ | `StepWidth`, `Child` |
| `IntrinsicHeight` | ✓ | 子の最大intrinsic高さへ収縮し、任意のstep単位で切り上げ | `StepHeight`, `Child` |
| `Padding` | ✓ | 子の制約を余白分だけ縮小し、子を余白の左上位置へ配置 | `Spacing` (`EdgeInsets`, 必須), `Child` |
| `Stack` | ✓ | 複数の子を重ね、非Positioned子を`Alignment`と`Fit`で配置 | `Children`, `Alignment`, `Fit` |
| `Positioned` | ✓ | `Stack`の子を辺からの距離または固定寸法で絶対配置 | `Left`, `Top`, `Right`, `Bottom`, `Width`, `Height`, `Child` |
| `IndexedStack` | ✓ | 全子をレイアウトし、`Index`で選んだ1子だけを描画・ヒットテスト。`null`なら全子を非表示 | `Children`, `Index`, `Alignment`, `Fit` |
| `Offstage` | ✓ | 子をレイアウトしたまま描画・ヒットテストから除外 | `IsOffstage`, `Child` |
| `Visibility` | ✓ | `Visible`に応じて必須の`Child`と`Replacement`を切り替え。非表示子の状態保持は行わない | `Visible`, `Child` (必須), `Replacement` |
| `RotatedBox` | ✓ | 90度単位でレイアウト寸法ごと時計回りに回転。負値・4以上は4を法として正規化 | `QuarterTurns`, `Child` |
| `Container` | ✗ `internal` スタブ | パディング・色・サイズなどを一括指定 | — |
| `ListView` | ✗ `internal` スタブ | スクロール可能なリスト | `Children` |
| `GridView` | ✗ `internal` スタブ | グリッドレイアウト | — |
| `SingleChildScrollView` | ✗ `internal` スタブ | 単一子をスクロール | `Child` |

`ConstrainedBox` は、親から渡される制約を無視せず、その範囲内で追加の最小・最大サイズを子へ適用します。

`IndexedStack.Index` は0始まりです。`null`は全子をレイアウトしたまま全非表示にし、負値または`Children`の範囲外は`ArgumentOutOfRangeException`になります。
`RotatedBox`は回転後の幅と高さをレイアウトへ反映します。レイアウト寸法を変えず描画だけを任意角度で変形する`Transform`とは用途が異なります。

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

`IntrinsicWidth` / `IntrinsicHeight` は、通常レイアウトの前に子へ自然な寸法を問い合わせます。
`StepWidth` / `StepHeight` を指定すると、計測値をその正の有限値の倍数へ切り上げます。
たとえば内容量が異なるカードを一定のstep幅へ揃える場合に使えます。

```csharp
Widget statusCard = new IntrinsicWidth
{
    StepWidth = 40,
    Child = new Text("VRChat: Online")
};
```

intrinsic測定は追加のツリー走査を必要とし、入れ子では最悪O(N²)になり得ます。
スクロール領域や大量の項目を持つツリーでは使用せず、寸法が分かる場合は`SizedBox`や`ConstrainedBox`を優先してください。

### Painting

| ウィジェット | 実装状況 | 説明 | 主なプロパティ |
|---|---|---|---|
| `ColoredBox` | ✓ | 単色背景 | `Color` (`Color`), `Child` |
| `DecoratedBox` | ✓ | `BoxDecoration` の背景色・角丸・ボーダーを子の前面または背面へ描画 | `Decoration`, `Position`, `Child` |
| `Image` (Paint) | ✓ | `ImageProvider` 経由で画像を表示 | `ImageProvider`, `Child` |
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

### Components

| ウィジェット | 実装状況 | 説明 | 主なプロパティ |
|---|---|---|---|
| `RichText` | ✓ | `TextSpan` でスタイル付きテキストを表示 | `Text` (`TextSpan`) |
| `Text` | ✓ | 単一書式のテキスト表示(`RichText` / `TextSpan` に委譲) | `Data` (string), `Style` (`TextStyle?`) |

`Text` は表示文字列を単一値コンストラクタで受け、書式は `init` プロパティで指定します。`Style` を省略すると、フォントサイズ30、Arial、黒、ウェイト400の既定書式を使用します。空文字列は有効です。

```csharp
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets.Components;

new Text("Hello, VR!")
{
    Style = new TextStyle
    {
        FontSize = 36,
        Color = new Color(255, 255, 255),
        FontFamily = "Arial",
        FontWeight = 700,
        IsItalic = false
    }
}
```

`Button` / `Icon` はデザインシステム層(`FloatSoda.UI.Cream` / `FloatSoda.UI.FizzyPop`)へ移動しました。振る舞いを担うヘッドレスウィジェット(`ButtonBase` など)は `FloatSoda.UI` にあります。詳細は [UILayering](UILayering.md) を参照してください。

### Gesture

| ウィジェット | 実装状況 | 説明 |
|---|---|---|
| `GestureDetector` | ✗ スタブ | タップ・ドラッグ検知 |
| `Listener` | ✗ スタブ | 低レベル入力ハンドラ |

---

## Key

`IKey` / `ValueKey<T>` / `UniqueKey` が定義され、`Widget.Key` プロパティと差分判定に組み込まれています。`Widget.CanUpdate(old, new)` は「同じ実行時型かつ `Key` が等しい」なら既存 Element を再利用します(Flutter と同じ型 + Key 判定)。`Element.UpdateChild` は先に record 等値の高速パスで同一 Widget をスキップし、その後 `CanUpdate` で更新可否を判断します。`MultiChildRenderObjectElement` の子リスト差分でも `Key` を使って要素の同一性を追跡します(詳細は [BuildPipeline](BuildPipeline.md))。

既存の子ウィジェット自体を変更せずにキーを付けたい場合は、`KeyedSubtree` の `Key` と `Child` を指定します。

---

## 関連ページ

- [BuildPipeline](BuildPipeline.md) — BuildOwner / dirty list / UpdateChild の詳細
- [RenderObjects](RenderObjects.md) — Widget が生成する RenderObject のリファレンス
- [GettingStarted](GettingStarted.md) — Widget を使った最初のアプリ
- [UILayering](UILayering.md) — ヘッドレスUI層とデザインシステム層の構成
- [APIDesign](APIDesign.md) — ウィジェット API の設計規約
