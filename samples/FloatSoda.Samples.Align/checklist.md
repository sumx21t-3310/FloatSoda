# Align 確認手順

同じ大きさの枠の中で、配置位置と収縮の効き方を5通り比較する。

## デスクトップ(`--desktop`)

HMD を接続し SteamVR を起動した状態で実行する。被る必要はなく、モニタ上で判定する。

```powershell
dotnet run --project samples/FloatSoda.Samples.Align -- --desktop
```

1. デスクトップウィンドウが開き、ラベル付きの枠が5つ横に並ぶ。
2. `TopLeft` の枠で、水色の四角が枠の左上隅に接している。
3. `Center` の枠で、水色の四角が上下左右とも中央にある。
4. `BottomRight` の枠で、水色の四角が枠の右下隅に接している。
5. `Center ウィジェット` の枠で、朱色の四角が3番と同じ中央位置にある(`Center` と `Align.Center` が一致する)。
6. `係数 2.0 で収縮` の枠で、黄色の四角が中央にあり、**その周囲に四角と同じ幅の余白がある**。枠いっぱいには広がっていない。
7. 5つの枠がすべて同じ大きさ(`150 x 150`)である。

## ダッシュボード(HMD を被って判定)

```powershell
dotnet run --project samples/FloatSoda.Samples.Align
```

1. SteamVR ダッシュボードに `Align` タブが現れ、開くとオーバーレイが表示される。
2. 表示内容がデスクトップ実行時と一致する(5枠の並びと各四角の位置)。
3. 隅に接している四角(`TopLeft` / `BottomRight`)が、オーバーレイの端で切れていない。
