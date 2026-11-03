← [ガイド](../Home.md)

# exe にして配る

## この手順でできること

自分の PC で動くオーバーレイを、.NET の SDK を入れていない PC でも動く形にして、zip で渡せます。修正版も、同じ手順で作り直して届けられます。手順は 4 つです。

1. 配布用にビルドする
2. 開発環境のない PC で動かして確かめる
3. 渡す
4. 修正版を届ける

「配る」は、zip などのファイルとして渡すことを指します。Booth の商品ページの作り方は [Booth で売る準備](booth-release.md) で扱います。

## 前提条件

次の 3 つがそろっていることを確かめます。

- .NET 10 の SDK が開発用の PC に入っていること
- 自分の PC で `dotnet run` して、オーバーレイの動作を確かめ済みであること
- 配る相手の PC に SteamVR が入っていること

配る相手の PC の動作環境は、リファレンスの [動作環境](../../reference/requirements.md) にあります。また、SteamVR に登録するときの `AppKey` は、既定値のままにせず、アプリごとに固有の値にしておきます(`FloatSodaOptions.AppKey`。既定値はすべての FloatSoda アプリで同じです)。

<!-- ※要確認: AppKey の既定値は FloatSodaOptions.cs で new("FloatSoda") だが、AppKey の許容文字は小文字の英字、数字、ドット、アンダースコアだけ(OVRApplication.cs)。既定値が許容文字の条件に合わない。検証が働くかは実行して確かめていない -->

## 手順1: 配布用にビルドする

プロジェクトのフォルダーで、次のコマンドを実行します。

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:DebugType=none -p:DebugSymbols=false -o publish
```

`--self-contained true` は、.NET のランタイムを出力に含める指定です。相手の PC に .NET を入れてもらう必要がなくなります。`-r win-x64` は、64 ビットの Windows 向けに作る指定です。

出力先の `publish` フォルダーに、次のファイルが並びます。

- 自分のプロジェクト名の `.exe` と `.dll`
- .NET のランタイム(`coreclr.dll`、`hostfxr.dll` など)
- ネイティブライブラリ(`openvr_api.dll`、`glfw3.dll`、`libSkiaSharp.dll`、`libHarfBuzzSharp.dll`)

上のコマンドの `-p:DebugType=none` は、自分のプロジェクトの `.pdb` を作らない指定です。SkiaSharp と HarfBuzz のネイティブライブラリ(`libSkiaSharp.pdb`、`libHarfBuzzSharp.pdb`)には、デバッグ用の `.pdb` が付いてきて、出力全体の約半分の大きさを占めます。配るときは使わないので、削除します。

```powershell
Remove-Item publish\*.pdb
```

ランタイムを同梱せず、相手の PC に .NET 10 のランタイムを入れてもらう方法もあります。その場合は `--self-contained true` を `--self-contained false` に替えます。配布物は小さくなりますが、相手に .NET のインストールを頼むことになるため、この手順では同梱する方法を選びます。

**できた?** `publish` フォルダーに、`.exe` と必要なファイルが並んでいる。

<!-- ※要確認: .pdb の大きさの割合は、samples/Showcase/FloatSoda.Samples.GettingStarted を上のコマンドで publish した出力で測った(.pdb 2つで約 105 MB、出力全体で約 210 MB)。依存ライブラリの版で変わる。出力に .pdb が残ることを確かめた後で削除しても exe が動くかは、実機で確かめていない -->
<!-- ※要確認: 上の publish コマンドで exe とネイティブライブラリが出力されることは、samples/Showcase/FloatSoda.Samples.GettingStarted で確かめた(ビルドと出力のファイル一覧まで)。出力した exe を実際に起動して、オーバーレイが出ることは確かめていない。単一ファイルの公開(PublishSingleFile)は、確かめていないため手順に含めない -->

## 手順2: 開発環境のない PC で動かして確かめる

開発環境に依存しない別の PC で、`publish` フォルダーのコピーを動かします。ユーザーアカウントを分けただけでは、PC に入った SDK やランタイムは残ります。次の順に進めます。

1. SteamVR を起動する
2. `publish` フォルダーの `.exe` をダブルクリックする
3. アプリで使う窓の種類に合った場所に、オーバーレイが出ることを見る(`DashboardWindow` なら、ダッシュボードを開く)

SteamVR を起動する前に `.exe` を動かすと、起動に失敗することがあります。必ず SteamVR を先に起動してください。

**できた?** ダッシュボードにオーバーレイが出る。

<details>
<summary>できなかったら</summary>

動かないときの切り分けは、[利用者の環境で動かないとき](../7-maintain/user-environment-issues.md) にあります。コンソールにエラーが出ているときは、[エラーを読んで直す](../5-troubleshoot/read-errors.md) を読みます。

</details>

<!-- ※要確認: .NET の SDK を入れていない PC で起動できること、SteamVR を起動していないと起動に失敗することは、実機で確かめていない -->

## 手順3: 渡す

動作を確かめた `publish` フォルダーを、zip にまとめます。

```powershell
Compress-Archive -Path publish\* -DestinationPath MyOverlay-1.0.0.zip
```

zip には、`.exe` と必要なファイルのほかに、配布物のライセンス条件が求める表示と文書([Booth で売る準備](booth-release.md) の手順 1)を入れます。さらに、次の内容を書いた説明書(テキストファイル)を入れます。

- 起動の前に、SteamVR を起動すること
- 起動したあとに、ダッシュボードにオーバーレイが出ること

説明書に書く項目の全体は、[Booth で売る準備](booth-release.md) の手順 2 にあります。

**できた?** 別の場所で zip を展開しても、動く。

## 手順4: 修正版を届ける

修正版は、手順 1 から 3 をもう一度行います。版を区別できるよう、zip の名前に版を入れます(例: `MyOverlay-1.0.1.zip`)。`AppKey` は、前の版と同じ値のままにします。SteamVR に登録済みの場合は、更新後も登録した起動先が有効かを確かめます。

届けるときに、利用者へ求める操作は 1 つです。アプリを終了してから、古いフォルダーを削除し、新しい zip を展開し直してもらいます。設定や保存データを同じフォルダーに置いている場合は、削除の前に退避してもらいます。

**できた?** 更新した版を展開したとき、前の版と同じように動く。

<!-- ※要確認: 更新版を上書き展開したときの挙動と、利用者が設定ファイルを編集している場合の扱い(上書きされるか)は、確かめていない -->

## 結果を確かめる

渡した版を受け取った人に、次の 3 点を確かめてもらいます。

- SteamVR を起動してから `.exe` を動かすと、オーバーレイが出る
- 説明書のとおりに操作できる
- 版の名前が、渡した zip の名前と一致している

確かめたあとは、次のページに進みます。

- 売る場合: [Booth で売る準備](booth-release.md)
- SteamVR の起動と同時にオーバーレイを自動で起動させたい場合: [SteamVR と一緒に起動する](launch-with-steamvr.md)

## うまくいかないとき

公開に失敗する、または相手の PC で動かないときは、[利用者の環境で動かないとき](../7-maintain/user-environment-issues.md) で切り分けます。
