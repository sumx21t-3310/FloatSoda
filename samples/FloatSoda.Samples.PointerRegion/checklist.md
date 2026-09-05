# PointerRegion 確認手順

1枚の的で Enter / Exit / Down / Up / Cancel / Tap の順序と排他を確認する。Cancel の確認はレーザー操作が要るため、ダッシュボード節にある。

## デスクトップ(`--desktop`)

HMD を接続し SteamVR を起動した状態で実行する。被る必要はなく、モニタ上でマウス操作して判定する。

```powershell
dotnet run --project samples/FloatSoda.Samples.PointerRegion -- --desktop
```

1. デスクトップウィンドウが開き、グレーの的に「AIM HERE」、下に「Status: OUTSIDE」と各カウント 0 が表示されている。
2. ポインターを的へ入れると、的が水色になり「HOVER」、Status が `ENTER / HOVERING`、Enter が 1 増える。
3. ポインターを的から出すと、的がグレーへ戻り、Status が `EXIT / OUTSIDE`、Exit が 1 増える。
4. 的の上で押すと的がオレンジ(「PRESSED」)になり、離すと Tap が 1 増えて Status が `TAP / SUCCESS` になる。
5. Enter / Exit は押下と無関係に、出入りのたびに毎回増える。

## ダッシュボード(HMD を被って判定)

```powershell
dotnet run --project samples/FloatSoda.Samples.PointerRegion
```

1. SteamVR ダッシュボードに `PointerRegion` タブが現れ、開くとオーバーレイが表示される。
2. レーザーの出し入れとトリガーで、デスクトップの2〜5番と同じ結果になる。
3. **トリガーを引いたままレーザーを的を越えてオーバーレイそのものの外へ出す**と、的が赤(「CANCELED」)になり、Cancel が 1 増え、**Tap は増えない**(Status は `CANCEL / TAP SUPPRESSED` を経由する)。
   的の外(オーバーレイ内)へ出しただけでは Cancel は増えず、Exit だけが増える。
4. Cancel の後にもう一度的へ入って普通にタップすると、4番と同様に Tap が成立する(Cancel 状態を引きずらない)。
