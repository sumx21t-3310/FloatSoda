# SteamVR `.vrmanifest` 調査ノート

調査日: 2026-07-13  
対象: OpenVR application manifest（ドライバー用の `driver.vrdrivermanifest` ではない）

## 結論

`.vrmanifest` は、SteamVR にアプリケーションの識別子、起動方法、表示名、画像、ダッシュボードオーバーレイかどうかなどを知らせる JSON ファイルである。登録は `IVRApplications.AddApplicationManifest()` に**マニフェストファイルの絶対パス**を渡して行う。

ただし、Valve は `IVRApplications` の API 契約とアプリケーションプロパティ一覧を公開している一方で、`.vrmanifest` の完全な JSON Schema や全キーの正式な仕様書を公開していない。本資料では確度を次のように区別する。

- **公式**: Valve の `openvr.h` に明記された API 契約・プロパティ
- **実例**: SteamVR/OpenVR の実運用マニフェストで一貫して使われる JSON 形式
- **未確認**: FloatSoda に型はあるが、Valve の公開資料や代表的な実例で裏付けられない形式

FloatSoda のダッシュボードオーバーレイで使う最小構成は次の形でよい。

```json
{
  "source": "builtin",
  "applications": [
    {
      "app_key": "com.example.my_overlay",
      "launch_type": "binary",
      "binary_path_windows": "MyOverlay.exe",
      "is_dashboard_overlay": true,
      "strings": {
        "en_us": {
          "name": "My Overlay",
          "description": "A SteamVR dashboard overlay"
        },
        "ja_jp": {
          "name": "マイオーバーレイ",
          "description": "SteamVR ダッシュボード用オーバーレイ"
        }
      }
    }
  ]
}
```

## ファイル構造

### ルート

| キー | 型 | 必須性 | 説明 | 確度 |
|---|---|---:|---|---|
| `source` | string | 慣例上あり | アプリ情報の提供元。非 Steam アプリの実例では `builtin` が多い。任意の製品名を使う実例もある | 実例 |
| `applications` | array | 必須 | 1 個以上のアプリケーション定義 | 実例 |

1 ファイルに複数のアプリケーションを記述できる。ただし `app_key` は SteamVR 全体で一意でなければならず、重複時には `VRApplicationError_AppKeyAlreadyExists` が定義されている。

### アプリケーション定義

| キー | 型 | 必須性 | 説明 | FloatSoda の現状 |
|---|---|---:|---|---|
| `app_key` | string | 必須 | アプリを一意に識別するキー。OpenVR の上限は 128 bytes 相当 | `AppKey` |
| `launch_type` | string | 必須 | 通常は `binary` または `url` | `LaunchType` だが文字列化設定がない |
| `binary_path_windows` | string | `binary` の Windows 実行時に必須 | Windows 実行ファイル | 対応済み |
| `binary_path_linux` | string | Linux の場合 | Linux 実行ファイル | 対応済み |
| `binary_path_osx` | string | macOS の場合 | macOS 実行ファイル | 対応済み |
| `url` | string | `url` の場合 | `steam://...` などの起動 URL | 未対応 |
| `arguments` | string | 任意 | 起動引数文字列 | **未対応。現状の `binary_args` は裏付けなし** |
| `working_directory` | string | 任意 | 起動時の作業ディレクトリ | 対応済み |
| `image_path` | string | 任意 | SteamVR UI に表示する画像のパスまたは URL | 対応済み |
| `image_path_capsule` | string | 任意 | capsule 用画像。OpenVR の取得プロパティに存在する | 未対応 |
| `action_manifest_path` | string | Input 使用時 | OpenVR Input の action manifest (`actions.json`) | 未対応 |
| `is_dashboard_overlay` | bool | overlay では重要 | ダッシュボードオーバーレイとして扱う | 対応済み |
| `is_template` | bool | 任意 | `LaunchTemplateApplication` 用テンプレート | 対応済み |
| `is_instanced` | bool | 任意 | インスタンス化可能なアプリ | 対応済み |
| `is_internal` | bool | 通常不要 | ランタイム内部アプリ向け | 対応済み |
| `is_hidden` | bool | 任意 | UI 上で非表示にする | 未対応 |
| `wants_compositor_pause_in_standby` | bool | 任意 | standby 中の compositor pause に関する希望 | 未対応 |
| `strings` | object | 実用上推奨 | ロケール別の `name` と `description` | 対応済み |

`image_path_capsule`、`is_hidden`、`wants_compositor_pause_in_standby` などは、公式ヘッダーの `EVRApplicationProperty` に対応するプロパティがあることを根拠としている。ただし Valve は、それぞれの JSON キーの詳細な入力仕様を公開していない。

