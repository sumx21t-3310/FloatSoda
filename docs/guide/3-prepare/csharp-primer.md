← [ガイド](../Home.md)

# C# がほぼ初めての人へ

FloatSoda のコードを読むのに要る C# の書き方は、オブジェクト初期化子と `init`、`record` と `with`、ラムダの3つです。このページは、この3つだけを FloatSoda の実際のコードで説明し、AI が書いたコードを読んで直せるようにします。変数、関数、`if` は分かっている前提で書いています。

型システム、LINQ、非同期処理(`async` / `await`)、ジェネリクスは、このページでは扱いません。必要になったときに、Microsoft Learn の該当ページを引いてください。分からない言葉は、リファレンスの[用語集](../../reference/glossary.md)でも引けます。

## このページで分かるようになること

AI が書いた FloatSoda のコードには、次の3つの形がたくさん出てきます。

| コードの形 | 読めるようになること | 節 |
| :-- | :-- | :-- |
| `new Align { Child = ... }` | 作りながら値を設定する書き方 | 値を設定しながら作る書き方 |
| `style with { FontSize = 60 }` | 一部だけ変えたコピーを作る書き方 | 値の入れ物と複製 |
| `OnTap = () => ...` | 押されたときの処理を渡す書き方 | 押されたときの処理を渡す書き方 |

自分が止まった形の節へ進んでください。

## 値を設定しながら作る書き方(オブジェクト初期化子と `init`)

`new 型 { 名前 = 値 }` の形は、部品を作りながら、プロパティに値を入れる書き方です。この形をオブジェクト初期化子と呼びます。FloatSoda のほとんどのコードは、この形でできています。

README の最小のコードを分解して読みます。

```csharp
Widget root = new Align
{
    Child = new SizedBox
    {
        Width = 100,
        Height = 100,
        Child = new ColoredBox
        {
            Color = Colors.Tomato
        }
    }
};
```

`new Align { ... }` で `Align` という部品を作ります。`{ }` の中の `Child = ...` は、`Align` の `Child` に値を入れる指定です。`Child` には別の部品が入るので、`new SizedBox { ... }` が入れ子になります。`SizedBox` の `Width = 100` と `Height = 100` は大きさの指定で、その `Child` に `ColoredBox` が入ります。

つまり、外側が親、`Child` に入っている内側が子です。この例では、`Align` の子が `SizedBox`、`SizedBox` の子が `ColoredBox` です。

FloatSoda の部品のプロパティは、`init` で宣言されています。`init` のプロパティは、作るとき(`{ }` の中)にだけ値を設定でき、作ったあとに別の値を代入し直せません。ただし、プロパティに入れたリストのような、中身を書き換えられる型の中身までは、固定されません。画面を変えたいときは、値を書き換えるのではなく、新しい部品を作り直します。その仕組みは[状態と作り直し](../4-build/mechanics/state-and-rebuild.md)で説明しています。

`required` が付いたプロパティは、作るとき必ず設定します。たとえば、ウィンドウの `Title` は `required` なので、`new DashboardWindow { Child = root }` のように書くと、ビルドでエラーになります。

**できた?** チュートリアルのコードを見て、入れ子になった部品のどれが親でどれが子かを言える。

<details>
<summary>できなかったら</summary>

