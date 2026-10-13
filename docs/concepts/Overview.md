# FloatSoda の考え方

FloatSoda では、オーバーレイに出す UI を「今の状態ならこう見える」という完成形として C# のコードで書きます。画面を作る作業と更新する作業は、フレームワークが受け持ちます。このページは、その考え方と、書いたコードが VR の中に表示されるまでの全体像を説明します。各トピックの詳しい説明へは、末尾の [次に読むページ](#次に読むページ) から進めます。

## 宣言的 UI とは

FloatSoda の UI は宣言的に書きます。宣言的とは、「ラベルの文字を書き換える」のような操作の手順を書く代わりに、「今の状態ならこう見える」という完成形をコードで返す書き方です。状態が変わったら、新しい完成形をもう一度返します。前の画面との違いを調べて画面へ反映するのは、フレームワークの役目です。

UI はすべて C# のコードで書きます。シーン、プレハブ、UI を定義する外部ファイルはありません。そのため、UI の変更を diff で読み、レビューし、LLM に書かせることができます(→ [TargetUsers](../TargetUsers.md))。

## Widget は UI の設計図

画面は Widget の入れ子で組み立てます。Widget は「この位置に何を、どんな設定で置くか」を表す設計図です。文字、余白、並べ方、タップの受け取りも、それぞれ1つの Widget で表します。

Widget は C# の `record` で、作ったあとに値を変えられません。見た目を変えたいときは、Widget を書き換える代わりに、新しい値で Widget を作り直します。作り直しても、画面上の部品は条件が合えば使い回されます。その条件は [Widget](Widget.md) と [State](State.md) で説明します。

## 三つのツリー

FloatSoda は、Flutter と同じく三つのツリーで UI を扱います。利用者がコードで書くのは Widget ツリーだけで、残りの2つはフレームワークが Widget ツリーから作ります。

| ツリー | 何を表すか | 利用者との関わり |
|---|---|---|
| Widget ツリー | コードで組み立てた設計図の入れ子 | 書く。自作の Widget の `Build` が返す |
| Element ツリー | 画面に今ある部品の入れ子。フレームをまたいで残り、`State` を持つ | 直接は触らない。`Build` に渡される `IBuildContext` が、この部品を指す |
| RenderObject ツリー | 大きさと位置を計算し、絵を描く部品の入れ子 | 通常は触らない。[Layout](Layout.md) の制約はここで解かれる |

Widget は毎回作り直す使い捨ての値で、Element と RenderObject は残り続ける部品です。この分担があるので、Widget を作り直しても、押した回数のような状態は Element 側の `State` に残ります。三つのツリーが内部でどう連携しているかは、コントリビューター向けの [Architecture](../contributor/Architecture.md) にあります。

## オーバーレイに表示されるまで

書いた Widget ツリーは、次の順でオーバーレイに表示されます。

1. `FloatSodaApp.CreateWindow` にウィンドウ(`DashboardWindow` など)を渡す。ウィンドウは Widget ツリーの根になる
2. 最初のフレームで、Widget ツリー全体から部品を作り、大きさと位置を計算して描く
3. 描いた絵を SteamVR のオーバーレイへ渡し、VR の中に表示する
4. 以降のフレームでは、`SetState` などで変更があった部分だけを作り直して描き直す。変更がないフレームでは何もしない

ウィンドウの作り方を含む最小のアプリは、[`samples/FloatSoda.Samples.GettingStarted/Program.cs`](../../samples/FloatSoda.Samples.GettingStarted/Program.cs) にあります。ウィンドウの種類と VR の中での大きさは [Window](Window.md) で、`Build` がどのスレッドで呼ばれるかは [Threading](Threading.md) で説明します。

## Unity / uGUI からの読み替え

uGUI の考え方は、次のように読み替えます。どの行にも共通する違いは、エディタで配置する代わりにコードで書く点です。

| uGUI | FloatSoda | 違う点 |
|---|---|---|
| Hierarchy の GameObject の入れ子 | Widget ツリー | コードの入れ子が、そのまま階層になる |
| Canvas | ウィンドウ(`DashboardWindow` など) | 1つのウィンドウが、SteamVR のオーバーレイ1つに対応する |
| RectTransform のアンカーと位置 | レイアウトの Widget(`Align`、`Padding`、`Row` など) | 部品は自分の位置を持たない。親の Widget で包んで位置を決める |
| Prefab | `StatelessWidget` か `StatefulWidget` を継承した自作の型 | 再利用の単位がコード上の型になる |
| `text.text = ...` のように部品を書き換える | `State` のフィールドを `SetState` の中で変える | 部品を直接書き換えない。`Build` が新しい完成形を返す |
| 毎フレーム呼ばれる `Update()` | 変更があったときだけ呼ばれる `Build` | 毎フレーム値を変える処理は [Animation](Animation.md) で書く |

Flutter を知っている読み手は、Widget、State、三つのツリーの関係を Flutter と同じものとして読めます。FloatSoda 固有の違いは、各ページの「制約と未実装」と、[Widget カタログ](../user/Home.md) の各サンプルの「Flutterとの違い」節にあります。

## 次に読むページ

次の順に読むと、ここで触れた概念を1つずつ詳しく追えます。

1. [Widget](Widget.md) — Widget の性質、自作の Widget の書き方、組み込み Widget の種類、色の指定
2. [State](State.md) — 状態の持ち方と、`SetState` で何が作り直されるか
3. [Layout](Layout.md) — 親子の間で大きさと位置が決まる仕組み
4. [Input](Input.md) — ポインタ、ジェスチャ、コントローラーのアクション入力
5. [Animation](Animation.md) — 時間とともに値を変える仕組み
6. [Window](Window.md) — オーバーレイの種類と VR の中での大きさ
7. [Threading](Threading.md) — メインスレッドと非同期処理の決まり
