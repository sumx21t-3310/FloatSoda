# 実験

FloatSoda の動作原理の問いを、動かして確かめるページです。どの実験からでも始められます。書き方は [WritingDocumentation の「実験」](../WritingDocumentation.md#実験) にあります。

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../WritingDocumentation.md) にあります。

## 実験の一覧

- [SteamVR へのテクスチャ転送はどうなっているの?](TextureTransfer.md) — プラットフォーム
- [オーバーレイの大きさは何で決まるの?](OverlaySize.md) — プラットフォーム
- [半透明の子が重なると、どう混ざるの?](OpacityBlend.md) — 描画
- [なぜ Layer ツリーは Clone してからレンダースレッドへ渡すの?](LayerClone.md) — 描画
- [親はどうやって子の大きさと位置を決めるの?](CircleLayout.md) — フレームワーク
- [1フレームで、どこが描き直されているの?](RepaintRegion.md) — フレームワーク
- [何も変わらないフレームは何をしているの?](IdleFrame.md) — フレームワーク
- [`Key` をつけると何が変わるの?](Keys.md) — フレームワーク
- [`InheritedWidget` を変えると、誰が作り直されるの?](InheritedRebuild.md) — フレームワーク