括弧の対応が分からないときは、`{` と `}` の組を探して、1つの `new` が閉じる位置を見てください。「部品」という言葉が分からないときは、用語集の[ウィジェット](../../reference/glossary.md)を引いてください。書き方そのものの詳しい説明は、Microsoft Learn の[オブジェクト初期化子とコレクション初期化子](https://learn.microsoft.com/ja-jp/dotnet/csharp/programming-guide/classes-and-structs/object-and-collection-initializers)と、[init キーワード](https://learn.microsoft.com/ja-jp/dotnet/csharp/language-reference/keywords/init)にあります。

</details>

<!-- ※要確認: Microsoft Learn の URL が実在するか。確かめ方: ブラウザーで3つの URL(オブジェクト初期化子、init、record、ラムダ)を開く -->

## 値の入れ物と複製(`record` と `with`)

`record` は、値を入れておく型を宣言する書き方です。FloatSoda の部品と設定は、`record` で宣言されています。`record` の値どうしは、プロパティの値を1つずつ比べて、すべて等しければ等しいものとして扱われます。`List<T>` のように、中身が同じでも別のインスタンスなら等しくならない型のプロパティは、同じインスタンスのときだけ等しくなります。

`with` は、`record` の値から、一部だけ変えたコピーを作る書き方です。元の値は変わりません。

```csharp
var options = new FloatSodaOptions();
var fast = options with { TargetFrameRate = 90 };
```

`options` の `TargetFrameRate` は既定の 60 のままで、`fast` は 90 になります。`TextStyle` のように、書式をまとめた `record` でも同じです。

```csharp
var title = new TextStyle { FontSize = 60, FontWeight = 700 };
var subtitle = title with { FontSize = 30 };
```

`subtitle` は、`title` の `FontWeight` を引き継いで、`FontSize` だけを 30 にしたコピーです。

**できた?** `with` を使ったあと、元の値が変わらず、新しい値ができると言える。

<details>
<summary>できなかったら</summary>

`record` の詳しい説明は、Microsoft Learn の[record 型](https://learn.microsoft.com/ja-jp/dotnet/csharp/language-reference/builtin-types/record)にあります。

</details>

<!-- ※要確認: with が利用者のコードに出てくる頻度。src/ では TextStyle や FloatSodaOptions に with を使えることは確かめたが、samples/ の利用者向けコードに with は出てこない。確かめ方: ジュニアコーダーテストで AI が書いたコードに with が出るかを数える -->

## 押されたときの処理を渡す書き方(ラムダ)

`() => ...` は、名前のない小さな関数を、その場で書く書き方です。ラムダと呼びます。FloatSoda では、ボタンが押されたときの処理を渡すときに使います。

`samples/Showcase/FloatSoda.Samples.OverlayApp/CounterWidget.cs` に、次の1行があります。

```csharp
OnTap = () => SetState(() => _count = 0),
```

`OnTap` は、`GestureDetector` のプロパティで、タップされたときに実行する処理を受け取ります。右辺の `() => SetState(...)` が、その処理です。`()` は引数がないことを表し、`=>` の右が関数の中身です。この1行は、「タップされたら、`SetState` を呼んで、`_count` を 0 にする」という意味です。内側の `() => _count = 0` も、同じラムダの形です。

FloatSoda では、イベントの処理を受け取るプロパティの名前は `On` で始まります。型は、引数がなければ `Action?`、引数が1つあれば `Action<T>?` です。`?` が付いた型は、`null`(何も渡さない状態)を許します。イベントのプロパティは `null` が既定なので、使わない処理は書かずに済みます。

**できた?** 内側のラムダ `() => _count = 0` が、`_count` を 0 にする処理だと言える。

<details>
<summary>できなかったら</summary>

`OnTap` に書いた処理が実行されないときは、そのウィンドウがダッシュボードかを確かめてください。ワールド座標固定と追従のオーバーレイでは、`OnTap` は呼ばれません。切り分けは[反応しないときに切り分ける](../5-troubleshoot/not-responding.md)にあります。ラムダの詳しい説明は、Microsoft Learn の[ラムダ式](https://learn.microsoft.com/ja-jp/dotnet/csharp/language-reference/operators/lambda-expressions)にあります。

</details>

## これより先は

この3つ以外で分からない書き方に出会ったら、次の3つの調べ先があります。

- リファレンスの[用語集](../../reference/glossary.md)で、言葉の意味を引く。
- Microsoft Learn で、C# の書き方を引く。
- AI に、分からない行を貼って「この行を説明して」と頼む。

読めるようになったら、チュートリアル[最初のオーバーレイを作る](../../tutorials/first-overlay.md)で、実際にコードを書いてください。

