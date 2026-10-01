# Concepts — FloatSoda の考え方

> **骨組み** — [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188) 手順4の骨組みです。本文はまだありません。各節の「元ネタ」は [WritingDocumentation の再編の地図](../contributor/WritingDocumentation.md#再編の地図) に沿っています。内容が増えた節は `concepts/<名前>.md` へ分けます。

各節は [WritingDocumentation 4 章](../contributor/WritingDocumentation.md#concept) の Concept テンプレートに沿って書きます。

## Widget — 宣言的 UI と Widget ツリー

元ネタ: [WidgetSystem](../contributor/WidgetSystem.md) の組み込みウィジェット一覧、[Architecture](../contributor/Architecture.md) の全体像、層1サンプル

### 概要

### いつ使うか

### 基本モデル

### 最小例

### Unity / uGUI からの読み替え

### 制約と未実装

## State — 状態と再ビルド

元ネタ: [WidgetSystem](../contributor/WidgetSystem.md)、[BuildPipeline](../contributor/BuildPipeline.md) のうち利用者から観測できる再ビルドの挙動

### 概要

### いつ使うか

### 基本モデル

- `StatefulWidget` と `SetState`
- `InheritedWidget`

### 最小例

### Unity / uGUI からの読み替え

### 制約と未実装

## Layout — 制約・サイズ・配置

元ネタ: [RenderObjects](../contributor/RenderObjects.md) の制約モデル、層1サンプル(レイアウト基本・制約変換系)

### 概要

### いつ使うか

### 基本モデル

### 最小例

### Unity / uGUI からの読み替え

### 制約と未実装

## Input — ポインタ・ジェスチャ・アクション入力

元ネタ: [Input](../contributor/Input.md)、[WidgetSystem](../contributor/WidgetSystem.md) の「ジェスチャとヒットテスト」、層1サンプル(入力系)

### 概要

### いつ使うか

### 基本モデル

### 最小例

### Unity / uGUI からの読み替え

### 制約と未実装

- ポインタ入力はダッシュボードオーバーレイだけで使える

## Animation

元ネタ: [Animation](../contributor/Animation.md) の使い方の部分

### 概要

### いつ使うか

### 基本モデル

### 最小例

### Unity / uGUI からの読み替え

### 制約と未実装

## Window / Overlay — ダッシュボード・ワールド座標・デバイス追従

元ネタ: [OVRIntegration](../contributor/OVRIntegration.md)、[Home](../Home.md) の実装状況(表示専用の制約)、[Issue #151](https://github.com/sumx21t-3310/FloatSoda/issues/151)(物理サイズの発見性)

### 概要

### いつ使うか

### 基本モデル

### 最小例

### Unity / uGUI からの読み替え

### 制約と未実装

- `WorldSpaceWindow` と `DeviceTrackedWindow` は表示専用

## 関連ページ

- [Guides](Guides.md)
- [Tutorials](Tutorials.md)
