# State — 状態と作り直し

状態は、押した回数や受け取ったメッセージのように、時間とともに変わる値です。FloatSoda では状態を `State<T>` のフィールドに持ち、`SetState` の中で変えると、その `State` の `Build` が次のフレームでもう一度呼ばれます。このページは、作り直される範囲、`State` が残る条件と消える条件、離れた Widget へ値を配る方法を説明します。

## 概要

Widget は作り直される使い捨ての値なので、変わる値を持てません(→ [Widget](Widget.md))。変わる値は、画面上の部品(Element)に結び付いた `State<T>` に置きます。`State<T>` は部品と同じだけ生き続け、Widget が作り直されても値を保ちます。

画面を変えたいときは、`State` のフィールドを `SetState` の中で変えます。フレームワークは次のフレームで `Build` を呼び直し、返ってきた Widget と前回の Widget を比べて、違う部分だけを画面に反映します。

## いつ使うか

状態を表示に結び付ける方法は、値がどこから来るかで選びます。

| やりたいこと | 使うもの |
|---|---|
| タップなどで変わる値を、その Widget の中で持つ | `StatefulWidget<T>` と `State<T>`、`SetState` |
| テーマや設定のように、多くの子が読む値を上から配る | `InheritedWidget` |
| Widget の外にあるオブジェクト(`INotifyPropertyChanged` を実装した ViewModel など)の変化を表示に映す | `ListenableBuilder` |
| 1回で終わる非同期処理(`Task<T>`)の結果を表示する | `TaskBuilder<T>` |

## 基本モデル

### SetState は次のフレームの作り直しを予約する

`SetState(Action)` は、渡した処理をその場で実行し、この `State` の部品に「作り直しが必要」という印を付けます。`Build` が呼ばれるのは、`SetState` の中ではなく、次のフレームです。同じフレームの中で `SetState` を何度呼んでも、`Build` は次のフレームで1回だけ呼ばれます。

`Build` が新しい Widget を返すと、フレームワークは子を1つずつ前回と比べます。

- 前回と等しい(`record` の値が同じ)Widget は、その下の処理ごと省く
- 型と `Key` が同じ Widget は、既存の部品を新しい設定で更新する。部品の `State` は残る
- 型か `Key` が違う Widget は、古い部品を捨てて新しく作る。古い部品の `State` は消える

`State` のフィールドを `SetState` の外で変えても、作り直しは予約されません。値は変わっても、画面は次に何かが作り直されるまで古いままです。`SetState` は `protected` なので、`State` の中から呼びます。

### State の寿命は、ツリーの中の位置と Key で決まる

`State` は、部品が初めて作られたときに `CreateState()` で1つ作られ、部品がツリーから外れるまで生きます。親が作り直されても、同じ位置に同じ型で、`Key` が等しい Widget が置かれる限り、同じ `State` が使われ続けます。

`State<T>` のライフサイクルメソッドは、次のときに呼ばれます。

| メソッド | 呼ばれるとき | 使いどころ |
|---|---|---|
| `InitState()` | 部品に結び付いた直後に1回 | タイマーや購読を始める |
| `DidChangeDependencies()` | 初回の `Build` の直前と、依存する `InheritedWidget` が変わったとき | 上から配られた値に応じた準備をする |
| `DidUpdateWidget(T oldWidget)` | 親が同じ位置に、設定の違う Widget を置いたとき | 新しいプロパティに合わせて、購読を付け替える |
| `Build(IBuildContext)` | 初回と、作り直しが予約された次のフレーム | 今の状態から Widget を返す |
| `Dispose()` | 部品がツリーから外れるときに1回 | `InitState()` で始めたタイマーや購読を止める |

親が新しいプロパティで Widget を作り直しても、`State` は作り直されず、`InitState()` も再び呼ばれません。今の Widget のプロパティは `State.Widget` から読みます。プロパティの変化に応じて処理を変えたいときは、`DidUpdateWidget` に書きます。

`Dispose()` で止めなかったタイマーや購読は、部品が画面から消えたあとも動き続けます。

### InheritedWidget は上から下へ値を配る

`InheritedWidget` は、自分より下にあるすべての Widget へ値を配ります。値を読む側は、引数で値を受け渡す必要がありません。

値を読む側は、`IBuildContext.DependOnInheritedWidgetOfExactType<T>()` で、自分より上にあるいちばん近い `T` を受け取ります。上に `T` がなければ `null` が返ります。この呼び出しは、読んだ Widget を「依存している」として記録します。`InheritedWidget` が新しい値で作り直され、その `UpdateShouldNotify(oldWidget)` が `true` を返すと、依存している Widget だけが作り直されます。依存していない Widget は作り直されません。

組み込みの Widget を使う場合のほか、値の型ごとに自作の `InheritedWidget` を定義することもできます。この使い方の層1サンプルはないため、最小の形を示します。

```csharp
using FloatSoda.Elements;
using FloatSoda.Widgets;

public sealed record AlbumTheme : InheritedWidget
{
    public required string Title { get; init; }

    public static AlbumTheme? Of(IBuildContext context) =>
        context.DependOnInheritedWidgetOfExactType<AlbumTheme>();

    public override bool UpdateShouldNotify(InheritedWidget oldWidget) =>
        oldWidget is AlbumTheme old && old.Title != Title;
}
```

