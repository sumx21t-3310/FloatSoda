# フレームワーク(コア): 作業の手順

コアの層でよく行う作業の手順と規約をまとめます。いまの対象は、ジオメトリの値型を1つ足す作業です。初めて値型を足すときは、題材を決めて一本道で進める [コアの Tutorial](Tutorial.md) から始めると、手順の意味をつかみやすくなります。

このページに書くのは、値型を足す作業に絞った手順だけです。隣り合う規約の置き場所は次のとおりです。

| 知りたいこと | 置き場所 |
|---|---|
| 値型の設計規約と、その理由 | [APIDesign](../../APIDesign.md) の「7. ファクトリメソッドの方針」「8」「8.5」 |
| 値型で維持する条件と、破ったときに起きること | [コアの Reference](Reference.md) |
| namespace とディレクトリ、テストの命名と観点、PR のスコープ | [CONTRIBUTING.md](../../../../CONTRIBUTING.md) |
| XML ドキュメントコメントの書き方 | [DocumentationComments](../../DocumentationComments.md) |

## 規約

必須の規約は次のとおりです。

- `readonly record struct` で定義すること
- 公開する実数は `double` にし、SkiaSharp の型を公開 API に出さないこと
- 自分と同じ型を返す計算プロパティを持たせるときは、`PrintMembers` を書くこと
- namespace と物理ディレクトリを一致させること(CONTRIBUTING.md の「namespace とディレクトリ」)
- `public` なメンバーに、日本語の XML ドキュメントコメントを付けること
- テストメソッド名を `対象メンバー名_条件_期待結果` にし、条件と期待結果を日本語で書くこと(CONTRIBUTING.md の「テストの命名」)
- Flutter 由来の型では、Flutter の実装と公式テストに対応するケースを検証すること(CONTRIBUTING.md の「テスト観点」)

推奨の規約は次のとおりです。

- よく使う値と変換は、APIDesign の「7.1 命名規則」に従う静的ファクトリで用意することが望ましい
- Flutter を参照した PR では、参照した Flutter の commit・ソース・テストを PR の `## Flutter reference` 欄に書くことが望ましい

## 手順

### 手順1: 置き場所を決める

型を使う範囲で、置くディレクトリを決めます。

| 使う範囲 | ディレクトリ | namespace |
|---|---|---|
| エンジン(`FloatSoda.Engine`)や OpenVR のラッパーからも使う | `src/FloatSoda.Abstractions/Geometries/` | `FloatSoda.Abstractions.Geometries` |
| フレームワーク(`FloatSoda`)の中だけで使う | `src/FloatSoda/Geometrics/` | `FloatSoda.Geometrics` |

ファイル名は型名と同じにします。結果として、置くファイルのパスと namespace が1つに決まります。

### 手順2: Flutter の対応する型を読む

Flutter に対応する型があるときは、ローカル参照クローン(`~/code_reading/flutter_reference`)で実装とテストを探します。

```bash
grep -rnE "^class RelativeRect\b" ~/code_reading/flutter_reference/src/flutter/packages/flutter/lib/src/
```

見つかったファイルから、フィールド、ファクトリ(名前付きコンストラクタと static)、演算、`assert` を書き出します。テストは `packages/flutter/test/` の同じ領域にあります。結果として、移植するメンバーの一覧と、検証するケースの一覧ができます。

### 手順3: 型を書く

`readonly record struct` のプライマリコンストラクタで、値を持つメンバーを並べます。

```csharp
namespace FloatSoda.Geometrics;

/// <summary>…</summary>
public readonly record struct EdgeInsets(double Left = 0, double Top = 0, double Right = 0, double Bottom = 0)
{
    /// <summary>すべての辺の余白が0の値を取得します。</summary>
    public static readonly EdgeInsets Zero = All(0);

    /// <summary>すべての辺へ同じ余白を設定した値を作成します。</summary>
    public static EdgeInsets All(double value) => new(value, value, value, value);
}
```

この形は `src/FloatSoda/Geometrics/EdgeInsets.cs` から抜き出しています。Flutter の名前付きコンストラクタは、APIDesign の「7.1 命名規則」の表で名前を決めて、静的ファクトリにします。Dart の `operator +` などは C# の演算子として定義します。

`dotnet build` がエラーなしで通り、足したファイルについての警告が出なければ、この手順は終わりです。

### 手順4: 自分と同じ型を返すプロパティがあれば PrintMembers を書く

`EdgeInsets.Flipped` のように、自分と同じ型を毎回作って返す計算プロパティがあるときは、出力するメンバーを `PrintMembers` で選びます。

```csharp
private bool PrintMembers(System.Text.StringBuilder builder)
{
    builder.Append($"Left = {Left}, Top = {Top}, Right = {Right}, Bottom = {Bottom}");
    return true;
}
```

`dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~RecordToStringTest"` が通れば、この手順は終わりです。書き忘れていると、このテストが型名を挙げて落ちます。

### 手順5: テストを書く

`tests/FloatSoda.Test/Geometrics/<型名>Test.cs` を作ります。namespace は `FloatSoda.Test.Geometrics` です。値型で考える観点は、CONTRIBUTING.md の「テスト観点」のうち次の3つです。

- 代表的な正常系 — ファクトリと演算が、Flutter のテストと同じ値を返す
- 境界と退化ケース — 0、負の値、無限大
- 不正入力 — Flutter が `assert` で止める入力を、FloatSoda がどう扱うか

`dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~<型名>Test"` がすべて通れば、この手順は終わりです。

### 手順6: CI と同じ手順で確かめて PR を出す

CONTRIBUTING.md の「ブランチ・PRフロー」にあるコマンドを、ローカルで実行します。すべて通ったら PR を出します。PR の本文には、breaking change の有無の判断を書きます。新しい `public` な型を足す PR は、マージの前にジュニアコーダーテストを通します(CONTRIBUTING.md の「新しい public API を追加する場合の受け入れ条件」)。

## 規約が守られる場所

| 規約 | 検出する仕組み |
|---|---|
| ファイルスコープの namespace | `.editorconfig` の `IDE0161` がビルド時に強制する |
| `PrintMembers` の書き忘れ | `tests/FloatSoda.Test/RecordToStringTest.cs` |
| ビルドとテストが通ること | CI(`.github/workflows/ci.yml`) |
| `readonly record struct`、`double`、Skia 型を出さないこと | レビュー。機械の検査はない |
| namespace とディレクトリの一致 | レビュー([REVIEW.md](../../../../REVIEW.md) の「7. namespace / ディレクトリ」) |
| Flutter の公式テストに対応するケース | レビュー(REVIEW.md の「5. テスト」) |

## 関連

- [コアの Tutorial](Tutorial.md) — 値型を1つ足して PR を出すまでの一本道
- [コアの Reference](Reference.md) — 値型と `RenderPipeline` で守る条件
- [APIDesign](../../APIDesign.md) — 値型の設計規約と理由
