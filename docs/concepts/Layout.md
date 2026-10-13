# Layout — 制約・サイズ・配置

FloatSoda のレイアウトでは、親が子へ「この範囲の大きさにする」という制約を渡し、子がその範囲の中で自分の大きさを決めて親へ返し、最後に親が子の位置を決めます。この1往復は Flutter と同じです。このページは、この仕組みと、並べる・重ねる・制約を変える Widget の役割を説明します。

## 概要

Widget は、自分の大きさを自由には決められません。親から受け取った制約の範囲に収まる大きさだけを選べます。たとえば `SizedBox { Width = 90 }` を置いても、親が「幅はちょうど 300」という制約を渡せば、幅は 300 になります。

位置も、子は自分で決めません。子の位置を決めるのは親です。そのため Widget には X 座標や Y 座標のプロパティがありません。位置を変えたいときは、`Align`、`Padding`、`Positioned` のような、子の位置を決める Widget で包みます。

## いつ使うか

このページの考え方は、次の場面で使います。

- 思った大きさにならない、はみ出す、レイアウト中に例外が出るときに、原因を調べる
- 並べる、重ねる、大きさを固定する、のどれに当たるかで、使う Widget を選ぶ

## 基本モデル

### 制約は下へ、大きさは上へ、位置は親が決める

制約は `BoxConstraints` で表します。`BoxConstraints` は、幅と高さそれぞれの最小値と最大値(`MinWidth`、`MaxWidth`、`MinHeight`、`MaxHeight`)を持ちます。単位は論理ピクセルで、最大値には正の無限大も入ります。

制約の形によって、子が選べる大きさが変わります。

| 制約の形 | 条件 | 子が選べる大きさ |
|---|---|---|
| tight | 最小値と最大値が等しい | その1つだけ。子が指定した大きさは無視される |
| loose | 最小値が 0 | 0 から最大値までの好きな大きさ |
| 上限が無限 | 最大値が正の無限大 | 上限なし。子の内容に合わせた大きさを選ぶ |

レイアウトは、毎フレーム計算し直されるわけではありません。Widget の設定が変わったときなど、変更があった部分だけが計算し直されます。

### 根の制約はウィンドウの Size で決まる

Widget ツリーの根に渡る制約は、ウィンドウの `Size` で決まります。`Size` を指定すると、その大きさの tight な制約が渡ります。`Size` を省略すると、上限が無限の制約が渡り、ウィンドウは中身の大きさになります。ウィンドウの大きさの詳しい決まり方は [Window](Window.md) で説明します。

`Size` を省略したウィンドウの直下では、上限が無限になります。そこに、余った幅を分ける `Expanded` を含む `Row` を置くと、余った幅を計算できず、`InvalidOperationException` を投げます。この場合は、ウィンドウに `Size` を指定するか、`Row` を `SizedBox` で包んで幅を有限にします。

### 並べる: Row / Column と Expanded

`Row` は子を横に、`Column` は子を縦に並べます。並べる方向を主軸、それと直角の方向を交差軸と呼びます。

主軸の大きさは、次の順で決まります。

1. `Expanded`、`Flexible`、`Spacer` で包んでいない子を、それぞれの大きさでレイアウトする
2. 主軸の大きさから、1の子の大きさの合計を引いた残りを、余った領域とする
3. 余った領域を、`Expanded`、`Flexible`、`Spacer` の `Flex` の比で分ける

3つの違いは、分けられた領域の使い方です。`Expanded` は子を領域いっぱいに広げます。`Flexible` は領域を上限として子に渡し、子はそれ以下の大きさを選べます。`Spacer` は領域を空白にします。`Flex` には 1 以上の整数を指定します。

`Expanded`、`Flexible`、`Spacer` は、`Row`、`Column`、`Flex` の直接の子として置きます。ほかの Widget の下に置くと、`InvalidOperationException` を投げます。主軸に収まらない子を折り返して並べたいときは、`Wrap` を使います。

`Row` / `Column` と `Flex` は、揃え方を省略したときの既定値が違います。違いは [Flex](../../samples/FloatSoda.Samples.Flex/README.md) のサンプルにあります。

### 重ねる: Stack と Positioned

`Stack` は子を重ねて置きます。`Children` の後ろにある子ほど手前に描かれます。

`Stack` の子の位置は、2つの方法で決まります。`Positioned` で包んでいない子は、`Stack` の `Alignment` と `Fit` に従って置かれます。`Positioned` で包んだ子は、`Stack` の辺からの距離(`Left`、`Top`、`Right`、`Bottom`)や大きさ(`Width`、`Height`)で置かれます。`Positioned` は、`Stack` の直接の子として置きます。

### 制約を変える Widget

親から来た制約を変えてから子へ渡す Widget があります。思った大きさにならないときは、目的に合う Widget で子を包みます。

