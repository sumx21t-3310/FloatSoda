# フレームワーク(アニメーション): 貢献までの流れ

このチュートリアルでは、Flutter の `Split` カーブを FloatSoda に移植し、テストを付けて PR を出すところまで進めます。終えると、Flutter の `Curve` を読んで C# の `record` に置き換える流れと、アニメーションの層の PR に要るものがひととおり分かります。

> ※要確認 — `Split` は、Flutter の `curves.dart` にあって FloatSoda にまだない `Curve` の中から選んだ題材です。移植を追跡する Issue はまだなく、取り込むかどうかはオーナーが決めます。

## 作るもの

`Split` は、分割点の前と後で別のカーブを使うカーブです。`new Split(0.3)` は、時刻 0〜0.3 を直線で 0〜0.3 まで進み、0.3〜1 を `EaseOutCubic` で 0.3〜1 まで進みます。

変えるファイルは次の2つです。

- `src/FloatSoda/Animation/Curve.cs` — `Split` を足す
- `tests/FloatSoda.Test/Animation/CurveTest.cs` — `Split` のテストを足す

## 前提

- リポジトリを clone し、`dotnet build` と `dotnet test` が通る状態になっている([CONTRIBUTING.md](../../../../CONTRIBUTING.md) の「開発環境のセットアップ」)
- Flutter のソースを読める(ローカルに clone していなければ、GitHub の `flutter/flutter` で同じファイルを開く)

`Curve` の仕組みは、このチュートリアルでは一文ずつしか説明しません。詳しくは [アニメーションの Explanation](Explanation.md) にあります。

## Step 1: Issue を立ててブランチを切る

新しい `public` な型を足すので、先に Issue を立てます。Issue が要る変更の範囲は CONTRIBUTING.md の「Issue が必須な変更」にあります。ブランチ名は `<Issue 番号>-<主な領域>-<内容>` の形にします(CONTRIBUTING.md の「ブランチ名」)。

`git branch --show-current` で、作ったブランチ名が表示されれば完了です。

## Step 2: Flutter の Split を読む

Flutter の `Split` は `packages/flutter/lib/src/animation/curves.dart` にあり、テストは `packages/flutter/test/animation/curves_test.dart` にあります。ローカル参照クローン(`~/code_reading/flutter_reference`)では、次のコマンドで場所を確かめられます。

```bash
grep -rn "^class Split" ~/code_reading/flutter_reference/src/flutter/packages/flutter/lib/src/animation/
```

Flutter の `Split` を読むと、次の4点が分かります。

- フィールドは、分割点の `split`、前半の `beginCurve`(既定は `Curves.linear`)、後半の `endCurve`(既定は `Curves.easeOutCubic`)の3つ
- `t` が `split` と等しいときは、`split` を返す
- `t` が `split` より前なら、`beginCurve` の値を 0〜`split` の範囲に引き延ばす
- `t` が `split` より後なら、`endCurve` の値を `split`〜1 の範囲に引き延ばす

テストの `Split interpolates values properly` は、`Split(0.3)` について、時刻 0.1、0.25、0.3、0.5、0.75 の値を確かめています。

## Step 3: Split を書く

`src/FloatSoda/Animation/Curve.cs` を開き、`Interval` の後ろ(`Threshold` の前)に次の型を足します。`Curve` の兄弟の型は同じファイルに置きます([APIDesign](../../APIDesign.md) の「2.5 ファイル構成」)。

```csharp
/// <summary>
/// <see cref="SplitValue"/>の前は<see cref="BeginCurve"/>、後は<see cref="EndCurve"/>で進むカーブ。
/// 分割点では、分割点と同じ値を返します。FlutterのSplit相当。
/// </summary>
/// <param name="SplitValue">2つのカーブを切り替える時刻。0以上1以下です。</param>
/// <param name="BeginCurve">分割点より前に使うカーブ。<see langword="null"/>の場合は<see cref="Curves.Linear"/>です。</param>
/// <param name="EndCurve">分割点より後に使うカーブ。<see langword="null"/>の場合は<see cref="Curves.EaseOutCubic"/>です。</param>
public record Split(double SplitValue, ICurve? BeginCurve = null, ICurve? EndCurve = null) : Curve
{
    /// <inheritdoc/>
    protected override double TransformInternal(double t)
    {
        Debug.Assert(SplitValue >= 0.0);
        Debug.Assert(SplitValue <= 1.0);

        if (t == SplitValue) return SplitValue;

        if (t < SplitValue)
        {
            var progress = t / SplitValue;
            return SplitValue * (BeginCurve ?? Curves.Linear).Transform(progress);
        }

        var endProgress = (t - SplitValue) / (1.0 - SplitValue);
        return SplitValue + (1.0 - SplitValue) * (EndCurve ?? Curves.EaseOutCubic).Transform(endProgress);
    }
}
```

