# OpenVR の API を包む

このページは、プラットフォームの層(`FloatSoda.Abstractions`、`FloatSoda.Engine`、`FloatSoda.OVR`)でよく行う2つの作業、OpenVR の API を調べて包む作業と、OpenVR のエラーに対応する例外を足す作業の手順です。層の仕組みは [プラットフォームの仕組みと理由](../explanation/Platform.md) に、変更の前後で守る条件は [プラットフォームで守ること](../reference/Platform.md) にあります。

| 知りたいこと | 置き場所 |
|---|---|
| Issue が要る変更、ブランチ名、PR のスコープ | [CONTRIBUTING.md](../../../CONTRIBUTING.md) |
| ネイティブ API を包むときの規約(包まない API、型名の単数形と複数形) | [APIDesign](../APIDesign.md) の「11. ネイティブAPIラップの方針」 |
| breaking change の判定 | [APIDesign](../APIDesign.md) の「6. バージョニングと後方互換性」 |
| XML ドキュメントコメントの書き方 | [DocumentationComments](../DocumentationComments.md) |
| 例外メッセージの追加手順と文体 | [Localization](../Localization.md) の「3. 例外メッセージ」 |
| 初めて API を包むときの一本道 | [OpenVR の API を1つ包んで PR を出す](../tutorials/OverlayPreCurvePitch.md) |

## OpenVR の API を調べて包む

### 規約

- 包む前に、`public` API の追加として Issue を立てること(CONTRIBUTING.md の「Issue が必須な変更」)
- OpenVR の API の一次情報は、OpenVR SDK から生成した生バインディング `src/FloatSoda.OVR/openvr_api.cs` とし、包むコードはこのファイルの外に書くこと
- enum の名前を返すだけの API(`*NameFromEnum`)は包まないこと(APIDesign の 11.1)
- 型名は、ライフサイクルを持つ型なら単数形、持たない `static` クラスなら複数形にすること(APIDesign の 11.2)
- オーバーレイ1枚に対する設定は、ハンドルだけを持ち読み書きのたびに OpenVR を呼ぶ能力オブジェクトとして、`src/FloatSoda.OVR/Overlay/Capability.cs` に書くこと(`OverlayCurvature` と同じ形)
- OpenVR の戻り値のエラーは、捨てずに `ThrowIfError()` で型付きの例外に変えること
- `IOverlay` などの `public` インターフェースにメンバーを足すときは、外部の実装型がコンパイルできなくなるため、breaking change かどうかを判定して PR の本文に書くこと
- 新しい `public` API には、XML ドキュメントコメントを付け、ジュニアコーダーテストを通すこと(CONTRIBUTING.md の「新しい public API を追加する場合の受け入れ条件」)

### 手順

1. 包みたい機能の OpenVR のインターフェース名(`IVROverlay`、`IVRSystem` など)を決める(迷ったら [`openvr-api-reference`](../../../.agents/skills/openvr-api-reference/SKILL.md) スキルで、機能の一覧と包む必要性の評価を得る)
2. `openvr_api.cs` の `CVRxxx` クラスのメソッドで、API の引数と戻り値を確かめる

   ```bash
   grep -n "OverlayPreCurvePitch" src/FloatSoda.OVR/openvr_api.cs
   ```

   `public EVROverlayError SetOverlayPreCurvePitch(ulong ulOverlayHandle,float fRadians)` のような行が出れば、そのメソッドを呼べる
3. 置き場所を、オーバーレイ1枚の設定なら能力オブジェクト、アプリ全体の操作なら `OVRApplication` か `OVRApplications`、イベントなら `VREventDispatcher` への登録に決める
4. 戻り値のエラーを `ThrowIfError()` で例外に変える形で、包むコードを書く
5. 能力オブジェクトを足した場合は、`IOverlay`(または `IDashboardOverlay`、`IMovableOverlay`)にプロパティを足し、`DashboardOverlay` と `MovableOverlay<TTransform>` の両方で初期化する
6. CONTRIBUTING.md の「ブランチ・PRフロー」にある、CI と同じコマンドを実行する
7. SteamVR を起動し、[`samples/FloatSoda.Samples.PrimitiveOverlay`](../../../samples/FloatSoda.Samples.PrimitiveOverlay/Program.cs) に一時的に呼び出しを足して実行し、実機で確かめる
8. 更新する文書を、[WritingDocumentation](../WritingDocumentation.md) の「3. Documentation Impact Matrix」で決める

