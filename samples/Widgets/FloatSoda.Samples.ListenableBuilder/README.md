# ListenableBuilder

## これは何か

`ListenableBuilder` は、モデルの変更通知を受け取るたびに、自分の中だけを作り直すウィジェットです。Flutter の `ListenableBuilder` に対応します。

FloatSoda では、モデルの変更通知に .NET の `INotifyPropertyChanged` を使います。WPF や MVVM で使うものと同じインターフェイスです。画面の一部だけがモデルの値に合わせて変わるとき、`StatefulWidget` の `SetState` を呼ばずに、その部分だけを作り直せます。

## 使い方

### モデルを用意する

モデルは `INotifyPropertyChanged` を実装し、値が変わったときに `PropertyChanged` を発生させます。このサンプルの `CounterModel` は、押された回数 `Count` を持ちます。

```csharp
public sealed class CounterModel : INotifyPropertyChanged
{
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>押された回数を取得または設定します。</summary>
    public int Count
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }
    }
}
```

モデルは `State` のフィールドに持ちます。`Build` の中で作ると、作り直すたびに別のモデルになり、通知を受け取る先が毎回替わってしまいます。

```csharp
private readonly CounterModel _counter = new();
```

### モデルの変更で作り直す

`Listenable` にモデルを、`ChildBuilder` に子を作る関数を渡します。モデルが `PropertyChanged` を発生させるたびに、`ChildBuilder` が呼び直されます。

```csharp
new ListenableBuilderWidget
{
    Listenable = _counter,
    ChildBuilder = _ => Value($"Count: {_counter.Count}", new Color(255, 209, 102))
},
```

このサンプルは名前空間が `FloatSoda.Samples.ListenableBuilder` で型名 `ListenableBuilder` と衝突するため、`ListenableBuilderWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new ListenableBuilder { ... }` と書けます。

ボタンのタップでは、モデルの値を変えるだけで、`SetState` は呼びません。

```csharp
OnTap = () => _counter.Count++,
```

同じモデルを、いくつの `ListenableBuilder` から聞いてもかまいません。サンプルでは、2つ目の `ListenableBuilder` が同じ `_counter` から偶数か奇数かを出しています。

### 作り直されるのは ListenableBuilder の中だけ

作り直されるのは `ChildBuilder` の中だけで、外側の `State.Build` は呼ばれません。サンプルの一番下の行は `State.Build` の中で `Count` を読んでいるため、ボタンを押しても 0 のまま変わりません。

```csharp
Value($"Count: {_counter.Count}", new Color(169, 180, 204))
```

値が変わったときに画面に出したいものは、`ChildBuilder` の中で読みます。

### 通知を出すスレッド

`PropertyChanged` は、ウィジェットを置いたスレッド(フレームを処理するスレッド)から発生させること。タイマーや `Task.Run` の中のような別のスレッドから発生させると、`ListenableBuilder` は `InvalidOperationException` を投げます。このサンプルでは、タップの処理(`OnTap`)の中でモデルを変えているため、フレームを処理するスレッドから通知が出ます。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 聞く相手の型 | `INotifyPropertyChanged` | `Listenable`(`ChangeNotifier` / `ValueNotifier` など) |
| 子を作る関数 | `ChildBuilder`。引数は `context` だけ | `builder`。引数は `context` と `child` |
| 作り直さない子を渡す `child` | **未提供** | `child` に渡した子は、作り直さずに `builder` へ渡される |
| 別のスレッドからの通知 | `InvalidOperationException` を投げる | UI のコードは1つの isolate の中で動くため、該当する制約がない |

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/Widgets/FloatSoda.Samples.ListenableBuilder -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/Widgets/FloatSoda.Samples.ListenableBuilder
```

ボタンのタップは、ダッシュボードとデスクトップウィンドウで受け取れます。

## 関連

- [FloatSoda.Samples.GestureDetector](../FloatSoda.Samples.GestureDetector) — タップの受け取りと `SetState`
- [docs/reference/find-widgets.md](../../../docs/reference/find-widgets.md) — 組み込みウィジェット一覧
