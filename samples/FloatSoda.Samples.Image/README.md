# Image 実機テストサンプル

`FileImageProvider` から PNG を非同期ロードし、SteamVR Dashboard 上で次の表示を比較します。

- `DIRECT`: `Image` を `320 x 220` へ直接レイアウトし、画像の上へ子要素を描画
- `CONTAIN`: `FittedBox` の `BoxFit.Contain` で縦横比を維持して全体表示
- `COVER`: `FittedBox` の `BoxFit.Cover` と `Clip.HardEdge` で中央を切り抜き

## 実行

SteamVR を起動してから、リポジトリルートで実行します。

```powershell
dotnet run --project samples/FloatSoda.Samples.Image
```

Dashboard の `Image Test` を開き、次を確認してください。

1. 起動後に3枚すべての画像が表示される。
2. 3枚の色とディテールが一致する。
3. `DIRECT` だけが横長に変形している。
4. `CONTAIN` は正方形の全体が表示され、左右に背景色が見える。
5. `COVER` は横幅を満たし、画像の上下が領域外へはみ出さず切り抜かれる。
6. `DIRECT` の `CHILD ON TOP` が画像より手前に表示される。
