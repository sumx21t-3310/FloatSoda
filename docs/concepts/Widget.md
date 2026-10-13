# Widget — 宣言的 UI と Widget ツリー

Widget は、UI の一部分を「何を、どんな設定で置くか」として表す、作ったあとに変えられない設計図です。画面は Widget の入れ子(Widget ツリー)で組み立てます。このページは、Widget の性質、自作の Widget を書くための2つの基底型、組み込み Widget の種類、色の指定を説明します。

## 概要

FloatSoda の UI は、すべて Widget の組み合わせです。文字は `Text`、余白は `Padding`、横並びは `Row`、タップの受け取りは `GestureDetector` のように、1つの役割を1つの Widget が受け持ちます。複数の役割を持たせたいときは、Widget を入れ子にします。たとえば「余白を付けた、タップできる文字」は、`GestureDetector` の中に `Padding` を置き、その中に `Text` を置いて作ります。

## いつ使うか

画面に何かを出すときは、必ず Widget を使います。組み込みの Widget を組み合わせるだけで足りる場合と、組み合わせに名前を付けて自作の Widget にする場合があります。自作の Widget は、同じ組み合わせを何度も使うときや、状態を持たせたいときに作ります。

## 基本モデル

### Widget は作り直す値である

Widget の基底型 `Widget` は `abstract record` です。プロパティは `init` で設定するので、作ったあとに値を変えられません。値は object initializer で渡し、必須のプロパティには `required` が付いています。子が1つなら `Child`、複数なら `Children` に渡します。表示する文字列のように、単一の値をコンストラクタで受け取る Widget もあります(例: `new Text("Hello")`)。

