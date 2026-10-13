# Flutter のウィジェットを移植する

三つのツリーの層でよく行う3つの作業の手順を
このページに書くのは、作業の順序だけです。規約の本文は、次の置き場所にあります。

| 知りたいこと | 置き場所 |
|---|---|
| コアに受け入れるウィジェットの範囲 | [CONTRIBUTING.md](../../../CONTRIBUTING.md) の「受け入れ範囲」 |
| テストの命名と観点 | CONTRIBUTING.md の「テストの命名」「テスト観点」 |
| 層1サンプルの構成と README の型 | CONTRIBUTING.md の「サンプルを追加する場合の規約」 |
| Flutter との差異を作ってよい条件と、記録する5点 | [APIDesign](../APIDesign.md) の「判断原則: Flutter 由来の observable behavior に差異を作らない」 |
| 確認済みの Flutter との差異 | [`known-divergences.md`](../../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md)(以下、台帳) |
| ウィジェットの API の形 | APIDesign の「2. コンポーネントAPI設計ガイドライン」「3. プロパティ命名規則」 |
| ツリーで守る条件 | [三つのツリーで守ること](../reference/Trees.md) と [REVIEW.md](../../../REVIEW.md) の「4. FloatSoda 固有の不変条件」 |

## 規約

必須の規約は次のとおりです。

- コアに入れるのは、Flutter の `widgets/basic.dart` に当たるプリミティブ層のウィジェットだけにすること
- Flutter 由来のウィジェットは、Flutter の実装の分岐と公式テストを確かめ、対応するケースを FloatSoda でもテストすること
- Flutter と違う挙動を作るときは、APIDesign の記録義務の5点を台帳に記録すること
- 新しいウィジェットを足したら、層1サンプルを足すこと
- 新しい `public` API を足したら、docs を更新し、ジュニアコーダーテストを通すこと
- RenderObject のプロパティの setter は、値が変わったときだけ `MarkNeedsLayout` か `MarkNeedsPaint` を呼ぶこと

推奨の規約は次のとおりです。

- Flutter を参照した PR では、参照した Flutter の commit・ソース・テストを PR の `## Flutter reference` 欄に書くことが望ましい
- C# のシンボルをまたぐ変更(名前の変更、namespace の移動)には、文字列の置換ではなく IDE の semantic refactoring を使うことが望ましい

## Flutter のウィジェットを移植する

### 手順1: 受け入れ範囲と Issue を確かめる

移植するウィジェットが、CONTRIBUTING.md の「受け入れ範囲」に入るかを確かめます。単一の RenderObject の薄いラッパーで、状態やビジネスロジックを持たないものが対象です。対応する Issue がなければ立てます。Issue の番号から、ブランチ名が決まります(CONTRIBUTING.md の「ブランチ名」)。

### 手順2: Flutter の Widget・Element・RenderObject とテストを特定する

ローカル参照クローン(`~/code_reading/flutter_reference`)で、Widget クラスを探します。エージェントで作業するときは、`flutter-widget-source` スキルが Widget・Element・RenderObject の場所と permalink を返します。

```bash
grep -rnE "^(abstract |base |final |sealed )*class Padding\b" ~/code_reading/flutter_reference/src/flutter/packages/flutter/lib/src/widgets/
```

見つけた RenderObject のファイルから、プロパティ、既定値、`performLayout`、`paint`、`hitTestChildren`、`computeMinIntrinsicWidth` などを書き出します。公式テストは `packages/flutter/test/widgets/` と `packages/flutter/test/rendering/` にあります。結果として、移植するメンバーと、検証するケースの一覧ができます。

### 手順3: 台帳と既存の差異を確かめる

台帳を開き、移植するウィジェットと同じ仕組みに関わるエントリを探します。たとえば、`Stack` の系統なら #10、`Container` の系統なら #11 と #12 が関わります。差異を作るかどうかは、APIDesign の「判断原則: Flutter 由来の observable behavior に差異を作らない」で決めます。

