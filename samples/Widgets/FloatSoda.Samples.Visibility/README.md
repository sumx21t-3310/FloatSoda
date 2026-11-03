# Visibility

## これは何か

このサンプルは、子を隠す・切り替える・90度ずつ回す4つのウィジェットを並べて見せます。いずれも Flutter の同名のウィジェットに対応します。

| ウィジェット | すること |
|---|---|
| `Visibility` | `Visible` が `false` のとき、子の代わりに `Replacement` を出す |
| `Offstage` | `IsOffstage` が `true` のとき、子を描かず、場所も取らない |
| `IndexedStack` | 子を重ね、`Index` の位置の子だけを描く。大きさは一番大きい子に合わせる |
| `RotatedBox` | 子を `QuarterTurns` の数だけ、時計回りに90度ずつ回す |

Unity の uGUI でいえば、`GameObject.SetActive(false)` で部品を隠したり、タブの中身を1枚だけ出したりする場面に当たります。

## 使い方

### Visibility で子を隠す

`Visible = false` にすると、`Child` の代わりに `Replacement` を出します。`Replacement` を省略すると大きさ0の `SizedBox` になるため、隠した子の場所は詰まります。サンプルでは、青と黄の四角の間に置いた赤い四角が消え、青と黄が隙間なく並びます。

```csharp
new VisibilityWidget
{
    Visible = false,
    Child = Marker(new Color(255, 111, 97))
},
```

このサンプルは名前空間が `FloatSoda.Samples.Visibility` で型名 `Visibility` と衝突するため、`VisibilityWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new Visibility { ... }` と書けます。

隠したときに別のものを出したいときは、`Replacement` を指定します。

```csharp
new VisibilityWidget
{
    Visible = false,
    Child = Marker(new Color(255, 111, 97)),
    Replacement = Marker(new Color(90, 98, 117))
},
```

`Visibility` は、隠しているあいだ `Child` をウィジェットツリーから外します。そのため、`Child` の中の `StatefulWidget` の状態は、隠すたびに失われます。

### Offstage で描かずに置いておく

`IsOffstage = true` の子は、描かれず、場所も取りません。`IsOffstage` の既定値は `true` です。

```csharp
new Offstage
{
    IsOffstage = true,
    Child = Marker(new Color(255, 111, 97))
},
```

`Visibility` と違い、`Offstage` は子をツリーに残したまま隠します。子の状態は、隠しているあいだも保たれます。

### IndexedStack で1枚だけ出す

`Children` を重ね、`Index` の位置の子だけを描きます。大きさは、描いていない子も含めて一番大きい子に合わせます。サンプルでは、灰色の背景が一番大きい青い子(120×120)の大きさになり、その左上に `Index = 1` の赤い子(60×60)だけが描かれます。

```csharp
new ColoredBox
{
    Color = new Color(40, 47, 64),
    Child = new IndexedStack
    {
        Index = 1,
        Children =
        {
            new ColoredBox
            {
                Color = new Color(124, 205, 255),
                Child = new SizedBox { Width = 120, Height = 120 }
            },
            // ...
        }
    }
},
```

`Index` には、`Children` の中の位置か `null` を指定します。範囲の外の値を指定すると `ArgumentOutOfRangeException` を投げます。`null` を指定すると、どの子も描きません。子の置き場所は `Alignment`(既定は `Alignment.TopLeft`)で、子に渡す制約は `Fit` で変えられます。

### RotatedBox で90度ずつ回す

`QuarterTurns` の数だけ、子を時計回りに90度ずつ回します。回転はレイアウトの前に効くため、1回と3回では、子の幅と高さが入れ替わった大きさで場所を取ります。

```csharp
new RotatedBox { QuarterTurns = 0, Child = Tag("0回") },
new SizedBox { Width = 32 },
new RotatedBox { QuarterTurns = 1, Child = Tag("1回") },
```

90度単位でない角度で回すときは、レイアウトに影響しない `Transform` を使います。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| `Visibility` の指定できる項目 | `Visible` / `Child` / `Replacement` のみ | `maintainState` / `maintainAnimation` / `maintainSize` / `maintainSemantics` / `maintainInteractivity` / `maintainFocusability` も取る |
| 隠した子の状態を残す `Visibility` | **未提供**。状態を残したいときは `Offstage` を使う | `maintainState: true` や `Visibility.maintain` で残せる |
| `Offstage` のプロパティ名 | `IsOffstage` | `offstage` |
| `IndexedStack` の切り抜き | **未提供**。はみ出した子は切り抜かない | `clipBehavior`(既定は `Clip.hardEdge`)で切り抜く |
| `IndexedStack` の大きさの指定 | `Fit` | `sizing` |
| `IndexedStack` の既定の配置 | `Alignment.TopLeft` | `AlignmentDirectional.topStart`(書字方向で左右が変わる) |
| `RotatedBox.QuarterTurns` | 省略できる(既定は 0) | `quarterTurns` は必須 |

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/Widgets/FloatSoda.Samples.Visibility -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/Widgets/FloatSoda.Samples.Visibility
```

## 関連

- [FloatSoda.Samples.Stack](../FloatSoda.Samples.Stack) — 子を全部重ねて描く `Stack`
- [FloatSoda.Samples.Transform](../FloatSoda.Samples.Transform) — 任意の角度で回す `Transform`
- [docs/reference/find-widgets.md](../../../docs/reference/find-widgets.md) — 組み込みウィジェット一覧
