# Contributing to FloatSoda

FloatSoda は現在 **Alpha 段階** です。API は予告なく破壊的に変更されることがあります。この点をご理解の上でコントリビューションをお願いします。

---

## 開発環境のセットアップ

- .NET 10 / C# 14 SDK
- SteamVR(サンプルアプリを実行する場合のみ必須)

```bash
# ソリューション全体のビルド
dotnet build

# 全テストの実行
dotnet test

# サンプルアプリの起動(SteamVRを起動してから)
dotnet run --project samples/FloatSoda.Samples.OverlayApp
```

---

## ブランチ・PRフロー

- `main` へは直接pushせず、必ずPR経由でマージしてください。
- ブランチ名は `feature/xxx`、`fix/xxx` のように用途が分かる名前を付けてください。
- PRを出す前に、CI(`.github/workflows/ci.yml`)と同じ手順をローカルで実行し、パスすることを確認してください。

```bash
dotnet build --configuration Release
dotnet test tests/FloatSoda.Rendering.Test --configuration Release --no-build
dotnet test tests/FloatSoda.Test --configuration Release --no-build
```

バグ報告・機能要望は `.github/ISSUE_TEMPLATE/` のテンプレートを使って Issue を立ててください。

---

## テスト方針

テストは xunit を使用しています。

- `tests/FloatSoda.Test` — ジオメトリ型、RenderObject、Widget のテスト
- `tests/FloatSoda.Rendering.Test` — Layer ツリーのテスト

追加する機能に応じて、対応するテストプロジェクトにテストを追加してください。

```bash
# 特定のテストのみ実行する例
dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~AlignmentTest.TopLeft"
```

---

## コーディング規約

API設計の詳細な規約は **[docs/APIDesign.md](docs/APIDesign.md)** を参照してください。要点のみ挙げると:

- コンストラクタ引数は使わず、`init` プロパティのみで構成する(オブジェクト初期化子ファースト)
- 単一の子は `Child`、複数の子は `Children`(`IList<Widget>`)
- ジオメトリ型は `readonly record struct`、Context/Theme等は `record`
- `public` プロパティには XML ドキュメントコメントを付与する(**日本語**で書く。理由と英語化の方針は [docs/Localization.md](docs/Localization.md) を参照)
- イベントハンドラは `Action?` / `Action<T>?` / `Func<Task>?` で `On` プレフィックスを付ける
- スタイル属性はコンポーネント本体ではなく別の `*Style` record に分離する
- ユーザーに露出する例外・診断メッセージは resx でローカライズする(ニュートラル = 日本語、`en` サテライト。手順は [docs/Localization.md](docs/Localization.md))

---

## 受け入れ範囲

現段階のFloatSodaは、Flutter の `widgets/basic.dart` に相当する **プリミティブ層のウィジェット**(単一の RenderObject への薄いラッパーで、状態やビジネスロジックを持たないもの)のみをコアに受け入れます。

- **対象内の例**: レイアウト系(`Align`, `SizedBox`, `Row`/`Column`, `Padding` 等)、単純な描画・クリップ系ウィジェット
- **対象外(PRをリジェクトする例)**: `Button` や `Card` のような装飾済み・複合ウィジェット、Material/Cupertino 相当のスタイル付きコンポーネント、ビジネスロジックを含むウィジェット

コアが肥大化すると RenderObject / Layer / Widget-Element の三層アーキテクチャの一貫性を保てなくなるための制約です。このようなウィジェットのコントリビューションは、コア以外の別パッケージやユーザーランドでの実装を検討してください。

---

## 新しい public API を追加する場合の受け入れ条件

FloatSoda の第一の利用者は「コードを自分では書かず LLM に書かせる VRChatter」です(→ [docs/TargetUsers.md](docs/TargetUsers.md))。そのため public API の合否基準は **「更新後の docs だけを読んだ LLM が、そのAPIを正しく使えるか」** です。単に動くだけ・XMLコメントが付いているだけでは不十分で、**LLM から見て発見でき、誤用しにくい**ことまで求めます。

新しい public API(ウィジェット、ビルダーメソッド、オーバーレイ種別、`*Style`、使い方が変わる public プロパティ等)を追加・変更する PR は、マージ前に **ジュニアコーダーテスト**を通してください。手順はスキル [`floatsoda-junior-coder-test`](.claude/skills/floatsoda-junior-coder-test/SKILL.md) にあります。要点:

1. PR ブランチで **docs を先に更新**する(このAPIを説明するページ。`docs/*.md` は Wiki に同期される一次情報)。
2. **その新APIを使わないと解けないお題**を書き、低〜中エフォートのモデルに、更新後の **docs だけ**を渡して実装させる(ソース `src/` は見せない=ブラックボックス)。
3. モデルの結果を読み、次のように扱う:
   - **正しく使えた** → 合格。
   - **見つけられない / 使い方を誤る** → docs の発見性か API 形状の問題。docs を直すか、API を見直す。
   - **より自然な別の形を“捏造”した** → モデルが実装より直感的な API を書いた可能性が高い。**マージ前に API をその直感へ寄せることを強く検討**する(受け入れ前が一番安く直せる)。

このゲートは「動作の回帰(xunit)」ではなく「LLM 体験の回帰」を守るものです。落ちた場合は、コードではなく docs や API 形状の側を直してから再実行してください。

---

## アーキテクチャを理解する

FloatSoda は RenderObject ツリー / Layer ツリー / Widget-Element ツリーの三層構造を持ちます。コードを読み始める前に **[docs/Home.md](docs/Home.md)** から目を通すことを推奨します。

Widget/Element 層は `StatelessWidget` / `StatefulWidget` / `InheritedWidget` とそれぞれの Element、`BuildOwner` による差分ビルド、`MultiChildRenderObjectElement` のキー付き二端リスト差分まで実装済みです。一方で、多くの便利ウィジェット(`Padding`, `Container`, `ListView`, `GridView`, `Opacity`, `GestureDetector`, `Listener` 等)は未実装(`NotImplementedException`)のスタブで、ジェスチャ・ヒットテストも未実装です。これらの領域に貢献する場合は、事前に設計方針を Issue 等ですり合わせることを推奨します。

---

## ドキュメントを更新する場合の注意

`docs/*.md` は `.github/workflows/sync-wiki.yml` によって GitHub Wiki に自動同期されます。**Wiki側を直接編集しても同期時に上書きされる**ため、ドキュメントの変更は必ず `docs/` 配下のファイルに対して行ってください。

---

## PRを出す前のチェックリスト

- [ ] `dotnet build` が通る
- [ ] 該当するテストプロジェクトにテストを追加・更新した
- [ ] `dotnet test` が通る
- [ ] 追加した `public` プロパティに XML ドキュメントコメントを付けた
- [ ] Wiki同期対象の `docs/*.md` を直接編集した(Wiki側は編集していない)
- [ ] プリミティブ層(basic.dart相当)を超える複合ウィジェットを追加していない
- [ ] **新しい public API を追加した場合**、docs を更新し、ジュニアコーダーテスト([`floatsoda-junior-coder-test`](.claude/skills/floatsoda-junior-coder-test/SKILL.md))を通した
