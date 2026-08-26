# リリース手順

FloatSoda のリリースは **`v*` タグの push** をトリガーに [`.github/workflows/release.yml`](.github/workflows/release.yml) が自動実行します(ビルド → テスト → pack → NuGet への Trusted Publishing)。このドキュメントは、その**タグを切る前後に人手で行う儀式**を定めます。

FloatSoda は **Alpha 段階**(API は予告なく破壊的変更あり)ですが、公開パッケージが起動即クラッシュするような事故を防ぐため、下記のゲートを必ず通してください。

> この手順を実行するスキルが [`floatsoda-release`](.claude/skills/floatsoda-release/SKILL.md) にあります。**方針の正本はこのドキュメント**で、スキルはその実行役です。手順やゲートの方針を変えるときは必ずこちら側を直してください(スキル側に「今はこうする」という分岐を持たせない)。

---

## リリースの流れ

### 1. リリース範囲を確定する

前回タグから `HEAD` までの変更を洗い出し、[`CHANGELOG.md`](CHANGELOG.md) の `[Unreleased]` に漏れがないか突き合わせます。

```bash
git describe --tags --abbrev=0
```

```bash
git log --oneline <前回タグ>..HEAD
```

あわせて該当するマイルストーン(Phase 1〜7)の Issue を照合し、このリリースに含まれる変更で閉じられる Issue が実際に閉じているかを確認します。

### 2. バージョンを決めて更新する

SemVer で刻みを決め、[`Directory.Build.props`](Directory.Build.props) の `<Version>` を更新します。0.x 系では **破壊的変更で minor を上げます**(patch は後方互換の修正のみ)。

タグ名(`vX.Y.Z`)はこの値と**完全一致**している必要があります。不一致だと release.yml の `Verify tag matches...` ステップで失敗します。

### 3. CHANGELOG を確定する

