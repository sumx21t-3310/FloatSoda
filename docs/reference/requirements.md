← [リファレンス](Home.md)

# 動作環境

FloatSoda で開発する PC と、作ったアプリを動かす PC に要るものを、このページで確かめられます。どちらの PC にも SteamVR が要ります。確かめていない項目は、最後の節に分けて書いています。

## 開発する PC に要るもの

開発する PC に要るものは、次の表にまとめています。FloatSoda の対象は .NET 10 で、言語は C# 14 です。

| 項目 | 必要な値 | 確かめ方 |
| :-- | :-- | :-- |
| .NET の SDK | 10 | `dotnet --version` の出力が `10.` で始まる |
| SteamVR | 起動できる | Steam から SteamVR を起動し、ダッシュボードが開く |
| HMD | SteamVR につないである | SteamVR の画面に HMD が認識されている |
| OS | Windows(x64) | 同梱する OpenVR のネイティブ DLL の対象が x64 の Windows だけ(他の OS での動作は確かめていない) |

**できた?** 表の全部の行に「ある」と答えられた。

<details>
<summary>できなかったら</summary>

足りないものの入れ方は、ガイドの[開発環境を用意する](../guide/3-prepare/set-up-environment.md)にあります。

</details>

開発に使うエディターや IDE は、決まっていません。C# が書ける環境であれば使えます。開発だけの人は、準備ができたらチュートリアルの[最初のオーバーレイを作る](../tutorials/first-overlay.md)へ進めます。

## 実行する PC に要るもの

作ったアプリを動かす PC には、SteamVR と HMD が要ります。.NET 10 のランタイムは、フレームワーク依存形式で配るときに要ります。

| 項目 | 必要な値 | 確かめ方 |
| :-- | :-- | :-- |
| SteamVR | アプリを起動する前に、起動してある | SteamVR のステータス画面が出ている |
| HMD | SteamVR につないである | SteamVR の画面に HMD が認識されている |
| OS | Windows(x64) | 開発する PC と同じ |
| .NET のランタイム | 10 | `dotnet --list-runtimes` に `Microsoft.NETCore.App 10.` が出る。自己完結形式で配ったアプリには要らない |

配る人は、ガイドの[Booth で売る準備](../guide/6-distribute/booth-release.md)で、商品ページに書く動作環境の書き方を確かめられます。配られたアプリが動かないときは、ガイドの[利用者の環境で動かないとき](../guide/7-maintain/user-environment-issues.md)を開いてください。

## SteamVR が要らない場面

[PaintingSample](../../samples/Showcase/FloatSoda.Samples.PaintingSample/Program.cs)は、`FloatSoda.Testing` でツリーを画像に書き出す作例で、SteamVR が要りません。この作例は `AddFloatSoda` も OpenVR も呼びません。

デスクトップのウィンドウに表示する `--desktop` 付きの作例も、SteamVR の起動が要ります。起動時に OpenVR を初期化するためです。この場合も HMD の接続が要ります。

HMD を被らずに確かめる方法は、ガイドの[HMD を被らずに確かめる](../guide/4-build/input/test-without-hmd.md)にあります。

## 依存するライブラリ

FloatSoda が依存するライブラリとバージョンは、次のプロジェクトファイルで確かめてください。

- [src/FloatSoda/FloatSoda.csproj](../../src/FloatSoda/FloatSoda.csproj)
- [src/FloatSoda.Engine/FloatSoda.Engine.csproj](../../src/FloatSoda.Engine/FloatSoda.Engine.csproj)

OpenVR のネイティブ DLL(`openvr_api.dll`)は、`FloatSoda.OVR` のパッケージに含まれています。[src/FloatSoda.OVR/FloatSoda.OVR.csproj](../../src/FloatSoda.OVR/FloatSoda.OVR.csproj)で、`runtimes/win-x64/native/` に置く設定を確かめられます。

## 確かめていないこと

次の項目は、動作を確かめていません。

- Windows 以外の OS で動くか(同梱の OpenVR のネイティブ DLL は x64 の Windows 用だけ)<!-- ※要確認: Windows 以外の OS で動くか。DLL の同梱先しか確かめていない。Linux 環境で起動して確かめる -->
- 対応する HMD の機種(SteamVR が対応する機種であること以外は確かめていない)<!-- ※要確認: 機種ごとの動作。複数の HMD で起動して確かめる -->
- GPU と OpenGL の最低バージョン<!-- ※要確認: GLView が要求する OpenGL のバージョンをコードで確かめていない。src/FloatSoda.Engine/GLView.cs と Renderer.cs を読み、古い GPU で起動して確かめる -->
- 自己完結形式で配ったアプリの実行要件<!-- ※要確認: 自己完結形式で配った exe を、.NET を入れていない PC で起動して確かめていない -->

自分の環境が表にないときは、動いたかどうかを[Issue](https://github.com/sumx21t-3310/FloatSoda/issues)で知らせてください。書き方は、コントリビューター向けの[不具合を切り分けて報告する](../contributor/1-get-involved/report-bugs.md)にあります。確かめられた項目は、このページの表へ移します。