成功すると、手順6のコマンドがすべて失敗 0 で終わり、手順7で SteamVR 上のオーバーレイに設定が反映されます。

手順7の確かめ方をあとに残したいときは、[`floatsoda-device-test-gen`](../../../.agents/skills/floatsoda-device-test-gen/SKILL.md) スキルでハーネスのシナリオにし、[`floatsoda-device-test-run`](../../../.agents/skills/floatsoda-device-test-run/SKILL.md) スキルで実行します。

## 例外を足す

OpenVR のインターフェースを新しく包み、そのインターフェースのエラー列挙(`EVRxxxError`)に対応する例外がまだないときの手順です。いまある例外は [`src/FloatSoda.OVR/Exceptions/`](../../../src/FloatSoda.OVR/Exceptions) にあります。

### 規約

- 例外クラスは `src/FloatSoda.OVR/Exceptions/` に置き、`OpenVRSystemException<TError>` を継承すること
- エラー値を受け取るコンストラクタを用意し、メッセージはエラー値から `ExceptionMessages` のプロパティで引くこと
- エラーが `None` のときは何もせず、それ以外で例外を投げる `static` の `ThrowIfError(error)` を用意すること
- `OpenVRExceptionHelper` に、そのエラー列挙の `ThrowIfError()` 拡張メソッドを足すこと
- メッセージは、日本語の `ExceptionMessages.resx` と英語の `ExceptionMessages.en.resx` の両方に、同じキーで足すこと(Localization の「3. 例外メッセージ」)

既存の例外では、未知のエラー値の扱いが2通りあります。`VROverlayException` などは汎用のメッセージ(`{例外クラス名}_UnexpectedError`)で例外を作り、`VRScreenshotException` などは `ArgumentOutOfRangeException` を投げます。新しい例外でどちらにそろえるかは ※要確認 です。

### 手順

1. `openvr_api.cs` で、エラー列挙 `EVRxxxError` の列挙子を確かめる
2. `ExceptionMessages.resx` に、列挙子ごとの日本語のメッセージを `{例外クラス名}_{列挙子}` のキーで足す
3. `ExceptionMessages.en.resx` に、同じキーで英語のメッセージを足す
4. `ExceptionMessages.cs` に、キーごとの `static` プロパティを足す
5. `VROverlayException` を雛形にして、列挙子からメッセージを引く `switch` 式と `ThrowIfError` を持つ `VRxxxException` を作る
6. `OpenVRExceptionHelper` に `public static void ThrowIfError(this EVRxxxError error)` を足す
7. 包むコードで、OpenVR の戻り値に `.ThrowIfError()` を付ける
8. CONTRIBUTING.md の「ブランチ・PRフロー」にある、CI と同じコマンドを実行する

成功すると、手順8のコマンドがすべて失敗 0 で終わり、包んだ API がエラーを返したときに `VRxxxException` が投げられます。

## 規約が守られる場所

- ビルドと既存のテストは、CI(`.github/workflows/ci.yml`)で検出する
- OpenVR を呼ぶコードは CI では実行されないため、実機のテスト(`floatsoda-device-test-run` スキル)とレビューで確認する
- `ThrowIfError()` の付け忘れ、resx の2つのファイルのキーの食い違い、インターフェースへのメンバー追加の判定は、検出する仕組みがないためレビューで確認する

## 関連

- [OpenVR の API を1つ包んで PR を出す](../tutorials/OverlayPreCurvePitch.md) — 実在の API を1つ包んで PR を出すまで
- [プラットフォームで守ること](../reference/Platform.md) — 例外メッセージのキーとスレッドの条件
- [プラットフォームの仕組みと理由](../explanation/Platform.md) — 能力オブジェクトと、起動を止めない失敗
