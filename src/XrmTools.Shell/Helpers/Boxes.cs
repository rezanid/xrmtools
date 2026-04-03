#nullable enable
namespace XrmTools.Shell.Helpers;

using System;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using XrmTools.Shell.Styles;

public static class Boxes
{
    public static readonly object BoolFalse = (object)false;
    public static readonly object BoolTrue = (object)true;
    public static readonly object ButtonKindAccent = (object)ButtonKind.Accent;
    public static readonly object ButtonKindStandard = (object)ButtonKind.Standard;
    public static readonly object ButtonKindSubtle = (object)ButtonKind.Subtle;
    public static readonly object DockBottom = (object)Dock.Bottom;
    public static readonly object DockLeft = (object)Dock.Left;
    public static readonly object DockRight = (object)Dock.Right;
    public static readonly object DockTop = (object)Dock.Top;
    public static readonly object DoubleZero = (object)0.0;
    public static readonly object FeedbackDislike = (object)Feedback.Dislike;
    public static readonly object FeedbackLike = (object)Feedback.Like;
    public static readonly object FeedbackNone = (object)Feedback.None;
    public static readonly object IntZero = (object)0;
    public static readonly object IntOne = (object)1;
    public static readonly object IntTwo = (object)2;
    public static readonly object IntThree = (object)3;
    public static readonly object IntFour = (object)4;
    public static readonly object IntFive = (object)5;
    public static readonly object MediaPlaybackModeLoop = (object)MediaPlaybackMode.Loop;
    public static readonly object MediaPlaybackModeManual = (object)MediaPlaybackMode.Manual;
    public static readonly object MediaPlaybackModeOnce = (object)MediaPlaybackMode.Once;
    public static readonly object MessageBoxButtonOk = (object)MessageBoxButton.OK;
    public static readonly object MessageBoxButtonOkCancel = (object)MessageBoxButton.OKCancel;
    public static readonly object MessageBoxButtonYesNo = (object)MessageBoxButton.YesNo;
    public static readonly object MessageBoxButtonYesNoCancel = (object)MessageBoxButton.YesNoCancel;
    public static readonly object MessageBoxResultCancel = (object)System.Windows.MessageBoxResult.Cancel;
    public static readonly object MessageBoxResultNo = (object)System.Windows.MessageBoxResult.No;
    public static readonly object MessageBoxResultNone = (object)System.Windows.MessageBoxResult.None;
    public static readonly object MessageBoxResultOk = (object)System.Windows.MessageBoxResult.OK;
    public static readonly object MessageBoxResultYes = (object)System.Windows.MessageBoxResult.Yes;
    public static readonly object OrientationHorizontal = (object)Orientation.Horizontal;
    public static readonly object OrientationVertical = (object)Orientation.Vertical;
    public static readonly object PlacementBottom = (object)PlacementMode.Bottom;
    public static readonly object PlacementLeft = (object)PlacementMode.Left;
    public static readonly object PlacementRight = (object)PlacementMode.Right;
    public static readonly object PlacementTop = (object)PlacementMode.Top;
    public static readonly object ProgressKindBarDeterminate = (object)ProgressKind.BarDeterminate;
    public static readonly object ProgressKindBarIndeterminate = (object)ProgressKind.BarIndeterminate;
    public static readonly object ProgressKindRingDeterminate = (object)ProgressKind.RingDeterminate;
    public static readonly object ProgressKindRingIndeterminate = (object)ProgressKind.RingIndeterminate;

    private static object Box(bool value) => !value ? Boxes.BoolFalse : Boxes.BoolTrue;

    private static object Box(ButtonKind value)
    {
        switch (value)
        {
            case ButtonKind.Standard:
                return Boxes.ButtonKindStandard;
            case ButtonKind.Subtle:
                return Boxes.ButtonKindSubtle;
            case ButtonKind.Accent:
                return Boxes.ButtonKindAccent;
            default:
                throw new NotImplementedException(value.ToString());
        }
    }

    public static object? Box<T>(T value)
    {
        if ((object?)value == null)
            return (object?)value;
        switch (value)
        {
            case bool flag:
                return Boxes.Box(flag);
            case double num1:
                return Boxes.Box(num1);
            case int num2:
                return Boxes.Box(num2);
            case ButtonKind buttonKind:
                return Boxes.Box(buttonKind);
            case Dock dock:
                return Boxes.Box(dock);
            case Feedback feedback:
                return Boxes.Box(feedback);
            case MediaPlaybackMode mediaPlaybackMode:
                return Boxes.Box(mediaPlaybackMode);
            case MessageBoxButton messageBoxButton:
                return Boxes.Box(messageBoxButton);
            case System.Windows.MessageBoxResult messageBoxResult:
                return Boxes.Box(messageBoxResult);
            case Orientation orientation:
                return Boxes.Box(orientation);
            case PlacementMode placementMode:
                return Boxes.Box(placementMode);
            case ProgressKind progressKind:
                return Boxes.Box(progressKind);
            default:
                return (object)value ?? throw new NotImplementedException(typeof(T).ToString());
        }
    }

