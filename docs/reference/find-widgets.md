← [リファレンス](Home.md)

# 部品を目的から探す

画面を組むときに使う部品の名前を、目的の言葉から引くための一覧です。部品の使い方はここには書いていません。部品の名前が分かったら、作例で動くコードを確かめられます。正確な定義は、API リファレンスのタブが公開されるまでは、`src/` のソースにある XML ドキュメントコメントで引きます(追跡: #219)。

## この一覧の使い方

目的を「並べる」「大きさと余白を決める」「見た目を整える」「文字や画像を出す」「押す・反応する」「動かす」「出し分ける・値を伝える」「出す場所を決める」の8つに分けています。自分の目的に近い節へ進んでください。

各節の表は、部品、違いの1文、作例の3列です。部品の名前は、そのまま `src/` のソースで型名として引けます。<!-- ※要確認: API リファレンスの列を置いていない。リンクの形式が DocFX の markdown 出力の取り込み後の URL で決まるため。決まったら、部品の名前をリンクにするか、列を足す --> 作例の列は、`samples/` にある作例の README へのリンクです。作例がない部品は「なし」と書いています。

この一覧は手で書いています。部品が増えたときは、`src/FloatSoda/Widgets/` の公開された型が正になります。<!-- ※要確認: 目的の分け方(8つ)が読み手の言葉に合っているか。ジュニアコーダーテストで引けるかを確かめる -->

## 並べる

横一列、縦一列、重ね、折り返しで並べます。

| 部品 | 違いの1文 | 作例 |
| :-- | :-- | :-- |
| `Row`、`Column`、`Flex` | 子を横または縦の一列に並べる(`Flex` は `Direction` で向きを選ぶ) | [Flex](../../samples/Widgets/FloatSoda.Samples.Flex/README.md) |
| `Wrap` | 一列に収まらなくなったら、次の行へ折り返して並べる | [Wrap](../../samples/Widgets/FloatSoda.Samples.Wrap/README.md) |
| `Stack`、`Positioned` | 子を重ね、`Positioned` で辺からの距離を指定する | [Stack](../../samples/Widgets/FloatSoda.Samples.Stack/README.md) |
| `IndexedStack` | 子を全部レイアウトし、指定した1つだけを表示する | なし |
| `Expanded` | 一列の余った領域を比率で分け、子を割り当てた領域いっぱいに広げる | [Expanded](../../samples/Widgets/FloatSoda.Samples.Expanded/README.md) |
| `Flexible` | 一列の余った領域を比率で分け、割り当てた領域以下の大きさを子に選ばせる(`Fit` で `Expanded` と同じ動きにも切り替えられる) | [Expanded](../../samples/Widgets/FloatSoda.Samples.Expanded/README.md) |
| `Spacer` | 一列の余った領域のうち、比率で決めた分を空白にする | [Expanded](../../samples/Widgets/FloatSoda.Samples.Expanded/README.md) |

## 大きさと余白を決める

部品の大きさ、余白、位置を決めます。思った大きさにならないときは、ガイドの[レイアウトの考え方](../guide/5-troubleshoot/layout-model.md)を開いてください。

| 部品 | 違いの1文 | 作例 |
| :-- | :-- | :-- |
| `SizedBox` | 親の制約の範囲で、`Width` と `Height` に指定した大きさを子に求める | [SizedBox](../../samples/Widgets/FloatSoda.Samples.SizedBox/README.md) |
| `Padding` | 子の周りに余白を取る | [Padding](../../samples/Widgets/FloatSoda.Samples.Padding/README.md) |
| `Align`、`Center` | 子を、親の中の指定した位置に置く | [Align](../../samples/Widgets/FloatSoda.Samples.Align/README.md) |
| `ConstrainedBox` | 親から受け取った制約に、追加の寸法の制約を重ねる | なし |
| `LimitedBox` | 親の制約に上限がないときだけ、上限を与える | [LimitedBox](../../samples/Widgets/FloatSoda.Samples.LimitedBox/README.md) |
| `FractionallySizedBox` | 親の制約の最大値に `WidthFactor`・`HeightFactor` を掛けた大きさを、子に求める | [FractionallySizedBox](../../samples/Widgets/FloatSoda.Samples.FractionallySizedBox/README.md) |
| `AspectRatio` | 縦横比を保って、子の大きさを決める | [AspectRatio](../../samples/Widgets/FloatSoda.Samples.AspectRatio/README.md) |
| `FittedBox` | 子を拡大または縮小して、領域に収める | [FittedBox](../../samples/Widgets/FloatSoda.Samples.FittedBox/README.md) |
| `OverflowBox` | 親とは別の最小・最大の制約を子に渡し、自身の領域からのはみ出しを許す | [OverflowBox](../../samples/Widgets/FloatSoda.Samples.OverflowBox/README.md) |
| `SizedOverflowBox` | 自身は `Size` で指定した大きさになり、子には親から受け取った元の制約を渡す | なし |
| `ConstraintsTransformBox` | 親から受け取った制約を加工して、子に渡す | [ConstraintsTransformBox](../../samples/Widgets/FloatSoda.Samples.ConstraintsTransformBox/README.md) |
| `UnconstrainedBox` | `ConstrainedAxis` で指定した軸以外の親の制約を外し、子を自然な大きさで置く | [ConstraintsTransformBox](../../samples/Widgets/FloatSoda.Samples.ConstraintsTransformBox/README.md) |
| `IntrinsicWidth`、`IntrinsicHeight` | 子の自然な大きさに合わせて、幅または高さを決める(計測の負荷が高いので、多用しない) | なし |

## 見た目を整える

色、角丸、透明、回転、切り抜きを付けます。まとめて整える手順は、ガイドの[見た目を整える](../guide/4-build/screen/style-appearance.md)にあります。

| 部品 | 違いの1文 | 作例 |
| :-- | :-- | :-- |
| `ColoredBox` | 領域を1色で塗りつぶす | [ColoredBox](../../samples/Widgets/FloatSoda.Samples.ColoredBox/README.md) |
| `DecoratedBox` | 背景色、角丸、ボーダーを描く | [DecoratedBox](../../samples/Widgets/FloatSoda.Samples.DecoratedBox/README.md) |
| `Container` | 配置・余白・装飾・大きさ・変換をまとめて指定する | [Container](../../samples/Widgets/FloatSoda.Samples.Container/README.md) |
| `Opacity` | 子をまとめて半透明にする | [Opacity](../../samples/Widgets/FloatSoda.Samples.Opacity/README.md) |
| `Transform` | 子を、2次元の行列で変形して描く | [Transform](../../samples/Widgets/FloatSoda.Samples.Transform/README.md) |
| `RotatedBox` | 子を、レイアウトの大きさごと、90度単位で回転する | なし |
| `ClipRect`、`ClipRoundRect`、`ClipOval`、`ClipCustomPath` | 子を、四角、角丸の四角、楕円、任意の形で切り抜く | [Clip](../../samples/Widgets/FloatSoda.Samples.Clip/README.md) |
| `RepaintBoundary` | 子の再描画を、周りとは別の単位に分ける | なし |

## 文字や画像を出す

文字、画像、アイコンを出します。

| 部品 | 違いの1文 | 作例 |
| :-- | :-- | :-- |
| `Text` | 1つの書式で文字列を出す | [Text](../../samples/Widgets/FloatSoda.Samples.Text/README.md) |
| `RichText` | 書式つきの文字列(`TextSpan`)を段落として出す | [Text](../../samples/Widgets/FloatSoda.Samples.Text/README.md) |
| `Image` | 画像ファイルを読み込んで出す | [Image](../../samples/Widgets/FloatSoda.Samples.Image/README.md) |
| `Icon` | アイコンフォントの字を、正方形の領域に出す | なし |

`Text` には、折り返しと省略を指定する項目がありません。詳しくは、[できないこと・まだ使えないもの](limitations.md#見た目と文字)を開いてください。<!-- ※要確認: Icon が利用者向けの公開 API として使えるか。src/FloatSoda/Widgets/Paint/Icon.cs は public sealed record で、CHANGELOG.md の 0.4.0 も追加と書く。AGENTS.md の記述は internal のスタブで食い違うため、コードと CHANGELOG に従った。IconData と FontProvider の指定でグリフが出ることは実行して確かめていない -->

## 押す・反応する

ボタン、ホバー、ドラッグを作ります。ボタンの部品はありません。`GestureDetector` で押された操作を受け取り、見た目は他の部品で組み立てます。

| 部品 | 違いの1文 | 作例 |
| :-- | :-- | :-- |
| `GestureDetector` | タップとドラッグを検出する | [GestureDetector](../../samples/Widgets/FloatSoda.Samples.GestureDetector/README.md) |
| `Listener` | 押した、離した、動いた、入った、出たという生のポインターイベントを受け取る | [Listener](../../samples/Widgets/FloatSoda.Samples.Listener/README.md) |
| `PointerRegion` | ポインターが領域に入ったこと、出たことを受け取る | [PointerRegion](../../samples/Widgets/FloatSoda.Samples.PointerRegion/README.md) |
| `AbsorbPointer` | 子を描いたまま、ポインター入力を自分で止めて子に届けない | なし |
| `IgnorePointer` | 子を描いたまま、子の部分をポインター入力の対象から外す | なし |

ポインターの座標が届くのは、ダッシュボードのオーバーレイだけです。詳しくは、[できないこと・まだ使えないもの](limitations.md#置く場所と入力)を開いてください。ボタンの組み立て方は[ボタンやトグルを組み立てる](../guide/4-build/screen/build-buttons-toggles.md)、コントローラーのボタンやトリガーは[コントローラーの入力を受け取る](../guide/4-build/input/controller-input.md)にあります。

## 動かす

値が変わったら画面を変えます。時間で値を動かします。

| 部品 | 違いの1文 | 作例 |
| :-- | :-- | :-- |
| `StatefulWidget<T>`、`State<T>`、`SetState()` | `State<T>` が持つ値を `SetState()` に渡した処理で書き換え、画面を描き直す | [CounterWidget.cs](../../samples/Showcase/FloatSoda.Samples.OverlayApp/CounterWidget.cs) |
| `AnimationController` | 時間に合わせて、0 から 1 の値を動かす | [PulseWidget.cs](../../samples/Showcase/FloatSoda.Samples.OverlayApp/PulseWidget.cs) |
| `FadeTransition` | アニメーションの値で、子の透明度を動かす | [PulseWidget.cs](../../samples/Showcase/FloatSoda.Samples.OverlayApp/PulseWidget.cs) |

使えるアニメーションの範囲は、[できないこと・まだ使えないもの](limitations.md#動きと状態)にあります。仕組みは、ガイドの[状態と作り直し](../guide/4-build/mechanics/state-and-rebuild.md)と[アニメーションを付ける](../guide/4-build/screen/add-animation.md)で説明しています。

## 出し分ける・値を伝える

条件で出し分けます。離れた部品へ値を渡します。

| 部品 | 違いの1文 | 作例 |
| :-- | :-- | :-- |
| `Visibility` | 表示するかどうかで、子か代わりの部品のどちらか一方を出す | なし |
| `Offstage` | 子をレイアウトしたまま、描画とポインター入力から外す | なし |
| `TaskBuilder<T>` | `Task<T>` の進み具合(読み込み中、完了、失敗)で、出す部品を切り替える | なし |
| `Builder` | 自分の位置の構築コンテキストで、部品を組み立てる | なし |
| `InheritedWidget` | 子孫の部品が参照できる値を持ち、値が変わると参照している部品を組み立て直す | なし |
| `DefaultTextStyle` | 木の下にある `Text` へ、文字の書式を伝える | [Text](../../samples/Widgets/FloatSoda.Samples.Text/README.md) |
| `ListenableBuilder` | `INotifyPropertyChanged` の通知のたびに、`ChildBuilder` で部品を組み立て直す | なし |

## 出す場所を決める

オーバーレイを SteamVR のどこに出すかを、ウィンドウの種類で決めます。`CreateWindow` に渡します。

| 部品 | 違いの1文 | 作例 |
| :-- | :-- | :-- |
| `DashboardWindow` | SteamVR のダッシュボードのタブに出し、ポインターの座標が届く | [GettingStarted](../../samples/Showcase/FloatSoda.Samples.GettingStarted/Program.cs) |
| `WorldSpaceWindow` | 空間の決まった位置(メートル単位)に固定して出す | [OverlayApp](../../samples/Showcase/FloatSoda.Samples.OverlayApp/Program.cs) |
| `DeviceTrackedWindow` | HMD やコントローラーなどのデバイスに付いて動く | [OverlayApp](../../samples/Showcase/FloatSoda.Samples.OverlayApp/Program.cs) |
| `DesktopWindow` | PC のデスクトップのウィンドウに出す | [Flex](../../samples/Widgets/FloatSoda.Samples.Flex/Program.cs) |

SteamVR 上のオーバーレイでポインターの座標が届くのは、ダッシュボードだけです。<!-- ※要確認: `DesktopWindow` に `GestureDetector` の入力が届くか。src/FloatSoda.Engine/DesktopWindow.cs は GLFW のポインター入力源を持つが、ウィジェットまで届くことは実行して確かめていない -->種類の選び方は、ガイドの[オーバーレイの種類と選び方](../guide/4-build/mechanics/overlay-types.md)にあります。

## 見つからないとき

目的に合う部品が見つからないときは、理由が3つ考えられます。

- まだない: スクロール系の部品や `Button` などは未公開。[できないこと・まだ使えないもの](limitations.md)で確かめる
- Flutter の名前で探している: 名前が変わった部品がある。[Flutter の読み替え表](flutter-mapping.md)で、Flutter の名前から引く
- 部品を組み合わせて作る: 表にない形の画面は、表の部品を入れ子にして作る。AI に書かせる手順は、ガイドの[AI に FloatSoda を書かせる](../guide/3-prepare/write-with-llm.md)にある

自分で新しい部品を作る場合は、`StatelessWidget`、`StatefulWidget<T>`、`LeafRenderObjectWidget<T>`、`ParentDataWidget<T>` を継承します。
