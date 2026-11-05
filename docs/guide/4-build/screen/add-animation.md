← [ガイド](../../Home.md)

# アニメーションを付ける

## この手順でできること

画面の一部を、ふわっと出したり点滅させたりするには、`AnimationController` と `FadeTransition` で、子の不透明度を時間で変えます。フレームごとに `SetState()` を呼ぶ必要はありません。この手順を終えると、HMD の中で四角がフェードインとフェードアウトを繰り返します。片道は 1.5 秒です。

この手順で扱うのは、不透明度を変えるアニメーションです。移動や拡大に使える専用のウィジェットは、まだありません(追跡: #219)。目的が移動や拡大の場合は、[できないこと・まだ使えないもの](../../../reference/limitations.md)で、今の範囲を確かめます。

## 前提条件

- `StatefulWidget` と `SetState()` で値を変えたことがあること。なければ、[最初のオーバーレイを作る](../../../tutorials/first-overlay.md)に戻る
- 動かしたい部分が、ウィジェットとして書けていること

## 手順1: アニメーションを動かす State を作る

アニメーションを動かす `State` は、`State<T>` の代わりに `TickerProviderState<T>` を継承します。`TickerProviderState<T>` は、フレームごとの時刻の知らせを、アニメーションに渡す役目を持ちます。

```csharp
public record BlinkBox : StatefulWidget<BlinkBox>
{
    public override State<BlinkBox> CreateState() => new BlinkBoxState();
}

public class BlinkBoxState : TickerProviderState<BlinkBox>
{
    public override Widget Build(IBuildContext context) => new Align
    {
        Alignment = Alignment.Center,
        Child = new SizedBox
        {
            Width = 200,
            Height = 200,
            Child = new ColoredBox { Color = Colors.MediumSlateBlue },
        },
    };
}
```

`using` には、`FloatSoda.Animation`、`FloatSoda.Elements`、`FloatSoda.Geometrics`、`FloatSoda.Widgets`、`FloatSoda.Widgets.Animation`、`FloatSoda.Widgets.Layout`、`FloatSoda.Widgets.Paint` を足します。

起動のコードで、ウィンドウの `Child` を `new BlinkBox()` に置き換えます。

**できた?** コンパイルが通れば、手順1は完了です。

## 手順2: AnimationController を作って動かす

`AnimationController` は、`InitState()` で作ります。`Vsync` には `this` を、`Duration` には片道にかける時間を渡します。作ったあとに `Forward()` を呼ぶと、動き始めます。

```csharp
private AnimationController? _controller;

public override void InitState()
{
    _controller = new AnimationController
    {
        Vsync = this,
        Duration = TimeSpan.FromSeconds(1.5),
    };

    _controller.Forward();
}
```

変化の速さを変えたいときは、`Curve` に `Curves.EaseInOut` のような値を渡します。次のコードは、`InitState()` にある `_controller` を作る部分を置き換えます。

```csharp
_controller = new AnimationController
{
    Vsync = this,
    Duration = TimeSpan.FromSeconds(1.5),
    Curve = Curves.EaseInOut,
};
```

`AnimationController` の解放の条件は、次のとおりです。

- 作成した `AnimationController` は、`State` の `Dispose()` で解放すること

`State` の `Dispose()` に書くと、ウィジェットが画面から外れるときに解放されます。

```csharp
public override void Dispose()
{
    _controller?.Dispose();
    base.Dispose();
}
```

**できた?** 実行して、例外が出なければ、手順2は完了です。この段階では画面は変わりません。コントローラーが動いているかは、手順3のフェードインで確かめます。

<details>
<summary>できなかったら</summary>

`Vsync` や `Duration` を書き忘れると、コンパイルが通りません。どちらも必須です。例外が出るときは、[エラーを読んで直す](../../5-troubleshoot/read-errors.md)で原因を切り分けます。

</details>

## 手順3: FadeTransition で子に反映する

動かしたい部分を `FadeTransition` で包み、`Opacity` にコントローラーを渡します。`FadeTransition` は、`SetState()` なしで、描画だけを更新します。

```csharp
public override Widget Build(IBuildContext context) => new FadeTransition
{
    Opacity = _controller!,
    Child = new Align
    {
        Alignment = Alignment.Center,
        Child = new SizedBox
        {
            Width = 200,
            Height = 200,
            Child = new ColoredBox { Color = Colors.MediumSlateBlue },
        },
    },
};
```

不透明度を変えない固定値だけでよい場合は、`Opacity` ウィジェットを使います。

**できた?** HMD の中で、四角が1.5秒かけてフェードインすれば、手順3は完了です。フェードインのあとは、そのまま止まります。

<!-- ※要確認: HMD の中でフェードインが見えること。実機で確かめる -->

## 手順4: 往復させる

往復させるには、コントローラーの `StatusChanged` で、終わりに着いたら戻し、始めに戻ったら進め直します。`Completed` なら `Reverse()`、`Dismissed` なら `Forward()` を呼びます。

次のコードは、手順2の `InitState()` にある `_controller.Forward();` の1行を置き換えます。

```csharp
_controller.StatusChanged += status =>
{
    switch (status)
    {
        case AnimationStatus.Completed:
            _controller.Reverse();
            break;
        case AnimationStatus.Dismissed:
            _controller.Forward();
            break;
    }
};

_controller.Forward();
```

`StatusChanged` の購読は、`Forward()` を呼ぶ前に書きます。書き終えた `InitState()` と `Dispose()` は、[サンプルの PulseWidget.cs](../../../../samples/Showcase/FloatSoda.Samples.OverlayApp/PulseWidget.cs) と同じ形です。

**できた?** フェードインとフェードアウトが、止まらずに続けば、手順4は完了です。

## 結果を確かめる

HMD で、次の2点を見ます。

- 四角が、薄くなったり濃くなったりを、繰り返している
- しばらく見ていても、途中で止まらない

HMD を被らずに確かめたいときは、[HMD を被らずに確かめる](../input/test-without-hmd.md)に進みます。作った部品を押せるようにするには、[ボタンやトグルを組み立てる](build-buttons-toggles.md)に進みます。

## うまくいかないとき

動かない、または途中で止まるときは、症状に合うページに進みます。画面が反応しない場合は[反応しないときに切り分ける](../../5-troubleshoot/not-responding.md)、動作が重い場合は[負荷を抑える](../../5-troubleshoot/reduce-load.md)です。
