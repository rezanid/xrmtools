namespace XrmTools.Shell.Converters;

using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

internal sealed class RingGeometryConverter : MultiValueConverter<double, double, double, Geometry>
{
    public const double IndeterminateProgress = 0.67;

    protected override Geometry Convert(
        double progress,
        double diameter,
        double stroke,
        object parameter,
        CultureInfo culture)
    {
        var center = diameter / 2;
        var inset = stroke / 2;
        var radius = center - inset;
        if (radius <= 0 || progress == 0) return Geometry.Empty;
        if (progress > 0.995) return new EllipseGeometry(new Point(center, center), radius, radius);

        var angle = progress * 360 - 90;
        var radians = angle * Math.PI / 180;
        var arc = new ArcSegment
        {
            Size = new Size(radius, radius),
            SweepDirection = SweepDirection.Clockwise,
            IsLargeArc = progress > 0.5,
            Point = new Point(center + radius * Math.Cos(radians), center + radius * Math.Sin(radians)),
        };
        return new PathGeometry(new[]
        {
            new PathFigure(new Point(center, inset), new PathSegment[] { arc }, false),
        });
    }
}
