namespace XrmTools.Shell.Styles;

using Microsoft.VisualStudio.PlatformUI;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

internal class ClipConverter : MultiValueConverter<double, double, CornerRadius, Geometry>
{
    protected override Geometry Convert(
      double width,
      double height,
      CornerRadius cornerRadius,
      object parameter,
      CultureInfo culture)
    {
        Geometry clipGeometry = this.GenerateClipGeometry(new Size(width, height), cornerRadius);
        if (clipGeometry.CanFreeze)
            clipGeometry.Freeze();
        return clipGeometry;
    }

    private Geometry GenerateClipGeometry(Size size, CornerRadius cornerRadius)
    {
        Point point = new Point(cornerRadius.TopLeft, 0.0);
        Point endPoint1 = new Point(size.Width - cornerRadius.TopRight, 0.0);
        Point endPoint2 = new Point(size.Width, cornerRadius.TopRight);
        Point endPoint3 = new Point(size.Width, size.Height - cornerRadius.BottomRight);
        Point endPoint4 = new Point(size.Width - cornerRadius.BottomRight, size.Height);
        Point endPoint5 = new Point(cornerRadius.BottomLeft, size.Height);
        Point endPoint6 = new Point(0.0, size.Height - cornerRadius.BottomLeft);
        Point endPoint7 = new Point(0.0, cornerRadius.TopLeft);
        StreamGeometry clipGeometry = new StreamGeometry();
        using (StreamGeometryContext context = clipGeometry.Open())
        {
            context.BeginFigure(point, true, true);
            DrawLineTo(context, endPoint1);
            DrawArcTo(context, endPoint2, cornerRadius.TopRight, SweepDirection.Clockwise);
            DrawLineTo(context, endPoint3);
            DrawArcTo(context, endPoint4, cornerRadius.BottomRight, SweepDirection.Clockwise);
            DrawLineTo(context, endPoint5);
            DrawArcTo(context, endPoint6, cornerRadius.BottomLeft, SweepDirection.Clockwise);
            DrawLineTo(context, endPoint7);
            DrawArcTo(context, point, cornerRadius.TopLeft, SweepDirection.Clockwise);
            clipGeometry.Freeze();
        }
        return (Geometry)clipGeometry;

        static void DrawArcTo(
          StreamGeometryContext context,
          Point endPoint,
          double radius,
          SweepDirection direction)
        {
            context.ArcTo(endPoint, new Size(radius, radius), 0.0, false, direction, true, false);
        }

        static void DrawLineTo(StreamGeometryContext context, Point endPoint)
        {
            context.LineTo(endPoint, true, false);
        }
    }
}
