# フレームワーク(コア): 貢献までの流れ

このチュートリアルでは、Flutter の `RelativeRect` を FloatSoda のジオメトリの値型として移植し、テストを付けて PR を出すところまで進めます。終えると、Flutter の型を読んで C# の `readonly record struct` に置き換える流れと、コアの層の PR に要るものがひととおり分かります。

> ※要確認 — `RelativeRect` は、FloatSoda にまだない値型の中から選んだ題材です。移植を追跡する Issue はまだなく、取り込むかどうかはオーナーが決めます。

## 作るもの

作るのは、次の2つのファイルです。

- `src/FloatSoda/Geometrics/RelativeRect.cs` — 親の矩形の各辺から子の矩形の各辺までの距離で、子の位置を表す値型
- `tests/FloatSoda.Test/Geometrics/RelativeRectTest.cs` — Flutter の公式テストに対応するテスト

`RelativeRect(10, 20, 30, 40)` は、「親の左から 10、上から 20、右から 30、下から 40 だけ内側にある矩形」を表します。Flutter では、`Positioned` を親からの距離で指定するときにこの型を使います。

## 前提

- リポジトリを clone し、`dotnet build` と `dotnet test` が通る状態になっている([CONTRIBUTING.md](../../../../CONTRIBUTING.md) の「開発環境のセットアップ」)
- Flutter のソースを読める(ローカルに clone していなければ、GitHub の `flutter/flutter` で同じファイルを開く)
- C# の `record struct` を書いたことがある

## Step 1: Issue を立ててブランチを切る

新しい `public` な型を足すので、先に Issue を立てます。Issue が要る変更の範囲は CONTRIBUTING.md の「Issue が必須な変更」に、ラベルの付け方は「タイトルとラベル」にあります。

ブランチ名は `<Issue 番号>-<主な領域>-<内容>` の形にします(CONTRIBUTING.md の「ブランチ名」)。Issue が #400 なら、`400-<領域>-port-relative-rect` のようになります。`<領域>` には Issue の `area:*` ラベルから1つを選びます。

`git branch --show-current` で、作ったブランチ名が表示されれば完了です。

## Step 2: Flutter の RelativeRect を読む

Flutter の `RelativeRect` は `packages/flutter/lib/src/rendering/stack.dart` にあり、テストは `packages/flutter/test/rendering/relative_rect_test.dart` にあります。ローカル参照クローン(`~/code_reading/flutter_reference`)では、次のコマンドで場所を確かめられます。

```bash
grep -rn "^class RelativeRect" ~/code_reading/flutter_reference/src/flutter/packages/flutter/lib/src/
```

Flutter の `RelativeRect` には、次のメンバーがあります。このチュートリアルで移植するのは、表の「移植する」の行です。