見た目を変えたいときは、既存の Widget を書き換える代わりに、新しい値で Widget を作り直します。作り直すのは設計図だけで、画面上の部品(Element と RenderObject)は条件が合えば使い回されます(→ [FloatSoda の考え方](Overview.md#三つのツリー))。作り直した Widget が前回と等しい(`record` の値が同じ)場合、フレームワークはその Widget から下の処理を省きます。

### 自作の Widget は2つの基底型から作る

自作の Widget は、状態を持つかどうかで基底型を選びます。

| 基底型 | 選ぶ場面 | 書くもの |
|---|---|---|
| `StatelessWidget` | 表示がプロパティだけで決まる | `record` で宣言し、`Build(IBuildContext)` で子の Widget を返す |
| `StatefulWidget<T>` | 押した回数のように、時間とともに変わる値を持つ | `record` で宣言して `CreateState()` を書き、`State<T>` を継承した `class` に `Build` を書く |

`StatelessWidget` の最小の形は次のとおりです。

```csharp
using FloatSoda.Elements;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

public record Greeting : StatelessWidget
{
    public required string Name { get; init; }

    public override Widget Build(IBuildContext context) =>
        new Center { Child = new Text($"Hello, {Name}") };
}
```

Widget に変わる値をフィールドとして持たせてはいけません。Widget は作り直されるたびに新しいインスタンスになるため、値が引き継がれません。変わる値は `State<T>` のフィールドに置きます。`State<T>` は `record` ではなく `class` で宣言します。`record` は `record` 以外のクラスを継承できないためです。状態の扱いは [State](State.md) で説明します。

### Key は「同じ部品」の判定を変える

作り直した Widget を受け取ると、フレームワークは同じ位置の既存の部品を使い回せるかを調べます。使い回すのは、実行時の型が同じで、`Key` が等しいときです(`Widget.CanUpdate`)。使い回された部品は、`State` もそのまま持ち続けます。

`Key` を指定しない Widget は、同じ位置に同じ型があれば使い回されます。状態を持つ子のリストを並べ替える場合は、各子に `ValueKey<T>` を付けると、状態が並べ替え後の正しい子に付いていきます。既存の Widget を変えずに `Key` を付けたいときは、`KeyedSubtree` で包みます。

`UniqueKey` は、作るたびに別の値になります。`Build` の中で `new UniqueKey()` を書くと、`Build` のたびに `Key` が変わり、その子は毎回作り直されて状態を失います。

## 組み込み Widget の種類

組み込みの Widget は、役割ごとに名前空間が分かれています。下の表は種類の地図で、すべての Widget の一覧ではありません。一覧は [`src/FloatSoda/Widgets/`](../../src/FloatSoda/Widgets/) のディレクトリと、[API Reference](../api/Home.md) が一次情報です。

| 種類 | 名前空間 | 役割 | 例 | 詳しいページ |
|---|---|---|---|---|
| レイアウト | `FloatSoda.Widgets.Layout` | 子の大きさと位置を決める | `Row`、`Column`、`Padding`、`Stack`、`SizedBox`、`Container` | [Layout](Layout.md) |
| 描画 | `FloatSoda.Widgets.Paint` | 色、装飾、画像、アイコン、切り抜き、不透明度、変形を付ける | `ColoredBox`、`DecoratedBox`、`Image`、`Icon`、`Opacity` | — |
| 入力 | `FloatSoda.Widgets.Gesture` | タップ、ドラッグ、ホバーを受け取る | `GestureDetector`、`Listener`、`PointerRegion` | [Input](Input.md) |
| アニメーション | `FloatSoda.Widgets.Animation` | 時間とともに変わる値で子を描く | `FadeTransition` | [Animation](Animation.md) |
| 文字とツリーの補助 | `FloatSoda.Widgets` | 文字を表示する。状態や値を子へ渡す | `Text`、`DefaultTextStyle`、`Builder`、`KeyedSubtree`、`ListenableBuilder` | [State](State.md) |

各 Widget の見た目と最小の使い方は、[Widget カタログ](../user/Home.md) にあります。カタログの元は、リポジトリの `samples/` にある各サンプルの README です。

## 色

色を受け取るプロパティ(`ColoredBox.Color`、`Container.Color`、`TextStyle.Color`、`BoxDecoration.Color` など)の型は、FloatSoda の `Color`(`FloatSoda.Geometrics` 名前空間)です。SkiaSharp の `SKColor` とは別の型です。`Color` は赤・緑・青・不透明度(`Alpha`)の4つの成分を、それぞれ 0〜255 の `byte` で持ちます。

色は次の3つの書き方で指定できます。

| 書き方 | 例 | 注意 |
|---|---|---|
| `new Color(r, g, b)` / `new Color(r, g, b, a)` | `new Color(255, 0, 0)` | 省いた成分は 255 になる。`new Color()` は不透明な白 |
| `Color.Parse` | `Color.Parse("#FF0000")`、`Color.Parse("#F00")` | `#` は省略できる。3桁か6桁の16進数だけを受け付け、結果は常に不透明。それ以外の書式は `FormatException`、空文字列は `ArgumentException` |
| SkiaSharp の `SKColors` | `SKColors.Red` | `SKColor` から `Color` への暗黙の変換で代入できる。`using SkiaSharp;` が要る |

名前付きの色(赤、白など)は、SkiaSharp の `SKColors` にだけあります。FloatSoda の `Color` には `Color.Red` のような定数がありません。

半透明にするときは、4つ目の成分 `Alpha` を指定します。0 が完全な透明、255 が完全な不透明です。`Color.Parse` は不透明度を読めないため、半透明の色は `new Color(255, 0, 0, 128)` のように書くか、`Color.Parse("#FF0000") with { Alpha = 128 }` のように後から変えます。

`ColoredBox.Color` には既定値がありません。`Color` を指定しないと、すべての成分が 0 の色、つまり透明になり、何も描かれません。

公開 API から SkiaSharp の型を取り除く計画があります(追跡: [#131](https://github.com/sumx21t-3310/FloatSoda/issues/131))。`SKColors` を経由する書き方がその後も使えるかは ※要確認 です。

## 最小例

最小のコード例は、Widget カタログのサンプルにあります。このページの断片より、サンプルのコードを正とします。

- [`samples/FloatSoda.Samples.GettingStarted/Program.cs`](../../samples/FloatSoda.Samples.GettingStarted/Program.cs) — Widget を入れ子にしてウィンドウへ渡す、最小のアプリ
- [ColoredBox](../../samples/FloatSoda.Samples.ColoredBox/README.md) — 単色の背景と色の指定
- [Container](../../samples/FloatSoda.Samples.Container/README.md) — 余白、装飾、大きさを1つの Widget でまとめて指定する
- [Text](../../samples/FloatSoda.Samples.Text/README.md) — 文字の表示と書式
- [GestureDetector](../../samples/FloatSoda.Samples.GestureDetector/README.md) — `StatefulWidget` と組み合わせて、タップで表示を変える

## Unity / uGUI からの読み替え

| uGUI | FloatSoda | 違う点 |
|---|---|---|
| GameObject に Image や Text などのコンポーネントを足す | Widget を入れ子にする | 1つの Widget に役割を足さない。役割ごとに Widget で包む |
| Prefab | `StatelessWidget` か `StatefulWidget<T>` を継承した自作の型 | Inspector で値を変える代わりに、`init` プロパティで値を渡す |
| Inspector で設定する値 | Widget の `init` プロパティ | 実行中に値を書き換えない。新しい値で Widget を作り直す |
| `UnityEngine.Color`(各成分 0〜1 の `float`) | `FloatSoda.Geometrics.Color`(各成分 0〜255 の `byte`) | 成分の範囲が違う。Unity の `Color32` と同じ範囲になる |
| `SetActive(false)` で隠す | `Visibility`、`Offstage`、`IndexedStack` で切り替える | 隠した子の状態が残るかは Widget ごとに違う([State](State.md)) |

## 制約と未実装

- スクロールする Widget(`ListView`、`GridView`、`SingleChildScrollView`)は `internal` で、公開 API として使えない(追跡: [#69](https://github.com/sumx21t-3310/FloatSoda/issues/69))
- `Button` のような既製の部品はまだ提供していない。押せる部品は `GestureDetector` で組み立てる([Input](Input.md)。追跡: [#102](https://github.com/sumx21t-3310/FloatSoda/issues/102))
- `HSVColor` / `HSLColor` と `Color` の間の変換は、呼ぶと `NotImplementedException` を投げる ※要確認(追跡する Issue 番号)

## 関連ページ

- [FloatSoda の考え方](Overview.md) — 三つのツリーと、Widget がオーバーレイに表示されるまで
- [State](State.md) — 状態の持ち方と作り直しの範囲
- [Layout](Layout.md) — 大きさと位置の決まり方
- [Widget カタログ](../user/Home.md) — 各 Widget の最小の使用例
- [API Reference](../api/Home.md) — 各 Widget のプロパティの正確な契約
