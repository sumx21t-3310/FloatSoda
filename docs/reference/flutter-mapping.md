← [リファレンス](Home.md)

# Flutter の読み替え表

Flutter で知っている書き方が、FloatSoda でそのまま通るか、変わるか、まだないかを、このページで引けます。Flutter のウィジェットの名前から、該当する節の表を探してください。

## この表の使い方

区分は3つです。

- 同じ: 名前も使い方も同じで、挙動も同等
- 違う: 名前や書き方が違う、または見た目や反応が違う
- まだない: Flutter にあって、FloatSoda にまだないもの、または実装しないもの

FloatSoda は、Flutter の3つの木(ウィジェット、Element、RenderObject)に合わせて作ってあります。ウィジェットの名前、レイアウトの用語(`MainAxisAlignment`、`CrossAxisAlignment`)、`Child` と `Children` は、Flutter と同じ名前です。表の「詳細」の列は、各作例の README の「Flutterとの違い」へのリンクです。差異の詳しい説明は、その README に1か所だけ書いてあり、このページにはコピーしていません。

## 同じように書けるもの

次のウィジェットは、作例の README で「同等」と確かめたものです。基本の挙動は Flutter と同じです。C# の書き方の違いと未対応の項目は、各行の補足で確かめてください。

| Flutter | FloatSoda | 補足 | 詳細 |
| :-- | :-- | :-- | :-- |
| [`Align`](https://api.flutter.dev/flutter/widgets/Align-class.html)、[`Center`](https://api.flutter.dev/flutter/widgets/Center-class.html) | `Align`、`Center` | `textDirection` による左右の切り替えがない | [Align](../../samples/Widgets/FloatSoda.Samples.Align/README.md) |
| [`LimitedBox`](https://api.flutter.dev/flutter/widgets/LimitedBox-class.html) | `LimitedBox` | 差異なし | [LimitedBox](../../samples/Widgets/FloatSoda.Samples.LimitedBox/README.md) |
| [`FractionallySizedBox`](https://api.flutter.dev/flutter/widgets/FractionallySizedBox-class.html) | `FractionallySizedBox` | `textDirection` による左右の切り替えがない | [FractionallySizedBox](../../samples/Widgets/FloatSoda.Samples.FractionallySizedBox/README.md) |
| [`OverflowBox`](https://api.flutter.dev/flutter/widgets/OverflowBox-class.html) | `OverflowBox` | `textDirection` による左右の切り替えがない | [OverflowBox](../../samples/Widgets/FloatSoda.Samples.OverflowBox/README.md) |
| [`FittedBox`](https://api.flutter.dev/flutter/widgets/FittedBox-class.html) | `FittedBox` | `textDirection` による左右の切り替えがない | [FittedBox](../../samples/Widgets/FloatSoda.Samples.FittedBox/README.md) |
| [`ConstraintsTransformBox`](https://api.flutter.dev/flutter/widgets/ConstraintsTransformBox-class.html)、[`UnconstrainedBox`](https://api.flutter.dev/flutter/widgets/UnconstrainedBox-class.html) | `ConstraintsTransformBox`、`UnconstrainedBox` | `textDirection` による左右の切り替えと、debug 時のはみ出し表示がない | [ConstraintsTransformBox](../../samples/Widgets/FloatSoda.Samples.ConstraintsTransformBox/README.md) |
| [`Wrap`](https://api.flutter.dev/flutter/widgets/Wrap-class.html) | `Wrap` | `textDirection` と `clipBehavior` は未対応(既定の見た目は一致する) | [Wrap](../../samples/Widgets/FloatSoda.Samples.Wrap/README.md) |
| [`ColoredBox`](https://api.flutter.dev/flutter/widgets/ColoredBox-class.html) | `ColoredBox` | `Color` は `init` プロパティで指定する(次の節) | [ColoredBox](../../samples/Widgets/FloatSoda.Samples.ColoredBox/README.md) |
| [`StatefulWidget`](https://api.flutter.dev/flutter/widgets/StatefulWidget-class.html)、[`State`](https://api.flutter.dev/flutter/widgets/State-class.html)、`setState` | `StatefulWidget<T>`、`State<T>`、`SetState()` | 型を `StatefulWidget<T>` のようにジェネリックで書く。`SetState()` で再ビルドする仕組みは同じ | [状態と作り直し](../guide/4-build/mechanics/state-and-rebuild.md) |

レイアウトの仕組み(制約が親から子へ下り、大きさが子から親へ上る)も、Flutter と同じです。

作例のないウィジェットは、Flutter と同じと確かめていません。`IndexedStack`、`ConstrainedBox`、`RotatedBox`、`Visibility`、`Offstage`、`IntrinsicWidth`、`IntrinsicHeight`、`RepaintBoundary`、`Builder`、`KeyedSubtree` などが当たります。<!-- ※要確認: 作例のない型の「同じ」を言えるか。型ごとに Flutter の実装と見比べていない。ConstrainedBox は AdditionalConstraints に改名されている(CHANGELOG.md の 0.4.0)ため、下の改名の表にも載せた -->

## 名前や書き方が違うもの

名前や書き方が違うと、コンパイルが通りません。表のとおりに書き換えてください。

### 名前が変わるもの

| Flutter | FloatSoda | 理由 | 詳細 |
| :-- | :-- | :-- | :-- |
| `ClipRRect` | `ClipRoundRect` | 記録なし | [Clip](../../samples/Widgets/FloatSoda.Samples.Clip/README.md) |
| `ClipPath` | `ClipCustomPath` | 記録なし | [Clip](../../samples/Widgets/FloatSoda.Samples.Clip/README.md) |
| `MouseRegion` | `PointerRegion` | VR のレーザーもマウスも「ポインター」と呼ぶ | [PointerRegion](../../samples/Widgets/FloatSoda.Samples.PointerRegion/README.md) |
| `Opacity` の `opacity` | `Opacity.Value` | C# ではメンバー名を型名と同じにできない | [Opacity](../../samples/Widgets/FloatSoda.Samples.Opacity/README.md) |
| `AspectRatio` の `aspectRatio` | `AspectRatio.Ratio` | C# ではメンバー名を型名と同じにできない | [AspectRatio](../../samples/Widgets/FloatSoda.Samples.AspectRatio/README.md) |
| `Padding` の `padding` | `Padding.Spacing` | 記録なし | [Padding](../../samples/Widgets/FloatSoda.Samples.Padding/README.md) |
| `behavior`(`HitTestBehavior`) | `Behaviour`(`HitTestBehaviour`) | 綴りが違う(Flutter は米国式の behavior) | [GestureDetector](../../samples/Widgets/FloatSoda.Samples.GestureDetector/README.md) |
| `ConstrainedBox` の `constraints` | `ConstrainedBox.AdditionalConstraints` | 親の制約に追加で課す制約であることを名前で示す | [CHANGELOG.md](../../CHANGELOG.md) |

### 書き方が変わるもの

| Flutter | FloatSoda | 理由 | 詳細 |
| :-- | :-- | :-- | :-- |
| 子と設定を、コンストラクタの引数で渡す | 子は `Child` か `Children`(`IList<Widget>`)、設定は各プロパティに、オブジェクト初期化子で渡す。名前付きコンストラクタは持たない(`Icon` のように、必須の値をコンストラクタ引数に取る型もある) | C# の流儀に合わせた方針 | [SizedBox](../../samples/Widgets/FloatSoda.Samples.SizedBox/README.md) |
| 色は `Color(0xAARRGGBB)` や `Colors.*` | `Colors.*`、`new Color(r, g, b)`、`Color.FromArgb(0xAARRGGBB)`(どれも `FloatSoda.Geometrics`。成分は各 0〜255 の整数) | 色の型が違う。`Colors` は Material の段階色ではなく、CSS の色名を並べたもの | [ColoredBox](../../samples/Widgets/FloatSoda.Samples.ColoredBox/README.md) |
| `FontWeight.w700` | `FontWeight = 700`(`int`) | 列挙型を持たない | [Text](../../samples/Widgets/FloatSoda.Samples.Text/README.md) |
| `Matrix4` | `Matrix3x2`(2次元のアフィン変換だけ) | 3次元の変換を持たない | [Transform](../../samples/Widgets/FloatSoda.Samples.Transform/README.md) |
| `addListener` | 通知元ごとの `event`(`AnimationController` は `Changed`、`INotifyPropertyChanged` は `PropertyChanged`。`+=` で登録し、`-=` で解除する) | C# の標準の仕組みを使う方針 | [API_DESIGN.md の 3.6](../../API_DESIGN.md) |
| `SizedBox.square()`、`SizedBox.expand()` | `square()` は `Width` と `Height` に同じ値を指定して書く。`expand()` は、`ConstrainedBox` の `AdditionalConstraints` に `BoxConstraints.Expand()` を指定して書く。`SizedBox` の `Width` と `Height` を省略すると、親の制約がそのまま子に渡るだけで、親の制約が緩いときは領域いっぱいに広がらない(`BoxConstraints.TightFor(null, null)`) | 名前付きコンストラクタを持たない | [SizedBox](../../samples/Widgets/FloatSoda.Samples.SizedBox/README.md) |
| `Padding` の `EdgeInsets.only(left: 48)` | `new EdgeInsets(Left: 48)` | 名前付きコンストラクタを持たない | [Padding](../../samples/Widgets/FloatSoda.Samples.Padding/README.md) |

<!-- ※要確認: 改名の一覧に漏れがないか(作例のない型の改名は README から拾えない) -->

### 受け付ける項目が少ないもの

Flutter のウィジェットが取る引数のうち、FloatSoda にないものがあります。書くとコンパイルが通りません。

| ウィジェット | FloatSoda にないもの | 詳細 |
| :-- | :-- | :-- |
| `GestureDetector` | ダブルタップ、長押し、水平ドラッグと垂直ドラッグ、スケール。タップと Pan だけを持つ | [GestureDetector](../../samples/Widgets/FloatSoda.Samples.GestureDetector/README.md) |
| `BoxDecoration` | グラデーション、画像、影、円形(`shape`)。背景色、角丸、ボーダーだけを持つ | [DecoratedBox](../../samples/Widgets/FloatSoda.Samples.DecoratedBox/README.md) |
| `Text` | `softWrap`、`overflow`、`maxLines`、`textAlign` | [Text](../../samples/Widgets/FloatSoda.Samples.Text/README.md) |
| `Flex`、`Row`、`Column` | `spacing`、`clipBehavior`、`textBaseline`。間隔は `SizedBox` を挟んで作る | [Flex](../../samples/Widgets/FloatSoda.Samples.Flex/README.md) |
| `Container` | `margin`、`constraints`、`foregroundDecoration`、`clipBehavior`。外側の余白は `Padding` で包んで作る | [Container](../../samples/Widgets/FloatSoda.Samples.Container/README.md) |
| `Stack` | `clipBehavior`、`Positioned.fill`、`Positioned.fromRect` | [Stack](../../samples/Widgets/FloatSoda.Samples.Stack/README.md) |
| `Opacity` | `alwaysIncludeSemantics` | [Opacity](../../samples/Widgets/FloatSoda.Samples.Opacity/README.md) |
| `Transform` | `Transform.rotate`、`scale`、`translate`、`flip`、`filterQuality` | [Transform](../../samples/Widgets/FloatSoda.Samples.Transform/README.md) |
| `PointerRegion` | `onHover`、`cursor` | [PointerRegion](../../samples/Widgets/FloatSoda.Samples.PointerRegion/README.md) |

文字方向(`textDirection`)による左右の切り替えは、どのウィジェットにもありません。

**できた?** 書き換えたあと、コンパイルが通った。

<details>
<summary>できなかったら</summary>

エラーの読み方は、ガイドの[エラーを読んで直す](../guide/5-troubleshoot/read-errors.md)にあります。C# の書き方が分からないときは、ガイドの[C# がほぼ初めての人へ](../guide/3-prepare/csharp-primer.md)を開いてください。

</details>

コンパイルが通ってから見た目や反応が違うときは、次の節を開きます。

## 挙動が違うもの

コンパイルは通るのに、見た目や反応が Flutter と違うものです。「追跡」の列に Issue がある行は、Flutter に揃える作業が進んでいます。追跡がない行は、Flutter に揃える作業が見つかっていない差異です。

| ウィジェット | 症状 | 直し方 | 追跡 | 詳細 |
| :-- | :-- | :-- | :-- | :-- |
| `Stack` | 範囲を超えた子も、切り取られずに見える | `Stack` を `ClipRect` で包む | #257 | [Stack](../../samples/Widgets/FloatSoda.Samples.Stack/README.md) |
| `Expanded` | `Flex` に 0 以下を指定すると例外になる。主軸の長さが決まらない場所の `Expanded` も、常に例外になる | `Flex` に 1 以上を指定する | なし | [Expanded](../../samples/Widgets/FloatSoda.Samples.Expanded/README.md) |

<!-- ※要確認: 各行の「直し方」が今も有効か。実際に試していない -->

表のとおりに直しても思った見た目にならないときは、ガイドの[レイアウトの考え方](../guide/5-troubleshoot/layout-model.md)を開きます。押しても反応しないときは、[反応しないときに切り分ける](../guide/5-troubleshoot/not-responding.md)です。

## まだないもの・実装しないもの

Flutter にあって FloatSoda で見つからないものは、次の2つのどちらかです。

| 区分 | 該当するもの | 追跡 |
| :-- | :-- | :-- |
| まだない | Material と Cupertino に当たるデザインシステム(`FloatSoda.UI` 系) | #126 |
| まだない | スクロール系(`ListView`、`GridView`、`SingleChildScrollView`) | #69 |
| まだない | `Tween`、`CurvedAnimation` | #137 |
| まだない | `AnimatedContainer` などの暗黙的アニメーション | #163 |
| まだない | `LayoutBuilder` | #174 |
| まだない | `CustomPaint` | #171 |
| まだない | `MediaQuery` に当たるもの(`ViewMetrics`) | #173 |
| まだない | `Notification` | #175 |
| 実装しない | `Semantics`(アクセシビリティ) | 追跡なし |

`Navigator` と `Theme` に当たる型は、型名の検索では `src/` に見つかりませんでした。別名の部品は確かめていません。<!-- ※要確認: Navigator と Theme に当たるものが、別の名前であるのか。src/ で class Navigator と Theme を検索して見つからなかっただけで、別名の部品は確かめていない -->

ないものは、自分で組むか、実装を待つかを選べます。各項目の詳しい状況は、[できないこと・まだ使えないもの](limitations.md)にあります。