    private static object Box(Dock value)
    {
        switch (value)
        {
            case Dock.Left:
                return Boxes.DockLeft;
            case Dock.Top:
                return Boxes.DockTop;
            case Dock.Right:
                return Boxes.DockRight;
            case Dock.Bottom:
                return Boxes.DockBottom;
            default:
                throw new NotImplementedException(value.ToString());
        }
    }

    private static object Box(double value) => value != 0.0 ? (object)value : Boxes.DoubleZero;

    private static object Box(Feedback value)
    {
        switch (value)
        {
            case Feedback.None:
                return Boxes.FeedbackNone;
            case Feedback.Like:
                return Boxes.FeedbackLike;
            case Feedback.Dislike:
                return Boxes.FeedbackDislike;
            default:
                throw new NotImplementedException(value.ToString());
        }
    }

    private static object Box(int value)
    {
        object obj;
        switch (value)
        {
            case 0:
                obj = Boxes.IntZero;
                break;
            case 1:
                obj = Boxes.IntOne;
                break;
            case 2:
                obj = Boxes.IntTwo;
                break;
            case 3:
                obj = Boxes.IntThree;
                break;
            case 4:
                obj = Boxes.IntFour;
                break;
            case 5:
                obj = Boxes.IntFive;
                break;
            default:
                obj = (object)value;
                break;
        }
        return obj;
    }

    private static object Box(MediaPlaybackMode value)
    {
        switch (value)
        {
            case MediaPlaybackMode.Once:
                return Boxes.MediaPlaybackModeOnce;
            case MediaPlaybackMode.Manual:
                return Boxes.MediaPlaybackModeManual;
            case MediaPlaybackMode.Loop:
                return Boxes.MediaPlaybackModeLoop;
            default:
                throw new NotImplementedException(value.ToString());
        }
    }

    private static object Box(MessageBoxButton value)
    {
        switch (value)
        {
            case MessageBoxButton.OK:
                return Boxes.MessageBoxButtonOk;
            case MessageBoxButton.OKCancel:
                return Boxes.MessageBoxButtonOkCancel;
            case MessageBoxButton.YesNoCancel:
                return Boxes.MessageBoxButtonYesNoCancel;
            case MessageBoxButton.YesNo:
                return Boxes.MessageBoxButtonYesNo;
            default:
                throw new NotImplementedException(value.ToString());
        }
    }

    private static object Box(System.Windows.MessageBoxResult value)
    {
        switch (value)
        {
            case System.Windows.MessageBoxResult.None:
                return Boxes.MessageBoxResultNone;
            case System.Windows.MessageBoxResult.OK:
                return Boxes.MessageBoxResultOk;
            case System.Windows.MessageBoxResult.Cancel:
                return Boxes.MessageBoxResultCancel;
            case System.Windows.MessageBoxResult.Yes:
                return Boxes.MessageBoxResultYes;
            case System.Windows.MessageBoxResult.No:
                return Boxes.MessageBoxResultNo;
            default:
                throw new NotImplementedException(value.ToString());
        }
    }

    private static object Box(Orientation value)
    {
        if (value == Orientation.Horizontal)
            return Boxes.OrientationHorizontal;
        if (value == Orientation.Vertical)
            return Boxes.OrientationVertical;
        throw new NotImplementedException(value.ToString());
    }

    private static object Box(PlacementMode value)
    {
        switch (value)
        {
            case PlacementMode.Bottom:
                return Boxes.PlacementBottom;
            case PlacementMode.Right:
                return Boxes.PlacementRight;
            case PlacementMode.Left:
                return Boxes.PlacementLeft;
            case PlacementMode.Top:
                return Boxes.PlacementTop;
            default:
                throw new NotImplementedException(value.ToString());
        }
    }

    private static object Box(ProgressKind value)
    {
        switch (value)
        {
            case ProgressKind.BarDeterminate:
                return Boxes.ProgressKindBarDeterminate;
            case ProgressKind.BarIndeterminate:
                return Boxes.ProgressKindBarIndeterminate;
            case ProgressKind.RingDeterminate:
                return Boxes.ProgressKindRingDeterminate;
            case ProgressKind.RingIndeterminate:
                return Boxes.ProgressKindRingIndeterminate;
            default:
                throw new NotImplementedException(value.ToString());
        }
    }
}

