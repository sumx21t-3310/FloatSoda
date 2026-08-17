# FS-RENDER-SAFE-001: Render layer snapshot isolation

## Claim

レンダースレッドへ渡すために `ILayer.Clone()` で複製されたレイヤーツリーは、複製後にメインスレッド側の元レイヤーツリーが変更されても、その変更の影響を受けない。

## Claim Source

`Architectural Invariant`

`AGENTS.md` の Frame Pipeline と Layer Tree の説明では、レイヤーツリーをレンダースレッドへ渡す前に `ILayer.Clone()` し、メインスレッドとレンダースレッドのデータ競合を避ける設計としている。

## Impact if violated

複製後の元レイヤーツリーへの更新が、すでにレンダースレッドへ公開済みのフレームへ混入する可能性がある。結果として、1フレーム内で異なるWidget/RenderObject状態の描画内容が混在したり、メインスレッドとレンダースレッドの競合が発生したりする。

## Evidence / rationale

`ContainerLayer.Clone()` は子レイヤーを再帰的に `Clone()` し、新しい `Children` リストへ格納する。`PictureLayer.Clone()` は新しい `PictureLayer` インスタンスを生成し、その時点の `SKPicture` 参照をコピーする。

## Confidence

High

## Owner / Authority

FloatSoda rendering architecture

## Property

### Property ID

`FS-RENDER-SAFE-001`

### Property Type

Safety / Snapshot isolation

### Formal Property

時刻 `t0` にレイヤーツリー `L` から `S = Clone(L)` を生成した場合、`t0` より後に `L` の構造または各レイヤーの公開プロパティを変更しても、`S` を描画した結果はその変更によって変化してはならない。

今回のパイロットでは、次の有限シナリオへ具体化する。

1. 元の `ContainerLayer` に赤色の `PictureLayer` を追加する。
2. `Clone()` して snapshot を作る。
3. 元の `ContainerLayer` の子を青色の `PictureLayer` へ置き換える。
4. snapshot を描画する。
5. snapshot の描画結果が赤色のままであることを確認する。

## Executable Test

`tests/FloatSoda.Rendering.Test/Layers/LayerSnapshotPropertyTest.cs`

## Expected classification

テストが成功する場合、この有限シナリオについては `Implementation-Guarded` とする。これは一般的な並行実行すべてに対する安全性の証明ではない。
