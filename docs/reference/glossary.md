← [リファレンス](Home.md)

# 用語集

FloatSoda の docs や、AI が書いたコードに出てくる言葉の意味を、このページで引けます。言葉を4つの分類に分け、それぞれ意味、出てくる場面、詳しく知るページを並べています。

## この用語集の使い方

分からない言葉は、検索で探します。検索は2つあり、探したいものによって使い分けます。

| 探したいもの | 使う検索 | 結果 |
| :-- | :-- | :-- |
| 用語集に載っている、言葉の意味 | ブラウザーのページ内検索(Ctrl+F) | 用語集の項目にそのまま移る |
| その言葉が、docs のどのページで使われているか | サイトの検索(Ctrl+K。画面の上部の検索欄) | 言葉が出てくるページの一覧が出る。用語集の項目が上に来るとは限らない |

サイトの検索は、公開サイトでだけ使えます。リポジトリの `docs/` を直接読んでいるときは、ページ内検索を使います。<!-- ※要確認: サイトの検索(Pagefind)が、日本語の言葉と、`SetState` のような識別子を正しく拾うか。サイトで検索して確かめていない -->

見出しから、言葉の分類を探すこともできます。言葉は、各分類の中で、docs に出てくる順に並べています。

各項目は、1〜2文の意味にとどめています。詳しい説明は、右端の列のページにあります。意味を読んだら、元のページへ戻ってください。

<!-- ※要確認: 載せる言葉の範囲。AI が書いたコードに出る語を、実際に生成されたコードから拾って確かめていない -->

## FloatSoda と Flutter の言葉

FloatSoda は Flutter に合わせて作ってあるので、Flutter の言葉がそのまま出てきます。Flutter との対応は、[Flutter の読み替え表](flutter-mapping.md)にあります。

| 言葉 | 意味 | 出てくる場面 | 詳しく |
| :-- | :-- | :-- | :-- |
| ウィジェット(Widget) | 画面の構成や設定を宣言する、`Widget` を継承した部品(作ったあとは書き換えず、変えるときは作り直す) | 画面のコードのほぼ全部 | [FloatSoda の仕組み](../guide/4-build/mechanics/how-floatsoda-works.md) |
| Element | ウィジェットの木を実際に管理する、書き換えられる木の節。使う側は意識しなくてよい | エラーメッセージ、仕組みの説明 | [FloatSoda の仕組み](../guide/4-build/mechanics/how-floatsoda-works.md) |
| RenderObject | 大きさの計算と描画を担い、ウィジェットに対応して作られる木の節 | エラーメッセージ、自作の部品 | [FloatSoda の仕組み](../guide/4-build/mechanics/how-floatsoda-works.md) |
| `StatelessWidget` | 更新する状態を持たず、`Build()` で画面を返すウィジェット | 自作の部品 | [状態と作り直し](../guide/4-build/mechanics/state-and-rebuild.md) |
| `StatefulWidget<T>`、`State<T>` | ウィジェットの宣言と、更新する状態を保持するオブジェクト | カウンターなど、値が変わる画面 | [状態と作り直し](../guide/4-build/mechanics/state-and-rebuild.md) |
| `SetState` | 状態を変更する処理を渡し、ウィジェットの再構築を要求する操作 | `OnTap` の中など | [状態と作り直し](../guide/4-build/mechanics/state-and-rebuild.md) |
| `InheritedWidget` | 子孫のウィジェットから参照できる値を持つウィジェット | `DashboardWindow`、`DefaultTextStyle` | [FloatSoda の仕組み](../guide/4-build/mechanics/how-floatsoda-works.md) |
| `Key` | 同じウィジェットかどうかを見分ける印(`ValueKey<T>` と `UniqueKey` がある) | 子のリストを並べ替えるとき | [FloatSoda の仕組み](../guide/4-build/mechanics/how-floatsoda-works.md) |
| 制約(`BoxConstraints`) | 親が子に渡す幅と高さの上限と下限(制約が親から子へ下り、大きさが子から親へ上る) | 思った大きさにならないとき | [レイアウトの考え方](../guide/5-troubleshoot/layout-model.md) |
| 主軸と交差軸 | `Row` や `Column` で、並べる向きと、それに直角な向き | `MainAxisAlignment`、`CrossAxisAlignment` | [Flex の作例](../../samples/Widgets/FloatSoda.Samples.Flex/README.md) |
| ポインター | レーザーポインターとマウスをまとめた呼び方 | `PointerRegion`、`Listener` | [PointerRegion の作例](../../samples/Widgets/FloatSoda.Samples.PointerRegion/README.md) |
| ヒットテスト | ポインターの位置で、どのウィジェットが反応するかを決める処理 | 押しても反応しないとき | [GestureDetector の作例](../../samples/Widgets/FloatSoda.Samples.GestureDetector/README.md) |
| ジェスチャー | ポインターの動きに付けた意味(タップとドラッグ) | `GestureDetector` | [GestureDetector の作例](../../samples/Widgets/FloatSoda.Samples.GestureDetector/README.md) |
| `AnimationController` | 時間に合わせて、0 から 1 の値を動かす部品 | アニメーション | [アニメーションを付ける](../guide/4-build/screen/add-animation.md) |
| `WidgetTicker` | フレームごとに、開始からの経過時間を渡す時計 | アニメーション、チュートリアル | [アニメーションを付ける](../guide/4-build/screen/add-animation.md) |
| `Curve` | アニメーションの動きの緩急(`Curves` に定義済みのものがある) | アニメーション | [アニメーションを付ける](../guide/4-build/screen/add-animation.md) |

