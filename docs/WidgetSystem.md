← [Home](Home.md)

# ウィジェット/エレメントシステム

> **実装状況:**
> - **実装済み:** `StatelessWidget` / `StatefulWidget` / `InheritedWidget` とそれぞれの Element が動作します。`State.SetState()` による再ビルド、`InheritedWidget` の依存追跡・通知、`MultiChildRenderObjectElement` の `Key` 対応の子リスト差分も実装済みです。`SingleChildRenderObjectWidget<T>` / `MultiChildRenderObjectWidget<T>` ベースのウィジェット(`ColoredBox`, `Align`, `Flex`, `Clip*`, `SizedBox`, `ConstrainedBox`, `RichText`, `Text` など)も使用可能で、`BuildOwner` による差分ビルドが動作します([BuildPipeline](BuildPipeline.md) 参照)。
> - **スタブ:** `Padding`, `Container`, `ListView`, `GridView`, `SingleChildScrollView`, `Opacity`, `Button`, `Icon`, `GestureDetector`, `Listener` は `NotImplementedException` を投げます。
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
| `RenderObjectWidget<T>` | `CreateRenderObject()` / `UpdateRenderObject(T)` で RenderObject を所有 | `RenderObjectElement<T>` ✓ |
| `SingleChildRenderObjectWidget<T>` | 単一の `Child` を持つ RenderObjectWidget | `SingleChildRenderObjectElement<T>` ✓ |
| `MultiChildRenderObjectWidget<T>` | `Children`(`List<Widget>`)を持つ RenderObjectWidget | `MultiChildRenderObjectElement<T>` ✓(`Key` 対応の子リスト差分) |
| `RenderObjectToWidgetAdapter` | Widget ツリーのルートを `RenderView` に接続 | `RenderObjectToWidgetElement<RenderView>` ✓ |

---

## StatelessWidget

状態を持たない純粋関数コンポーネント。`Build(IBuildContext)` でウィジェットツリーを返します。

```csharp
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

---

## Hooks(FloatSoda.Hooks)

> **WIP:** `FloatSoda.Hooks` プロジェクトに R3 ベースの `HookWidget` / `HookElement` が部分実装されていますが、フレームワークのビルドループとは未統合です。`HookExtension` の `UseState` / `UseEffect` / `Depends` / `UseMemo` / `UseAction` は `NotImplementedException` を投げます。

`HookWidget.Build()` 内で `UseState(initialValue)` を呼ぶと `ReactiveProperty<T>` が返り、値の変更が再ビルドをトリガーする、という React フック風の API を目指しています。

```csharp
// 構想中の API(未動作)
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
| `ConstrainedBox` | ✓ | 追加制約を適用 | `Constraints` (`BoxConstraints`), `Child` |
| `Padding` | ✗ スタブ | 子に余白を追加(`RenderSiftedBox.PerformLayout` 未実装) | `Spacing` (`EdgeInsets`), `Child` |
| `Container` | ✗ スタブ | パディング・色・サイズなどを一括指定 | — |
| `ListView` | ✗ スタブ | スクロール可能なリスト | `Children` |
| `GridView` | ✗ スタブ | グリッドレイアウト | — |
| `SingleChildScrollView` | ✗ スタブ | 単一子をスクロール | `Child` |

### Painting

| ウィジェット | 実装状況 | 説明 | 主なプロパティ |
|---|---|---|---|
| `ColoredBox` | ✓ | 単色背景 | `Color` (`SKColor`), `Child` |
| `Image` (Paint) | ✓ | `ImageProvider` 経由で画像を表示 | `ImageProvider`, `Child` |
| `ClipRect` | ✓ | 矩形クリップ | `Clipper`, `ClipBehavior`, `Child` |
| `ClipRoundRect` | ✓ | 角丸矩形クリップ | `BorderRadius`, `Clipper`, `ClipBehavior`, `Child` |
| `ClipOval` | ✓ | 楕円クリップ | `CustomClipper`, `ClipBehavior`, `Child` |
| `ClipCustomPath` | ✓ | カスタムパスクリップ | `Clipper`, `ClipBehavior`, `Child` |
| `Opacity` | ✗ スタブ | 透明度を適用 | `Child` |

### Components

| ウィジェット | 実装状況 | 説明 | 主なプロパティ |
|---|---|---|---|
| `RichText` | ✓ | スタイル付きテキスト(Topten.RichTextKit) | `Text` (`TextSpan`) |
| `Text` | ✓ | プレーンテキスト表示(`RichText` に委譲) | `Data` (string) |
| `Image` (Components) | ✗ スタブ | 高レベルな画像ウィジェット(予定) | — |
| `Icon` | ✗ スタブ | アイコン | `Color`, `Size` |
| `Button` | ✗ スタブ | タップイベント付きボタン(ジェスチャ未実装のため) | — |

### Gesture

| ウィジェット | 実装状況 | 説明 |
|---|---|---|
| `GestureDetector` | ✗ スタブ | タップ・ドラッグ検知 |
| `Listener` | ✗ スタブ | 低レベル入力ハンドラ |

---

## Key

`IKey` / `ValueKey<T>` / `UniqueKey` が定義され、`Widget.Key` プロパティと差分判定に組み込まれています。`Widget.CanUpdate(old, new)` は「同じ実行時型かつ `Key` が等しい」なら既存 Element を再利用します(Flutter と同じ型 + Key 判定)。`Element.UpdateChild` は先に record 等値の高速パスで同一 Widget をスキップし、その後 `CanUpdate` で更新可否を判断します。`MultiChildRenderObjectElement` の子リスト差分でも `Key` を使って要素の同一性を追跡します(詳細は [BuildPipeline](BuildPipeline.md))。

---

## 関連ページ

- [BuildPipeline](BuildPipeline.md) — BuildOwner / dirty list / UpdateChild の詳細
- [RenderObjects](RenderObjects.md) — Widget が生成する RenderObject のリファレンス
- [GettingStarted](GettingStarted.md) — Widget を使った最初のアプリ
- [APIDesign](APIDesign.md) — ウィジェット API の設計規約
