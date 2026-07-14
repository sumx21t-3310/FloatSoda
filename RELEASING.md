# リリース手順

FloatSoda のリリースは **`v*` タグの push** をトリガーに [`.github/workflows/release.yml`](.github/workflows/release.yml) が自動実行します(ビルド → テスト → pack → NuGet への Trusted Publishing)。このドキュメントは、その**タグを切る前に人手で行う儀式**を定めます。

FloatSoda は **Alpha 段階**(API は予告なく破壊的変更あり)ですが、公開パッケージが起動即クラッシュするような事故を防ぐため、下記のゲートを必ず通してください。

---

## リリースの流れ

1. **バージョンを更新する**
   [`Directory.Build.props`](Directory.Build.props) の `<Version>` を上げる。タグ名(`vX.Y.Z`)は**この値と完全一致**している必要があります(不一致だと release.yml が `Verify tag matches...` ステップで失敗します)。

2. **ローカルで CI と同じ検証を通す**(内部動作の回帰)
   ```bash
   dotnet build --configuration Release
   dotnet test tests/FloatSoda.Common.Test --configuration Release --no-build
   dotnet test tests/FloatSoda.Test --configuration Release --no-build
   ```

3. **ジュニアコーダーゲートを通す**(LLM 体験の回帰) ← このリリースの目玉
   下記「ジュニアコーダーゲート」を参照。

4. **タグを切って push する**
   ```bash
   git tag vX.Y.Z      # Directory.Build.props の Version と一致させる
   git push origin vX.Y.Z
   ```
   以降は release.yml がタグ整合の確認 → Release ビルド/テスト → `dotnet pack` → NuGet push(Trusted Publishing)まで自動で行います。

5. **公開を確認する**
   NuGet 上に新バージョンが出たこと、GitHub Actions の Release ワークフローが緑であることを確認する。

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
