← [ガイド](../Home.md)

# 反応しないときに切り分ける

## 最初に、症状を1つ選ぶ

このページは、エラーが出ないのに期待どおりに動かないときの切り分け手順です。症状ごとに、FloatSoda の制約によるものか、書き間違いかを分けて直します。

最初に、次の 2 点を確かめてください。

- 動かしている窓の種類(`DashboardWindow`、`WorldSpaceWindow`、`DeviceTrackedWindow`)
- コンソールのログに、エラーや警告が出ていないか

ログに例外やエラーが出ているときは、このページではなく [エラーを読んで直す](read-errors.md) へ進みます。出ていなければ、症状に合う節を選びます。

| 症状 | 進む先 |
| :-- | :-- |
| ボタンを押しても何も起きない | [ボタンを押しても反応しない](#ボタンを押しても反応しない) |
| 値は変わっているのに、数字や文字が変わらない | [数字や文字が変わらない](#数字や文字が変わらない) |
| 文字や画像が出ない | [文字や画像が出ない](#文字や画像が出ない) |
| 中身が変わっても、窓の大きさが変わらない | [表示の大きさが更新されない](#表示の大きさが更新されない) |
| `WorldSpaceWindow` や `DeviceTrackedWindow` で押せない | [ほかの種類のオーバーレイで試した場合](#ほかの種類のオーバーレイで試した場合) |

## ボタンを押しても反応しない

押しても何も起きないときは、次の順に確かめます。

1. 窓の種類が `DashboardWindow` であることを確かめる
2. 押す範囲に、ヒットテストに応じる子があることを確かめる
3. 上に重なって入力を塞ぐウィジェットがないことを確かめる
4. `OnTap` が、離した時点で呼ばれる前提で動いていることを確かめる

各項目の理由は次のとおりです。

- 窓の種類: ポインタの座標が届くのは `DashboardWindow` だけです。`WorldSpaceWindow` と `DeviceTrackedWindow` では、`GestureDetector` のコールバックが呼ばれません(追跡: #182)。デスクトップの窓で反応しても、ダッシュボードで反応する証明にはなりません。
- 押す範囲: `Behaviour` の既定は `HitTestBehaviour.DeferToChild` で、子が押された位置を受け取れる範囲でだけ反応します。`ColoredBox` と `Text` は自分の範囲全体で受け取りますが、`Padding` や `SizedBox` の余白は受け取りません。空白を含む領域全体で反応させるには、`Behaviour = HitTestBehaviour.Opaque` を指定します。
- 重なり: `Stack` などで重ねたウィジェットの上側に、`AbsorbPointer` や `Opaque` の `GestureDetector` があると、下側に入力が届きません。入力を素通りさせたい上側のウィジェットは、`IgnorePointer` で包みます。
- `OnTap`: 押した時点ではなく、離した時点でタップが成立して呼ばれます。押したまま 18 ピクセル(`GestureConstants.TouchSlop`)を超えて動かすと、タップは成立せず、`OnTap` は呼ばれません。そのとき `OnPanUpdate` などを指定していれば、ドラッグとして扱われます。

```csharp
new GestureDetector
{
    Behaviour = HitTestBehaviour.Opaque,
    OnTap = () => SetState(() => _count++),
    Child = new ColoredBox
    {
        Color = Colors.CornflowerBlue,
        Child = new SizedBox { Width = 220, Height = 64 },
    },
}
```

`Button` などの UI コンポーネントは、まだ提供していません(追跡: #126)。ボタンは `GestureDetector` で組み立てます(組み立て方は [ボタンやトグルを組み立てる](../4-build/screen/build-buttons-toggles.md))。

**できた?** ボタンを押すと反応する。

<details>
<summary>できなかったら</summary>

ダッシュボード以外で押したい場合は、直せません。ポインタ座標の接続が未対応の制約です(追跡: #182)。操作が要る画面を `DashboardWindow` に置いてください。ほかの制約は、リファレンスの [できないこと・まだ使えないもの](../../reference/limitations.md) にあります。

上の 4 点を確かめて必要な修正をしても反応しないときは、[それでも直らないとき](#それでも直らないとき) へ進みます。

</details>

<!-- ※要確認: ダッシュボードのオーバーレイで GestureDetector のボタンが実機で反応することは、HMD で確かめていない。重ねたウィジェットが入力を塞ぐ条件(ColoredBox を上に重ねた場合など)は、Stack の上下で再現して確かめていない -->

## 数字や文字が変わらない

値は変わっているのに画面が変わらないときは、まず、値を書き換えるときに `SetState()` を使っているかを確かめます。ウィジェットは、作り直されて初めて新しい値を画面に反映します。`SetState()` は、値の書き換えと作り直しの依頼を一度に行います。

確かめ方は、値を書き換えている行を探すことです。`State` のフィールドを `_count++` のように直接書き換えているだけなら、画面は変わりません。その行を `SetState()` の中に入れます。

```csharp
// 画面が変わらない書き方
_count++;

// 画面が変わる書き方
SetState(() => _count++);
```

仕組みの詳細は [状態と作り直し](../4-build/mechanics/state-and-rebuild.md) にあります。

**できた?** 値が変わると画面が変わる。

<details>
<summary>できなかったら</summary>

`SetState()` を呼んでいるのに変わらないときは、値を書き換えた先が、画面に表示している値と同じ場所かを確かめます。ウィジェットは、作り直しで新しいインスタンスに置き換わることがあり、そのときウィジェットのフィールドに持たせた値は引き継がれません。変わる値は `State` に持たせます。それでも変わらないときは、[それでも直らないとき](#それでも直らないとき) へ進みます。

</details>

## 文字や画像が出ない

文字や画像が出ないときは、次の順に確かめます。

1. **文字の色が背景と同じではないか。** `Text` の既定の色は黒です(`TextStyle` の `Color` を省略したとき)。背景も暗い色だと、文字が見えません。`Color` を明るい色にして試します。
2. **文字の大きさが小さすぎないか。** `TextStyle` の `FontSize` の既定は 30 です。0 以下の値を入れると、描かれないのではなく `ArgumentOutOfRangeException` が出ます。
3. **ウィンドウの `Title` が重複していないか。** 同じ `Title` で 2 つのウィンドウを作ると、後から作ったウィンドウは描画されません。`Title` はウィンドウごとに別の値にします。
4. **画像は、読み込みが終わるまで何も出ないことを踏まえているか。** `Image` の読み込みは非同期です。`LoadingBuilder` と `ErrorBuilder` を指定しなければ、読み込み中も、読み込みに失敗したときも、何も表示されません。失敗してもアプリは停止しません。`ErrorBuilder` を指定すると、失敗に気づけます。
5. **画像とアイコンに、`FloatSoda.Widgets.Paint` 名前空間の `Image` と `Icon` を使っているか。** 画像は `Image` に `FileImageProvider` を渡して表示します。

`ErrorBuilder` には、エラーを受け取ってウィジェットを返す関数(`Func<IBuildContext, Exception, Widget>`)を渡します。次の例は、読み込みに失敗したときに、エラーの文を `Text` で出します。

```csharp
new Image
{
    Provider = new FileImageProvider("photo.png"),
    ErrorBuilder = (context, error) => new Text($"読み込めません: {error.Message}"),
}
```

**できた?** 文字や画像が出る。

<details>
<summary>できなかったら</summary>

色、大きさ、`Title` の重複、画像の読み込みの 4 つが当てはまらないときは、[それでも直らないとき](#それでも直らないとき) へ進みます。文字の色や大きさの指定方法は [見た目を整える](../4-build/screen/style-appearance.md) にあります。

</details>

<!-- ※要確認: 文字が出ない原因として、Text の既定色(黒)と背景色の一致、FontSize 0、Title 重複を挙げた。いずれもコード(TextStyle の既定値、CHANGELOG、FloatSodaApp.CreateWindow の _bindings.TryAdd)を読んで書いたもので、samples/Widgets/FloatSoda.Samples.Text で再現して確かめていない。フォント未解決など、ほかの原因は確かめていない -->

## 表示の大きさが更新されない

起動後に中身の大きさが変わっても、SteamVR 上の窓の大きさが最初のままのときは、まず `Size` を省略していないかを確かめます。`Size` を省略したウィンドウは、最初のレイアウト結果の大きさで表示され、その後に中身が大きくなっても更新されません(追跡: #285)。

中身の大きさが変わるウィンドウには、`Size` を指定します。

```csharp
app.CreateWindow(new DashboardWindow
{
    Title = "MyDashboard",
    Size = new SKSize(600, 400),
    Child = root,
});
```

大きさの決まり方の詳細は、[レイアウトの考え方](layout-model.md#ウィンドウの大きさはsize-を指定するかで変わる) にあります。

**できた?** 表示の大きさが指定どおりになる。

<details>
<summary>できなかったら</summary>

`Size` を指定しても大きさが変わらないときは、ダッシュボードのオーバーレイの大きさを SteamVR が管理していることが原因の可能性があります。その場合は、[オーバーレイの種類と選び方](../4-build/mechanics/overlay-types.md) で `DashboardWindow` の大きさの扱いを確かめます。それでも直らないときは、[それでも直らないとき](#それでも直らないとき) へ進みます。

</details>

## ほかの種類のオーバーレイで試した場合

`WorldSpaceWindow` や `DeviceTrackedWindow` で押せないのは、書き間違いではなく制約です。この 2 種類では、ポインタの座標を使うウィジェット(`GestureDetector` など)が動きません(追跡: #182)。コントローラーのボタンで操作するなら、アクション入力を使えます。操作が要る画面は `DashboardWindow` に置き、ダッシュボードに替えて試してください。3 種類の違いは [オーバーレイの種類と選び方](../4-build/mechanics/overlay-types.md) にあります。

## それでも直らないとき

直らないときの行き先は、ガイドの [docs で見つからないとき](../Home.md#docs-で見つからないとき) にあります。聞くときは、次の情報を添えると、原因を絞りやすくなります。

- 症状(何をしたら、何が起きなかったか)
- 窓の種類(`DashboardWindow` など)
- 再現するコード(貼り付けるだけで動く最小のもの)
- FloatSoda のバージョン
