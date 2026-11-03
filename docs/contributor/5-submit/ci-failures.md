← [コントリビューター](../Home.md)

# CI が落ちたとき

## この手順でできること

PR の CI が落ちたときに、落ちたステップを見つけ、手元で同じ失敗を再現して直せます。手順が終わると、手元で CI と同じコマンドが通り、CI も通る状態になります。手順は4つです。

1. 落ちたステップを特定する
2. 手元で同じコマンドを実行する
3. 原因を切り分ける
4. 直して確かめる

コード側の CI は [ci.yml](../../../.github/workflows/ci.yml) です。動く契機と実行環境は ci.yml で確かめます。ステップは次のとおりです。この表と ci.yml が食い違うときは、ci.yml に合わせます。

| ステップ | コマンド |
| :-- | :-- |
| Build | `dotnet build --configuration Release` |
| Test (FloatSoda.Rendering.Test) | `dotnet test tests/FloatSoda.Rendering.Test --configuration Release --no-build` |
| Test (FloatSoda.Test) | `dotnet test tests/FloatSoda.Test --configuration Release --no-build` |
| Test (FloatSoda.Engine.Test) | `dotnet test tests/FloatSoda.Engine.Test --configuration Release --no-build` |
| Test (FloatSoda.OVR.Test) | `dotnet test tests/FloatSoda.OVR.Test --configuration Release --no-build` |

CI は SteamVR を使いません。実機でしか分からない失敗は、CI には出ません。CI が通っても、実機が要る変更は別に実機で確かめる必要があります。

## 前提条件

- 手元のクローンでビルドとテストを実行できること

実行できなければ、先に [開発環境を用意する](../3-prepare/dev-environment.md)を済ませてください。

## 手順1: 落ちたステップを特定する

PR の Checks から CI のログを開き、落ちたステップの名前と、エラーの最初の行を控えます。

**できた?** 「ビルドで落ちた」「どのテストで落ちた」を言えます。

## 手順2: 手元で同じコマンドを実行する

上の表のうち、落ちたステップのコマンドを、そのまま手元で実行します。PR を出す前に手元で通すのは規約です([コントリビューターの規約](../3-prepare/conventions.md) から案内している CONTRIBUTING.md の「ブランチ・PRフロー」)。

Debug ではなく Release で実行してください。CI が Release 構成で動くためです。テストのステップは `--no-build` を付けるので、先に `dotnet build --configuration Release` を実行しておきます。

<!-- ※要確認: Release と Debug で結果が変わった実例がリポジトリに無い。確かめ方: オーナーに、過去に差が出た例を確認する -->

**できた?** 手元でも同じ失敗が出ます。再現できたら手順3へ、再現できなければ「うまくいかないとき」へ進みます。

## 手順3: 原因を切り分ける

再現した失敗を、自分の変更、既存の問題、環境のどれかに分けます。

- ビルドの失敗: ログの `error` の行にある診断 ID と位置を読みます。コードスタイルの診断(`IDE0161` など)は `.editorconfig` で `warning` にしてあり、警告だけではビルドは失敗しません。警告が出ていれば、合わせて直します。namespace とディレクトリの対応は診断では見つからないので、CONTRIBUTING.md の「namespace とディレクトリ」で確かめてください
- テストの失敗: どのテストかを特定し、期待値と実際の値を見ます

<!-- ※要確認: 失敗したテストが自分の変更によるかを git stash などで切り分ける手順を、リポジトリの文書に書いてよいか。一次情報が無い。確かめ方: オーナーに確認する -->

**できた?** 原因を一文で言えます。

## 手順4: 直して確かめる

直す範囲は、原因に絞ります。無関係な変更は混ぜません。バグなら、修正を戻すと落ちるテストを足します。

手元で CI と同じコマンドを通してから push してください。

**できた?** 手元でコマンドがすべて通り、CI も通ります。

既存のテストが落ちたときは、仕様と実装のどちらが期待値に合っていないかを調べます。通すためだけにテストを消さないでください。直せないときは、理由を PR に書きます。

## docs サイトの検査が落ちたとき

`docs/` か `website/` を触った PR でサイトの検査が落ちたときは、[website/README.md](../../../website/README.md) を開いてください。

## うまくいかないとき

手元で再現しない失敗は、環境の差を疑います。失敗した CI のログで、実際の OS、.NET SDK の版、ビルド構成(Release か Debug か)を確かめ、手元の値と1つずつ比べて揃えてください。

揃えても再現しないときは、CI のログを PR に貼って相談してください。

<!-- ※要確認: コントリビューターが CI を再実行(re-run)できるか。環境差の具体例。確かめ方: オーナーに権限と過去の例を確認する -->
