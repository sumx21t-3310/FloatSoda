# Contributing to FloatSoda

FloatSoda は現在 **Alpha / 概念実証(PoC)段階** です。API は予告なく破壊的に変更されることがあります。この点をご理解の上でコントリビューションをお願いします。

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
dotnet test tests/FloatSoda.Common.Test --configuration Release --no-build
dotnet test tests/FloatSoda.Test --configuration Release --no-build
```

バグ報告・機能要望は `.github/ISSUE_TEMPLATE/` のテンプレートを使って Issue を立ててください。

---

## テスト方針

テストは xunit を使用しています。

- `tests/FloatSoda.Test` — ジオメトリ型、RenderObject、Widget のテスト
- `tests/FloatSoda.Common.Test` — Layer ツリーのテスト

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
- `public` プロパティには XML ドキュメントコメントを付与する
- イベントハンドラは `Action?` / `Action<T>?` / `Func<Task>?` で `On` プレフィックスを付ける
- スタイル属性はコンポーネント本体ではなく別の `*Style` record に分離する

---

## 受け入れ範囲

現段階のFloatSodaは、Flutter の `widgets/basic.dart` に相当する **プリミティブ層のウィジェット**(単一の RenderObject への薄いラッパーで、状態やビジネスロジックを持たないもの)のみをコアに受け入れます。

- **対象内の例**: レイアウト系(`Align`, `SizedBox`, `Row`/`Column`, `Padding` 等)、単純な描画・クリップ系ウィジェット
- **対象外(PRをリジェクトする例)**: `Button` や `Card` のような装飾済み・複合ウィジェット、Material/Cupertino 相当のスタイル付きコンポーネント、ビジネスロジックを含むウィジェット

コアが肥大化すると RenderObject / Layer / Widget-Element の三層アーキテクチャの一貫性を保てなくなるための制約です。このようなウィジェットのコントリビューションは、コア以外の別パッケージやユーザーランドでの実装を検討してください。

---

## アーキテクチャを理解する

FloatSoda は RenderObject ツリー / Layer ツリー / Widget-Element ツリーの三層構造を持ちます。コードを読み始める前に **[docs/Home.md](docs/Home.md)** から目を通すことを推奨します。

特に Widget/Element 層は `StatelessWidget` のみ実装済みで、`StatefulWidget` / `InheritedWidget` / `MultiChildRenderObjectElement.PerformRebuild()`(子リストの差分更新)は未実装(`NotImplementedException`)です。これらの領域に貢献する場合は、事前に設計方針を Issue 等ですり合わせることを推奨します。

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
- [ ] プリミティブ層(basics.dart相当)を超える複合ウィジェットを追加していない
