# LimitedBox 確認手順

同じ大きさの枠の中で、bounded / unbounded での効き方の違いを3通り比較する。

## デスクトップ(`--desktop`)

HMD を接続し SteamVR を起動した状態で実行する。被る必要はなく、モニタ上で判定する。

```powershell
dotnet run --project samples/FloatSoda.Samples.LimitedBox -- --desktop
```

1. デスクトップウィンドウが開き、ラベル付きの枠が3つ横に並ぶ。
2. `bounded では何もしない` の枠が、`MaxWidth = 80` の指定にもかかわらず水色で枠いっぱい(`150 x 150`)に塗られている(枠の制約が勝つ)。
3. `unbounded で上限になる` の枠で、朱色の帯が幅 `100`・高さ `40` で中央にある。幅 `500` の要求が `MaxWidth = 100` で止まっている。
4. `LimitedBox なしの比較` の枠で、黄色の帯(幅 `220`)が**枠の左右の端を越えて、枠の外まで描かれている**。
5. 3番の帯(`100`)が4番の帯(`220`)より明らかに短い。

## ダッシュボード(HMD を被って判定)

```powershell
dotnet run --project samples/FloatSoda.Samples.LimitedBox
```

1. SteamVR ダッシュボードに `LimitedBox` タブが現れ、開くとオーバーレイが表示される。
2. 表示内容がデスクトップ実行時と一致する(3枠の帯の幅とはみ出し)。