### 手順4: RenderObject を書く

`src/FloatSoda/RenderObjects/<分類>/Render<名前>.cs` に置き、namespace を `FloatSoda.RenderObjects.<分類>` にします。分類は `Layout`、`Painting`、`Gesture`、`Animation` のどれかです。単一の子を持つなら `RenderProxyBox` を継承します。

プロパティの setter は、`src/FloatSoda/RenderObjects/Layout/RenderPadding.cs` と同じ形にします。

```csharp
public EdgeInsets Padding
{
    get => _padding;
    set
    {
        Validate(value);

        if (_padding == value) return;

        _padding = value;
        MarkNeedsLayout();
    }
}
```

レイアウトが変わる値なら `MarkNeedsLayout`、見た目だけが変わる値なら `MarkNeedsPaint` を呼びます。Flutter のコードにある Semantics の呼び出し(`markNeedsSemanticsUpdate` など)は削ります。`PerformLayout`、`Paint`、`HitTestChildren` で守る条件は、[三つのツリーで守ること](../reference/Trees.md) にあります。

### 手順5: Widget を書く

`src/FloatSoda/Widgets/<分類>/<名前>.cs` に、`SingleChildRenderObjectWidget<T>` などを継承した `record` を置きます。Widget 側の分類のディレクトリは `Layout`、`Paint`、`Gesture`、`Animation` で、描画系だけ RenderObject 側(`Painting`)と名前が違います。`CreateRenderObject` で RenderObject を作り、`UpdateRenderObject` で既存の RenderObject にプロパティを書き込みます。

```csharp
public record Padding : SingleChildRenderObjectWidget<RenderPadding>
{
    public required EdgeInsets Spacing { get; init; }

    public override RenderPadding CreateRenderObject() => new() { Padding = Spacing };

    public override void UpdateRenderObject(RenderPadding renderObject)
    {
        renderObject.Padding = Spacing;
    }
}
```

この形は `src/FloatSoda/Widgets/Layout/Padding.cs` から XML ドキュメントコメントを除いて抜き出したものです。`public` なメンバーには、日本語の XML ドキュメントコメントを付けます([DocumentationComments](../DocumentationComments.md))。

`dotnet build` がエラーなしで通れば、この手順は終わりです。

### 手順6: テストを書く

`tests/FloatSoda.Test/Widgets/<名前>Test.cs` と、必要なら `tests/FloatSoda.Test/RenderObjects/Render<名前>Test.cs` を作ります。観点は CONTRIBUTING.md の「テスト観点」から、変更に当てはまるものをすべて選びます。手順2で書き出した Flutter の公式テストのケースは、FloatSoda でも検証します。テストの組み立て方は、このページの「ヘッドレスでテストする」にあります。

`dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~<名前>"` がすべて通れば、この手順は終わりです。

### 手順7: 層1サンプルを足す

`samples/FloatSoda.Samples.<名前>/` に、`Program.cs`、`<名前>Demo.cs`、`README.md`、`checks/items.jsonl`、`.csproj` を置きます。構成と禁止事項は CONTRIBUTING.md の「サンプルを追加する場合の規約」に従います。参考実装は `samples/FloatSoda.Samples.Image` です。`checks/items.jsonl` は手で書かず、`floatsoda-device-test-gen` スキルで `<名前>Demo.cs` から生成します。

`dotnet run --project samples/FloatSoda.Samples.<名前> -- --desktop` でウィンドウが開き、`<名前>Demo.cs` に書いたケースが表示されれば、この手順は終わりです。`--desktop` でも SteamVR の起動と HMD の接続は必要です。

### 手順8: Flutter との差異を記録する

手順3で差異を作ると決めた場合は、APIDesign の「差異が必要な場合の記録義務」の5点を台帳に追記します。利用者から見える差異は、サンプルの README の `## Flutterとの違い` にも書きます。差異がなければ、README の `## Flutterとの違い` に「同等」と1行書きます。