配る値を変えたいときは、`InheritedWidget` より上にある `StatefulWidget` が値を持ち、`SetState` で新しい値の `InheritedWidget` を作り直します。`InheritedWidget` を作ったのと同じ `Build` の中では、その値を読めません。読むときは、`Builder` で `IBuildContext` を1段下げます。

組み込みの `InheritedWidget` には、次のものがあります。

- `DefaultTextStyle` — 下にある `Text` の既定の書式を配る
- `ServiceProvider` — DI コンテナ(`IServiceProvider`)を配る。上にないと `ServiceProvider.Of` は `InvalidOperationException` を投げる
- `WindowWidget` — 自分が載っているウィンドウを配る([Window](Window.md))

### ListenableBuilder は外のオブジェクトの変化を表示に映す

`ListenableBuilder` は、`Listenable` に渡した `INotifyPropertyChanged` を購読します。`PropertyChanged` が発生すると、`ChildBuilder` が返す部分だけを作り直します。プロパティ名では絞り込まないため、どのプロパティの通知でも作り直します。購読は、`ListenableBuilder` がツリーから外れたときと、`Listenable` を別のオブジェクトに差し替えたときに解除されます。

`PropertyChanged` は、`ListenableBuilder` が置かれたスレッド(通常は FloatSoda のメインスレッド)から発生させます。ほかのスレッドから発生させると、`InvalidOperationException` を投げます。OSC の受信のように別のスレッドで届く値の扱いは、[Threading](Threading.md) で説明します。

### TaskBuilder は Task の結果を表示に映す

`TaskBuilder<T>` は、`Task` に渡した `Task<T>` の完了を待ち、完了したら `Builder` を呼び直します。`Builder` は、待っている途中か、値が出たか、例外で終わったかを `TaskSnapshot<T>` で受け取ります。完了後の作り直しは、メインスレッドで行われます。

`Task` は `Build` の外(`InitState` など)で作り、同じ処理には同じ `Task` のインスタンスを渡します。`Build` の中で `Task` を作ると、親が作り直されるたびに処理が最初からやり直しになります。

## 最小例

最小のコード例は、Widget カタログのサンプルにあります。このページの断片より、サンプルのコードを正とします。

- [GestureDetector](../../samples/FloatSoda.Samples.GestureDetector/README.md) — タップで `SetState` を呼び、表示を変える
- [PointerRegion](../../samples/FloatSoda.Samples.PointerRegion/README.md) — ホバー、押下、取り消しの状態を `State` に持って表示する
- [`samples/FloatSoda.Samples.OverlayApp/CounterWidget.cs`](../../samples/FloatSoda.Samples.OverlayApp/CounterWidget.cs) — `StatefulWidget` と `SetState` のカウンター

`InheritedWidget`、`ListenableBuilder`、`TaskBuilder<T>` の層1サンプルはまだありません。

## Unity / uGUI からの読み替え

| Unity / uGUI | FloatSoda | 違う点 |
|---|---|---|
| MonoBehaviour のフィールドを変えて、`text.text = ...` で部品を書き換える | `State` のフィールドを `SetState` の中で変える | 部品を書き換えない。`Build` が新しい完成形を返す |
| `Awake` / `Start` | `InitState()` | 部品に結び付いた直後に1回だけ呼ばれる |
| `OnDestroy` | `Dispose()` | タイマーや購読は自分で止める |
| `Update()` | 該当なし | `Build` は毎フレームは呼ばれない。毎フレームの変化は [Animation](Animation.md) で書く |
| シングルトンや `FindObjectOfType` で共有の値を探す | `InheritedWidget`、`ServiceProvider` | 値を探す範囲は、自分より上にある Widget だけ |
| `SetActive(false)` で隠しても、コンポーネントの値は残る | `Visibility` で隠すと、通常は `State` が消える | `State` を残したいときは `Offstage` か `IndexedStack` を使う |
| C# の event や UnityEvent で変化を受け取る | `INotifyPropertyChanged` と `ListenableBuilder` | 通知はメインスレッドから発生させる |

## 制約と未実装

- `SetState` は排他制御をしない。`System.Threading.Timer` のコールバックや `await` の続きのように、メインスレッド以外から呼ぶと、作り直しと競合するおそれがある。安全な書き方は [Threading](Threading.md) にある(追跡: [#281](https://github.com/sumx21t-3310/FloatSoda/issues/281))
- ほかのスレッドで発生した `PropertyChanged` を、メインスレッドへ自動で戻す公開 API はない ※要確認(追跡する Issue 番号)
- `FloatSoda.Hooks` の `HookWidget` と `UseState` などは使えない。`UseState` などは呼ぶと `NotImplementedException` を投げる(追跡: [#72](https://github.com/sumx21t-3310/FloatSoda/issues/72))

## 関連ページ

- [Widget](Widget.md) — Widget の性質と `Key`
- [Threading](Threading.md) — ほかのスレッドから状態を変えるときの決まり
- [Input](Input.md) — タップを受け取って `SetState` を呼ぶ
- [Animation](Animation.md) — 毎フレーム値を変える
- [BuildPipeline](../contributor/BuildPipeline.md) — 作り直しの内部の仕組み(コントリビューター向け)