| Flutter のメンバー | このチュートリアル | 理由 |
|---|---|---|
| `fromLTRB`、`left`・`top`・`right`・`bottom` | 移植する | プライマリコンストラクタにする |
| `fill` | 移植する | 静的フィールド `Fill` にする |
| `hasInsets`、`shift`、`inflate`、`deflate`、`intersect`、`toSize` | 移植する | FloatSoda にある型(`Offset`、`Size`)だけで書ける |
| `fromSize`、`fromRect`、`toRect` | 移植しない | FloatSoda に `Rect` 型がない |
| `fromDirectional` | 移植しない | RTL のレイアウトは対象外([Localization](../../Localization.md) の「6」、台帳の #13) |
| `lerp` | 移植しない | 補間を使う `Tween` がまだない(追跡: [#137](https://github.com/sumx21t-3310/FloatSoda/issues/137)) |

移植しないメンバーは、PR の本文の `## Scope / follow-up` に書きます。

## Step 3: 値型を書く

置き場所は `src/FloatSoda/Geometrics/` です。この型はフレームワークの中でだけ使うためです。置き場所の決め方は [コアの HowTo](HowTo.md) の「手順1」にあります。

`src/FloatSoda/Geometrics/RelativeRect.cs` を作り、次のように書きます。

```csharp
using FloatSoda.Abstractions.Geometries;

namespace FloatSoda.Geometrics;

/// <summary>親の矩形の各辺から、子の矩形の各辺までの距離で、子の位置を表す不変値です。</summary>
/// <param name="Left">親の左辺から子の左辺までの距離。単位は論理ピクセルです。</param>
/// <param name="Top">親の上辺から子の上辺までの距離。単位は論理ピクセルです。</param>
/// <param name="Right">親の右辺から子の右辺までの距離。単位は論理ピクセルです。</param>
/// <param name="Bottom">親の下辺から子の下辺までの距離。単位は論理ピクセルです。</param>
public readonly record struct RelativeRect(double Left, double Top, double Right, double Bottom)
{
    /// <summary>親の矩形全体を覆う値を取得します。</summary>
    public static readonly RelativeRect Fill = new(0, 0, 0, 0);

    /// <summary>いずれかの辺の距離が正かどうかを取得します。</summary>
    public bool HasInsets => Left > 0 || Top > 0 || Right > 0 || Bottom > 0;

    /// <summary>子の矩形を、指定した量だけ移動した値を返します。</summary>
    /// <param name="offset">移動する量。</param>
    /// <returns>移動した後の値。</returns>
    public RelativeRect Shift(Offset offset) =>
        new(Left + offset.X, Top + offset.Y, Right - offset.X, Bottom - offset.Y);

    /// <summary>子の矩形を、各辺で指定した量だけ外側へ広げた値を返します。</summary>
    /// <param name="delta">広げる量。単位は論理ピクセルです。</param>
    /// <returns>広げた後の値。</returns>
    public RelativeRect Inflate(double delta) =>
        new(Left - delta, Top - delta, Right - delta, Bottom - delta);

    /// <summary>子の矩形を、各辺で指定した量だけ内側へ縮めた値を返します。</summary>
    /// <param name="delta">縮める量。単位は論理ピクセルです。</param>
    /// <returns>縮めた後の値。</returns>
    public RelativeRect Deflate(double delta) => Inflate(-delta);

    /// <summary>2つの値が表す矩形の重なりを返します。</summary>
    /// <param name="other">重ねる値。</param>
    /// <returns>各辺の距離の大きいほうを採った値。</returns>
    public RelativeRect Intersect(RelativeRect other) => new(
        Math.Max(Left, other.Left),
        Math.Max(Top, other.Top),
        Math.Max(Right, other.Right),
        Math.Max(Bottom, other.Bottom));

    /// <summary>指定した大きさの親に置いたときの、子の大きさを返します。</summary>
    /// <param name="container">親の大きさ。</param>
    /// <returns>子の幅と高さ。</returns>
    public Size ToSize(Size container) =>
        new(container.Width - Left - Right, container.Height - Top - Bottom);
}
```

Flutter のコードとの違いは、C# の書き方に合わせた次の3点です。どれも挙動の差異ではありません([REVIEW.md](../../../../REVIEW.md) の「6. Flutter parity」)。

- `record struct` が値で比べる実装を作るので、`==` と `hashCode` は書かない
- 実数は `double` にする([APIDesign](../../APIDesign.md) の「8.5」)
- メソッド名を PascalCase にする

`dotnet build` がエラーなしで通れば完了です。

## Step 4: ToString が終わることを確かめる

record が自動で作る `ToString()` は、`public` なプロパティをすべて出力します。自分と同じ型を返す計算プロパティがあると、出力が終わらなくなります。`RelativeRect` の計算プロパティは `bool` を返す `HasInsets` だけで、同じ型を返すのはメソッドなので、`PrintMembers` は要りません。

次のコマンドで確かめます。

```bash
dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~RecordToStringTest"
```

すべて合格すれば完了です。もし `Flipped` のような同じ型を返すプロパティを足したら、このテストが落ちます。そのときの直し方は [コアの HowTo](HowTo.md) の「手順4」にあります。

## Step 5: テストを書く

Flutter の `relative_rect_test.dart` のうち、移植したメンバーのテストを C# に置き換えます。値も Flutter のテストと同じものを使います。加えて、境界のケースとして、負の距離だけの値で `HasInsets` が `false` になることを確かめます。

`tests/FloatSoda.Test/Geometrics/RelativeRectTest.cs` を作ります。

```csharp
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;

namespace FloatSoda.Test.Geometrics;

public class RelativeRectTest
{
    [Fact]
    public void Shift_オフセットを渡す_左上へ足し右下から引く()
    {
        var rect = new RelativeRect(10, 20, 30, 40);

        Assert.Equal(new RelativeRect(15, 70, 25, -10), rect.Shift(new Offset(5, 50)));
    }

    [Fact]
    public void Inflate_5を渡す_各辺の距離を5ずつ減らす()
    {
        var rect = new RelativeRect(10, 20, 30, 40);

        Assert.Equal(new RelativeRect(5, 15, 25, 35), rect.Inflate(5));
    }

    [Fact]
    public void Deflate_5を渡す_各辺の距離を5ずつ増やす()
    {
        var rect = new RelativeRect(10, 20, 30, 40);

        Assert.Equal(new RelativeRect(15, 25, 35, 45), rect.Deflate(5));
    }

    [Fact]
    public void Intersect_2つの値_順序によらず各辺の大きいほうを返す()
    {
        var a = new RelativeRect(10, 20, 30, 40);
        var b = new RelativeRect(0, 30, 60, 0);

        Assert.Equal(new RelativeRect(10, 30, 60, 40), a.Intersect(b));
        Assert.Equal(a.Intersect(b), b.Intersect(a));
    }

    [Fact]
    public void ToSize_親の大きさを渡す_子の大きさを返す()
    {
        var rect = new RelativeRect(10, 20, 30, 40);

        Assert.Equal(new Size(40, 100), rect.ToSize(new Size(80, 160)));
    }

    [Fact]
    public void HasInsets_Fill_falseを返す()
    {
        Assert.False(RelativeRect.Fill.HasInsets);
    }

    [Fact]
    public void HasInsets_負の距離だけ_falseを返す()
    {
        Assert.False(new RelativeRect(-1, -1, -1, -1).HasInsets);
    }
}
```

テストメソッド名は `対象メンバー名_条件_期待結果` の形にし、条件と期待結果を日本語で書きます(CONTRIBUTING.md の「テストの命名」)。何を検証するかの観点は、CONTRIBUTING.md の「テスト観点」にあります。

```bash
dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~RelativeRectTest"
```

7件すべて合格すれば完了です。

## Step 6: CI と同じ手順で確かめる

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
| `## Flutter reference` | 参照した Flutter の commit、`rendering/stack.dart`、`test/rendering/relative_rect_test.dart` |
| `## Breaking changes` | 型を足しただけで、既存の API は変えていないこと |
| `## Tests` | `RelativeRectTest` の7件と、Flutter のテストとの対応 |
| `## Scope / follow-up` | Step 2 で移植しなかったメンバーと、その理由 |

新しい `public` な型を足す PR は、マージの前にジュニアコーダーテストを通します(CONTRIBUTING.md の「新しい public API を追加する場合の受け入れ条件」)。PR を出す前の確認項目は、CONTRIBUTING.md の「PRを出す前のチェックリスト」にあります。

## 次に読むもの

- [コアの HowTo](HowTo.md) — 値型を足す作業の手順と規約で、題材を変えて2つ目の型を足すときに使う
- [コアの Reference](Reference.md) — 値型で守る条件と、破ったときに起きること
- [コアの Explanation](Explanation.md) — コアの層がフレームをどう回しているか
