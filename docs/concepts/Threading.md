# Threading — メインスレッドと非同期処理

FloatSoda の Widget ツリーは、FloatSoda のメインスレッドだけで扱う前提で作られています。`Build` や入力のコールバックはメインスレッドで呼ばれますが、`await` の続きや OSC 受信のコールバックはメインスレッドへ戻りません。そこから `SetState` を呼ぶコードは、今は安全である保証がありません。

## 概要

利用者のコードに関わるスレッドは、メインスレッドと、それ以外のスレッドの2つに分けて考えます。

| スレッド | そこで動く利用者のコード |
|---|---|
| メインスレッド | `Build`、`InitState`、`Dispose`、ポインタのコールバック(`OnTap` など)、アクション入力のイベント、`AnimationController` の `Changed` と `StatusChanged` |
| それ以外のスレッド | `await` の続き、`System.Threading.Timer` のコールバック、OSC やネットワークの受信コールバック |

メインスレッドで動くコードからは、`SetState` をそのまま呼べます。それ以外のスレッドで得た結果を画面に出すには、メインスレッドへ戻す手順が要ります。その手順は [〜するには](../user/Guides.md) にあります。

## いつ使うか

このページの決まりは、FloatSoda の外から届く情報を画面に出すときに関わります。たとえば、OSC で受けたアバターの表情を表示する、Web API から取ったフレンドの一覧を表示する、`await Task.Delay(...)` のあとで表示を変える、といった場面です。

ボタンのタップやアニメーションのように、FloatSoda のコールバックの中だけで完結する処理では、スレッドを意識する必要はありません。

## 基本モデル

### メインスレッドが Widget ツリーを持つ

FloatSoda は起動すると、専用のメインスレッドで1フレームずつ処理を回します。1フレームの中で、入力のイベントを配り、`SetState` で印の付いた Widget を作り直し、レイアウトと描画を行います。Widget ツリーと、作り直しを待つ Widget の一覧は、このスレッドだけが読み書きする前提で、排他制御をしていません。

描いた結果は、レンダースレッドへ渡され、SteamVR のオーバーレイに送られます。レンダースレッドでは利用者のコードは動かないため、利用者が意識する必要はありません。

### await の続きはメインスレッドへ戻らない

FloatSoda のメインスレッドには `SynchronizationContext` がありません。そのため、メインスレッドで始めた非同期メソッドでも、`await` の続きはスレッドプールのスレッドで動きます。

```csharp
private async void OnTap()
{
    SetState(() => _isPressed = true);    // メインスレッド
    await Task.Delay(150);
    SetState(() => _isPressed = false);   // スレッドプールのスレッド
}
```

`SetState` は、呼ばれたスレッドを確かめず、例外も出しません。メインスレッド以外から呼ぶと、メインスレッドの作り直しと同時に同じ一覧を書き換える可能性があります。`Build` が `List` を列挙している最中に、別のスレッドがその `List` を書き換える可能性もあります。この競合で実際に壊れた例は確認されていませんが、安全である保証もありません。

### ListenableBuilder は別スレッドの通知を例外で知らせる

`ListenableBuilder` は、マウントされたスレッド以外から `PropertyChanged` が届くと、`InvalidOperationException` を投げます。メッセージは次のとおりです。

```text
ListenableBuilderのPropertyChangedは、ウィジェットをマウントしたスレッドから通知してください。
```

`SetState` と違い、誤ったスレッドからの通知はその場で分かります。

## Unity / uGUI からの読み替え

| Unity | FloatSoda | 違う点 |
|---|---|---|
| メインスレッド(`Update()` や UI のイベントが動くスレッド) | FloatSoda のメインスレッド | 同じく、UI を触れるのはこのスレッドだけ |
| `UnitySynchronizationContext` が `await` の続きをメインスレッドへ戻す | `SynchronizationContext` がない | `await` の続きはスレッドプールで動く。Unity と同じ書き方では、メインスレッドへ戻らない |
| 別スレッドから `Transform` などを触ると例外になる | 別スレッドから `SetState` を呼んでも例外にならない | 誤りがその場で分からない |

Flutter では、`await` の続きもタイマーのコールバックも `build` と同じスレッドで順番に動きます。FloatSoda の `SetState` は Flutter と同じ形で移植していますが、C# の `await` とタイマーはこの前提を満たしません。

## 制約と未実装

- `await` の続きは、メインスレッドへ戻らない(追跡: [#281](https://github.com/sumx21t-3310/FloatSoda/issues/281))
- 別スレッドで受けた通知や結果を、メインスレッドへ戻す公開 API はない(追跡: [#150](https://github.com/sumx21t-3310/FloatSoda/issues/150))
- メインスレッド以外から `SetState` を呼んでも、例外や警告は出ない(追跡: [#281](https://github.com/sumx21t-3310/FloatSoda/issues/281))

## 関連ページ

- [〜するには](../user/Guides.md) — バックグラウンドで取った結果を画面に出す手順
- [State](State.md) — `SetState` と作り直し
- [Input](Input.md) — 入力のコールバックが呼ばれるスレッド
- [Animation](Animation.md) — フレームごとに呼ばれるコールバック
