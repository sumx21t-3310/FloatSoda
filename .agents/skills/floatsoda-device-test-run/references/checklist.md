<!-- Source-verified 2026-09-13 against branch test/188-all-samples. Demo.cs line numbers drift when samples change: spot-check before each walk (SKILL.md step 0.5). -->

# FloatSoda Issue #188 層1サンプル 目視チェックリスト

**目的**：Issue #188 における 17 個の層1サンプルについて、HMD 内での目視（またはコントローラー操作）で検証できるチェック項目を一覧化

**検証方法**：各サンプルの Demo を SteamVR ダッシュボード上で実行し、「見るべき点」の yes/no を判定

---

## 1. Padding

**見るべき点**：
1. **EdgeInsets.All(16) で四辺の余白が同じ幅に見えているか？**
   - 根拠：samples/FloatSoda.Samples.Padding/PaddingDemo.cs:30-34 の All(16) で両軸対称
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:2317 (Padding クラス)
   - 読み上げ指示：「最初の青い四角に、左右上下同じ幅の枠があるか見て」

2. **EdgeInsets.Symmetric で左右と上下が異なる幅に見えているか？**
   - 根拠：samples/FloatSoda.Samples.Padding/PaddingDemo.cs:38-42 の Symmetric(vertical: 12, horizontal: 36)
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:2317 (Padding クラス)
   - 読み上げ指示：「左右の余白が上下より広いか確認」

3. **子に合わせて縮むケースで茶色の枠が 80x80 に見えているか？**
   - 根拠：samples/FloatSoda.Samples.Padding/PaddingDemo.cs:56-71 で Center を挟んだ緩い制約
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:2317 (Padding クラス)
   - 読み上げ指示：「最後のセルで茶色い小さな枠が見えるか」

---

## 2. Stack

**見るべき点**：
1. **重ね順が青 → 赤 → 黄で手前から見えているか？**
   - 根拠：samples/FloatSoda.Samples.Stack/StackDemo.cs:30-39 で子をリストの順に下から積む
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:4743 (Stack クラス)
   - 読み上げ指示：「青い四角の上に赤が乗って、その上に黄があるか」

2. **Alignment.BottomRight で右下に配置されているか？**
   - 根拠：samples/FloatSoda.Samples.Stack/StackDemo.cs:43-51 の Alignment.BottomRight
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:4743 (Stack クラス)
   - 読み上げ指示：「青と赤が右下のコーナーに集まっているか」

3. **はみ出しが見えているか？**
   - 根拠：samples/FloatSoda.Samples.Stack/StackDemo.cs:93-108 のコメント「FloatSoda では既定で Clip.None」
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:4743 (Stack クラス)
   - 読み上げ指示：「最後のセルで赤い四角がはみ出して見えているか」

---

## 3. Expanded

**見るべき点**：
1. **Flex 比 1:2:1 で幅が 155/310/155 に分かれているか？**
   - 根拠：samples/FloatSoda.Samples.Expanded/ExpandedDemo.cs:33-42 で 620px を 1:2:1 に分配
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:6021 (Expanded クラス)
   - 読み上げ指示：「左と右の帯が同じ幅で、真ん中が倍の幅か見て」

2. **Expanded vs Flexible で同じ幅 90 の子が異なる結果になっているか？**
   - 根拠：samples/FloatSoda.Samples.Expanded/ExpandedDemo.cs:62-92 で Expanded と Flexible を比較
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:6021 (Expanded クラス)
   - 読み上げ指示：「上の Expanded は引き伸ばされてるが、下の Flexible は小さいままか」

3. **Spacer 1:2 で空白の比が 1:2 になっているか？**
   - 根拠：samples/FloatSoda.Samples.Expanded/ExpandedDemo.cs:97-108 で Spacer の間隔比
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:6021 (Expanded クラス)
   - 読み上げ指示：「左の余白が右より狭いか」

---

## 4. Wrap

**見るべき点**：
1. **幅 360 で折り返しが起きているか？**
   - 根拠：samples/FloatSoda.Samples.Wrap/WrapDemo.cs:34-49 で幅 360 の枠
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:6080 (Wrap クラス)
   - 読み上げ指示：「複数の色付きボタンが2行以上に折り返されているか」

2. **Alignment.Center で行の余りが中央へ配置されているか？**
   - 根拠：samples/FloatSoda.Samples.Wrap/WrapDemo.cs:55-71 の Alignment.Center
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:6080 (Wrap クラス)
   - 読み上げ指示：「2番目のセルで各行のボタンが左詰ではなく中央寄りか」

3. **CrossAxisAlignment.Center で高さ違いが中央で揃えられているか？**
   - 根拠：samples/FloatSoda.Samples.Wrap/WrapDemo.cs:77-90 で異なる高さのボックス
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:6080 (Wrap クラス)
   - 読み上げ指示：「3番目のセルで異なる高さの四角が中央ラインで揃っているか」