### 手順9: docs を更新して PR を出す

新しい `public` API を説明するページを更新し、ジュニアコーダーテストを通します(CONTRIBUTING.md の「新しい public API を追加する場合の受け入れ条件」)。CONTRIBUTING.md の「ブランチ・PRフロー」にある CI と同じコマンドを実行し、すべて通ったら PR を出します。PR の本文の `## Flutter reference` には、手順2で読んだ Flutter のファイルと commit を書きます。

## カスタムクリッパーを実装する

クリップ系の Widget(`ClipRect`、`ClipRoundRect`、`ClipOval`、`ClipCustomPath`)は、`Clipper` に `CustomClipper<T>` を渡すと、クリップの形を自分で決められます。`T` は、`ClipCustomPath` なら `SKPath`、`ClipRect` と `ClipOval` なら `SKRect`、`ClipRoundRect` なら `SKRoundRect` です。

### 手順1: CustomClipper を継承する

`GetClip(SKSize size)` で、子のサイズに合わせた形を返します。次の例は、下の端が波打つ形を返します(`samples/FloatSoda.Samples.OverlayApp/ArcClipper.cs`)。

```csharp
public class ArcClipper : CustomClipper<SKPath>
{
    public override SKPath GetClip(SKSize size)
    {
        var path = new SKPath();
        path.LineTo(0f, size.Height - 30);
        path.QuadTo(
            new SKPoint(size.Width / 4, size.Height),
            new SKPoint(size.Width / 2, size.Height));
        path.QuadTo(
            new SKPoint(size.Width * 3 / 4, size.Height),
            new SKPoint(size.Width, size.Height - 30));
        path.LineTo(size.Width, 0);
        path.Close();
        return path;
    }

    public override bool ShouldReclip(CustomClipper<SKPath> oldClipper) => false;
}
```

### 手順2: ShouldReclip で、形を作り直す条件を返す

Widget を作り直すたびに新しいクリッパーを渡すと、`RenderCustomClip<T>` は古いクリッパーと比べます。型が違えば必ず描き直し、型が同じなら `ShouldReclip(oldClipper)` が `true` のときだけ描き直します。形を決める値(上の例なら波の高さ)をクリッパーに持たせるときは、その値が古いクリッパーと違う場合に `true` を返します。値を持たないクリッパーは、上の例のように `false` を返します。子のサイズが変わったときは、`ShouldReclip` によらず描き直します。

### 手順3: Widget に渡す

```csharp
new ClipCustomPath
{
    Clipper = new ArcClipper(),
    Child = new ColoredBox { Color = SKColors.Tomato },
}
```

`WidgetBitmapRenderer` で描き、クリップの外の画素が透明になっていれば完了です。確かめ方は次の節にあります。

## ヘッドレスでテストする

`FloatSoda.Testing` と `tests/FloatSoda.Test` の既存のテストの形を使うと、SteamVR も GPU のウィンドウも使わずにツリーを動かせます。確かめたいものに合わせて、次の表から形を選びます。

| 確かめたいもの | 使う形 | 実例 |
|---|---|---|
| 描画の結果(色、クリップ) | `WidgetBitmapRenderer` か `RenderObjectBitmapRenderer` | `tests/FloatSoda.Test/Widgets/ColoredBoxTest.cs` |
| レイアウトの結果と、Widget を更新したときの反映 | `RenderPipeline` と `RenderObjectToWidgetAdapter` で組み立てる | `tests/FloatSoda.Test/Widgets/PaddingTest.cs` |
| 差分更新の範囲(どこが dirty になるか) | RenderObject を直接組み、`NodesNeedingPaint` と `NeedsPaint` を見る | `tests/FloatSoda.Test/Widgets/RepaintBoundaryTest.cs` |
| ヒットテストの結果 | レイアウト後に `HitTest` を直接呼び、`HitTestResult.Path` を見る | `tests/FloatSoda.Test/RenderObjects/HitTestTest.cs` |
| ポインタ入力からジェスチャまで | `WidgetBinding` に偽の入力源をつなぐ | `tests/FloatSoda.Test/Core/PointerInputIntegrationTest.cs` |

