# GettingStarted

## これは何か

SteamVR のダッシュボードに、青い四角を1つ出すだけの最小のアプリです。FloatSoda のアプリに必ず要る部品(Host の構築、`AddFloatSoda`、`CreateWindow`、`RunAsync`)だけでできています。

このサンプルは、チュートリアルの入口に当たる作例です。主な読者は、これから FloatSoda で UI を作る利用者です。チュートリアル[最初のオーバーレイを作る](../../../docs/tutorials/first-overlay.md)の最初の段に近い形なので、チュートリアルと見比べながら読めます。

## 使い方

`Program.cs` の流れは次のとおりです。

1. `Host.CreateApplicationBuilder` で Host を作り、`AddFloatSoda()` で FloatSoda を登録する
2. Host から `FloatSodaApp` を取り出す
3. 画面の中身を、ウィジェットの組み合わせで書く。ここでは `Align` で中央に寄せた 100×100 の `ColoredBox`
4. `CreateWindow` に `DashboardWindow` を渡し、ダッシュボードのタブとして出す
5. `RunAsync` でアプリを動かす

画面の中身は、次の部分です。

```csharp
Widget root = new Align
{
    Alignment = Alignment.Center,
    Child = new SizedBox
    {
        Width = 100,
        Height = 100,
        Child = new ColoredBox
        {
            Color = SKColors.CornflowerBlue
        }
    }
};
```

`DashboardWindow` の `Dpm` は、1メートルあたりのピクセル数です。ピクセル数と、VR の中で見える長さを換算するときに使います。

## Flutterとの違い

対象外です。ウィジェット1つではなく、アプリ全体の最小の形を見せる作例です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。`--desktop` は持ちません。

```powershell
dotnet run --project samples/Showcase/FloatSoda.Samples.GettingStarted
```

## 関連

- [最初のオーバーレイを作る](../../../docs/tutorials/first-overlay.md) — 同じ形から始めるチュートリアル
- [作例のカタログ](../../../docs/reference/sample-catalog.md) — ほかの作例の一覧
