# プラットフォーム: 仕組みと理由

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / explanation(WritingDocumentation 5 章の Architecture と Design)
- 読み手: FloatSoda 本体を変えるコントリビュータと Coding Agent / プラットフォームの層(`FloatSoda.Abstractions`、`FloatSoda.Engine`、`FloatSoda.OVR`)を触ろうとしている / いまどう動き、なぜそう設計したかを理解したい
- テーマ: プラットフォーム層がどう動き、なぜその設計にしたか
- Why: いまどう動き、なぜそう設計したかを知らないと、変更の影響を見積もれないため
- What: OpenVR の初期化、オーバーレイの種類、イベント、スレッド、テクスチャ転送
- How: 今のページの節を移し、足りない節を新しく書く
- 要素:
  - OpenVR の初期化(OVRIntegration の「OVRApplication」)
  - オーバーレイの種類(OVRIntegration の「オーバーレイ種別」)
  - イベント(OVRIntegration の「VREventDispatcher」)
  - メインスレッドとレンダースレッド(Architecture の「スレッドモデル」)
  - アクション入力の実装(Input)
  - テクスチャ転送と FramePacer(新しく書く)
- 見出し(案):
  - 実装状況
  - 責務と境界
  - 関係するコンポーネント
  - 処理の流れ
  - 所有権・ライフサイクル
  - なぜそう設計したか
  - Flutter との対応
  - テストでの駆動
  - 関連
- ※要確認: なし