### `app_key`

公式ヘッダーの `k_unMaxApplicationKeyLength` は 128。API が C 文字列を受け取るため、安全側では UTF-8 の終端を含めてこの範囲に収める。ASCII の逆ドメイン名風キーにすると、衝突と byte 数の解釈を避けやすい。

```text
com.example.float_soda_overlay
```

Steam アプリが使う `steam.app.<AppId>` や、他製品の予約済みキーを流用しない。大文字・空白を避け、マニフェスト、`IdentifyApplication`、`SetApplicationAutoLaunch` など全箇所で完全に同じ文字列を使う。

### `launch_type`

確認できた一般的な値は次の 2 つ。

| 値 | 必要なキー | 用途 |
|---|---|---|
| `binary` | 実行 OS に対応する `binary_path_*` | exe などを SteamVR が直接起動する |
| `url` | `url` | Steam URL や URL handler 経由で起動する |

FloatSoda の `LaunchType.Scene` に対応する JSON 値 `scene` は、今回確認した Valve の公開 API、代表的なマニフェスト、SteamVR の実例からは確認できなかった。`EVRApplicationType.Scene` と manifest の `launch_type` は別の概念であり、同名と推定してはならない。FloatSoda のオーバーレイ exe には `binary` を使う。

### パス

- `AddApplicationManifest` の引数は、公式 API 上 `pchApplicationManifestFullPath`、つまりマニフェスト自体のフルパスである。
- `binary_path_windows` にファイル名だけを置く構成は、OVR Advanced Settings などの実例で使われている。配布時は `.vrmanifest` と exe の相対配置を固定する。
- JSON 内で Windows の絶対パスを直接書く場合、バックスラッシュは `\\` とエスケープする。`System.Text.Json` にシリアライズさせれば自動で処理される。
- `working_directory` が必要なアプリは明示する。指定しない場合の挙動に依存した相対ファイルアクセスは避ける。

### `strings`

ロケールコードをキーにして、少なくとも `name` を持つオブジェクトを置く。未対応ロケールでどの言語へ fallback するかは公開仕様から確認できないため、`en_us` を常に含めるのが安全である。

## 登録と寿命

公式 API の登録処理は次の契約を持つ。

```cpp
EVRApplicationError AddApplicationManifest(
    const char *pchApplicationManifestFullPath,
    bool bTemporary = false);
```

1. `.vrmanifest` をディスクに書く。
2. インストーラーなら `VRApplication_Utility` で OpenVR を初期化する。公式には Utility で `IVRApplications` の利用が保証されている。
3. `AddApplicationManifest(Path.GetFullPath(path), temporary)` を呼ぶ。
4. 必要に応じて `IsApplicationInstalled(appKey)` などで確認する。
5. 永続登録を解除するときは、登録時と同じマニフェストパスで `RemoveApplicationManifest` を呼ぶ。

`bTemporary` の公式デフォルトは `false`。公式ヘッダーには「temporary manifests are not automatically loaded」とあるため、SteamVR 再起動後にも使うインストール用途では `false` にする。開発中の一時登録だけ `true` にする。

### 自動起動

`SetApplicationAutoLaunch` / `GetApplicationAutoLaunch` は、公式 API 上 `is_dashboard_overlay == true` のアプリにだけ有効である。自動起動は manifest 内のキーではなく、登録後に `IVRApplications.SetApplicationAutoLaunch(appKey, true)` で設定する。

manifest の登録と自動起動設定を同一プロセスで直ちに行う構成では、SteamVR バージョンや不正な manifest により `UnknownApplication` になる報告がある。まず `IsApplicationInstalled` と SteamVR のログを確認する。

### 主なエラー

| エラー | 意味 | 最初に確認する点 |
|---|---|---|
| `NoManifest` | manifest が見つからない | 絶対パス、配置、アクセス権 |
| `InvalidManifest` | manifest が不正 | JSON 構文、必須キー、値の型 |
| `InvalidApplication` | application 定義が不正 | `app_key`、`launch_type`、対応する path / URL |
| `AppKeyAlreadyExists` | キーが重複 | 他 manifest と `app_key` が衝突していないか |
| `UnknownApplication` | キーを解決できない | 登録結果、キーの完全一致、一時登録の寿命 |
| `LaunchFailed` | プロセス起動失敗 | binary path、working directory、引数 |

## FloatSoda.OVR の現状評価