## VR と SteamVR の言葉

SteamVR での表示場所、入力、大きさ、アプリの識別に使う言葉です。

| 言葉 | 意味 | 出てくる場面 | 詳しく |
| :-- | :-- | :-- | :-- |
| オーバーレイ | SteamVR が、ゲームなどの画面の上に重ねて出す画面<!-- ※要確認: SteamVR の公式な言い方に合わせてよいか。公式資料で確かめていない --> | FloatSoda が作るものすべて | [オーバーレイとは](../guide/1-discover/what-is-an-overlay.md) |
| ダッシュボード | SteamVR のメニューの画面で、ポインターの座標が届くオーバーレイの置き場所 | `DashboardWindow` | [オーバーレイの種類と選び方](../guide/4-build/mechanics/overlay-types.md) |
| ウィンドウ(`DashboardWindow`、`WorldSpaceWindow`、`DeviceTrackedWindow`) | `CreateWindow` に渡すオーバーレイの置き場所の指定で、ダッシュボード、空間の固定位置、デバイスへの追従から選ぶ | 起動のコード | [オーバーレイの種類と選び方](../guide/4-build/mechanics/overlay-types.md) |
| アクション入力 | コントローラーのボタンやスティックの入力で、SteamVR のバインディングにより割り当てを利用者が変えられる | `InputAction` | [コントローラーの入力を受け取る](../guide/4-build/input/controller-input.md) |
| `Dpm` | 1メートルあたりのドット数で、画像の大きさから実寸を決める(既定は 4000、1ピクセルが 0.25mm)<!-- ※要確認: 実寸の決め方の説明。#151 で指定方法を分かりやすくする作業が open のため、説明が変わりうる --> | `Dpm = new Dpm(1000)` | [オーバーレイの種類と選び方](../guide/4-build/mechanics/overlay-types.md) |
| `AppKey` | SteamVR がアプリを見分ける名前 | `FloatSodaOptions` | [起動と Generic Host](../guide/4-build/mechanics/startup-and-host.md) |
| OpenVR、SteamVR | VR のランタイムである SteamVR と、そのアプリ向けの API である OpenVR | 動作環境、エラーメッセージ | [動作環境](requirements.md) |

## 起動と .NET の言葉

チュートリアルの[最初のオーバーレイを作る](../tutorials/first-overlay.md)にある、起動のコードに出てくる言葉です。

| 言葉 | 意味 | 出てくる場面 | 詳しく |
| :-- | :-- | :-- | :-- |
| Generic Host(`Microsoft.Extensions.Hosting`) | .NET の起動の仕組み。設定、ログ、依存性注入、バックグラウンド処理をまとめて持つ | `Host.CreateApplicationBuilder`、`host.RunAsync()` | [起動と Generic Host](../guide/4-build/mechanics/startup-and-host.md) |
| 依存性注入(DI) | 必要な部品を、自分で作らず、外から受け取る仕組み | `host.Services.GetRequiredService<FloatSodaApp>()` | [Generic Host の仕組みを使う](../guide/4-build/mechanics/use-generic-host.md) |
| `AddFloatSoda` | FloatSoda を Generic Host に登録する呼び出し | Generic Host のサービス登録 | [起動と Generic Host](../guide/4-build/mechanics/startup-and-host.md) |
| `FloatSodaApp` | `CreateWindow` でウィンドウを作る、FloatSoda の入口 | 起動のコード | [起動と Generic Host](../guide/4-build/mechanics/startup-and-host.md) |
| Skia(SkiaSharp) | 描画のライブラリ。画面のコードでは直接は触らないが、ウィンドウの大きさの `SKSize` のように、型の名前に出ることがある | `SKSize` のような `SK` で始まる型名 | [FloatSoda の仕組み](../guide/4-build/mechanics/how-floatsoda-works.md) |

## C# の言葉

C# の言葉は、名前と意味だけを書いています。詳しい説明は、ガイドの[C# がほぼ初めての人へ](../guide/3-prepare/csharp-primer.md)にあります。

| 言葉 | 意味 |
| :-- | :-- |
| オブジェクト初期化子 | `new SizedBox { Width = 100 }` のように、生成時のプロパティの値を波かっこの中で指定する書き方 |
| `init` | 作るときだけ値を入れられ、そのあとは書き換えられないプロパティ |
| `record` | 値の比較などのメンバーが自動で生成される型。ウィジェットの多くが `record` |
| `with` | `record` を複製し、指定したプロパティだけを変えた新しい値を作る式(参照先のオブジェクトまでは複製しない) |
| ラムダ式 | `() => SetState(...)` のように、名前を付けずに書く小さな処理 |
| `event` | 通知を受け取る処理を登録・解除するための C# の仕組み。Flutter の `addListener` に近い |