Flutter のコードとの違いは、C# の書き方に合わせた次の4点です。どれも挙動の差異ではありません。

- `t` が 0 と 1 のときの分岐を書かない(基底の `Curve.Transform` が先に返すため、`TransformInternal` には 0 と 1 の間の値だけが届く)
- `Threshold` の `ThresholdValue` と同じく、分割点のプロパティ名を `SplitValue` にする(C# では、メンバー名を型名と同じ `Split` にできない)
- `Interval` の `CurveValue` と同じく、既定のカーブを `ICurve?` の `null` で表し、使うときに `Curves.Linear` などへ置き換える(C# の引数の既定値には定数しか書けない)
- Flutter の `assert` を `Debug.Assert` にする

`dotnet build` がエラーなしで通れば完了です。

## Step 4: テストを書く

`tests/FloatSoda.Test/Animation/CurveTest.cs` に、2か所を足します。

1つ目は、端点を確かめる `EndpointCurves` への追加です。リストの末尾に `new Split(0.3)` を足すと、`Transform_PreservesEndpoints` が 0 と 1 の値を確かめます。

```csharp
        new Threshold(0.5),
        new Split(0.3),
    ];
```

2つ目は、Flutter の `Split interpolates values properly` に対応するテストです。値は Flutter のテストと同じものを使い、許容誤差も Flutter と同じ `1e-6` にします。

```csharp
[Fact]
public void Split_分割点より前_BeginCurveで0から分割点まで進む()
{
    var curve = new Split(0.3);

    Assert.Equal(0.1, curve.Transform(0.1), tolerance: 1e-10);
    Assert.Equal(0.25, curve.Transform(0.25), tolerance: 1e-10);
}

[Fact]
public void Split_分割点_分割点と同じ値を返す()
{
    Assert.Equal(0.3, new Split(0.3).Transform(0.3));
}

[Fact]
public void Split_分割点より後_EndCurveで分割点から1まで進む()
{
    var curve = new Split(0.3);

    Assert.Equal(0.760461, curve.Transform(0.5), tolerance: 1e-6);
    Assert.Equal(0.962055, curve.Transform(0.75), tolerance: 1e-6);
}
```

テストメソッド名は `対象メンバー名_条件_期待結果` の形にし、条件と期待結果を日本語で書きます(CONTRIBUTING.md の「テストの命名」)。同じファイルの既存のテストは英語の名前ですが、一括で改名はしません。

次のコマンドで、`CurveTest` と、`ToString()` が終わることを確かめる `RecordToStringTest` を実行します。

```bash
dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~CurveTest|FullyQualifiedName~RecordToStringTest"
```

すべて合格すれば完了です。

## Step 5: CI と同じ手順で確かめる

CONTRIBUTING.md の「ブランチ・PRフロー」にある、CI と同じ3つのコマンドを実行します。

```bash
dotnet build --configuration Release
dotnet test tests/FloatSoda.Rendering.Test --configuration Release --no-build
dotnet test tests/FloatSoda.Test --configuration Release --no-build
```

3つとも成功すれば完了です。

## PR を出す

PR の本文は [`.github/PULL_REQUEST_TEMPLATE.md`](../../../../.github/PULL_REQUEST_TEMPLATE.md) に沿って書きます。この題材で書く内容は次のとおりです。

| 欄 | 書く内容 |
|---|---|
| `## Flutter reference` | 参照した Flutter の commit、`lib/src/animation/curves.dart`、`test/animation/curves_test.dart` |
| `## Breaking changes` | 型を足しただけで、既存の API は変えていないこと |
| `## Tests` | 足した3件と、`EndpointCurves` への追加。Flutter のテストとの対応 |
| `## Scope / follow-up` | `Curves` に標準のインスタンスを足していないこと(Flutter の `Curves` にも `Split` のインスタンスはない) |

新しい `public` な型を足す PR は、マージの前にジュニアコーダーテストを通します(CONTRIBUTING.md の「新しい public API を追加する場合の受け入れ条件」)。PR を出す前の確認項目は、CONTRIBUTING.md の「PRを出す前のチェックリスト」にあります。

## 次に読むもの

- [アニメーションの HowTo](HowTo.md) — `Curve` 以外の部品(`ISimulation`、`*Transition`)を移植する手順
- [アニメーションの Reference](Reference.md) — アニメーションで守る条件
- [アニメーションの Explanation](Explanation.md) — `WidgetTicker` とフレームの関係
