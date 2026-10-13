# プラットフォーム: 貢献までの流れ

このページでは、プラットフォームの層で OpenVR の API を1つ包み、PR を出すまでを一本道で進めます。題材は [#103](https://github.com/sumx21t-3310/FloatSoda/issues/103)(`OverlayPreCurvePitch` ケーパビリティクラスを追加する)です。曲面オーバーレイの曲げる軸を傾ける API で、生バインディングはすでにあり、同じ形の `OverlayCurvature` を手本にできます。

## 作るもの

最後には、能力オブジェクト `OverlayPreCurvePitch` を足したブランチと、それを出した PR ができます。変更するのは2つのファイルです。`src/FloatSoda.OVR/Overlay/Capability.cs` にクラスを足し、`src/FloatSoda.OVR/Overlay/Overlay.cs` の `IOverlay` と、その実装 `DashboardOverlay`・`MovableOverlay<TTransform>` に、プロパティ `PreCurvePitch` を足します。

`overlay.PreCurvePitch.Value = 0.5f;` と書くと、SteamVR の `SetOverlayPreCurvePitch` が呼ばれるようになります。

## 前提

- .NET 10 SDK が入っていて、リポジトリをクローン済みであること
- `dotnet build` が手元で通ること
- 実機で確かめるために、SteamVR と HMD が使えること
- [プラットフォーム: 仕組みと理由](Explanation.md) の「オーバーレイと能力オブジェクト」を読んでいること

## Step 1: Issue を読み、ブランチを切る

[#103](https://github.com/sumx21t-3310/FloatSoda/issues/103) を開き、実装する内容を読みます。Issue は、能力オブジェクトの追加、`IOverlay` と実装へのプロパティの追加、文書の更新の3点を挙げています。

ブランチ名は、Issue 番号と、Issue の `area:*` ラベルから決めます([CONTRIBUTING.md](../../../CONTRIBUTING.md) の「ブランチ名」)。#103 のラベルは `area:ovr` です。

```bash
git switch main
git pull
git switch -c 103-ovr-overlay-pre-curve-pitch
```

`git branch --show-current` が `103-ovr-overlay-pre-curve-pitch` を返せば、このステップは終わりです。

## Step 2: 生バインディングで API を確かめる

OpenVR の API の一次情報は、生バインディング `src/FloatSoda.OVR/openvr_api.cs` です。包む API を検索します。

```bash
grep -n "OverlayPreCurvePitch" src/FloatSoda.OVR/openvr_api.cs
```

次の2行を含む出力が出れば、このステップは終わりです。角度の単位はラジアンで、どちらも `EVROverlayError` を返します。

```text
public EVROverlayError SetOverlayPreCurvePitch(ulong ulOverlayHandle,float fRadians)
public EVROverlayError GetOverlayPreCurvePitch(ulong ulOverlayHandle,ref float pfRadians)
```

ほかの API を題材にしたいときは、[`openvr-api-reference`](../../../.agents/skills/openvr-api-reference/SKILL.md) スキルで、インターフェースの機能の一覧と、包む必要があるかの評価を得られます。

## Step 3: 能力オブジェクトを書く

`src/FloatSoda.OVR/Overlay/Capability.cs` の `OverlayCurvature` の下に、次のクラスを足します。形は `OverlayCurvature` と同じで、値を読むたび、書くたびに OpenVR を呼び、エラーは `ThrowIfError()` で `VROverlayException` に変えます。

```csharp
/// <summary>曲面オーバーレイの曲げる軸の傾きを読み書きします。</summary>
/// <param name="overlayHandle">操作するオーバーレイのハンドル。</param>
public sealed class OverlayPreCurvePitch(ulong overlayHandle)
{
    /// <summary>曲げる前にオーバーレイを傾ける角度(ラジアン)を取得または設定します。</summary>
    public float Value
    {
        get
        {
            var value = 0.0f;
            OpenVR.Overlay.GetOverlayPreCurvePitch(overlayHandle, ref value).ThrowIfError();
            return value;
        }
        set => OpenVR.Overlay.SetOverlayPreCurvePitch(overlayHandle, value).ThrowIfError();
    }
}
```

XML ドキュメントコメントの書き方は [DocumentationComments](../DocumentationComments.md) にあります。

## Step 4: IOverlay と実装へプロパティを足す

`src/FloatSoda.OVR/Overlay/Overlay.cs` の3か所に、`Curvature` の下へ1行ずつ足します。

```csharp
// IOverlay
/// <summary>曲げる軸の傾きを制御するオブジェクトを取得します。</summary>
OverlayPreCurvePitch PreCurvePitch { get; }

// DashboardOverlay と MovableOverlay<TTransform> の両方
/// <inheritdoc />
public OverlayPreCurvePitch PreCurvePitch { get; } = new(identity.Handle);
```

ビルドします。

```bash
dotnet build
```

ビルドが成功すれば、このステップは終わりです。※要確認: この Tutorial の書き手は、Step 3 と Step 4 のコードでビルドを通していません。

## Step 5: 実機で確かめる

`OpenVR` を呼ぶコードは、SteamVR がないと動きません。手元で確かめるために、OpenVR だけを使うサンプル `samples/FloatSoda.Samples.PrimitiveOverlay` に、一時的に呼び出しを足します。この変更は PR に含めません。

1. `dotnet run --project samples/FloatSoda.Samples.PaintingSample` を実行し、表示する画像 `layer_tree_output.png` をデスクトップに作る
2. `samples/FloatSoda.Samples.PrimitiveOverlay/Program.cs` の `overlay.Visibility.Show();` の下に、次の2行を足す

   ```csharp
   overlay.Curvature.Value = 0.5f;
   overlay.PreCurvePitch.Value = 0.5f;
   ```

3. SteamVR を起動してから、`dotnet run --project samples/FloatSoda.Samples.PrimitiveOverlay` を実行する
4. HMD をかぶってオーバーレイの曲がり方を見てから、2行目の値を `0f` にして実行し直し、曲がり方が変わることを確かめる
5. 足した2行を消し、`git status` で `PrimitiveOverlay` に変更が残っていないことを確かめる

曲がり方がどう変わって見えるかは ※要確認 です。この Tutorial の書き手は実機で確かめていません。

確かめる手順をあとに残したいときは、[`floatsoda-device-test-gen`](../../../.agents/skills/floatsoda-device-test-gen/SKILL.md) スキルでハーネスのシナリオにし、[`floatsoda-device-test-run`](../../../.agents/skills/floatsoda-device-test-run/SKILL.md) スキルで実行します。

## PR を出す

PR を出す前に、CI と同じコマンドを手元で実行します。コマンドは [CONTRIBUTING.md](../../../CONTRIBUTING.md) の「ブランチ・PRフロー」にあります。3つのコマンドがすべて失敗 0 で終われば、PR を出せます。

`public` API を足す変更なので、PR の前に文書を更新し、ジュニアコーダーテストを通します。#103 は OVRIntegration の表への追記を挙げていますが、どのページを更新するかは [WritingDocumentation](../WritingDocumentation.md) の「3. Documentation Impact Matrix」で決めます。ジュニアコーダーテストの条件は、CONTRIBUTING.md の「新しい public API を追加する場合の受け入れ条件」にあります。

PR の本文は `.github/PULL_REQUEST_TEMPLATE.md` に沿って書き、breaking change の判定と、Step 5 で実機で確かめた結果を含めます。`IOverlay` は `public` インターフェースなので、メンバーを足すと、外部で `IOverlay` を実装している型はコンパイルできなくなります。判定の基準は [APIDesign](../APIDesign.md) の 6 章にあります。

## 次に読むもの

- [プラットフォーム: 作業の手順](HowTo.md) — API を包む規約と、例外を足す手順
- [プラットフォーム: 仕組みと理由](Explanation.md) — テクスチャ転送、スレッド、入力の流れ
- [プラットフォーム: 守ること](Reference.md) — `internal` の契約
