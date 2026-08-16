namespace XrmTools.Shell.Styles;

using System.Windows;

public static class CornerRadii
{
    public static readonly double PrimitiveNone = 0.0;
    public static readonly double PrimitiveS = 2.0;
    public static readonly double PrimitiveM = 4.0;
    public static readonly double PrimitiveL = 6.0;
    public static readonly double PrimitiveXL = 8.0;
    public static readonly double PrimitiveFocusRect = 6.0;
    public static readonly CornerRadius None = new(PrimitiveNone);
    public static readonly CornerRadius S = new(PrimitiveS);
    public static readonly CornerRadius M = new(PrimitiveM);
    public static readonly CornerRadius L = new(PrimitiveL);
    public static readonly CornerRadius XL = new(PrimitiveXL);
    public static readonly CornerRadius FocusRect = new(PrimitiveFocusRect);
    public static readonly CornerRadius TopLeftS = new(PrimitiveS, PrimitiveNone, PrimitiveNone, PrimitiveNone);
    public static readonly CornerRadius TopLeftM = new(PrimitiveM, PrimitiveNone, PrimitiveNone, PrimitiveNone);
    public static readonly CornerRadius TopLeftL = new(PrimitiveL, PrimitiveNone, PrimitiveNone, PrimitiveNone);
    public static readonly CornerRadius TopLeftXL = new(PrimitiveXL, PrimitiveNone, PrimitiveNone, PrimitiveNone);
    public static readonly CornerRadius TopRightS = new(PrimitiveNone, PrimitiveS, PrimitiveNone, PrimitiveNone);
    public static readonly CornerRadius TopRightM = new(PrimitiveNone, PrimitiveM, PrimitiveNone, PrimitiveNone);
    public static readonly CornerRadius TopRightL = new(PrimitiveNone, PrimitiveL, PrimitiveNone, PrimitiveNone);
    public static readonly CornerRadius TopRightXL = new(PrimitiveNone, PrimitiveXL, PrimitiveNone, PrimitiveNone);
    public static readonly CornerRadius BottomRightS = new(PrimitiveNone, PrimitiveNone, PrimitiveS, PrimitiveNone);
    public static readonly CornerRadius BottomRightM = new(PrimitiveNone, PrimitiveNone, PrimitiveM, PrimitiveNone);
    public static readonly CornerRadius BottomRightL = new(PrimitiveNone, PrimitiveNone, PrimitiveL, PrimitiveNone);
    public static readonly CornerRadius BottomRightXL = new(PrimitiveNone, PrimitiveNone, PrimitiveXL, PrimitiveNone);
    public static readonly CornerRadius BottomLeftS = new(PrimitiveNone, PrimitiveNone, PrimitiveNone, PrimitiveS);
    public static readonly CornerRadius BottomLeftM = new(PrimitiveNone, PrimitiveNone, PrimitiveNone, PrimitiveM);
    public static readonly CornerRadius BottomLeftL = new(PrimitiveNone, PrimitiveNone, PrimitiveNone, PrimitiveL);
    public static readonly CornerRadius BottomLeftXL = new(PrimitiveNone, PrimitiveNone, PrimitiveNone, PrimitiveXL);
    public static readonly CornerRadius TopLeftBottomLeftS = new(PrimitiveS, PrimitiveNone, PrimitiveNone, PrimitiveS);
    public static readonly CornerRadius TopLeftBottomLeftM = new(PrimitiveM, PrimitiveNone, PrimitiveNone, PrimitiveM);
    public static readonly CornerRadius TopLeftBottomLeftL = new(PrimitiveL, PrimitiveNone, PrimitiveNone, PrimitiveL);
    public static readonly CornerRadius TopLeftBottomLeftXL = new(PrimitiveXL, PrimitiveNone, PrimitiveNone, PrimitiveXL);
    public static readonly CornerRadius TopRightBottomRightS = new(PrimitiveNone, PrimitiveS, PrimitiveS, PrimitiveNone);
    public static readonly CornerRadius TopRightBottomRightM = new(PrimitiveNone, PrimitiveM, PrimitiveM, PrimitiveNone);
    public static readonly CornerRadius TopRightBottomRightL = new(PrimitiveNone, PrimitiveL, PrimitiveL, PrimitiveNone);
    public static readonly CornerRadius TopRightBottomRightXL = new(PrimitiveNone, PrimitiveXL, PrimitiveXL, PrimitiveNone);
    public static readonly CornerRadius TopLeftTopRightS = new(PrimitiveS, PrimitiveS, PrimitiveNone, PrimitiveNone);
    public static readonly CornerRadius TopLeftTopRightM = new(PrimitiveM, PrimitiveM, PrimitiveNone, PrimitiveNone);
    public static readonly CornerRadius TopLeftTopRightL = new(PrimitiveL, PrimitiveL, PrimitiveNone, PrimitiveNone);
    public static readonly CornerRadius TopLeftTopRightXL = new(PrimitiveXL, PrimitiveXL, PrimitiveNone, PrimitiveNone);
    public static readonly CornerRadius BottomRightBottomLeftS = new(PrimitiveNone, PrimitiveNone, PrimitiveS, PrimitiveS);
    public static readonly CornerRadius BottomRightBottomLeftM = new(PrimitiveNone, PrimitiveNone, PrimitiveM, PrimitiveM);
    public static readonly CornerRadius BottomRightBottomLeftL = new(PrimitiveNone, PrimitiveNone, PrimitiveL, PrimitiveL);
    public static readonly CornerRadius BottomRightBottomLeftXL = new(PrimitiveNone, PrimitiveNone, PrimitiveXL, PrimitiveXL);
    public static readonly CornerRadius TopLeftBottomRightBottomLeftS = new(PrimitiveS, PrimitiveNone, PrimitiveS, PrimitiveS);
    public static readonly CornerRadius TopLeftBottomRightBottomLeftM = new(PrimitiveM, PrimitiveNone, PrimitiveM, PrimitiveM);
    public static readonly CornerRadius TopLeftBottomRightBottomLeftL = new(PrimitiveL, PrimitiveNone, PrimitiveL, PrimitiveL);
    public static readonly CornerRadius TopLeftBottomRightBottomLeftXL = new(PrimitiveXL, PrimitiveNone, PrimitiveXL, PrimitiveXL);
    public static readonly CornerRadius TopLeftTopRightBottomLeftS = new(PrimitiveS, PrimitiveS, PrimitiveNone, PrimitiveS);
    public static readonly CornerRadius TopLeftTopRightBottomLeftM = new(PrimitiveM, PrimitiveM, PrimitiveNone, PrimitiveM);
    public static readonly CornerRadius TopLeftTopRightBottomLeftL = new(PrimitiveL, PrimitiveL, PrimitiveNone, PrimitiveL);
    public static readonly CornerRadius TopLeftTopRightBottomLeftXL = new(PrimitiveXL, PrimitiveXL, PrimitiveNone, PrimitiveXL);
    public static readonly CornerRadius TopLeftTopRightBottomRightS = new(PrimitiveS, PrimitiveS, PrimitiveS, PrimitiveNone);
    public static readonly CornerRadius TopLeftTopRightBottomRightM = new(PrimitiveM, PrimitiveM, PrimitiveM, PrimitiveNone);
    public static readonly CornerRadius TopLeftTopRightBottomRightL = new(PrimitiveL, PrimitiveL, PrimitiveL, PrimitiveNone);
    public static readonly CornerRadius TopLeftTopRightBottomRightXL = new(PrimitiveXL, PrimitiveXL, PrimitiveXL, PrimitiveNone);
    public static readonly CornerRadius TopRightBottomRightBottomLeftS = new(PrimitiveNone, PrimitiveS, PrimitiveS, PrimitiveS);
    public static readonly CornerRadius TopRightBottomRightBottomLeftM = new(PrimitiveNone, PrimitiveM, PrimitiveM, PrimitiveM);
    public static readonly CornerRadius TopRightBottomRightBottomLeftL = new(PrimitiveNone, PrimitiveL, PrimitiveL, PrimitiveL);
    public static readonly CornerRadius TopRightBottomRightBottomLeftXL = new(PrimitiveNone, PrimitiveXL, PrimitiveXL, PrimitiveXL);
}

