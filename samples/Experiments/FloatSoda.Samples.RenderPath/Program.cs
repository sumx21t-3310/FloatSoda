using FloatSoda.Samples.RenderPath;

// 描画が SteamVR のオーバーレイに届くまでを、層を1つずつ足して確かめる。
// 段は --step 1 から --step 4 で選ぶ。段の中身は Step1.cs から Step4.cs にある。
//
//   --step 1 [png のパス]      OVR 層だけで、png をオーバーレイに出す(SteamVR と HMD が要る)
//   --step 2 [png の保存先]    Layer ツリーを組んで png にする(SteamVR も HMD も要らない)
//   --step 3                   Layer ツリーを GL テクスチャで出す(SteamVR と HMD が要る)
//   --step 4                   ウィジェットで色の四角を書き、ダッシュボードに出す(SteamVR と HMD が要る)
//
// png のパスの既定は、段1と段2で同じ(カレントディレクトリの my_layer_tree.png)。

var step = 0;
var positionalArgs = new List<string>();
for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "--step")
    {
        if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out var parsed))
        {
            Console.WriteLine("--step の後ろに、段の番号(1 から 4)を書いてください。");
            return 1;
        }

        step = parsed;
        i++;
    }
    else
    {
        positionalArgs.Add(args[i]);
    }
}

var pngPath = Path.GetFullPath(positionalArgs.Count > 0 ? positionalArgs[0] : "my_layer_tree.png");

switch (step)
{
    case 1:
        Step1.Run(pngPath);
        break;
    case 2:
        Step2.Run(pngPath);
        break;
    case 3:
        Step3.Run();
        break;
    case 4:
        await Step4.RunAsync(positionalArgs.ToArray());
        break;
    default:
        Console.WriteLine("段を --step 1 から --step 4 で指定してください。");
        return 1;
}

return 0;
