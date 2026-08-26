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
dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~AlignmentTest.ComputeOffset"
```

### テストの命名

テストメソッド名は **`対象メンバー名_条件_期待結果`** の形式で、条件と期待結果を**日本語**で書きます。クラス名とメンバー名の部分は英語のままにします。

```csharp
[Fact]
public void ComputeOffset_親子が同サイズ_原点を返す() { ... }

[Fact]
public void BorderSide_Widthが負_ArgumentOutOfRangeExceptionを投げる() { ... }

[Theory]
[MemberData(nameof(PresetData))]
public void Preset_各プリセット_XYが仕様どおり(Alignment alignment, float x, float y) { ... }
```

| 要素 | 言語 | 理由 |
|---|---|---|
| テストクラス名(`DecoratedBoxTest`) | 英語 | テスト対象の型名のミラー。ファイル名と `--filter` に揃える |
| メソッド名の先頭(対象メンバー名) | 英語 | API をリネームするとき「この API のテストはどこか」を grep で辿れるようにする |
| 条件・期待結果 | 日本語 | `SameSize_ReturnsZero` のような英語圧縮では仕様の粒度が落ちる。日本語なら同じ長さで正確に書ける |
| ヘルパーメソッド・ローカル変数 | 英語 | 通常のコーディング規約に従う |

補足:

- `[Fact(DisplayName = "…")]` は**使いません**。メソッド名と説明の二重管理になり、片方だけ更新される事故を招くためです。
- 既存テストの一括リネームはしません。新規テストから適用し、既存は別の理由で触るついでに寄せてください。
- この方針は「ニュートラル = 日本語」という設計判断([docs/Localization.md](docs/Localization.md))の一部です。

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

## サンプルを追加する場合の規約

`samples/` 配下のサンプルは、使い方の説明であると同時に **結合テストのシナリオ** でもあります。xunit はジオメトリ・レイアウト・描画の単体を押さえますが、「docs に書いてあるとおりに書いたら実際にそう表示されるか」は検証できません。サンプルがその役割を持ちます。

さらに各サンプルの `README.md` は、将来のドキュメントサイトのページ本文にそのまま転用します。ページは **README 全文 + `<Name>Demo.cs` 全文 + XML ドキュメントコメントから生成した API リファレンス表** を連結して生成する想定です。**この3つを機械的に合成できる形を保つこと**が、以下の規約の目的です。

### ディレクトリとファイル構成

1つのサンプルにつき1プロジェクトを作り、`samples/FloatSoda.Samples.<Name>/` に置きます。

| ファイル | 役割 |
|---|---|
| `Program.cs` | エントリーポイントのみ。Host の構築、表示先の切り替え、`CreateWindow`、`RunAsync`。**ウィジェットツリーを書かない** |
| `<Name>Demo.cs` | サンプル本体。`StatelessWidget` / `StatefulWidget` として実装する |
| `README.md` | チュートリアル本文。**そのままドキュメントサイトのページ本文になる** |
| `checklist.md` | 目視確認手順。**結合テストのシナリオそのもの**。サイトへは転用しない |
| `FloatSoda.Samples.<Name>.csproj` | `TargetFramework` を明示する(`samples/Directory.Build.props` は TFM を設定しない) |

**ディレクトリ名・プロジェクト名・ルート名前空間は一致させます。** `samples/FloatSoda.Samples.Wrap/` なら、プロジェクトファイルは `FloatSoda.Samples.Wrap.csproj`、名前空間は `FloatSoda.Samples.Wrap` です。`Sample` のようなサフィックスを足したり、言い換えたりしないでください。

作成したら `FloatSoda.slnx` の `<Folder Name="/samples/">` へ登録します。

参考実装は [`samples/FloatSoda.Samples.Image`](samples/FloatSoda.Samples.Image) です。

### 表示先の切り替え

サンプルは `--desktop` 引数でデスクトップウィンドウへ表示できるようにします。

```csharp
// --desktop を渡すと、SteamVRダッシュボードの代わりにデスクトップウィンドウへ表示する。
// 目視確認をモニタ上で完結させるためのもの(SteamVRの起動自体は必要)。
// Hostの構成バインダーは値を伴わない引数を解釈できないため、渡す前に取り除く。
var useDesktop = args.Contains("--desktop");
var hostArgs = args.Where(argument => argument != "--desktop").ToArray();

