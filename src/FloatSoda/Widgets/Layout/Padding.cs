// using FloatSoda.Common.Geometries;
// using FloatSoda.Geometrics;
// using FloatSoda.Render;
//
// namespace FloatSoda.Widgets.Layout;
//
// public record Padding : SingleChildRenderObjectWidget
// {
//     public EdgeInsets Spacing { get; init; } = EdgeInsets.Zero;
//
//     public override RenderObject CreateRenderObject()
//     {
//         return new RenderPadding
//         {
//             Spacing = Spacing
//         };
//     }
// }
//
// public class RenderSiftedBox : RenderBox, IHasSingleChildRenderObject<RenderBox>
// {
//     public RenderBox? Child { get; set; }
//     public RenderObject ThisRef => this;
//
//     protected override void PerformLayout(BoxConstraints constraints)
//     {
//         throw new NotImplementedException();
//     }
//
//     public override void Paint(PaintingContext context, Offset offset)
//     {
//         var child = Child;
//
//         if (child == null) return;
//
//         var childParentData = child.ParentData as BoxParentData;
//         child.Paint(context, offset + childParentData?.Offset ?? Offset.Zero);
//     }
// }
//
// public class RenderPadding : RenderSiftedBox
// {
//     public required EdgeInsets Spacing { get; init; } = EdgeInsets.Zero;
// }