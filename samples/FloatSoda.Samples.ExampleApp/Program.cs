using FloatSoda;
using Microsoft.Extensions.Hosting;

var builder = OverlayAppBuilder.CreateDefault("key", "name");

var app = builder.Build();

app.Run();