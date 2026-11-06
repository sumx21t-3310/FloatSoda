← [ガイド](../Home.md)

# AI に FloatSoda を書かせる

この手順で、AI に FloatSoda の資料を渡してコードを書かせ、出来上がったコードで SteamVR のダッシュボードにオーバーレイを出し、FloatSoda で起きやすい食い違いを点検できます。コードを読めなくても進められます。手順は5つです。

AI は、Flutter の知識で書いた「動かないコード」を出すことがあります。FloatSoda は Flutter に似ていますが、名前や引数が違う部品があり、まだない部品もあります。資料を渡すこと、ビルドと実行で確かめること、よくある食い違いを検索で点検することが、この事故を減らします。

始める前に、[開発環境を用意する](set-up-environment.md)を済ませて、FloatSoda を足したコンソールアプリを用意してください。この手順では、そのアプリの `Program.cs` を書き換えます。

## 手順1: AI に資料を渡す

FloatSoda の公開サイトは、AI 向けに2つのファイルを配信しています。AI には、このどちらかを渡します。

| ファイル | 中身 | 向く場面 |
| :-- | :-- | :-- |
| `llms.txt` (<https://docs.floatsoda.dev/llms.txt>) | 索引 | AI が自分でページを開ける(コーディングエージェントなど) |
| `llms-full.txt` (<https://docs.floatsoda.dev/llms-full.txt>) | 全文 | チャットに貼る、またはファイルとして取り込む |

渡し方は、URL を貼るか、ファイルを取り込みます。URL を開けない AI には、ファイルの本文を貼ってください。あわせて、リファレンスの[できないこと・まだ使えないもの](../../reference/limitations.md)を読むように伝えてください。作例の README にも、ウィジェットごとの使い方と「Flutterとの違い」があります。AI が部品を迷うときは、`samples/` の該当する作例の `README.md` を渡せます。

**できた?** AI に使う API と制約を挙げさせ、その内容が、渡した資料の本文にある。

<!-- ※要確認: 取得できる文字数に上限がある AI では、llms-full.txt が大きすぎることがある。API リファレンスの取り込み後の llms.txt の構成(別ファイル名)も確かめていない。確かめ方: 公開サイトの llms.txt を開いて、系統ごとの分割ファイルの名前を確かめる。ジュニアコーダーテストで、資料だけで書けるかを確かめる -->

## 手順2: 作りたいものを頼む

作りたいものを、次の3つを入れて頼む。

- 何を出すか(例: 現在の時刻、表情を切り替えるボタン)
- どこに置くか(ダッシュボード、ワールド座標に固定、コントローラーに追従)
- 押す操作が要るか

押す操作が要る画面は、ダッシュボードに置くよう伝えてください。ワールド座標固定と追従のオーバーレイは、ポインタで押せません(追跡: #182)。

頼み方の例です。

```text
FloatSoda の資料を読んだうえで、SteamVR のダッシュボードに、現在の時刻を毎秒更新して出す
オーバーレイを作ってください。押す操作は要りません。定期更新と SetState の呼び出し条件は、
資料で確かめてください。資料にない API は推測せず、確認が必要な点を示してください。
Program.cs 全体を、そのまま貼れる形で出してください。
```

**できた?** コードが `Program.cs` に貼れる形で返ってきた。

<!-- ※要確認: 頼み方の例を、どの AI でも通していない。確かめ方: 複数の AI でこの文を実行し、ビルドが通るコードが返るかを確かめる -->

## 手順3: ビルドする

返ってきたコードを `Program.cs` に貼り、次を実行する。

```powershell
dotnet build
```

エラーが出たら、エラーの文をそのまま AI に貼って、直させてください。存在しない名前を AI が書いたときは、ここでコンパイルエラーになります。

**できた?** 「ビルドに成功しました」と出た。

<details>
<summary>できなかったら</summary>

AI に何度か直させても消えないエラーは、[エラーを読んで直す](../5-troubleshoot/read-errors.md)を読んでください。`using` のエラーが出るときは、参照している FloatSoda の版と、`using` の綴りを確かめてください。FloatSoda は、リリースで名前空間を移すことがあります。たとえば、`Text` と `RichText` は 0.4.0 で `FloatSoda.Widgets.Components` から `FloatSoda.Widgets` に移りました。AI が古い `using` を書いている可能性があります。

</details>

## 手順4: 動かす

SteamVR を起動してから、次を実行する。

```powershell
dotnet run
```

SteamVR のダッシュボードを開く。

**できた?** ダッシュボードにオーバーレイが出て、頼んだ動き(例の依頼なら、時刻が毎秒変わる)が見えた。

<details>
<summary>できなかったら</summary>

アプリが終了する、例外が出るときは、[エラーを読んで直す](../5-troubleshoot/read-errors.md)を読んでください。起動しているのに出ない、押しても反応しないときは、[反応しないときに切り分ける](../5-troubleshoot/not-responding.md)を読んでください。

</details>

## 手順5: 点検する

動いたコードでも、FloatSoda で起きやすい食い違いが残っていることがあります。次の語を、コードの中で検索してください。見つかった箇所は、表の「起きること」に当たるかを確かめます。当たるときは、その行の「AI への指示の例」を AI に伝えて、直させます。たとえば `WorldSpaceWindow` や `Task.Run` は、それ自体は正しい使い方があるので、語が見つかっただけでは直しません。

| 検索する語 | 起きること | AI への指示の例 |
| :-- | :-- | :-- |
| `Button` | `Button` などの UI コンポーネントは提供していない | `Button` を使わず、`GestureDetector` で組み立てて |
| `WorldSpaceWindow` または `DeviceTrackedWindow` の中の `GestureDetector` | 押しても `OnTap` が呼ばれない | 押す画面は `DashboardWindow` に置いて |
| `ListView`、`GridView`、`SingleChildScrollView` | 公開されていない。使えない | スクロールが使えない前提で、表示する件数を絞り、`Stack` と `Positioned` などで表示範囲に収まるように並べて。元の要件がそれで満たせないときは、その点を教えて |
| `ClipRRect`、`ClipPath` | FloatSoda では名前が違う | `ClipRRect` は `ClipRoundRect`、`ClipPath` は `ClipCustomPath` に対応する。名前だけでなく、引数とプロパティも、[Flutter の読み替え表](../../reference/flutter-mapping.md)と作例の README で確かめて書き直して |
| `MouseRegion` | FloatSoda では名前が違う | `MouseRegion` は `PointerRegion` に対応する。名前だけでなく、引数とコールバックも、読み替え表と作例の README で確かめて書き直して |
| `margin`、`Margin`、`spacing`、`Spacing` | `Container` に `margin` はなく、`Row` や `Column` に `spacing` はない(`Wrap` の `Spacing` は実在する) | 見つかった型に、そのプロパティがあるかを確かめ、なければ `Padding` と `SizedBox` で余白と間隔を作って |
| `SetState` を呼ぶ `Task.Run` や `Timer` の中身 | メインスレッド(フレームを処理するスレッド)以外から呼ぶと、画面の更新が失われる、またはクラッシュする(追跡: #150、#281) | 定期的な更新は、メインスレッドから呼ばれる `WidgetTicker` の中で `SetState` を呼んで。[チュートリアル](../../tutorials/first-overlay.md#2-数字を表示して書き換える)の書き方に合わせて |

`Size` を指定していないウィンドウは、起動後に中身が大きくなっても、SteamVR 上の表示範囲が最初の大きさに固定されます。描かれた内容は正常で、増えた部分が表示されません。中身の大きさが変わる画面は、ウィンドウに `Size` を指定するよう AI に伝えてください(追跡: #285)。

この表は語の一覧だけで、制約の詳しい内容は[できないこと・まだ使えないもの](../../reference/limitations.md)、Flutter の名前の違いは[Flutter の読み替え表](../../reference/flutter-mapping.md)にあります。

**できた?** 表の語が見つかった箇所のすべてで、「起きること」に当たるかを確かめ、当たるものを直した。

<details>
<summary>できなかったら</summary>

「起きること」に当たる箇所は、表の指示を AI に渡して直させ、手順3に戻ってください。直し方の根拠は、上の2つのリファレンスにあります。

</details>

<!-- ※要確認: ClipPath、MouseRegion、margin、spacing の対応は samples/ の README の「Flutterとの違い」で確かめた。SetState をバックグラウンドスレッドから呼ぶと画面の更新が失われる、またはクラッシュする、という症状は、実行して確かめていない。src/ を読んだ範囲では、State.SetState と Element.MarkNeedsBuild にスレッドの確認や排他制御がない。確かめ方: 別スレッドから SetState を呼ぶ最小のコードを実行する -->

動いて点検も済んだら、次は「4. 作る」です。最初の1本をまだ作っていないときは、チュートリアル[最初のオーバーレイを作る](../../tutorials/first-overlay.md)を先に読んでください。見た目の整え方は、[見た目を整える](../4-build/screen/style-appearance.md)から始められます。