対象コードは `VRManifest.cs` と `OVRApplication.cs`。現状の API は、そのままでは有効な `.vrmanifest` を生成・登録できない可能性が高い。

### 1. `AppKey` が JSON 文字列にならない

SteamVR が要求する `"app_key": "com.example.overlay"` に対し、現在の `AppKey` は独自 record で `JsonConverter` がない。`System.Text.Json` は `ToString()` を JSON 値への変換には使わず、record のオブジェクトとして扱うため、期待する JSON 文字列にならない。

さらに `AppKey.ToString()` の `Select(p => p.Split(...))` は文字列配列の列を作っており、意図した「各 part を正規化して `.` で連結する」処理にもなっていない。

### 2. `launch_type` が数値になる

`.vrmanifest` は `"launch_type": "binary"` の文字列形式を使う。現在の serializer options には `JsonStringEnumConverter` がないため、`LaunchType.Binary` は既定では `0` として出力される。また一般的な値は小文字なので、標準 converter を追加する場合も命名方針の指定が必要になる。

### 3. 起動引数の JSON キーが異なる

OpenVR の公式取得プロパティは `Arguments_String` で、実際の manifest 例では `arguments` が使われる。現在の `binary_args` は今回の調査では裏付けられなかった。

### 4. `AddManifest` が manifest を書き出さない

現在の実装は `manifest` 引数をローカル変数へ代入するだけで、`path` に JSON を書いていない。その後、既にディスクに存在する `path` を OpenVR に登録するだけである。

呼び出し側が「渡した `VRManifest` が保存される」と理解すると、古いファイルを登録するか `NoManifest` になる。API 名と引数構成から見ても誤用しやすい。

### 5. 一時登録がデフォルトになっている

OpenVR 公式 API の既定値は `bTemporary = false` だが、FloatSoda の `AddManifest(..., bool temporal = true)` は逆である。また英語の API 用語は `temporary` であり、`temporal` は「時間的な」という別の意味になる。配布アプリの登録で引数を省略すると、SteamVR 再起動後に自動ロードされない設定になる。

### 6. モデルと仕様の対応が不完全

少なくとも `url`、正しい `arguments`、`action_manifest_path`、`is_hidden` が表現できない。一方で `LaunchType.Scene` のように公開資料で確認できない値が公開 API に含まれている。型が存在することで有効だと誤認させるため、将来の修正では「確認済みの値だけを型で表現する」方針が安全である。

## 推奨する修正方針（未実装）

この調査ではコード変更を行わない。修正する場合は次の順序がよい。

1. SteamVR が受け取る JSON を golden test として固定する。
2. `AppKey` を単純な string にするか、専用 `JsonConverter<AppKey>` を実装する。
3. `LaunchType` を `binary` / `url` へ確実に変換する converter を実装し、未確認の `Scene` を再検討する。
4. `BinaryArgs` の JSON 名を `arguments` に直す。
5. `AddManifest` を「保存して登録」と「既存ファイルを登録」の 2 操作へ分け、絶対パス化とファイル存在確認を行う。
6. `temporary` の既定値を公式 API と揃え、永続登録と一時登録を名前で判別できる API にする。
7. 最小 manifest、複数ロケール、URL launch、無効 JSON、重複 app key のテストを追加する。

## 参照資料

一次資料:

- [ValveSoftware/openvr: `openvr.h`](https://github.com/ValveSoftware/openvr/blob/master/headers/openvr.h) — `EVRApplicationError`、`EVRApplicationProperty`、`IVRApplications`、`k_unMaxApplicationKeyLength`
- [Valve OpenVR Wiki: API Documentation](https://github.com/ValveSoftware/openvr/wiki/API-Documentation) — `VRApplication_Utility` で `IVRApplications` が保証されることなど

実ファイル・補助資料:

- [OVR Advanced Settings: `manifest.vrmanifest`](https://github.com/OpenVR-Advanced-Settings/OpenVR-AdvancedSettings/blob/master/src/package_files/manifest.vrmanifest) — dashboard overlay の実運用例
- [ValveSoftware/openvr issue #106](https://github.com/ValveSoftware/openvr/issues/106) — permanent / temporary manifest と再読込に関する挙動報告
- [ValveSoftware/openvr issue #1378](https://github.com/ValveSoftware/openvr/issues/1378) — manifest 不備と `UnknownApplication` / auto-launch の事例

> 注意: issue と第三者プロジェクトは JSON Schema の一次仕様ではない。Valve の公開ヘッダーにない詳細挙動は SteamVR の更新で変わり得るため、SteamVR の実機ログでも確認すること。