[`CHANGELOG.md`](CHANGELOG.md) は [Keep a Changelog](https://keepachangelog.com/ja/1.1.0/) 形式です。

- `## [Unreleased]` を `## [X.Y.Z] - YYYY-MM-DD` に確定し、その上に新しい空の `## [Unreleased]` を置く
- 節見出しは `Added` / `Changed` / `Deprecated` / `Removed` / `Fixed` / `Security`。破壊的変更は `### Removed (Breaking)` のように明示する
- 記述は**利用者から見た変化**で書く(内部リファクタは、利用者に影響がなければ書かない)
- **ファイル末尾のリンク定義を更新する** ← 最も抜けやすい
  - `[Unreleased]` の比較リンクを `.../compare/vX.Y.Z...main` に付け替える
  - `[X.Y.Z]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/vX.Y.Z` の行を追加する

### 4. ローカルで CI と同じ検証を通す(内部動作の回帰)

```bash
dotnet build --configuration Release
```

```bash
dotnet test tests/FloatSoda.Rendering.Test --configuration Release --no-build
```

```bash
dotnet test tests/FloatSoda.Test --configuration Release --no-build
```

あわせて、CI に投げる前にローカルで潰せるものを潰します。

- **タグ整合の事前確認** — release.yml と同じ判定(タグ名から先頭 `v` を除いた文字列 == `Directory.Build.props` の `<Version>`)をローカルで確認する。切ろうとしているタグが既に存在しないことも確認する
- **pack の中身確認** — `dotnet pack --configuration Release --no-build --output <リポジトリ外のディレクトリ>` を実行し、生成された `.nupkg` が意図した公開パッケージだけであることを確認する(`IsPackable=false` の `FloatSoda.UI` 系・`FloatSoda.Hooks` が混ざっていないこと)。**出力先はリポジトリ外**にしてください。`artifacts/` は `.gitignore` されておらず、リポジトリ内に出すと未追跡ファイルが残ります

### 5. ジュニアコーダーゲートを通す(LLM 体験の回帰) ← このリリースの目玉

下記「ジュニアコーダーゲート」を参照。

このゲートは**最終的にタグを打つコミットに対して有効である必要があります**。手順6のマージ後に `origin/main` が動いていた場合は、手順7で通し直してください。

### 6. リリース変更をコミットして main へ反映する

手順2・3で編集した `Directory.Build.props` の `<Version>` と `CHANGELOG.md` は、この時点ではまだ**未コミット**です。`main` へは直接pushできない([CONTRIBUTING.md](CONTRIBUTING.md) のブランチ・PRフロー)ため、他の変更と同じく通常のPRフローで反映します。

```bash
git checkout -b release/vX.Y.Z
git add Directory.Build.props CHANGELOG.md
git commit -m "chore(release): vX.Y.Z のバージョンとCHANGELOGを確定する"
git push origin release/vX.Y.Z
gh pr create --base main --title "chore(release): vX.Y.Z"
```

PRが `main` にマージされ、**そのマージコミットが `origin/main` の HEAD になっていること**を確認してから次の手順に進みます。ここを飛ばしてタグだけ切ると、release.yml が旧 `<Version>` のコミットを checkout して `Verify tag matches Directory.Build.props version` ステップで失敗し、CHANGELOG の更新もリリースに含まれません。

### 7. タグを切って push する

タグは、手順6で `origin/main` の HEAD になったことを確認したリリースコミットに対して打ちます。**そのとき確認した SHA を固定して使ってください。** `git checkout main && git pull` で取り直すと、確認からタグ作成までの間に別のコミットが入った場合に、意図しないコミットへタグが付きます。release.yml の検証はタグ名と `<Version>` の一致しか見ないため、このズレは検出されません。

```bash
git fetch origin main
RELEASE_SHA=<手順6で確認したマージコミットのSHA>
test "$(git rev-parse origin/main)" = "$RELEASE_SHA"   # 一致しなければ止める
```

**`origin/main` が動いていた場合は、手順5のジュニアコーダーゲートをタグ対象のコミットで通し直します。** ゲートは特定のコミットの docs と API に対する検証なので、通過後に別のコミットが入ると、検証していない状態をリリースすることになります。

```bash
git tag vX.Y.Z "$RELEASE_SHA"
```

```bash
git push origin vX.Y.Z
```

タグ名は `Directory.Build.props` の `Version` と一致させます。以降は release.yml がタグ整合の確認 → Release ビルド/テスト → `dotnet pack` → NuGet push(Trusted Publishing)まで自動で行います。

### 8. 自動リリースの完了を確認する

- GitHub Actions の Release ワークフローが緑であること
- NuGet 上に新バージョンが公開されたこと(反映まで数分かかることがあります)

### 9. GitHub Release を作成する

release.yml は NuGet へ push するだけで、**GitHub Release ページは作りません**。NuGet の説明文だけでは変更点が利用者に届かないため、タグ push 後に必ず手で作成します。

```bash
gh release create vX.Y.Z --title "vX.Y.Z — <一言見出し>" --notes-file <本文ファイル>
```

- タイトルは `vX.Y.Z — <一言見出し>` 形式(例: `v0.1.0 — 宣言的UIコア完成・初回正式リリース`)。見出しが不要な小さいリリースは `vX.Y.Z` だけでも構いません
- 本文は CHANGELOG の当該バージョンの節をそのまま使います

---

## ジュニアコーダーゲート

FloatSoda の第一利用者は「コードを LLM に書かせる VRChatter」です。xunit テストは**内部動作**の回帰を守りますが、それとは別に **「docs だけを読んだ LLM が FloatSoda で物を作れるか」= LLM 体験** の回帰を守るのがこのゲートです。実際にこの手法は、公開前のコードから「全オーバーレイをクラッシュさせる docs 推奨API」と「子の動的削除で無限再帰するレンダーツリーのバグ」を釣り上げた実績があります。

**リリース対象のコミット上で**、スキル [`floatsoda-junior-coder-test`](.claude/skills/floatsoda-junior-coder-test/SKILL.md) を実行します。

- **お題**: `main`(状態を持つ動的UI)を基本とし、レンダーツリーや OVR 層に変更が入ったリリースでは `hard` も追加。
- **テーマは毎リリース持ち回り**(トースト通知 / 写真アルバム / FaceEmo 切替 …)。1つのお題に過学習させないため。
- ジュニア役モデル(既定 Sonnet 5・中エフォート)に **docs だけ**を渡し、`src/` は見せない(ブラックボックス)。
- ビルド → SteamVR 実機で実行し、4分類(ⓐ捏造 / ⓑdocsバグ / ⓒlibバグ / ⓓ逸脱)でトリアージ。

### 合格基準

- [ ] `src/` を見せていないのにスクラッチプロジェクトが**ビルドできる**(= docs だけで API に到達できた)
- [ ] SteamVR 実機で**クラッシュせず動作する**
- [ ] **ⓑ docs バグ / ⓒ ライブラリバグが 0 件**(見つかったら**リリースをブロック**し、修正してから再実行)
- [ ] ⓐ 発見性ギャップ / ⓓ 仕様逸脱は記録する(重大でなければブロックしない)
- [ ] 実行条件(モデル・エフォート・お題・docs 提供方法)と結果をメモログに追記した

ⓑ/ⓒ は「動くコードにする」修正、ⓐ の一部と発見性の問題は「docs / API 側」の修正です。前者はこのリリースに含め、後者は状況に応じて Issue 化(→ [`.github/ISSUE_TEMPLATE/`](.github/ISSUE_TEMPLATE/))を検討してください。

---

## 関連

- [CONTRIBUTING.md](CONTRIBUTING.md) — PR フローと、**新 public API の受け入れ条件**(こちらも同じジュニアコーダーテストを、新APIを狙い撃つお題で使う)
- [.github/workflows/release.yml](.github/workflows/release.yml) — タグ push 後の自動リリースパイプライン
- [.claude/skills/floatsoda-release/SKILL.md](.claude/skills/floatsoda-release/SKILL.md) — この手順を実行するエージェント用スキル
