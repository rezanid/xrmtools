namespace XrmTools.Shell.Styles;

using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;

public class Typography : Microsoft.VisualStudio.PlatformUI.ObservableObject, IVsBroadcastMessageEvents
{
    private const uint WM_SYSCOLORCHANGE = 21;
    public static readonly double BodyLineHeight = 20.0;
    public static readonly double BodyLargeLineHeight = 24.0;
    public static readonly double CaptionLineHeight = 16.0;
    public static readonly double CodeLineHeight = 20.0;
    public static readonly double DisplayLineHeight = 92.0;
    public static readonly double SubtitleLineHeight = 28.0;
    public static readonly double TitleLineHeight = 36.0;
    public static readonly double TitleLargeLineHeight = 52.0;
    private static Typography? instance;
    private double bodyFontSize = 14.0;
    private double bodyLargeFontSize = 18.0;
    private double captionFontSize = 12.0;
    private FontFamily codeFontFamily = new FontFamily("Cascadia Mono");
    private double codeFontSize = 14.0;
    private double displayFontSize = 68.0;
    private FontFamily shellFontFamily = new FontFamily("Segoe UI");
    private double shellFontSize = 12.0;
    private double subtitleFontSize = 20.0;
    private double titleFontSize = 28.0;
    private double titleLargeFontSize = 40.0;

    //TODO: in Microsoft.VisualStudio.PlatformUI v18 chain to `base(false)`
    private Typography()
      : base() //false)
    {
        Dispatcher.CurrentDispatcher.VerifyAccess();
        if (!(ServiceProvider.GlobalProvider.GetService(typeof(SVsShell)) is IVsShell service))
            return;
        this.UpdateFontValues();
        service.AdviseBroadcastMessages((IVsBroadcastMessageEvents)this, out uint _);
    }

    public static Typography Instance
    {
        get => Typography.instance ?? (Typography.instance = new Typography());
    }

    public double BodyFontSize
    {
        get => this.bodyFontSize;
        private set => this.SetProperty<double>(ref this.bodyFontSize, value, nameof(BodyFontSize));
    }

    public double BodyLargeFontSize
    {
        get => this.bodyLargeFontSize;
        private set
        {
            this.SetProperty<double>(ref this.bodyLargeFontSize, value, nameof(BodyLargeFontSize));
        }
    }

    public double CaptionFontSize
    {
        get => this.captionFontSize;
        private set
        {
            this.SetProperty<double>(ref this.captionFontSize, value, nameof(CaptionFontSize));
        }
    }

    public FontFamily CodeFontFamily
    {
        get => this.codeFontFamily;
        private set
        {
            this.SetProperty<FontFamily>(ref this.codeFontFamily, value, nameof(CodeFontFamily));
        }
    }

    public double CodeFontSize
    {
        get => this.codeFontSize;
        private set => this.SetProperty<double>(ref this.codeFontSize, value, nameof(CodeFontSize));
    }

    public double DisplayFontSize
    {
        get => this.displayFontSize;
        private set
        {
            this.SetProperty<double>(ref this.displayFontSize, value, nameof(DisplayFontSize));
        }
    }

    public FontFamily ShellFontFamily
    {
        get => this.shellFontFamily;
        private set
        {
            this.SetProperty<FontFamily>(ref this.shellFontFamily, value, nameof(ShellFontFamily));
        }
    }

    public double ShellFontSize
    {
        get => this.shellFontSize;
        private set => this.SetProperty<double>(ref this.shellFontSize, value, nameof(ShellFontSize));
    }

    public double SubtitleFontSize
    {
        get => this.subtitleFontSize;
        private set
        {
            this.SetProperty<double>(ref this.subtitleFontSize, value, nameof(SubtitleFontSize));
        }
    }

    public double TitleFontSize
    {
        get => this.titleFontSize;
        private set => this.SetProperty<double>(ref this.titleFontSize, value, nameof(TitleFontSize));
    }

    public double TitleLargeFontSize
    {
        get => this.titleLargeFontSize;
        private set
        {
            this.SetProperty<double>(ref this.titleLargeFontSize, value, nameof(TitleLargeFontSize));
        }
    }

    int IVsBroadcastMessageEvents.OnBroadcastMessage(uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == 21U)
            this.UpdateFontValues();
        return 0;
    }

    private string GetFontFamilyName(UIDLGLOGFONT logFont)
    {
        StringBuilder stringBuilder = new StringBuilder(32 /*0x20*/);
        for (int index = 0; index < logFont.lfFaceName.Length && logFont.lfFaceName[index] != (ushort)0; ++index)
            stringBuilder.Append((char)logFont.lfFaceName[index]);
        return stringBuilder.ToString();
    }

    private void UpdateFontValues()
    {
        if (!(ServiceProvider.GlobalProvider.GetService(typeof(SUIHostLocale)) is IUIHostLocale service))
            return;
        UIDLGLOGFONT[] pLOGFONT = new UIDLGLOGFONT[1];
        if (Microsoft.VisualStudio.ErrorHandler.Failed(service.GetDialogFont(pLOGFONT)))
            return;
        this.ShellFontFamily = new FontFamily(this.GetFontFamilyName(pLOGFONT[0]));
        this.ShellFontSize = (double)Math.Abs(pLOGFONT[0].lfHeight) * 96.0 / DpiAwareness.SystemDpiY;
        this.BodyFontSize = Math.Round(this.ShellFontSize * 1.1667);
        this.BodyLargeFontSize = Math.Round(this.ShellFontSize * 1.5);
        this.CaptionFontSize = this.ShellFontSize;
        this.CodeFontSize = Math.Round(this.ShellFontSize * 1.1667);
        this.DisplayFontSize = Math.Round(this.ShellFontSize * 5.6667);
        this.SubtitleFontSize = Math.Round(this.ShellFontSize * 1.6667);
        this.TitleFontSize = Math.Round(this.ShellFontSize * 2.3333);
        this.TitleLargeFontSize = Math.Round(this.ShellFontSize * 3.3333);
    }
}