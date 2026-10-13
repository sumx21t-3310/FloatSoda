# アニメーションの部品を移植する

アニメーションの層でよく行う作業の手順と規約を
このページに書くのは、作業の順序だけです。規約の本文は、次の置き場所にあります。

| 知りたいこと | 置き場所 |
|---|---|
| アニメーションで守る条件 | [アニメーションで守ること](../reference/Animation.md) |
| Flutter との差異を作ってよい条件と、記録する5点 | [APIDesign](../APIDesign.md) の「判断原則: Flutter 由来の observable behavior に差異を作らない」 |
| 兄弟の型を同じファイルに置く規約 | APIDesign の「2.5 ファイル構成 — 兄弟ウィジェットは同じファイルに置く」 |
| 通知を `event` で書く規約 | APIDesign の「3.6 リスナーパターンを実装せず `event` を使う」 |
| テストの命名と観点、PR の流れ | [CONTRIBUTING.md](../../../CONTRIBUTING.md) |
| Widget と RenderObject を移植する手順 | [Flutter のウィジェットを移植する](PortWidget.md) |

## 規約

必須の規約は次のとおりです。

- `Curve` の派生型は `record` で書き、`TransformInternal` だけを実装すること
- Flutter の `assert` は `Debug.Assert` に置き換えること(`src/FloatSoda/Animation/Curve.cs` の `Interval` と同じ)
- Flutter の公式テストに対応するケースを、FloatSoda でもテストすること(CONTRIBUTING.md の「テスト観点」)
- 値を購読する RenderObject は、`Attach` で購読し、`Detach` で解除すること
- 値を購読する RenderObject は、描画に使う値が変わったときだけ `MarkNeedsPaint` を呼ぶこと

推奨の規約は次のとおりです。

- Flutter を参照した PR では、参照した Flutter の commit・ソース・テストを PR の `## Flutter reference` 欄に書くことが望ましい
- 標準のインスタンスを持つ `Curve` は、`Curves` に `public static readonly ICurve` として足すことが望ましい

## 手順

### 手順1: Flutter の実装とテストを特定する

部品の種類ごとに、Flutter のソースと公式テストは次の場所にあります。パスは `packages/flutter/` からの相対です。

| 部品 | Flutter のソース | 公式テスト |
|---|---|---|
| `Curve` | `lib/src/animation/curves.dart` | `test/animation/curves_test.dart` |
| `AnimationController` と `Tween` の系統 | `lib/src/animation/` | `test/animation/` |
| `Simulation` | `lib/src/physics/` | `test/physics/` |
| `*Transition` の Widget | `lib/src/widgets/transitions.dart` | `test/widgets/transitions_test.dart` |

ローカル参照クローン(`~/code_reading/flutter_reference`)で、クラスの場所を確かめます。

```bash
grep -rnE "^(abstract |base |final |sealed )*class Split\b" ~/code_reading/flutter_reference/src/flutter/packages/flutter/lib/src/
```

結果として、移植するメンバーと、検証するケースの一覧ができます。移植する部品が FloatSoda にまだない型(`Tween` など)に依存するときは、その型を先に移植するか、依存するメンバーを今回の範囲から外します。

### 手順2: 置き場所を決める

| 部品 | 置き場所 |
|---|---|
| `Curve` の派生型 | `src/FloatSoda/Animation/Curve.cs`(兄弟の型と同じファイル) |
| `Curves` の標準インスタンス | `src/FloatSoda/Animation/Curves.cs` |
| `ISimulation` の実装 | `src/FloatSoda/Animation/Simulation.cs` |
| `*Transition` の Widget | `src/FloatSoda/Widgets/Animation/` |
| 値を購読する RenderObject | `src/FloatSoda/RenderObjects/Animation/` |

### 手順3: 部品を書く

**`Curve`** は、`Curve` を継承する `record` にし、`TransformInternal(double t)` を実装します。`t` が 0 と 1 のときは基底の `Transform` が先に返すので、`TransformInternal` には 0 と 1 の間の値だけが届きます。Flutter の引数の既定値が `Curve` のインスタンスのときは、C# の既定値に定数しか書けないため、`ICurve?` にして `null` のときに既定のカーブを使います(`Interval` の `CurveValue` と同じ)。プロパティ名が型名と同じになるときは、`Value` を付けて避けます(`Threshold` の `ThresholdValue` と同じ)。

**`ISimulation`** は、`X(time)`(値)、`Dx(time)`(1秒あたりの変化量)、`IsDone(time)`(終わったか)を実装します。`time` はシミュレーションの開始からの秒数です。作った実装は `AnimationController.AnimateWith` に渡して動かします。

**`*Transition` の Widget** は、`IAnimation<T>` を受け取る Widget と、それを購読する RenderObject の組で作ります。実例は `FadeTransition` と `RenderAnimatedOpacity` です。Widget と RenderObject の書き方は、[Flutter のウィジェットを移植する](PortWidget.md) の「Flutter のウィジェットを移植する」に従います。

`dotnet build` がエラーなしで通れば、この手順は終わりです。

### 手順4: テストを書く

テストは `tests/FloatSoda.Test/Animation/` に置きます。部品ごとの確かめ方は次のとおりです。

| 部品 | 確かめ方 |
|---|---|
| `Curve` | `CurveTest.cs` の `EndpointCurves` に足して端点を確かめ、Flutter のテストと同じ時刻と値で `Transform` を確かめる |
| `ISimulation` | `X`、`Dx`、`IsDone` を時刻ごとに確かめる。`AnimationController.AnimateWith` と `FakeFrameScheduler` で、値が進むことも確かめる |
| `*Transition` | `FakeFrameScheduler` で `AnimationController` を進め、RenderObject の dirty の状態と描画の結果を確かめる(`tests/FloatSoda.Test/RenderObjects/RenderAnimatedOpacityTest.cs`) |

`FakeFrameScheduler` の使い方は、[アニメーションの仕組みと理由](../explanation/Animation.md) の「テストでの駆動」にあります。

`dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~FloatSoda.Test.Animation"` がすべて通れば、この手順は終わりです。

### 手順5: 差異を記録して PR を出す

Flutter と違う挙動を作った場合は、APIDesign の「差異が必要な場合の記録義務」の5点を、台帳([`known-divergences.md`](../../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md))に追記します。CONTRIBUTING.md の「ブランチ・PRフロー」にある CI と同じコマンドを実行し、すべて通ったら PR を出します。新しい `public` API を足す PR は、マージの前にジュニアコーダーテストを通します(CONTRIBUTING.md の「新しい public API を追加する場合の受け入れ条件」)。

## 規約が守られる場所

| 規約 | 検出する仕組み |
|---|---|
| `Curve` の端点 | `CurveTest.cs` の `Transform_PreservesEndpoints`。ただし、`EndpointCurves` に足した型だけを確かめる |
| `Curve` の `ToString()` が終わること | `tests/FloatSoda.Test/RecordToStringTest.cs` |
| ビルドとテストが通ること | CI(`.github/workflows/ci.yml`) |
| `Attach` と `Detach` での購読、値が変わったときだけの `MarkNeedsPaint` | レビュー([REVIEW.md](../../../REVIEW.md) の「4」と「5. テスト」) |
| Flutter の公式テストへの対応 | レビュー(REVIEW.md の「5. テスト」) |

## 関連

- [アニメーションで守ること](../reference/Animation.md) — アニメーションで守る条件
- [アニメーションの仕組みと理由](../explanation/Animation.md) — `WidgetTicker`、`AnimationController`、`Curve` の仕組み
