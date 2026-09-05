# Opacity

## これは何か

`Opacity` は、子のサブツリー全体を指定した不透明度で合成するウィジェットです。Flutter の `Opacity` に対応します。

子を1枚のレイヤーへ描いてからまとめて薄くするため、サブツリー内で図形が重なっていても、重なり部分だけが二重に濃くなることはありません。

## 使い方

### 不透明度を指定する

`Value` に 0(完全に透明)から 1(不透明、既定)までを渡します。

```csharp
using OpacityWidget = FloatSoda.Widgets.Paint.Opacity;

new OpacityWidget
{
    Value = 0.5,
    Child = Pair()
}
```

このサンプルは名前空間が `FloatSoda.Samples.Opacity` で型名 `Opacity` と衝突するため、`OpacityWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new Opacity { ... }` と書けます。

`Pair()` は重なった2つの四角です。0.5 を指定すると2つがまとめて薄くなり、重なり部分の色は手前の四角の 0.5 と同じになります(グループ単位の合成)。

### 0.0 でもレイアウト領域は残る

`Value = 0.0` の子は描画されませんが、レイアウト上の領域は占有したままです。並びの間隔を保ったまま「見えなくする」用途に使えます。

```csharp
new OpacityWidget
{
    Value = 0.0,
    Child = Marker(new Color(255, 111, 97))
}
```

領域ごと消したい場合は `Visibility` や `Offstage` を使います。また、単色の塗りを薄くするだけなら、`Color` の第4引数(アルファ値)で足ります。中間レイヤーを作る `Opacity` より軽く済みます。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 不透明度プロパティの名前 | `Value` | `opacity`(C# ではメンバー名が型名 `Opacity` と同名にできないための改名) |
| セマンティクス | 対応する仕組みが無い | `alwaysIncludeSemantics` でスクリーンリーダー向け情報を保持できる |

グループ単位で合成する点、0.0 でもレイアウト領域が残る点、0 と 1 では中間レイヤーを作らない点は Flutter と同等です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.Opacity -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.Opacity
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.DecoratedBox](../FloatSoda.Samples.DecoratedBox) — アルファ値つきの単色装飾の例(`Foreground`)
