using FloatSoda.Widgets;
using FloatSoda.Widgets.Components;
using FloatSoda.Widgets.Layout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FloatSoda.Samples.OverlayApp;

public record MyApp : StatelessWidget
{
    public override Widget Build(IBuildContext context)
    {
        var count = context.UseState(() => 0);
        var logger = context.Depends(provider => provider.GetService<ILogger>());

        return new OverlayWindow
        {
            Child = new Center
            {
                Child = new Column
                {
                    Children =
                    [
                        new Text("Hello, Hello Float Soda!"),
                        new Text($"Cora count: {count.Value}"),
                        new Button()
                        {
                            Child = new Text("Add cora!"),
                            OnPressed = () => count.Value++,
                        },
                        new Button
                        {
                            Child = new Text("Order!"),
                            OnPressed = () => logger?.LogInformation("Order!")
                        }
                    ]
                }
            }
        };
    }
}