---

## 5. AspectRatio

**見るべき点**：
1. **16:9 の比率が 150 幅で約 84 高さに決まっているか？**
   - 根拠：samples/FloatSoda.Samples.AspectRatio/AspectRatioDemo.cs:30-37 で 150x84
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:3748 (AspectRatio クラス)
   - 読み上げ指示：「最初の水色が幅狭めの長方形に見えるか」

2. **3:4 の比率が幅 112.5、高さ 150 に逆算されているか？**
   - 根拠：samples/FloatSoda.Samples.AspectRatio/AspectRatioDemo.cs:42-49 で縦長
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:3748 (AspectRatio クラス)
   - 読み上げ指示：「2番目の赤が幅狭めで背が高いか」

3. **tight 制約下では比率が効かず 150x150 に見えているか？**
   - 根拠：samples/FloatSoda.Samples.AspectRatio/AspectRatioDemo.cs:64-68 で Ratio が無視される
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:3748 (AspectRatio クラス)
   - 読み上げ指示：「最後のセルで水色が正方形に見えるか」


---

## 6. ConstraintsTransformBox

**見るべき点**：
1. **幅の制約は残す場合、220 の要求が 150 で止まっているか？**
   - 根拠：samples/FloatSoda.Samples.ConstraintsTransformBox/ConstraintsTransformBoxDemo.cs:32-36
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:2958
   - 読み上げ指示：「最初のセルで赤い帯が枠内に収まっているか」

2. **HardEdge クリップが機能して、はみ出しが切られているか？**
   - 根拠：samples/FloatSoda.Samples.ConstraintsTransformBox/ConstraintsTransformBoxDemo.cs:40-44
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:2958
   - 読み上げ指示：「2番目のセルで黄い帯が枠の端で切れているか」

3. **カスタム変換で最大寸法が 100 に制限されているか？**
   - 根拠：samples/FloatSoda.Samples.ConstraintsTransformBox/ConstraintsTransformBoxDemo.cs:49-53
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:2958
   - 読み上げ指示：「3番目のセルで大きな正方形が 100x100 に制限されているか」

---

## 7. FittedBox

**見るべき点**：
1. **Contain(既定)で比率を維持して最大化されているか？**
   - 根拠：samples/FloatSoda.Samples.FittedBox/FittedBoxDemo.cs:31-34
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:2108
   - 読み上げ指示：「最初の旗が比率を保ったまま大きく表示されているか」

2. **Fill で比率を無視して引き伸ばされているか？**
   - 根拠：samples/FloatSoda.Samples.FittedBox/FittedBoxDemo.cs:38-42
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:2108
   - 読み上げ指示：「2番目の旗が横長に歪んでいるか」

3. **Cover + HardEdge ではみ出しが切られているか？**
   - 根拠：samples/FloatSoda.Samples.FittedBox/FittedBoxDemo.cs:54-59
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:2108
   - 読み上げ指示：「4番目の旗が枠いっぱいで、上下が切れているか」

---

## 8. FractionallySizedBox

**見るべき点**：
1. **WidthFactor = 0.5 で幅だけが親の半分になっているか？**
   - 根拠：samples/FloatSoda.Samples.FractionallySizedBox/FractionallySizedBoxDemo.cs:30-34
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:3254
   - 読み上げ指示：「最初の水色が幅狭いまま高さ全体か」

2. **0.5 x 0.5 で子が親の 1/4 になり中央に置かれているか？**
   - 根拠：samples/FloatSoda.Samples.FractionallySizedBox/FractionallySizedBoxDemo.cs:38-43
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:3254
   - 読み上げ指示：「2番目の赤が1/4サイズで中央にあるか」

3. **WidthFactor = 1.4 で子が親より大きくなり、はみ出しが見えているか？**
   - 根拠：samples/FloatSoda.Samples.FractionallySizedBox/FractionallySizedBoxDemo.cs:57-62
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:3254
   - 読み上げ指示：「最後の水色が枠からはみ出しているか」

---

## 9. LimitedBox

**見るべき点**：
1. **bounded な場所では MaxWidth が無視されているか？**
   - 根拠：samples/FloatSoda.Samples.LimitedBox/LimitedBoxDemo.cs:31-36
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:3365
   - 読み上げ指示：「最初の水色が枠いっぱいに広がっているか」

2. **unbounded な場所で MaxWidth が上限として効いているか？**
   - 根拠：samples/FloatSoda.Samples.LimitedBox/LimitedBoxDemo.cs:41-49
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:3365
   - 読み上げ指示：「2番目の赤い帯が 100 幅で止まっているか」

