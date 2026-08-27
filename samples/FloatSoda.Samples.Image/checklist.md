# Image 確認手順

同じ 1000 x 1000 の PNG を `DIRECT` / `CONTAIN` / `COVER` の3通りで表示し、比較する。

## デスクトップ(`--desktop`)

HMD を接続し SteamVR を起動した状態で実行する。被る必要はなく、モニタ上で判定する。

```powershell
dotnet run --project samples/FloatSoda.Samples.Image -- --desktop
```

1. デスクトップウィンドウが開き、起動後に3枚すべての画像が表示される。
2. 3枚の色とディテールが一致する。
3. `DIRECT` だけが横長に変形している(`BoxFit.Fill` のため縦横比が維持されない)。
4. `CONTAIN` は正方形の全体が表示され、左右に背景色が見える。
5. `COVER` は横幅を満たし、画像の上下が領域外へはみ出さずに切り抜かれる。
6. `DIRECT` の `CHILD ON TOP` が画像より手前に表示される。
7. ウィンドウサイズが中身(1100 x 620)に追従している。

## ダッシュボード(HMD を被って判定)

```powershell
dotnet run --project samples/FloatSoda.Samples.Image
```

1. SteamVR ダッシュボードに `Image Test` タブが現れ、開くとオーバーレイが表示される。
2. 表示内容がデスクトップ実行時と一致する(3枚の並び、切り抜き、重ね描き)。
3. 画像のディテールが潰れていない(`Dpm = 1000` での解像度確認)。
4. ダッシュボードを閉じて開き直しても、画像が再表示される。
