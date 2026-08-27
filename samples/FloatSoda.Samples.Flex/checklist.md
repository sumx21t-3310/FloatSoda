# Flex 確認手順

主軸の揃え方3種、交差軸の揃え方2種、および `Column` と `Flex` の一致を比較する。

## デスクトップ(`--desktop`)

HMD を接続し SteamVR を起動した状態で実行する。被る必要はなく、モニタ上で判定する。

```powershell
dotnet run --project samples/FloatSoda.Samples.Flex -- --desktop
```

**MainAxisAlignment**

1. `Start` の帯で、3つの矩形が左端に詰まり、右側に余白が残っている。
2. `Center` の帯で、3つの矩形がまとまって中央にあり、左右の余白が等しい。
3. `SpaceBetween` の帯で、左端と右端に矩形が接し、間隔が2箇所とも等しい。
4. 3つの帯とも、矩形の幅と個数は同じである(違いは配置だけ)。

**CrossAxisAlignment**

5. `Start` の帯で、高さの異なる3つの矩形の**上端が揃っている**。
6. `Center` の帯で、3つの矩形の**上下中央が揃っている**(高い矩形の上下に均等な余白)。

**Column と Flex の一致**

7. 最下段で、2本の横棒が上端と下端に分かれて配置されている(`Direction = Vertical` と `SpaceBetween`)。
8. 2本の横棒の長さが異なる(160 と 220)。

## ダッシュボード(HMD を被って判定)

```powershell
dotnet run --project samples/FloatSoda.Samples.Flex
```

1. SteamVR ダッシュボードに `Flex` タブが現れ、開くとオーバーレイが表示される。
2. 表示内容がデスクトップ実行時と一致する(全帯の配置)。
3. `SpaceBetween` で端に接している矩形が、オーバーレイの端で切れていない。
4. ラベルの文字が VR 内で読める。
