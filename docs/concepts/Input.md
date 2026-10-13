# Input — ポインタ・ジェスチャ・アクション入力

FloatSoda のオーバーレイは、ユーザーの操作を2つの道筋で受け取ります。1つは、レーザーポインターで UI を指して押す **ポインタ入力**です。もう1つは、UI と関係なくコントローラーのボタン・トリガー・スティックを読む **アクション入力**です。ポインタ入力は、今はダッシュボードオーバーレイでだけ届きます。

## 概要

ポインタ入力は、ポインタが指した位置にある Widget へ届きます。`GestureDetector` を置くとタップやドラッグとして受け取れ、`Listener` を置くと押した・動かした・離したという生のイベントを受け取れます。どの Widget に届くかは、FloatSoda がポインタの座標から当たっている Widget を探して決めます。この探索をヒットテストと呼びます。

アクション入力は、Widget ツリーの外で扱います。アプリの起動時に「つかむ」「メニューを開く」のようなアクションを C# で宣言し、どの物理ボタンに割り当てるかは SteamVR が決めます。ポインタがどこを指しているかに関係なく、ボタンを押せば届きます。

## いつ使うか

2つの入力は、操作の対象で使い分けます。

| 使いたい操作 | 使う入力 | 主な型 |
|---|---|---|
| UI のボタンを押す、つまみをドラッグする | ポインタ入力 | `GestureDetector` |
| 押下・移動・離す・ホバーを個別に扱う | ポインタ入力 | `Listener`、`PointerRegion` |
| UI を見ずにショートカットを押す、トリガーの引き量やスティックの傾きを読む | アクション入力 | `InputAction<T>` |

## 基本モデル

### ポインタ入力: ヒットテストで届け先を決める

ポインタが動いたり押されたりするたびに、FloatSoda はその座標に重なる Widget を探します。重なった子が複数あれば、前面に描かれた子から順に調べます。たとえば `Stack` では、後ろに書いた子ほど前面に描かれるため、後ろに書いた子が先に当たります。

当たった Widget と、その祖先にある `GestureDetector` や `Listener` に、イベントが届きます。描画内容を持たない領域(`SizedBox` の空白など)は、既定では当たりません。空白も押せるようにするには、`Behaviour = HitTestBehaviour.Opaque` を指定します。ヒットテストから外したい範囲には `IgnorePointer` を、子へ届けたくない範囲には `AbsorbPointer` を置きます。

`GestureDetector` の `OnTap` は、押して指を離したときに1回だけ呼ばれます。押した瞬間に見た目を変えたいときは、`Listener` の `OnPointerDown` で押下を受け取るか、`PointerRegion` でホバーを受け取ります。

### アクション入力: 宣言は C#、割り当ては SteamVR

アクションは `InputAction<T>` で宣言し、`InputActionMap` にまとめて `FloatSodaOptions.InputActionMaps` に渡します。値の型 `T` は、ボタンなら `bool`、トリガーの引き量なら `float`、スティックなら `System.Numerics.Vector2` です。SteamVR に渡すアクションマニフェストの JSON は FloatSoda が生成するため、JSON を手で書く必要はありません。

`SuggestedPath` や `DefaultBindings` に書いたボタンは、初期の割り当ての提案です。実際の割り当ては SteamVR が持ち、ユーザーは SteamVR のコントローラーバインディング画面でいつでも変えられます。

### コールバックが呼ばれるスレッド

ポインタのコールバック(`OnTap` など)と、アクションのイベント(`OnPerformed`、`OnReleased`)は、どちらも FloatSoda のメインスレッドで呼ばれます。そのため、コールバックの中から `SetState` をそのまま呼べます。スレッドの決まりは [Threading](Threading.md) にあります。

## 最小例

ポインタ入力の最小例は、Widget ごとのサンプルにあります。

- [GestureDetector](../../samples/FloatSoda.Samples.GestureDetector/README.md) — タップとドラッグ
- [Listener](../../samples/FloatSoda.Samples.Listener/README.md) — 生のポインタイベント
- [PointerRegion](../../samples/FloatSoda.Samples.PointerRegion/README.md) — ホバーと押下中の見た目

アクション入力は、次の断片で宣言から受け取りまでがそろいます。

```csharp
using FloatSoda.OVR.Input;

var grab = new InputAction<bool>
{
    Name = "grab",
    SuggestedPath = "/user/hand/right/input/trigger/click",
};

builder.Services.AddFloatSoda(new FloatSodaOptions
{
    AppKey = new AppKey("com.example.myoverlay"),
    InputActionMaps = [new InputActionMap { Name = "main", Actions = [grab] }],
});

grab.OnPerformed += _ => Console.WriteLine("トリガーが引かれた");
grab.OnReleased += () => Console.WriteLine("トリガーが離された");
```

## Unity / uGUI からの読み替え

| Unity | FloatSoda | 違う点 |
|---|---|---|
| `IPointerClickHandler` を実装したコンポーネント | `GestureDetector` の `OnTap` | インターフェースを実装せず、Widget で包んでコールバックを渡す |
| `IPointerDownHandler` / `IPointerEnterHandler` | `Listener` / `PointerRegion` | 同上 |
| `GraphicRaycaster` が前面の UI から当たりを探す | ヒットテストが前面の子から当たりを探す | 「Raycast Target」のチェックの代わりに、`Behaviour`、`IgnorePointer`、`AbsorbPointer` で当たり方を決める |
| Input System の InputActionAsset / InputActionMap / InputAction | `FloatSodaOptions.InputActionMaps` / `InputActionMap` / `InputAction<T>` | 実行中にバインディングを足したり変えたりする API はない。割り当ては SteamVR が持つ |
| `action.performed` / `action.ReadValue<T>()` | `OnPerformed` / `Value` | — |

## 制約と未実装

ポインタ入力が届くのは、`DashboardWindow` と、開発用の `DesktopWindow`(マウス操作)だけです。`WorldSpaceWindow` と `DeviceTrackedWindow` に `GestureDetector` を置くと、コンパイルも実行も通りますが、コールバックは呼ばれません(追跡: [#182](https://github.com/sumx21t-3310/FloatSoda/issues/182))。

アクション入力には、次の制約があります。

- `InputAction<T>` の `T` に `bool`、`float`、`Vector2` 以外の型を使うと、その型を初めて使った時点で例外になる
- 実行中にアクションのバインディングを追加・変更・列挙する API はない
- 同じボタンを VRChat のようなゲーム本体も使っている場合、どちらに届くかは SteamVR のアクションセットの優先度で決まる

オーバーレイアプリはゲーム本体と同時に動くため、ゲームの操作と重なりにくいボタンを `SuggestedPath` に選ぶことが望ましいです。

## 関連ページ

- [〜するには](../user/Guides.md) — ボタンの押せる範囲を広げる手順と、`WorldSpaceWindow` で操作したいときの手順
- [Window](Window.md) — ウィンドウの種類と、それぞれで届く入力
- [State](State.md) — コールバックから `SetState` で表示を変える
- [Threading](Threading.md) — コールバックが呼ばれるスレッド
