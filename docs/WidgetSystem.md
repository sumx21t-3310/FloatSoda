# ウィジェット/エレメントシステム

> **実装状況:**
> - **実装済み:** `StatelessWidget` / `StatelessElement` は完全に動作します。`Build()` はフレームワークから自動的に呼ばれます。`SingleChildRenderObjectWidget` / `MultiChildRenderObjectWidget` ベースのウィジェット（`ColoredBox`, `Align`, `Flex`, `Clip*`, `SizedBox`, `RichText` など）も使用可能です。
> - **WIP:** `StatefulWidget` / `StatefulElement` はスケルトン実装のみです。`StatefulElement.Build()` は `NotImplementedException` を投げます。`UseState` フックは `StatefulElement` に依存しているため現時点では使用不可です。
> - **スタブ:** `Text`, `Button`, `Icon`, `Opacity`, `GestureDetector`, `Listener`, `Container`, `ConstrainedBox`, `ListView`, `GridView`, `SingleChildScrollView`, `Padding` は `NotImplementedException` を投げます。

## 三ツリーの役割

```
Widget (immutable record)
  │  CreateElement()
  ▼
Element (mutable)          ← 状態・ライフサイクル管理
  │  CreateRenderObject()
  ▼
RenderObject               ← レイアウト・描画
```

- **Widget** — UI の設計図。`abstract record` で不変。フレームごとに再生成されても `==` で差分検知できる。
- **Element** — Widget と RenderObject を橋渡しする永続ノード。ウィジェットが更新されても Element は再利用される。
- **RenderObject** — `Layout` と `Paint` を実装する描画エンジン。

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

### UseState フック

> **WIP:** `UseState` は `StatefulWidget` の状態管理に依存しており、`StatefulElement` が未実装のため現時点では使用できません。

R3 ライブラリを使ったリアクティブな状態管理です。`context.UseState<T>()` が返す `ReactiveProperty<T>` の `.Value` を読み書きすると自動的に再ビルドをスケジュールします。

```csharp
public override Widget Build(IBuildContext context)
{
    var count = context.UseState(() => 0);

    return new Button
    {
        Child = new Text($"Count: {count.Value}"),
        OnPressed = () => count.Value++,
    };
}
```

### Depends — DI アクセス

`context.Depends()` で `IServiceProvider` からサービスを取得できます。

```csharp
var logger = context.Depends(p => p.GetService<ILogger>());
logger?.LogInformation("clicked");
```

---

## StatefulWidget / State

> **WIP:** `StatefulWidget` のフレームワーク駆動（`StatefulElement.Build()` の呼び出し）は未実装です。`SetState()` による再ビルドトリガーも機能しません。

`StatefulWidget<T>` は外部から `State<T>` を分離するパターンです。スケルトン実装は以下のとおりです。

```csharp
public record MyCounter : StatefulWidget<MyCounterState>
{
    public override MyCounterState CreateState() => new();
}

public class MyCounterState : State<MyCounter>
{
    private int _count;

    public override Widget Build(IBuildContext context)
    {
        return new Text($"Count: {_count}");
    }

    public void Increment() => _count++;
}
```

---

## 組み込みウィジェット一覧

### Layout

| ウィジェット | 実装状況 | 説明 | 主なプロパティ |
|---|---|---|---|
| `Center` | ✓ | 子を中央に配置 | `Child` |
| `Align` | ✓ | 子を指定の `Alignment` で配置 | `Alignment`, `Child` |
| `Column` | ✓ | 垂直方向に並べる | `Children`, `MainAxisAlignment`, `CrossAxisAlignment` |
| `Row` | ✓ | 水平方向に並べる | `Children`, `MainAxisAlignment`, `CrossAxisAlignment` |
| `Flex` | ✓ | 方向指定のフレックスレイアウト | `Direction`, `Children` |
| `SizedBox` | ✓ | 固定サイズのボックス | `Width`, `Height`, `Child` |
| `Padding` | ✗ スタブ | 子に余白を追加 | `Padding` (`EdgeInsets`), `Child` |
| `ConstrainedBox` | ✗ スタブ | 追加制約を適用 | `AdditionalConstraints`, `Child` |
| `Container` | ✗ スタブ | パディング・色・サイズなどを一括指定 | `Child` など |
| `ListView` | ✗ スタブ | スクロール可能なリスト | `Children` |
| `GridView` | ✗ スタブ | グリッドレイアウト | `Children` |
| `SingleChildScrollView` | ✗ スタブ | 単一子をスクロール | `Child` |

### Painting

| ウィジェット | 実装状況 | 説明 | 主なプロパティ |
|---|---|---|---|
| `ColoredBox` | ✓ | 単色背景 | `Color` (`SKColor`) |
| `ClipRect` | ✓ | 矩形クリップ | `Child` |
| `ClipRoundRect` | ✓ | 角丸矩形クリップ | `BorderRadius`, `Child` |
| `ClipOval` | ✓ | 楕円クリップ | `Child` |
| `ClipCustomPath` | ✓ | カスタムパスクリップ | `Clipper`, `Child` |
| `Opacity` | ✗ スタブ | 透明度を適用 | `Opacity` (0.0–1.0), `Child` |

### Components

| ウィジェット | 実装状況 | 説明 | 主なプロパティ |
|---|---|---|---|
| `RichText` | ✓ | スタイル付きテキスト | `Text` |
| `Image` (Paint) | ✓ | 画像表示 | `Image` (`SKImage`) |
| `Text` | ✗ スタブ | テキスト表示（`RichText` に委譲予定） | `Data` (string) |
| `Icon` | ✗ スタブ | アイコン | — |
| `Button` | ✗ スタブ | タップイベント付きボタン（`StatefulWidget` 依存） | `Child`, `OnPressed` |

### Gesture

| ウィジェット | 実装状況 | 説明 | 主なプロパティ |
|---|---|---|---|
| `GestureDetector` | ✗ スタブ | タップ・ドラッグ検知 | `Child`, `OnTap` |
| `Listener` | ✗ スタブ | 低レベル入力ハンドラ | `Child`, `OnPointerDown` など |

### Root

| ウィジェット | 説明 | 主なプロパティ |
|---|---|---|
| `OverlayWindow` | オーバーレイの設定とウィジェットツリーのルート | `Overlay` (`IOverlay`), `Child` |

---

## Key

`Key` を指定すると、ウィジェットが別の位置に移動したときでも Element が再利用されます。

```csharp
new Text("Hello") { Key = new ValueKey("greeting") }
```