| 目的 | Widget | サンプル |
|---|---|---|
| 大きさを固定する | `SizedBox` | [SizedBox](../../samples/FloatSoda.Samples.SizedBox/README.md) |
| 最小値と最大値を足す | `ConstrainedBox` | なし |
| 上限が無限の軸だけに上限を付ける | `LimitedBox` | [LimitedBox](../../samples/FloatSoda.Samples.LimitedBox/README.md) |
| 縦横比を保つ | `AspectRatio` | [AspectRatio](../../samples/FloatSoda.Samples.AspectRatio/README.md) |
| 親の大きさに対する割合にする | `FractionallySizedBox` | [FractionallySizedBox](../../samples/FloatSoda.Samples.FractionallySizedBox/README.md) |
| 子を拡大・縮小して収める | `FittedBox` | [FittedBox](../../samples/FloatSoda.Samples.FittedBox/README.md) |
| 制約を外す・はみ出させる | `UnconstrainedBox`、`ConstraintsTransformBox`、`OverflowBox`、`SizedOverflowBox` | [ConstraintsTransformBox](../../samples/FloatSoda.Samples.ConstraintsTransformBox/README.md)、[OverflowBox](../../samples/FloatSoda.Samples.OverflowBox/README.md) |
| 子の内容に合わせた大きさにそろえる | `IntrinsicWidth`、`IntrinsicHeight` | なし |

`IntrinsicWidth` と `IntrinsicHeight` は、通常のレイアウトの前に、子へ内容に合わせた大きさを問い合わせます。この問い合わせは通常のレイアウトとは別にツリーをたどるため、入れ子にすると計算量が最悪で O(N²) になります。大きさが分かっている場合は、`SizedBox` か `ConstrainedBox` を使います。

## 最小例

最小のコード例は、Widget カタログのサンプルにあります。このページの断片より、サンプルのコードを正とします。上の表に挙げたもののほか、次のサンプルが基本の並べ方を示しています。

- [Padding](../../samples/FloatSoda.Samples.Padding/README.md) — 子の周りに余白を付ける
- [Align](../../samples/FloatSoda.Samples.Align/README.md) — 子を親の中の決まった位置に置く
- [Flex](../../samples/FloatSoda.Samples.Flex/README.md) — `Row` / `Column` で並べ、主軸と交差軸の揃え方を変える
- [Expanded](../../samples/FloatSoda.Samples.Expanded/README.md) — 余った領域を比で分ける。`Flexible`、`Spacer` との違い
- [Stack](../../samples/FloatSoda.Samples.Stack/README.md) — 重ねる。`Positioned` で辺からの距離で置く
- [Wrap](../../samples/FloatSoda.Samples.Wrap/README.md) — 収まらない子を折り返す

## Unity / uGUI からの読み替え

uGUI では、各 RectTransform が親のアンカーを基準に自分の矩形を決めます。FloatSoda では、子は親の制約に収まる大きさしか選べず、位置は親が決めます。

| uGUI | FloatSoda | 違う点 |
|---|---|---|
| RectTransform のアンカーと位置 | `Align`、`Padding`、`Stack` と `Positioned` | 子は自分の位置を持たない。包んだ親が位置を決める |
| Horizontal / Vertical Layout Group | `Row` / `Column` | 子の間隔は `MainAxisAlignment` や `SizedBox`、`Spacer` で作る |
| Layout Element の Flexible Width | `Expanded` / `Flexible` の `Flex` | 余った領域だけを比で分ける |
| Layout Element の Preferred Width | `SizedBox` | 親の制約が tight なら、指定した幅は無視される |
| Content Size Fitter | 上限が無限か loose な制約、`IntrinsicWidth` | 中身に合わせるかは、親から来る制約の形で決まる |
| Aspect Ratio Fitter | `AspectRatio` | — |
| Grid Layout Group | `Wrap` | `GridView` は使えない(下の「制約と未実装」) |
| Canvas Scaler | ウィンドウの `Size` と `Dpm` | VR の中での物理的な大きさは [Window](Window.md) で決まる |

## 制約と未実装

- スクロールはできない。`ListView`、`GridView`、`SingleChildScrollView` は `internal` で使えない。ウィンドウに収まらない中身は見えない(追跡: [#69](https://github.com/sumx21t-3310/FloatSoda/issues/69))
- `CrossAxisAlignment.Baseline` は、レイアウト中に `NotImplementedException` を投げる(追跡: [#176](https://github.com/sumx21t-3310/FloatSoda/issues/176))
- 主軸の上限が無限の場所に flex の子を置くと、`FlexFit.Loose` でも常に `InvalidOperationException` を投げる。Flutter では条件によって許される。差異の一覧は [Expanded](../../samples/FloatSoda.Samples.Expanded/README.md) の「Flutterとの違い」にある
- `Size` を省略したウィンドウでは、中身が大きくなっても SteamVR 上の表示範囲が起動時の大きさのまま変わらない([Window](Window.md)。追跡: [#285](https://github.com/sumx21t-3310/FloatSoda/issues/285))

## 関連ページ

- [Widget](Widget.md) — 組み込み Widget の種類
- [Window](Window.md) — ウィンドウの `Size` と、VR の中での大きさ
- [Widget カタログ](../user/Home.md) — 各 Widget の最小の使用例
- [RenderObjects](../contributor/RenderObjects.md) — 制約の計算の内部の仕組み(コントリビューター向け)
