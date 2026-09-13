← [Home](Home.md)

# Target Users

FloatSoda は、次の3タイプの利用者を想定して設計されています。目的に応じて、ドキュメントの読み方と開始地点が変わります。

## 1. AI と一緒に「自分用のツール」を作りたい VRChatter

VR で遊んでいて「こういうのがあれば便利なのに」と思ったことがあるなら、あなたが対象読者です。**コードが書けなくても構いません。** FloatSoda は、Claude や ChatGPT などの AI にコードを生成させる「バイブコーディング」で完結することを設計目標としています。

- UI はシーンファイルやプレハブを持たない **すべて C# コード** であり、AI がそのまま生成・修正可能
- このドキュメント群(docs/)自体が、AI に正しいコードを書かせるための一次情報

作れるものの例を以下に示します(現在 Phase 1(入力基盤)と Phase 2(表示系ウィジェット)が並行して進行中です。各 Phase の詳細は [GitHub のマイルストーン](https://github.com/sumx21t-3310/FloatSoda/milestones) を参照)。

| 作りたいもの | 使う機能 | 作れるようになる Phase |
|---|---|---|
| 時刻・FPS・配信コメントを流す表示専用 HUD | テキスト表示 + テーマ + フェードアニメーション | 現時点で可能 |
| FaceEmo の表情セットを OSC で切り替えるパネル | ボタングリッド + OSC 送信 | 現時点で可能(ダッシュボードオーバーレイに限る。ボタンは `GestureDetector` で自作する) |
| VRChat の写真フォルダを VR 内で眺めるアルバム | 画像グリッド + スクロール | Phase 3(スクロール。画像ウィジェットは Phase 2) |
| お気に入りフレンドがログインしたら出るトースト通知 | 通知オーバーレイ + バックグラウンド監視 | Phase 4(通知・テキスト入力) |

**→ まずは [GettingStarted](GettingStarted.md) のサンプルを動かし、あとは AI に「これを改造して◯◯を作って」と頼んでください。**

## 2. Booth でオーバーレイ作品を売りたいクリエイター

Unity でワールドやギミックを作れるなら、FloatSoda でのオーバーレイ開発に必要な知識は十分あります。新しく覚えるのは「宣言的 UI」の概念だけです。

- **シーンもプレハブも `.meta` も不要。** Hierarchy に相当するのは、コード上の Widget ツリー
- uGUI の「オブジェクトを配置してスクリプトから書き換える(`text.text = ...`)」方式とは異なり、「状態を変えると UI が再構築される」方式(`SetState`)を採用
- UI がすべてコードであるため、ドキュメントのコード片をコピー＆ペーストするだけで動作する。商品のサポートや説明にもコードを利用可能
- exe としてのビルド・配布手順は、通常の .NET アプリと同様

**→ [GettingStarted](GettingStarted.md) → [WidgetSystem](WidgetSystem.md) の順に読んでください。**

## 3. uGUI を使いたくないエンジニア

シーンとプレハブの YAML、GUID 参照、読めない diff、レビューできない UI 変更にうんざりしているエンジニアの課題を、FloatSoda は解決します。

- **UI が 100% C# コード**。diff の読み取り、PR レビュー、grep 検索、生成 AI の利用が可能
- Flutter の三ツリーモデル(Widget / Element / RenderObject)を .NET 上に実装。`StatelessWidget` / `StatefulWidget` / `InheritedWidget` による状態管理をそのまま利用可能
- Unity ランタイムに依存せず、素の .NET + SkiaSharp + OpenVR で完結

**→ [Architecture](Architecture.md) と [APIDesign](APIDesign.md) を読むと設計思想が掴めます。**

## この想定が設計に与えている影響

- **「コードに書けない状態」を作らない**: UI・テーマ・レイアウトはすべて C# コードで表現でき、外部のアセットファイルや GUI エディタが不要。これは前述の「1. AI が生成できる」「3. diff やレビューが機能する」を満たす前提
- **API は誤用しにくさを優先**: object-initializer 中心・`required` プロパティ・イミュータブルな設計([APIDesign](APIDesign.md))により、人間と AI の双方がコンパイル時に誤りに気づけるようにする

---

← [Home](Home.md)
