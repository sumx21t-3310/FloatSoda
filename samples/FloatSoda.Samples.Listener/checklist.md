# Listener 確認手順

2つの枠で HitTestBehaviour による反応範囲の違いを、実際にポインターで押して確認する。

## デスクトップ(`--desktop`)

HMD を接続し SteamVR を起動した状態で実行する。被る必要はなく、モニタ上でマウス操作して判定する。

```powershell
dotnet run --project samples/FloatSoda.Samples.Listener -- --desktop
```

1. デスクトップウィンドウが開き、`DeferToChild` と `Opaque` のラベル付きの枠が横に並ぶ。下に「最後のイベント: まだイベントはありません」「Down 0 回 / Up 0 回」と表示されている。
2. `DeferToChild` の枠の**中央の水色の四角**をクリックすると、「最後のイベント」が `DeferToChild で Down(位置 …)` → 離すと `Up` に変わり、Down / Up の回数が1ずつ増える。
3. `DeferToChild` の枠の**空白部分(四角の外)**をクリックしても、表示と回数が**一切変わらない**。
4. `Opaque` の枠は、**中央の朱色の四角でも空白部分でも**、クリックするたびに表示が `Opaque で Down …` に変わり、回数が増える。
5. 表示される位置の数値が、クリックした場所に応じて変わる。

## ダッシュボード(HMD を被って判定)

```powershell
dotnet run --project samples/FloatSoda.Samples.Listener
```

1. SteamVR ダッシュボードに `Listener` タブが現れ、開くとオーバーレイが表示される。
2. コントローラーのレーザーとトリガーで、2〜4番と同じ結果になる(`DeferToChild` は四角のみ、`Opaque` は枠全体で反応する)。
3. デスクトップとダッシュボードで反応範囲に差が無い。
