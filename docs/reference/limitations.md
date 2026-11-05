← [リファレンス](Home.md)

# できないこと・まだ使えないもの

FloatSoda で作りたいものが今できるかを、このページで確かめられます。使えないものを、置く場所と入力、見た目と文字、動きと状態、起動と終了、部品と UI コンポーネント、既知の不具合の順に並べています。

## このページの読み方

各項目は、制約の内容、起きる症状、今できる回避、追跡先の4つで書いています。解除の見込みは、追跡先の Issue で確かめてください。このページには、日付や「今は」を書いていません。

項目は3種類に分かれます。まだ実装していないもの、仕様として実装しないもの、既知の不具合です。対応方針と進捗は、追跡先の Issue で確かめてください。仕様として実装しないものは、最後の節に分けています。自分の作りたいものに関わる節へ進んでください。

## 置く場所と入力

ポインタの座標が届くのは、ダッシュボードのオーバーレイだけです。ボタンを置く画面は、ダッシュボードに置きます。

| 制約の内容 | 起きる症状 | 今できる回避 | 追跡 |
| :-- | :-- | :-- | :-- |
| ポインタの座標が届くのは、`DashboardWindow` だけ | `WorldSpaceWindow` と `DeviceTrackedWindow` に `GestureDetector` を置いても、コンパイルは通り例外も出ないが、コールバックは呼ばれない | ポインター操作が要る画面は `DashboardWindow` に置き、コントローラーのボタンで操作するならアクション入力を使う | #182 |
| `Size` を省略したウィンドウは、表示範囲が起動時の大きさに固定される | 起動後に中身が大きくなると、増えた部分が SteamVR 上に表示されない | 中身の大きさが変わるウィンドウに `Size` を指定する | #285 |
| `Size` を省略したウィンドウは、ルートの制約に幅の上限がない | 途中の部品が幅の上限を渡さない場合、`Wrap` が折り返さず横一列に並ぶ | `Size` を指定する。または `Wrap` を、幅を決めた `SizedBox` で包む | 追跡先なし(仕様) |
| 実行中にオーバーレイの種類を切り替えられない | `DashboardWindow` から `WorldSpaceWindow` への切り替えのような API がない | 起動時に、使う種類ごとにウィンドウを作る | #90 |

<!-- ※要確認: Size 省略時に Wrap が折り返さないこと。再編前の GettingStarted.md の記述が根拠で、samples/Widgets/FloatSoda.Samples.Wrap で Size を省略して動かして確かめていない。ルートが Loosen(無限上限)の制約でレイアウトされることは、src/FloatSoda/RenderObjects/RenderView.cs のコメントで確かめた。`SizedBox` で包む回避は動かして確かめていない -->

置き場所の選び方は、ガイドの[オーバーレイの種類と選び方](../guide/4-build/mechanics/overlay-types.md)にあります。コントローラーの入力は[コントローラーの入力を受け取る](../guide/4-build/input/controller-input.md)、押しても反応しないときの切り分けは[反応しないときに切り分ける](../guide/5-troubleshoot/not-responding.md)を開いてください。

## 見た目と文字

スクロールする一覧と文字入力は、今のところ作れません。`Text` は、親から受けた幅に収まるように自動で折り返しますが、折り返しと省略の制御はできません。

| 制約の内容 | 起きる症状 | 今できる回避 | 追跡 |
| :-- | :-- | :-- | :-- |
| `ListView`、`GridView`、`SingleChildScrollView` が `internal` | 公開 API として呼べず、コンパイルできない | `Stack` と `Positioned` などで自分で並べ、表示する件数を絞る | #69 |
| 文字入力(TextField)がない | 文字を打ち込む画面を作れない | なし | #107 |
| `Text` に折り返しと省略(`softWrap`、`overflow`、`maxLines`)を指定する項目がない | 幅が有限なら自動で折り返すが、折り返さない指定、省略記号、行数の上限は指定できない(`RenderParagraph.PerformLayout` は親の `MaxWidth` で改行する) | 行を自分で決めたいときは、文字列を自分で分け、`Column` に複数の `Text` を並べる<!-- ※要確認: この回避が動くか。実際に試していない --> | 追跡先なし<!-- ※要確認: 折り返しと省略の Issue が見つからない。未完了の Issue 一覧に該当がなかった --> |

回避のコードは、ガイドの[AI に FloatSoda を書かせる](../guide/3-prepare/write-with-llm.md)で AI に書かせられます。

## 動きと状態

アニメーションは `AnimationController`、`Curves`、`FadeTransition` で組めます。未実装のものは、次の表にまとめています。

