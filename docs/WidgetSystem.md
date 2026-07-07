← [Home](Home.md)

# ウィジェット/エレメントシステム

> **実装状況:**
> - **実装済み:** `StatelessWidget` / `StatelessElement` は完全に動作します。`SingleChildRenderObjectWidget<T>` / `MultiChildRenderObjectWidget<T>` ベースのウィジェット(`ColoredBox`, `Align`, `Flex`, `Clip*`, `SizedBox`, `ConstrainedBox`, `RichText`, `Text` など)も使用可能です。`BuildOwner` による差分ビルドが動作します([BuildPipeline](BuildPipeline.md) 参照)。
> - **WIP:** `StatefulWidget` / `StatefulElement` はスケルトン実装のみです。`StatefulElement.Build()` は `NotImplementedException` を投げます。`InheritedWidget` / `InheritedElement` も同様にスケルトンのみです。
> - **スタブ:** `Padding`, `Container`, `ListView`, `GridView`, `SingleChildScrollView`, `Opacity`, `Button`, `Icon`, `GestureDetector`, `Listener` は `NotImplementedException` を投げます。

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
| `StatefulWidget<T>` | `CreateState()` で `State<T>` を分離(WIP) | `StatefulElement` ✗ |
| `InheritedWidget` | ツリー下方へのコンテキスト伝播(WIP) | `InheritedElement` ✗ |
| `RenderObjectWidget<T>` | `CreateRenderObject()` / `UpdateRenderObject(T)` で RenderObject を所有 | `RenderObjectElement<T>` ✓ |
| `SingleChildRenderObjectWidget<T>` | 単一の `Child` を持つ RenderObjectWidget | `SingleChildRenderObjectElement<T>` ✓ |
| `MultiChildRenderObjectWidget<T>` | `Children`(`List<Widget>`)を持つ RenderObjectWidget | `MultiChildRenderObjectElement<T>` △(初回構築のみ) |
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

> **WIP:** `StatefulElement` の `Build()` / `MarkNeedsBuild()` は未実装です。`State.SetState()` を呼ぶと `NotImplementedException` になります。

`StatefulWidget<T>` は Widget から `State<T>` を分離するパターンです。API は以下の形で確定していますが、フレームワーク駆動が未実装です。

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

> **WIP:** `InheritedElement.PerformRebuild()` が未実装で、依存側への通知経路もまだありません。

ツリーの下方にコンテキスト(テーマなど)を伝播させるためのウィジェットです。`UpdateShouldNotify(InheritedWidget oldWidget)` で「依存している子孫を再ビルドすべきか」を判定する設計です。

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
| `Flex` | △ | 方向指定のフレックスレイアウト。初回構築は動作、再ビルド(`UpdateRenderObject` と子リスト差分)は未実装 | `Direction`, `Children`, `MainAxisAlignment`, `CrossAxisAlignment`, `VerticalDirection` |
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
| `Button` | ✗ スタブ | タップイベント付きボタン(`StatefulWidget` 依存) | — |

### Gesture

| ウィジェット | 実装状況 | 説明 |
|---|---|---|
| `GestureDetector` | ✗ スタブ | タップ・ドラッグ検知 |
| `Listener` | ✗ スタブ | 低レベル入力ハンドラ |

---

## Key

> **未接続:** `IKey` / `ValueKey<T>` / `UniqueKey` の型は定義済みですが、`Widget` に `Key` プロパティがなく、差分判定(`Widget.CanUpdate`)にも組み込まれていません。

Flutter と同様に「ウィジェットが同型のまま位置や内容が変わったときに Element を再利用するためのヒント」として導入予定です。現在の `Widget.CanUpdate` は record の完全一致比較であるため、プロパティが変わった Widget は Element ごと作り直されます(詳細は [BuildPipeline](BuildPipeline.md))。

---

## 関連ページ

- [BuildPipeline](BuildPipeline.md) — BuildOwner / dirty list / UpdateChild の詳細
- [RenderObjects](RenderObjects.md) — Widget が生成する RenderObject のリファレンス
- [GettingStarted](GettingStarted.md) — Widget を使った最初のアプリ
- [APIDesign](APIDesign.md) — ウィジェット API の設計規約