3. **LimitedBox なしでは子が要求どおりの幅になり枠からはみ出しているか？**
   - 根拠：samples/FloatSoda.Samples.LimitedBox/LimitedBoxDemo.cs:53-56
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:3365
   - 読み上げ指示：「最後の黄い帯が枠からはみ出しているか」

---

## 10. OverflowBox

**見るべき点**：
1. **親より大きい制約を渡した時、子がはみ出して見えているか？**
   - 根拠：samples/FloatSoda.Samples.OverflowBox/OverflowBoxDemo.cs:31-36
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:3426
   - 読み上げ指示：「最初の水色の帯が枠からはみ出しているか」

2. **Fit = Max(既定)で自身が領域いっぱいに広がっているか？**
   - 根拠：samples/FloatSoda.Samples.OverflowBox/OverflowBoxDemo.cs:41-51
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:3426
   - 読み上げ指示：「2番目の茶色が枠全体で、赤い正方形が中央にあるか」

3. **Fit = DeferToChild で自身が子と同じ寸法に縮んでいるか？**
   - 根拠：samples/FloatSoda.Samples.OverflowBox/OverflowBoxDemo.cs:56-67
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:3426
   - 読み上げ指示：「3番目で茶色が赤い正方形と同じサイズか」


---

## 11. DecoratedBox

**見るべき点**：
1. **背景色と角丸が描かれているか？**
   - 根拠：samples/FloatSoda.Samples.DecoratedBox/DecoratedBoxDemo.cs:30-41
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/container.dart:63
   - 読み上げ指示：「最初が丸みを帯びた水色か」

2. **ボーダー幅 6 が四辺に見えているか？**
   - 根拠：samples/FloatSoda.Samples.DecoratedBox/DecoratedBoxDemo.cs:45-60
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/container.dart:63
   - 読み上げ指示：「2番目が赤い内側に黄いボーダーか」

3. **Foreground で半透明の黄が水色を透かしているか？**
   - 根拠：samples/FloatSoda.Samples.DecoratedBox/DecoratedBoxDemo.cs:85-100
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/container.dart:63
   - 読み上げ指示：「最後が水色に黄がかぶさった混合色か」

---

## 12. Opacity

**見るべき点**：
1. **1.0(既定)で重なった2つの四角が原色のままか？**
   - 根拠：samples/FloatSoda.Samples.Opacity/OpacityDemo.cs:30
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:336
   - 読み上げ指示：「最初が鮮やかな水色と赤か」

2. **0.5 でグループ全体が薄くなり、重なり部分だけが濃くなっていないか？**
   - 根拠：samples/FloatSoda.Samples.Opacity/OpacityDemo.cs:35-39
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:336
   - 読み上げ指示：「2番目が全体的に薄い半透明か」

3. **0.0 で描画されず、レイアウト領域だけが残っているか？**
   - 根拠：samples/FloatSoda.Samples.Opacity/OpacityDemo.cs:44-62
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:336
   - 読み上げ指示：「3番目で中央が空いているか」

---

## 13. Transform

**見るべき点**：
1. **回転の原点が左上の場合、四角が左上軸で回転しているか？**
   - 根拠：samples/FloatSoda.Samples.Transform/TransformDemo.cs:32-39
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:1585
   - 読み上げ指示：「最初が左上を中心に回転した水色か」

2. **Alignment.Center で中心を原点に回転しているか？**
   - 根拠：samples/FloatSoda.Samples.Transform/TransformDemo.cs:43-51
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:1585
   - 読み上げ指示：「2番目が中心を軸に回転した赤か」

3. **平行移動でレイアウト領域は動かず、描画だけが右へはみ出しているか？**
   - 根拠：samples/FloatSoda.Samples.Transform/TransformDemo.cs:67-74
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:1585
   - 読み上げ指示：「最後が右へずれて、枠の端を越えているか」

---

## 14. Image

**見るべき点**：
1. **3枚の画像がすべて表示されているか？**
   - 根拠：samples/FloatSoda.Samples.Image/ImageDemo.cs:52-56
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/image.dart:343
   - 読み上げ指示：「3つのカードに画像が見えるか」

2. **DIRECT が 320x220 に引き伸ばされて、横長に見えているか？**
   - 根拠：samples/FloatSoda.Samples.Image/ImageDemo.cs:71-86
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/image.dart:343
   - 読み上げ指示：「左のカードの画像が横長に歪んでいるか」

3. **CONTAIN が比率を維持して全体表示されているか？**
   - 根拠：samples/FloatSoda.Samples.Image/ImageDemo.cs:88-102
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/image.dart:343
   - 読み上げ指示：「中央のカードの画像が全体表示で比率を保っているか」


---

## 15. Listener

