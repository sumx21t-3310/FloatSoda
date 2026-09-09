# Expanded 確認手順

同じ幅(`620`)の帯の中で、余剰領域の分配と Expanded / Flexible / Spacer の違いを5本比較する。

## デスクトップ(`--desktop`)

HMD を接続し SteamVR を起動した状態で実行する。被る必要はなく、モニタ上で判定する。

```powershell
dotnet run --project samples/FloatSoda.Samples.Expanded -- --desktop
```

1. デスクトップウィンドウが開き、ラベル付きの帯が4節・計5本並ぶ。
2. `1 : 2 : 1` の帯が水色・朱色・黄色で隙間なく埋まり、朱色の幅が水色・黄色のちょうど2倍である(`155 / 310 / 155`)。
3. `固定 220 + Expanded` の帯で、水色(`220`)の右側の残り全部(`400`)が朱色で埋まっている。
4. `Expanded` の帯が、幅90の指定を無視して水色で帯いっぱい(`620`)に埋まっている。
5. `Flexible` の帯では、朱色が左端の幅 `90` だけで、残りは帯の背景色(濃いグレー)のままである。
6. 4番と5番の子はどちらも「幅90を要求する `SizedBox`」であり、違いは `Expanded` か `Flexible` かだけである(コードで確認)。
7. `Spacer 1 : 2` の帯で、3本の四角(各 `90`)の間に空白が2つあり、右側の空白が左側のちょうど2倍である(約 `117 / 233`)。

## ダッシュボード(HMD を被って判定)

```powershell
dotnet run --project samples/FloatSoda.Samples.Expanded
```

1. SteamVR ダッシュボードに `Expanded` タブが現れ、開くとオーバーレイが表示される。
2. 表示内容がデスクトップ実行時と一致する(5本の帯の埋まり方)。