| 制約の内容 | 起きる症状 | 今できる回避 | 追跡 |
| :-- | :-- | :-- | :-- |
| `Tween`、`CurvedAnimation`、暗黙的アニメーション(`AnimatedContainer` 系)、スプリングの物理シミュレーションがない | Flutter の同名の部品を書いても、コンパイルできない | `AnimationController` の値(0 から 1)を、自分の式で変換して使う | #137、#163、#164 |
| バックグラウンドのスレッドから `SetState` を呼べない | 画面の更新が失われる、またはクラッシュする | フレームごとに呼ばれる `WidgetTicker` の中など、メインのスレッドから呼ぶ(例は、チュートリアルの[数字を表示して、書き換える](../tutorials/first-overlay.md#2-数字を表示して書き換える)) | #150、#281 |

アニメーションの付け方は、ガイドの[アニメーションを付ける](../guide/4-build/screen/add-animation.md)にあります。

## 起動と終了

SteamVR が起動していない状態でアプリを起動できません。ウィンドウ1つだけを閉じる API もありません。

| 制約の内容 | 起きる症状 | 今できる回避 | 追跡 |
| :-- | :-- | :-- | :-- |
| SteamVR の起動が前提。遅延接続と再接続を扱えない | SteamVR が起動していない状態では、アプリを起動できない(`--desktop` でも OpenVR を初期化するため同じ) | SteamVR を先に起動してから、アプリを起動する | #140、#233 |
| ウィンドウを閉じる API がない | 作ったウィンドウを、アプリの実行中に消せない | なし(アプリごと終了する) | #218 |

起動の作りは、ガイドの[SteamVR と一緒に起動する](../guide/6-distribute/launch-with-steamvr.md)で確かめられます。

## 部品と UI コンポーネント

`Button` のような UI コンポーネントは、NuGet に出していません。ボタンは、`GestureDetector` で自分で組み立てます。スライダー、ダイアログ、トースト(通知として出して消す部品)の組み立て方は、確かめていません。

| 制約の内容 | 起きる症状 | 今できる回避 | 追跡 |
| :-- | :-- | :-- | :-- |
| `FloatSoda.UI`、`FloatSoda.UI.Cream`、`FloatSoda.UI.FizzyPop` を NuGet に出していない(`IsPackable=false`) | `Button`、スライダー、ダイアログなどの部品がない | ボタンは `GestureDetector` と `ColoredBox` などで組み立てる(例は、チュートリアルの[ボタンを押して、数字を増やす](../tutorials/first-overlay.md#3-ボタンを押して数字を増やす))が、スライダーとダイアログの組み立て方は未確認 | #126、#102、#78、#100 |
| `FloatSoda.Hooks` を NuGet に出しておらず、`UseState` などの補助メソッドが `NotImplementedException` を投げる | Hooks を使う書き方は、実行時に例外になる | `StatefulWidget<T>` と `State<T>` で書く | #72 |
| 通知として出して消す部品(トースト)がない | 組み込みの通知用の部品を使えない | なし<!-- ※要確認: オーバーレイの表示と非表示を切り替える手段は OVR 層にあるが、ウィジェット層からの出し入れの手順を確かめていない --> | #123、#74 |

ボタンの組み立て方は、ガイドの[ボタンやトグルを組み立てる](../guide/4-build/screen/build-buttons-toggles.md)にあります。

## 既知の不具合

不具合は、[Issue](https://github.com/sumx21t-3310/FloatSoda/issues) で追跡しています。直ったかどうかは、Issue で確かめてください。

ここに載っていない症状は、ガイドの[反応しないときに切り分ける](../guide/5-troubleshoot/not-responding.md)か[エラーを読んで直す](../guide/5-troubleshoot/read-errors.md)を開きます。新しい不具合は、[Issue](https://github.com/sumx21t-3310/FloatSoda/issues)で知らせてください。

Flutter と挙動が違う箇所の一覧は、[Flutter の読み替え表](flutter-mapping.md#挙動が違うもの)にあります。差異を解消する作業の追跡先は、#224 です。

## 実装しないと決めたもの

スクリーンリーダー向けの情報を扱う、Flutter の `Semantics` に当たる機能は、実装しません。VR のオーバーレイのテクスチャに、スクリーンリーダーへ接続する出力先がないためです。解除の条件は決まっていません。<!-- ※要確認: 実装しない理由の出典。Semantics を移植しない方針は REVIEW.md の「Flutter parity」で確かめたが、「出力先がない」という理由の記述は REVIEW.md に見当たらない -->
