using FloatSoda.Core;

namespace FloatSoda.Test.Core;

public class WindowKeyGeneratorTest
{
    [Theory]
    [InlineData("WatchDashBoard", "watch_dash_board")]
    [InlineData("Left Hand", "left_hand")]
    [InlineData("FloatSoda.Samples.OverlayApp", "float_soda_samples_overlay_app")]
    [InlineData("OVRApp", "ovr_app")]
    [InlineData("simple", "simple")]
    [InlineData("My  Spaced   Title", "my_spaced_title")]
    [InlineData("Version2Overlay", "version2_overlay")]
    public void ToSnakeCase_ConvertsWordBoundaries(string input, string expected)
    {
        Assert.Equal(expected, WindowKeyGenerator.ToSnakeCase(input));
    }

    [Fact]
    public void GenerateKey_JoinsEndpointAndTitleWithDot()
    {
        var key = WindowKeyGenerator.GenerateKey("My Dashboard");

        // エンドポイント（エントリアセンブリ名）はテストホスト依存のため、形式のみ検証する
        Assert.EndsWith(".my_dashboard", key);
        Assert.Matches(@"^[a-z0-9_]+\.[a-z0-9_]+$", key);
    }
}
