---
name: openvr-api-reference
description: OpenVR/SteamVR APIのインターフェース(IVRSystem, IVRApplications, IVRHeadsetView, IVROverlayなど)について聞かれたとき、生バインディング(src/FloatSoda.OVR/openvr_api.cs)を根拠に機能を網羅的にリストアップし、FloatSodaにとっての実装必要性を評価する。「OpenVRのAPIについて教えて」「〇〇の機能を一覧化して」「これは実装すべき?」のような質問で発火。
user-invocable: true
---

# OpenVR APIリファレンス & 実装必要性評価

`src/FloatSoda.OVR/openvr_api.cs` はOpenVR SDKのC#生バインディング(P/Invoke)。
このスキルは、そこにあるインターフェースについて聞かれたときに **(1) 網羅的な機能リスト** と
**(2) FloatSodaでの実装必要性評価** の2部構成で答えるための手順。

## 手順

### 1. 対象インターフェースの特定

質問文からOpenVRのインターフェース名(例: `IVRHeadsetView`, `IVRApplications`, `IVRSystem`, `IVROverlay`)
または通称(例: 「ヘッドセットビュー」「アプリケーション登録API」)を特定する。曖昧なら
`Grep` で `openvr_api.cs` 内を検索して候補を絞る。

### 2. 生バインディングを読む

対象の以下3点を `openvr_api.cs` から読む:

- `IVRxxx` struct — 関数テーブル定義(delegateの並び順がAPIの全メソッド一覧)
- `CVRxxx` class — 上記を薄くラップした公開クラス(引数名・戻り値はこちらの方が読みやすい)
- 関連する `enum` / `const` / `VREvent_*`(同じ機能領域の定数や、モード値、イベント)

`Grep -n "<InterfaceName>"` で一括検索すると both struct/class/関連定数がまとめて拾える。

### 3. 網羅リストを作る

**全メソッドを1つも省略せず**表形式で出す(このAPIの「一覧化」が目的の質問なので、代表例だけ
挙げて終わるのは不可)。各行:

| # | メソッド | シグネチャ | 機能(日本語で一言) |

signatureが取得系(`Get*`)とセット系(`Set*`)で対になっている場合は隣接させる。
setterが無い読み取り専用値(例: AspectRatio)は「setterなし」と明記する。

表の後に、関連定数・関連enum・関連VREventを箇条書きで補足する。

### 4. 実装必要性を評価する

以下の観点で評価し、2〜4行程度の推奨(結論→理由)としてまとめる。**網羅リストとは別に
明確なセクションとして分ける。**

- **FloatSodaの本流(オーバーレイUI描画)と関係があるか**: `DashboardOverlay` /
  `WorldSpaceOverlay` / `DeviceTrackedOverlay` の生成・描画・入力に関わる機能か、それとも
  SteamVR側の周辺システム設定(マニフェスト登録、デスクトップミラー、チャペロン設定など)か。
  後者は優先度が低い。
- **3ペルソナに刺さるか**([floatsoda-target-users.md]のメモリ参照。VRChatterのvibe-coding用途 /
  Booth創作者のUnity的発想 / uGUI回避エンジニアのコードオンリー志向)。どのペルソナも
  触らなそうな機能(配信者向けミラー設定、開発者専用デバッグAPI等)は必要性が低いと判断する。
- **既存のラップ方針との整合性**: `src/FloatSoda.OVR/` に既にある型(`OVRApplication.cs` など)
  と役割が近ければ「同じノリで機械的にラップを広げる」選択肢もあることを触れる。ブランチ名
  (例: `feature/wrap-cvr-applications-api`)が示す現在の作業スコープと関係があるかも確認する。
- **CLAUDE.mdの原則**: 状態を増やす場合、コードで表現できない外部ファイル/設定に依存しないか
  (uGUI回避エンジニア向けの価値提案)。

### 5. 出力形式

1. 見出し「## 機能一覧」+ 表 + 補足箇条書き
2. 見出し「## 実装必要性」+ 短い推奨文

このスキル自体はコードを変更しない。CLAUDE.mdの「デフォルトでは調査・説明のみ、実装はオーナーの
明示的な指示があるまで行わない」方針に従い、評価だけで終える。
