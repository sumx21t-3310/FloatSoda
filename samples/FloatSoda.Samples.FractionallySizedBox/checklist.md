# FractionallySizedBox 確認手順

同じ大きさの枠の中で、割合指定・配置・1超の係数を4通り比較する。

## デスクトップ(`--desktop`)

HMD を接続し SteamVR を起動した状態で実行する。被る必要はなく、モニタ上で判定する。

```powershell
dotnet run --project samples/FloatSoda.Samples.FractionallySizedBox -- --desktop
```

1. デスクトップウィンドウが開き、ラベル付きの枠が4つ横に並ぶ。
2. `WidthFactor = 0.5` の枠で、水色の縦帯が幅 `75`(枠の半分)・高さ `150`(枠いっぱい)で左右中央にある。
3. `0.5 x 0.5` の枠で、朱色の四角が `75 x 75` で上下左右とも中央にある。
4. `Alignment.TopLeft` の枠で、黄色の四角(`75 x 75`)が枠の左上隅に接している。
5. `WidthFactor = 1.4` の枠で、水色の横帯(幅 `210`・高さ `45`)が**枠の左右の端を越えて、枠の外(ページの紺色の背景の上)まで描かれている**。上下は中央にある。
6. 4つの枠がすべて同じ大きさ(`150 x 150`)である。

## ダッシュボード(HMD を被って判定)

```powershell
dotnet run --project samples/FloatSoda.Samples.FractionallySizedBox
```

1. SteamVR ダッシュボードに `FractionallySizedBox` タブが現れ、開くとオーバーレイが表示される。
2. 表示内容がデスクトップ実行時と一致する(4枠の割合・配置・はみ出し)。
