# Padding

## これは何か

`Padding` は子の周囲に内側の余白を確保するウィジェットです。Flutter の `Padding` に対応します。

余白のぶんだけ子へ渡す領域を狭め、自身は「子の寸法 + 余白」の大きさになります。余白の量は `EdgeInsets` で辺ごとに指定します。

## 使い方

### 四辺へ同じ余白を付ける

`EdgeInsets.All` で全辺に同じ量を指定します。余白は `Padding` の内側に付くため、子は余白のぶん小さい領域を受け取ります。

```csharp
new PaddingWidget
{
    Spacing = EdgeInsets.All(16),
    Child = Fill(new Color(124, 205, 255))
}
```

このサンプルは名前空間が `FloatSoda.Samples.Padding` で型名 `Padding` と衝突するため、`PaddingWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new Padding { ... }` と書けます。

### 縦横で別々の余白を付ける

`EdgeInsets.Symmetric` は上下(`vertical`)と左右(`horizontal`)をまとめて指定します。

```csharp
new PaddingWidget
{
    Spacing = EdgeInsets.Symmetric(vertical: 12, horizontal: 36),
    Child = Fill(new Color(255, 111, 97))
}
```

### 一部の辺だけ余白を付ける

`EdgeInsets` はコンストラクタ引数が `Left` / `Top` / `Right` / `Bottom` の順で、すべて既定値0です。名前付き引数で必要な辺だけ指定します。

```csharp
new PaddingWidget
{
    Spacing = new EdgeInsets(Left: 48),
    Child = Fill(new Color(255, 209, 102))
}
```

### Padding は子に合わせて縮む

`Padding` は枠いっぱいに広がるウィジェットではなく、「子の寸法 + 余白」を自分の寸法として要求します。親から緩い制約を受け取っていれば、子が `48 x 48` で余白が各辺 `16` なら `80 x 80` に縮みます。

このサンプルでは `Center` を挟んで緩い制約を作り、`ColoredBox` で `Padding` 自身の範囲が見えるようにしています。

```csharp
new Center
{
    Child = new ColoredBox
    {
        Color = new Color(92, 76, 44),
        Child = new PaddingWidget
        {
            Spacing = EdgeInsets.All(16),
            Child = new ColoredBox
            {
                Color = new Color(124, 205, 255),
                Child = new SizedBox { Width = 48, Height = 48 }
            }
        }
    }
}
```

親が固定寸法を強制している場合(枠の直下に置いた場合)は、算出した寸法が親の制約へ収められるため枠いっぱいに広がります。最初の3つの枠がその状態です。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 余白プロパティの名前 | `Spacing` | `padding` |
| 一部の辺だけの指定 | コンストラクタの名前付き引数(`new EdgeInsets(Left: 48)`) | `EdgeInsets.only(left: 48)` |
| 文字方向の影響 | 受けない | `EdgeInsetsDirectional` を使うと `textDirection` の影響を受ける |

レイアウトの振る舞いは同等です。子へ渡す制約を余白のぶん狭める点、自身が「子 + 余白」の寸法になり親の制約へ収められる点は Flutter と同じです。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.Padding -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.Padding
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.Align](../FloatSoda.Samples.Align) — 領域内での子の配置と収縮
