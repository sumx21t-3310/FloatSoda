# Animation — 時間で値を変える

FloatSoda のアニメーションは、`AnimationController` が時間の経過に合わせて 0.0 から 1.0 までの値を進め、その値を Widget の見た目に結び付ける仕組みです。値はフレームごとに `Ticker` が進め、`Curve` が進み方の緩急を決めます。

## 概要

`AnimationController` は、指定した `Duration` をかけて値を下限から上限へ進める部品です。`Forward()` で上限へ、`Reverse()` で下限へ進み、`Stop()` でその場に止まります。値が変わると `Changed` イベントが、再生の状態が変わると `StatusChanged` イベントが発生します。

この値を見た目に結び付けるのが `FadeTransition` のような Widget です。`FadeTransition` に `AnimationController` を渡すと、値が変わるたびに子の不透明度が変わります。`SetState` を毎フレーム呼ぶ必要はありません。

## いつ使うか

時間に沿って見た目を変えたいときに使います。たとえば、通知のトーストをフェードインさせる、オーバーレイの一部を呼吸するように明滅させる、といった場面です。

ボタンを押した瞬間に色を切り替えるだけなら、アニメーションは要りません。`SetState` で状態を変えれば、次のフレームで表示が変わります([State](State.md))。

## 基本モデル

### 値はフレームごとに進む

`AnimationController` は自分では時計を持ちません。`Vsync` に渡した `ITickerProvider` から `Ticker` を作り、`Ticker` がフレームごとに「再生を始めてからの経過時間」を届けます。`AnimationController` はその経過時間を `Duration` で割り、`Curve` を通して値に変えます。

`Ticker` はフレームに合わせて動くため、値が進む細かさは `FloatSodaOptions.TargetFrameRate` で決まります。再生していない間は、`Ticker` はフレームを要求しません。

`Vsync` には、ふつうは `TickerProviderState<T>` を継承した `State` 自身(`this`)を渡します。`TickerProviderState<T>` は、`State<T>` に `ITickerProvider` の役割を足した基底クラスです。

### 状態は4つ

`Status` は、値の位置と進む向きを `AnimationStatus` で表します。

| `Status` | 意味 |
|---|---|
| `Dismissed` | 下限で止まっている |
| `Forward` | 上限へ向かって進んでいる |
| `Reverse` | 下限へ向かって進んでいる |
| `Completed` | 上限で止まっている |

往復させたいときは、`StatusChanged` で `Completed` を受けたら `Reverse()` を、`Dismissed` を受けたら `Forward()` を呼びます。

### Curve は進み方を変える

`Curve` は、時間の進み具合(0.0〜1.0)を値の進み具合に写します。既定は一定の速さで進む `Linear` です。`Curves.EaseInOut` のように、よく使う曲線は `Curves` にそろっています。どの曲線があるかは、[API Reference](../api/Home.md) の `Curves` で確かめます。

`Curves` の一部(`Back` 系と `Elastic` 系)は、途中で 1.0 を超える値や 0.0 を下回る値を返します。`AnimationController` の値は下限と上限の間に収められるため、はみ出した分は表示に出ません。

### 値を見た目に結び付ける

`FadeTransition` は、値が変わったときに描き直しだけを行い、Widget の作り直しもレイアウトも行いません。毎フレーム値が変わっても、作り直しの手間がかかりません。

`FadeTransition` 以外の見た目(大きさ、位置、色)を動かすときは、`Changed` を購読して `SetState` を呼びます。この場合は、値が変わったフレームごとに `Build` が呼ばれます。

### 使い終わったら Dispose する

`AnimationController` は、`Ticker` を通じてフレームの通知を受け続けます。`State` の `Dispose` で `AnimationController.Dispose()` を呼ぶと、フレームの通知が止まります。

## 最小例

不透明度を往復させる最小の例は、`samples/FloatSoda.Samples.OverlayApp` の [`PulseWidget.cs`](../../samples/FloatSoda.Samples.OverlayApp/PulseWidget.cs) にあります。要点だけを抜き出すと、次の形です。

```csharp
public class PulseState : TickerProviderState<PulseWidget>
{
    private AnimationController? _opacity;

    public override void InitState()
    {
        _opacity = new AnimationController
        {
            Vsync = this,
            Duration = TimeSpan.FromSeconds(1.5),
        };
        _opacity.Forward();
    }

    public override Widget Build(IBuildContext context) => new FadeTransition
    {
        Opacity = _opacity!,
        Child = new Text("Pulse"),
    };

    public override void Dispose()
    {
        _opacity?.Dispose();
        base.Dispose();
    }
}
```

`TickerProviderState<T>` は `class` なので、派生する `State` も `class` で宣言します。

## Unity / uGUI からの読み替え

| Unity | FloatSoda | 違う点 |
|---|---|---|
| コルーチンや `Update()` で `Time.deltaTime` を足して `Mathf.Lerp` する | `AnimationController` | 経過時間を自分で足さない。`Forward()` を呼ぶと、フレームごとに値が進む |
| `AnimationCurve`、Tween ライブラリのイージング | `Curve` / `Curves` | Inspector で曲線を描く代わりに、`Curves` の曲線を選ぶか `Cubic` で係数を指定する |
| `CanvasGroup.alpha` を毎フレーム書き換える | `FadeTransition` | 値を書き換えるコードを書かず、`AnimationController` を渡すだけで追従する |
| Animator のステート | `AnimationStatus` | 状態は4つだけ。遷移は `StatusChanged` の中で自分で書く |

## 制約と未実装

次の部品はまだ使えません。

- 値を任意の型や範囲へ変換する `Tween<T>` と、途中で曲線を変える `CurvedAnimation`(追跡: [#137](https://github.com/sumx21t-3310/FloatSoda/issues/137))
- プロパティを変えるだけで補間する、`AnimatedContainer` のような暗黙のアニメーション(追跡: [#163](https://github.com/sumx21t-3310/FloatSoda/issues/163))
- スプリングや摩擦の物理シミュレーション(追跡: [#164](https://github.com/sumx21t-3310/FloatSoda/issues/164))

値を見た目に結び付ける Widget は、今は不透明度の `FadeTransition` だけです。ほかの見た目は、`Changed` から `SetState` を呼んで動かします。

## 関連ページ

- [〜するには](../user/Guides.md) — フェードインさせる手順
- [State](State.md) — `State` のライフサイクルと `SetState`
- [Threading](Threading.md) — アニメーションのコールバックが呼ばれるスレッド