**見るべき点**：
1. **DeferToChild で子のみが反応し、枠の空白は反応しないか？**
   - 根拠：samples/FloatSoda.Samples.Listener/ListenerDemo.cs:52-60
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:7146
   - 読み上げ指示：「左の水色の正方形をクリック」
   - 操作手順：(1) 左の四角の中央の小さい水色部分をコントローラーのトリガーで押す

2. **Opaque で領域全体が反応しているか？**
   - 根拠：samples/FloatSoda.Samples.Listener/ListenerDemo.cs:65-74
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:7146
   - 読み上げ指示：「右の枠の空白部分をクリック」
   - 操作手順：(1) 右の四角の空白部分をコントローラーのトリガーで押す

3. **イベント記録が更新されているか？**
   - 根拠：samples/FloatSoda.Samples.Listener/ListenerDemo.cs:54-55, 68-69
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:7146
   - 読み上げ指示：「イベント表示が更新されているか」

---

## 16. GestureDetector

**見るべき点**：
1. **OnTap で青い領域をタップするとカウント増加しているか？**
   - 根拠：samples/FloatSoda.Samples.GestureDetector/GestureDetectorDemo.cs:53-73
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/gesture_detector.dart:223
   - 読み上げ指示：「最初の水色ボタンをクリック」
   - 操作手順：(1) 水色ボタンの上でトリガーを短く押して離す

2. **OnPan でドラッグして箱が移動しているか？**
   - 根拠：samples/FloatSoda.Samples.GestureDetector/GestureDetectorDemo.cs:102-106
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/gesture_detector.dart:223
   - 読み上げ指示：「赤い箱を横にドラッグ」
   - 操作手順：(1) 赤い箱にレーザーを合わせて (2) トリガーを押しながら左右に動かす

3. **ドラッグ中に色が黄に変わり、離すと赤に戻っているか？**
   - 根拠：samples/FloatSoda.Samples.GestureDetector/GestureDetectorDemo.cs:110-112
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/gesture_detector.dart:223
   - 読み上げ指示：「ドラッグ中の色変化を確認」

---

## 17. PointerRegion

**見るべき点**：
1. **ホバーで「HOVER」に変わり、色が青になっているか？**
   - 根拠：samples/FloatSoda.Samples.PointerRegion/PointerRegionDemo.cs:73-90, 116-122
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:7288
   - 読み上げ指示：「領域に狙いを合わせる」
   - 操作手順：(1) 「AIM HERE」の領域にレーザーを当てる

2. **トリガーを押すと「PRESSED」に変わり、色がオレンジになっているか？**
   - 根拠：samples/FloatSoda.Samples.PointerRegion/PointerRegionDemo.cs:157-165
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:7288
   - 読み上げ指示：「トリガーを押す」
   - 操作手順：(1) ホバー中にトリガーを押して押しっぱなしにする

3. **ホバー中にトリガーを押して領域外へ出ると「CANCELED」に変わるか？**
   - 根拠：samples/FloatSoda.Samples.PointerRegion/PointerRegionDemo.cs:178-188
   - Flutter 参照：src/flutter/packages/flutter/lib/src/widgets/basic.dart:7288
   - 読み上げ指示：「トリガーを押しながら領域外へ出す」
   - 操作手順：(1) 領域内でトリガーを押して (2) 押しっぱなしで領域外へレーザーを移動


---

## 実行順序(推奨)

**ステージ 1：レイアウト基礎**(入力なし)
1. Padding → 2. Stack → 3. Expanded → 4. Wrap → 5. AspectRatio → 6. ConstraintsTransformBox

**ステージ 2：高度なレイアウト**(入力なし)
7. FittedBox → 8. FractionallySizedBox → 9. LimitedBox → 10. OverflowBox

**ステージ 3：描画・視覚効果**(入力なし)
11. DecoratedBox → 12. Opacity → 13. Transform → 14. Image

**ステージ 4：入力処理**(コントローラー操作が必要)
15. Listener → 16. GestureDetector → 17. PointerRegion

---

## 集計

- **総サンプル数**：17 個
- **確認項目数**：51 項目（各サンプル平均 3.0 項目）
  - レイアウト基礎(ステージ 1)：15 項目
  - 高度なレイアウト(ステージ 2)：9 項目
  - 描画・視覚効果(ステージ 3)：9 項目
  - 入力処理(ステージ 4)：9 項目

---

## 既知の制約

- **ポインタが届くのはダッシュボード用のみ**(Issue #182)
- **ホバー座標が古い**(Issue #191)
- **VR ask_user ドッグフーディング想定**：本チェックリストはオーナーが HMD を被ったまま、音声で yes/no を答える運用を想定

---

**チェックリスト作成日**：2026-09-13  
**対象リポジトリ**：FloatSoda (test/188-all-samples ブランチ)  
**ジェネレーション**：Claude Code