### 描画の結果を確かめる

`WidgetBitmapRenderer.Render(widget, size)` は、Widget ツリーをビルド・レイアウト・描画して、ビットマップを返します。

```csharp
private static readonly WidgetBitmapRenderer Renderer = new();

[Fact]
public void PaintsCorrectColor()
{
    var widget = new SizedBox
    {
        Width = 100,
        Height = 100,
        Child = new ColoredBox { Color = SKColors.Blue }
    };

    using var bitmap = Renderer.Render(widget, new SKSizeI(100, 100));

    Assert.Equal(SKColors.Blue, bitmap.GetPixel(50, 50));
}
```

`Render` は、ビルド・レイアウト・描画を1回だけ実行します。`Image` のように非同期の読み込みを待つ Widget は、読み込みの前の状態で描かれます。画像まで描くには、先に同じプロバイダーで `await provider.ResolveAsync()` を実行し、返されたハンドルを描画が終わるまで持ちます。

### Widget を更新したときの反映を確かめる

`RenderView` と `RenderPipeline` を作り、`AttachToRenderTree` で Widget を接続します。2回目の `AttachToRenderTree` に同じ Element を渡すと、Widget の更新として扱われます。

```csharp
var view = new RenderView();
var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
view.PrepareInitialFrame();

var root = new RenderObjectToWidgetAdapter { Container = view, Child = initial }
    .AttachToRenderTree(new BuildOwner(() => { }), null);
pipeline.FlushLayout();

new RenderObjectToWidgetAdapter { Container = view, Child = updated }
    .AttachToRenderTree(root.Owner!, root);
root.Owner!.BuildScope();
pipeline.FlushLayout();
```

`view.Size` と、`view.Child` の RenderObject のプロパティで、更新が反映されたかを確かめます。

### ポインタ入力を確かめる

`WidgetBinding` の `internal` なコンストラクタ `WidgetBinding(RenderView)` と、`HandlePointerEvent`、`FlushBuildAndLayout` を使います。`internal` なメンバーは、`InternalsVisibleTo` で `FloatSoda.Test` だけに公開しているため、この形は `tests/FloatSoda.Test` の中でだけ使えます。偽の入力源(`IRawPointerSource` の実装)から `PointerController` を通してイベントを流す例は、`PointerInputIntegrationTest.cs` にあります。

### 観点の選び方

観点は CONTRIBUTING.md の「テスト観点」に従います。テストは observable behavior を確かめ、`private` なフィールドや呼び出し回数をなぞらないようにします。差分更新の範囲を確かめるテストでは、`RepaintBoundaryTest.cs` のように、`NodesNeedingPaint` と `NeedsPaint` の状態で確かめます。

## 規約が守られる場所

| 規約 | 検出する仕組み |
|---|---|
| ビルドとテストが通ること | CI(`.github/workflows/ci.yml`) |
| ファイルスコープの namespace | `.editorconfig` の `IDE0161` がビルド時に強制する |
| 受け入れ範囲、Flutter のテストへの対応、差異の記録 | レビュー([REVIEW.md](../../../REVIEW.md) の「5. テスト」「6. Flutter parity」) |
| 不要な `MarkNeedsLayout` / `MarkNeedsPaint` | レビュー(REVIEW.md の「4」の「差分更新」) |
| サンプルを足したこと、ジュニアコーダーテストを通したこと | PR を出す前のチェックリスト(CONTRIBUTING.md の「PRを出す前のチェックリスト」) |

## 関連

- [三つのツリーで守ること](../reference/Trees.md) — RenderObject とツリーで守る条件
- [三つのツリーの仕組みと理由](../explanation/Trees.md) — 三つのツリーの仕組み
- [RenderObjects](../RenderObjects.md) — 現行の RenderObject のページ