var builder = Host.CreateApplicationBuilder(hostArgs);

// ...

app.CreateWindow(useDesktop
    ? new DesktopWindow { Title = "Image Test", Child = demo }
    : new DashboardWindow { Dpm = new Dpm(1000), Title = "Image Test", Child = demo });
```

`DesktopWindow` は `GLFWRawPointerSource` によってマウス入力を受け取れるため、静的なレイアウト・描画に加えてポインタ操作の確認もモニタ上で行えます。HMD を被る回数を減らすための規約です。

注意点が2つあります。

- **`--desktop` でも HMD の接続と SteamVR の起動は必要です。** `FloatSodaApp.Initialize()` は表示先によらず OpenVR を初期化するため、HMD 未接続では `Init_HmdNotFound` で起動に失敗します(issue #140)。省けるのは「被る」ことだけです。
- **デスクトップで動いたことは、ダッシュボードオーバーレイで動くことの証明にはなりません。** ウィンドウ種別ごとに入力経路が異なります(issue #182)。ポインタ操作を伴うサンプルの `checklist.md` には、必ずダッシュボードでの確認項目を含めてください。

### README の構成

```markdown
# <ウィジェット名>

## これは何か        — 対応する Flutter 公式 docs のページに相当する概念説明(1〜2段落)
## 使い方            — 最小の例から段階的に機能を足す
## Flutterとの違い   — 差異があれば明記する。無ければ「同等」と1行書く(節を空にしない)
## 実行              — dotnet run コマンド2種(--desktop / ダッシュボード)
## 関連              — docs/ の該当ページ、関連サンプル、checklist.md へのリンク
```

`## Flutterとの違い` は省略しないでください。FloatSoda の docs は Flutter の語彙で概念を教えるため、読者(と LLM)は Flutter の挙動を期待して来ます。**差異がバグでも意図的な設計判断でも、利用者が払うコストは同じ**です。

### checklist.md の構成

```markdown
# <ウィジェット名> 確認手順

## デスクトップ(`--desktop`)
1. …

## ダッシュボード(HMD を被って判定)
1. …
```

HMD が要るかどうかは**節の見出しで区別**します(`★` のような記号は使いません)。デスクトップで完結する項目とダッシュボードが要る項目を最初から2節に分けておくと、実機セッションの前に全サンプルの「ダッシュボード」節だけを機械的に集約できます。

### 禁止事項

サイトへの機械的な合成を成立させるため、次を守ってください。

- **README に架空のコードを書かない。** コードブロックは `<Name>Demo.cs` からの抜粋のみにします。README とコードの乖離を構造的に防ぐためです。
- **`<Name>Demo.cs` を単体で読んで完結させる。** サンプル間で共有ヘルパーライブラリを作らないでください。サンプルコードはサイトに転載され読者がそのままコピーするため、外部依存があると成立しません。`--desktop` 判定の数行は各 `Program.cs` にコピーします。
- **README に API リファレンス表を手書きしない。** 表は XML ドキュメントコメントから生成します。手書きすると、同じ表がサンプルの数だけ二重管理になります。

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
- [ ] **新しいウィジェットを追加した場合**、サンプル(README + checklist.md)を追加した(→ [サンプルを追加する場合の規約](#サンプルを追加する場合の規約))
- [ ] **新しい public API を追加した場合**、docs を更新し、ジュニアコーダーテスト([`floatsoda-junior-coder-test`](.claude/skills/floatsoda-junior-coder-test/SKILL.md))を通した
