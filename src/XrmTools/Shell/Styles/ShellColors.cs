#nullable enable
namespace XrmTools.Shell.Styles;

using Microsoft.VisualStudio.Shell;
using System;

public static class ShellColors
{
    public static readonly Guid Category = new("73708ded-2d56-4aad-b8eb-73b20d3f4bff");
    private static ThemeResourceKey? _AccentFillAltColorKey;
    private static ThemeResourceKey? _AccentFillAltBrushKey;
    private static ThemeResourceKey? _AccentFillDefaultColorKey;
    private static ThemeResourceKey? _AccentFillDefaultBrushKey;
    private static ThemeResourceKey? _AccentFillDisabledColorKey;
    private static ThemeResourceKey? _AccentFillDisabledBrushKey;
    private static ThemeResourceKey? _AccentFillSecondaryColorKey;
    private static ThemeResourceKey? _AccentFillSecondaryBrushKey;
    private static ThemeResourceKey? _AccentFillSelectedTextBackgroundColorKey;
    private static ThemeResourceKey? _AccentFillSelectedTextBackgroundBrushKey;
    private static ThemeResourceKey? _AccentFillSelectedTextBackgroundSubtleColorKey;
    private static ThemeResourceKey? _AccentFillSelectedTextBackgroundSubtleBrushKey;
    private static ThemeResourceKey? _AccentFillSenaryColorKey;
    private static ThemeResourceKey? _AccentFillSenaryBrushKey;
    private static ThemeResourceKey? _AccentFillTertiaryColorKey;
    private static ThemeResourceKey? _AccentFillTertiaryBrushKey;
    private static ThemeResourceKey? _AccentTextFillDisabledColorKey;
    private static ThemeResourceKey? _AccentTextFillDisabledBrushKey;
    private static ThemeResourceKey? _AccentTextFillPrimaryColorKey;
    private static ThemeResourceKey? _AccentTextFillPrimaryBrushKey;
    private static ThemeResourceKey? _AccentTextFillSecondaryColorKey;
    private static ThemeResourceKey? _AccentTextFillSecondaryBrushKey;
    private static ThemeResourceKey? _AccentTextFillTertiaryColorKey;
    private static ThemeResourceKey? _AccentTextFillTertiaryBrushKey;
    private static ThemeResourceKey? _CardBackgroundFillDefaultColorKey;
    private static ThemeResourceKey? _CardBackgroundFillDefaultBrushKey;
    private static ThemeResourceKey? _CardBackgroundFillSecondaryColorKey;
    private static ThemeResourceKey? _CardBackgroundFillSecondaryBrushKey;
    private static ThemeResourceKey? _CardBackgroundFillTertiaryColorKey;
    private static ThemeResourceKey? _CardBackgroundFillTertiaryBrushKey;
    private static ThemeResourceKey? _CardStrokeDefaultColorKey;
    private static ThemeResourceKey? _CardStrokeDefaultBrushKey;
    private static ThemeResourceKey? _CardStrokeDefaultSolidColorKey;
    private static ThemeResourceKey? _CardStrokeDefaultSolidBrushKey;
    private static ThemeResourceKey? _CardStrokeDefaultSolidAltColorKey;
    private static ThemeResourceKey? _CardStrokeDefaultSolidAltBrushKey;
    private static ThemeResourceKey? _ControlAltFillDisabledColorKey;
    private static ThemeResourceKey? _ControlAltFillDisabledBrushKey;
    private static ThemeResourceKey? _ControlAltFillQuaternaryColorKey;
    private static ThemeResourceKey? _ControlAltFillQuaternaryBrushKey;
    private static ThemeResourceKey? _ControlAltFillSecondaryColorKey;
    private static ThemeResourceKey? _ControlAltFillSecondaryBrushKey;
    private static ThemeResourceKey? _ControlAltFillTertiaryColorKey;
    private static ThemeResourceKey? _ControlAltFillTertiaryBrushKey;
    private static ThemeResourceKey? _ControlAltFillTransparentColorKey;
    private static ThemeResourceKey? _ControlAltFillTransparentBrushKey;
    private static ThemeResourceKey? _ControlFillActiveInputColorKey;
    private static ThemeResourceKey? _ControlFillActiveInputBrushKey;
    private static ThemeResourceKey? _ControlFillDefaultColorKey;
    private static ThemeResourceKey? _ControlFillDefaultBrushKey;
    private static ThemeResourceKey? _ControlFillDisabledColorKey;
    private static ThemeResourceKey? _ControlFillDisabledBrushKey;
    private static ThemeResourceKey? _ControlFillQuaternaryColorKey;
    private static ThemeResourceKey? _ControlFillQuaternaryBrushKey;
    private static ThemeResourceKey? _ControlFillSecondaryColorKey;
    private static ThemeResourceKey? _ControlFillSecondaryBrushKey;
    private static ThemeResourceKey? _ControlFillTertiaryColorKey;
    private static ThemeResourceKey? _ControlFillTertiaryBrushKey;
    private static ThemeResourceKey? _ControlFillTransparentColorKey;
    private static ThemeResourceKey? _ControlFillTransparentBrushKey;
    private static ThemeResourceKey? _ControlOnImageFillDefaultColorKey;
    private static ThemeResourceKey? _ControlOnImageFillDefaultBrushKey;
    private static ThemeResourceKey? _ControlOnImageFillDisabledColorKey;
    private static ThemeResourceKey? _ControlOnImageFillDisabledBrushKey;
    private static ThemeResourceKey? _ControlOnImageFillSecondaryColorKey;
    private static ThemeResourceKey? _ControlOnImageFillSecondaryBrushKey;
    private static ThemeResourceKey? _ControlOnImageFillTertiaryColorKey;
    private static ThemeResourceKey? _ControlOnImageFillTertiaryBrushKey;
    private static ThemeResourceKey? _ControlSolidFillDefaultColorKey;
    private static ThemeResourceKey? _ControlSolidFillDefaultBrushKey;
    private static ThemeResourceKey? _ControlStrokeDefaultColorKey;
    private static ThemeResourceKey? _ControlStrokeDefaultBrushKey;
    private static ThemeResourceKey? _ControlStrokeForStrongFillWhenOnImageColorKey;
    private static ThemeResourceKey? _ControlStrokeForStrongFillWhenOnImageBrushKey;
    private static ThemeResourceKey? _ControlStrokeOnAccentDefaultColorKey;
    private static ThemeResourceKey? _ControlStrokeOnAccentDefaultBrushKey;
    private static ThemeResourceKey? _ControlStrokeOnAccentDisabledColorKey;
    private static ThemeResourceKey? _ControlStrokeOnAccentDisabledBrushKey;
    private static ThemeResourceKey? _ControlStrokeOnAccentSecondaryColorKey;
    private static ThemeResourceKey? _ControlStrokeOnAccentSecondaryBrushKey;
    private static ThemeResourceKey? _ControlStrokeOnAccentTertiaryColorKey;
    private static ThemeResourceKey? _ControlStrokeOnAccentTertiaryBrushKey;
    private static ThemeResourceKey? _ControlStrokeSecondaryColorKey;
    private static ThemeResourceKey? _ControlStrokeSecondaryBrushKey;
    private static ThemeResourceKey? _ControlStrokeTransparentColorKey;
    private static ThemeResourceKey? _ControlStrokeTransparentBrushKey;
    private static ThemeResourceKey? _ControlStrongFillDefaultColorKey;
    private static ThemeResourceKey? _ControlStrongFillDefaultBrushKey;
    private static ThemeResourceKey? _ControlStrongFillDisabledColorKey;
    private static ThemeResourceKey? _ControlStrongFillDisabledBrushKey;
    private static ThemeResourceKey? _ControlStrongStrokeDefaultColorKey;
    private static ThemeResourceKey? _ControlStrongStrokeDefaultBrushKey;
    private static ThemeResourceKey? _ControlStrongStrokeDisabledColorKey;
    private static ThemeResourceKey? _ControlStrongStrokeDisabledBrushKey;
    private static ThemeResourceKey? _DividerStrokeDefaultColorKey;
    private static ThemeResourceKey? _DividerStrokeDefaultBrushKey;
    private static ThemeResourceKey? _FocusStrokeInnerColorKey;
    private static ThemeResourceKey? _FocusStrokeInnerBrushKey;
    private static ThemeResourceKey? _FocusStrokeOuterColorKey;
    private static ThemeResourceKey? _FocusStrokeOuterBrushKey;
    private static ThemeResourceKey? _HyperlinkFillDisabledColorKey;
    private static ThemeResourceKey? _HyperlinkFillDisabledBrushKey;
    private static ThemeResourceKey? _HyperlinkFillPrimaryColorKey;
    private static ThemeResourceKey? _HyperlinkFillPrimaryBrushKey;
    private static ThemeResourceKey? _HyperlinkFillSecondaryColorKey;
    private static ThemeResourceKey? _HyperlinkFillSecondaryBrushKey;
    private static ThemeResourceKey? _HyperlinkFillTertiaryColorKey;
    private static ThemeResourceKey? _HyperlinkFillTertiaryBrushKey;
    private static ThemeResourceKey? _LayerFillAltColorKey;
    private static ThemeResourceKey? _LayerFillAltBrushKey;
    private static ThemeResourceKey? _LayerFillDefaultColorKey;
    private static ThemeResourceKey? _LayerFillDefaultBrushKey;
    private static ThemeResourceKey? _ShadowFlyoutColorKey;
    private static ThemeResourceKey? _ShadowFlyoutBrushKey;
    private static ThemeResourceKey? _SmokeFillDefaultColorKey;
    private static ThemeResourceKey? _SmokeFillDefaultBrushKey;
    private static ThemeResourceKey? _SmokeFillInverseColorKey;
    private static ThemeResourceKey? _SmokeFillInverseBrushKey;
    private static ThemeResourceKey? _SolidBackgroundFillBaseColorKey;
    private static ThemeResourceKey? _SolidBackgroundFillBaseBrushKey;
    private static ThemeResourceKey? _SolidBackgroundFillBaseAltColorKey;
    private static ThemeResourceKey? _SolidBackgroundFillBaseAltBrushKey;
    private static ThemeResourceKey? _SolidBackgroundFillQuaternaryColorKey;
    private static ThemeResourceKey? _SolidBackgroundFillQuaternaryBrushKey;
    private static ThemeResourceKey? _SolidBackgroundFillQuinaryColorKey;
    private static ThemeResourceKey? _SolidBackgroundFillQuinaryBrushKey;
    private static ThemeResourceKey? _SolidBackgroundFillSecondaryColorKey;
    private static ThemeResourceKey? _SolidBackgroundFillSecondaryBrushKey;
    private static ThemeResourceKey? _SolidBackgroundFillSenaryColorKey;
    private static ThemeResourceKey? _SolidBackgroundFillSenaryBrushKey;
    private static ThemeResourceKey? _SolidBackgroundFillTertiaryColorKey;
    private static ThemeResourceKey? _SolidBackgroundFillTertiaryBrushKey;
    private static ThemeResourceKey? _SubtleFillDisabledColorKey;
    private static ThemeResourceKey? _SubtleFillDisabledBrushKey;
    private static ThemeResourceKey? _SubtleFillSecondaryColorKey;
    private static ThemeResourceKey? _SubtleFillSecondaryBrushKey;
    private static ThemeResourceKey? _SubtleFillTertiaryColorKey;
    private static ThemeResourceKey? _SubtleFillTertiaryBrushKey;
    private static ThemeResourceKey? _SubtleFillTransparentColorKey;
    private static ThemeResourceKey? _SubtleFillTransparentBrushKey;
    private static ThemeResourceKey? _SurfaceBackgroundFillDefaultColorKey;
    private static ThemeResourceKey? _SurfaceBackgroundFillDefaultBrushKey;
    private static ThemeResourceKey? _SurfaceStrokeDefaultColorKey;
    private static ThemeResourceKey? _SurfaceStrokeDefaultBrushKey;
    private static ThemeResourceKey? _SurfaceStrokeFlyoutColorKey;
    private static ThemeResourceKey? _SurfaceStrokeFlyoutBrushKey;
    private static ThemeResourceKey? _SystemFillAttentionColorKey;
    private static ThemeResourceKey? _SystemFillAttentionBrushKey;
    private static ThemeResourceKey? _SystemFillAttentionBackgroundColorKey;
    private static ThemeResourceKey? _SystemFillAttentionBackgroundBrushKey;
    private static ThemeResourceKey? _SystemFillCautionColorKey;
    private static ThemeResourceKey? _SystemFillCautionBrushKey;
    private static ThemeResourceKey? _SystemFillCautionBackgroundColorKey;
    private static ThemeResourceKey? _SystemFillCautionBackgroundBrushKey;
    private static ThemeResourceKey? _SystemFillCriticalColorKey;
    private static ThemeResourceKey? _SystemFillCriticalBrushKey;
    private static ThemeResourceKey? _SystemFillCriticalBackgroundColorKey;
    private static ThemeResourceKey? _SystemFillCriticalBackgroundBrushKey;
    private static ThemeResourceKey? _SystemFillNeutralColorKey;
    private static ThemeResourceKey? _SystemFillNeutralBrushKey;
    private static ThemeResourceKey? _SystemFillNeutralBackgroundColorKey;
    private static ThemeResourceKey? _SystemFillNeutralBackgroundBrushKey;
    private static ThemeResourceKey? _SystemFillSolidAttentionBackgroundColorKey;
    private static ThemeResourceKey? _SystemFillSolidAttentionBackgroundBrushKey;
    private static ThemeResourceKey? _SystemFillSolidNeutralColorKey;
    private static ThemeResourceKey? _SystemFillSolidNeutralBrushKey;
    private static ThemeResourceKey? _SystemFillSolidNeutralBackgroundColorKey;
    private static ThemeResourceKey? _SystemFillSolidNeutralBackgroundBrushKey;
    private static ThemeResourceKey? _SystemFillSuccessColorKey;
    private static ThemeResourceKey? _SystemFillSuccessBrushKey;
    private static ThemeResourceKey? _SystemFillSuccessBackgroundColorKey;
    private static ThemeResourceKey? _SystemFillSuccessBackgroundBrushKey;
    private static ThemeResourceKey? _TextFillDisabledColorKey;
    private static ThemeResourceKey? _TextFillDisabledBrushKey;
    private static ThemeResourceKey? _TextFillPrimaryColorKey;
    private static ThemeResourceKey? _TextFillPrimaryBrushKey;
    private static ThemeResourceKey? _TextFillSecondaryColorKey;
    private static ThemeResourceKey? _TextFillSecondaryBrushKey;
    private static ThemeResourceKey? _TextFillTertiaryColorKey;
    private static ThemeResourceKey? _TextFillTertiaryBrushKey;
    private static ThemeResourceKey? _TextOnAccentFillDisabledColorKey;
    private static ThemeResourceKey? _TextOnAccentFillDisabledBrushKey;
    private static ThemeResourceKey? _TextOnAccentFillPrimaryColorKey;
    private static ThemeResourceKey? _TextOnAccentFillPrimaryBrushKey;
    private static ThemeResourceKey? _TextOnAccentFillSecondaryColorKey;
    private static ThemeResourceKey? _TextOnAccentFillSecondaryBrushKey;
    private static ThemeResourceKey? _TextOnAccentFillSelectedTextColorKey;
    private static ThemeResourceKey? _TextOnAccentFillSelectedTextBrushKey;

    public static ThemeResourceKey AccentFillAltColorKey => _AccentFillAltColorKey ??= new ThemeResourceKey(Category, "AccentFillAlt", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey AccentFillAltBrushKey => _AccentFillAltBrushKey ??= new ThemeResourceKey(Category, "AccentFillAlt", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey AccentFillDefaultColorKey => _AccentFillDefaultColorKey ??= new ThemeResourceKey(Category, "AccentFillDefault", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey AccentFillDefaultBrushKey => _AccentFillDefaultBrushKey ??= new ThemeResourceKey(Category, "AccentFillDefault", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey AccentFillDisabledColorKey => _AccentFillDisabledColorKey ??= new ThemeResourceKey(Category, "AccentFillDisabled", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey AccentFillDisabledBrushKey => _AccentFillDisabledBrushKey ??= new ThemeResourceKey(Category, "AccentFillDisabled", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey AccentFillSecondaryColorKey => _AccentFillSecondaryColorKey ??= new ThemeResourceKey(Category, "AccentFillSecondary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey AccentFillSecondaryBrushKey => _AccentFillSecondaryBrushKey ??= new ThemeResourceKey(Category, "AccentFillSecondary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey AccentFillSelectedTextBackgroundColorKey => _AccentFillSelectedTextBackgroundColorKey ??= new ThemeResourceKey(Category, "AccentFillSelectedTextBackground", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey AccentFillSelectedTextBackgroundBrushKey => _AccentFillSelectedTextBackgroundBrushKey ??= new ThemeResourceKey(Category, "AccentFillSelectedTextBackground", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey AccentFillSelectedTextBackgroundSubtleColorKey => _AccentFillSelectedTextBackgroundSubtleColorKey ??= new ThemeResourceKey(Category, "AccentFillSelectedTextBackgroundSubtle", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey AccentFillSelectedTextBackgroundSubtleBrushKey => _AccentFillSelectedTextBackgroundSubtleBrushKey ??= new ThemeResourceKey(Category, "AccentFillSelectedTextBackgroundSubtle", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey AccentFillSenaryColorKey => _AccentFillSenaryColorKey ??= new ThemeResourceKey(Category, "AccentFillSenary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey AccentFillSenaryBrushKey => _AccentFillSenaryBrushKey ??= new ThemeResourceKey(Category, "AccentFillSenary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey AccentFillTertiaryColorKey => _AccentFillTertiaryColorKey ??= new ThemeResourceKey(Category, "AccentFillTertiary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey AccentFillTertiaryBrushKey => _AccentFillTertiaryBrushKey ??= new ThemeResourceKey(Category, "AccentFillTertiary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey AccentTextFillDisabledColorKey => _AccentTextFillDisabledColorKey ??= new ThemeResourceKey(Category, "AccentTextFillDisabled", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey AccentTextFillDisabledBrushKey => _AccentTextFillDisabledBrushKey ??= new ThemeResourceKey(Category, "AccentTextFillDisabled", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey AccentTextFillPrimaryColorKey => _AccentTextFillPrimaryColorKey ??= new ThemeResourceKey(Category, "AccentTextFillPrimary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey AccentTextFillPrimaryBrushKey => _AccentTextFillPrimaryBrushKey ??= new ThemeResourceKey(Category, "AccentTextFillPrimary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey AccentTextFillSecondaryColorKey => _AccentTextFillSecondaryColorKey ??= new ThemeResourceKey(Category, "AccentTextFillSecondary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey AccentTextFillSecondaryBrushKey => _AccentTextFillSecondaryBrushKey ??= new ThemeResourceKey(Category, "AccentTextFillSecondary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey AccentTextFillTertiaryColorKey => _AccentTextFillTertiaryColorKey ??= new ThemeResourceKey(Category, "AccentTextFillTertiary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey AccentTextFillTertiaryBrushKey => _AccentTextFillTertiaryBrushKey ??= new ThemeResourceKey(Category, "AccentTextFillTertiary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey CardBackgroundFillDefaultColorKey => _CardBackgroundFillDefaultColorKey ??= new ThemeResourceKey(Category, "CardBackgroundFillDefault", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey CardBackgroundFillDefaultBrushKey => _CardBackgroundFillDefaultBrushKey ??= new ThemeResourceKey(Category, "CardBackgroundFillDefault", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey CardBackgroundFillSecondaryColorKey => _CardBackgroundFillSecondaryColorKey ??= new ThemeResourceKey(Category, "CardBackgroundFillSecondary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey CardBackgroundFillSecondaryBrushKey => _CardBackgroundFillSecondaryBrushKey ??= new ThemeResourceKey(Category, "CardBackgroundFillSecondary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey CardBackgroundFillTertiaryColorKey => _CardBackgroundFillTertiaryColorKey ??= new ThemeResourceKey(Category, "CardBackgroundFillTertiary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey CardBackgroundFillTertiaryBrushKey => _CardBackgroundFillTertiaryBrushKey ??= new ThemeResourceKey(Category, "CardBackgroundFillTertiary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey CardStrokeDefaultColorKey => _CardStrokeDefaultColorKey ??= new ThemeResourceKey(Category, "CardStrokeDefault", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey CardStrokeDefaultBrushKey => _CardStrokeDefaultBrushKey ??= new ThemeResourceKey(Category, "CardStrokeDefault", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey CardStrokeDefaultSolidColorKey => _CardStrokeDefaultSolidColorKey ??= new ThemeResourceKey(Category, "CardStrokeDefaultSolid", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey CardStrokeDefaultSolidBrushKey => _CardStrokeDefaultSolidBrushKey ??= new ThemeResourceKey(Category, "CardStrokeDefaultSolid", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey CardStrokeDefaultSolidAltColorKey => _CardStrokeDefaultSolidAltColorKey ??= new ThemeResourceKey(Category, "CardStrokeDefaultSolidAlt", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey CardStrokeDefaultSolidAltBrushKey => _CardStrokeDefaultSolidAltBrushKey ??= new ThemeResourceKey(Category, "CardStrokeDefaultSolidAlt", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlAltFillDisabledColorKey => _ControlAltFillDisabledColorKey ??= new ThemeResourceKey(Category, "ControlAltFillDisabled", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlAltFillDisabledBrushKey => _ControlAltFillDisabledBrushKey ??= new ThemeResourceKey(Category, "ControlAltFillDisabled", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlAltFillQuaternaryColorKey => _ControlAltFillQuaternaryColorKey ??= new ThemeResourceKey(Category, "ControlAltFillQuaternary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlAltFillQuaternaryBrushKey => _ControlAltFillQuaternaryBrushKey ??= new ThemeResourceKey(Category, "ControlAltFillQuaternary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlAltFillSecondaryColorKey => _ControlAltFillSecondaryColorKey ??= new ThemeResourceKey(Category, "ControlAltFillSecondary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlAltFillSecondaryBrushKey => _ControlAltFillSecondaryBrushKey ??= new ThemeResourceKey(Category, "ControlAltFillSecondary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlAltFillTertiaryColorKey => _ControlAltFillTertiaryColorKey ??= new ThemeResourceKey(Category, "ControlAltFillTertiary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlAltFillTertiaryBrushKey => _ControlAltFillTertiaryBrushKey ??= new ThemeResourceKey(Category, "ControlAltFillTertiary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlAltFillTransparentColorKey => _ControlAltFillTransparentColorKey ??= new ThemeResourceKey(Category, "ControlAltFillTransparent", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlAltFillTransparentBrushKey => _ControlAltFillTransparentBrushKey ??= new ThemeResourceKey(Category, "ControlAltFillTransparent", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlFillActiveInputColorKey => _ControlFillActiveInputColorKey ??= new ThemeResourceKey(Category, "ControlFillActiveInput", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlFillActiveInputBrushKey => _ControlFillActiveInputBrushKey ??= new ThemeResourceKey(Category, "ControlFillActiveInput", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlFillDefaultColorKey => _ControlFillDefaultColorKey ??= new ThemeResourceKey(Category, "ControlFillDefault", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlFillDefaultBrushKey => _ControlFillDefaultBrushKey ??= new ThemeResourceKey(Category, "ControlFillDefault", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlFillDisabledColorKey => _ControlFillDisabledColorKey ??= new ThemeResourceKey(Category, "ControlFillDisabled", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlFillDisabledBrushKey => _ControlFillDisabledBrushKey ??= new ThemeResourceKey(Category, "ControlFillDisabled", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlFillQuaternaryColorKey => _ControlFillQuaternaryColorKey ??= new ThemeResourceKey(Category, "ControlFillQuaternary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlFillQuaternaryBrushKey => _ControlFillQuaternaryBrushKey ??= new ThemeResourceKey(Category, "ControlFillQuaternary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlFillSecondaryColorKey => _ControlFillSecondaryColorKey ??= new ThemeResourceKey(Category, "ControlFillSecondary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlFillSecondaryBrushKey => _ControlFillSecondaryBrushKey ??= new ThemeResourceKey(Category, "ControlFillSecondary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlFillTertiaryColorKey => _ControlFillTertiaryColorKey ??= new ThemeResourceKey(Category, "ControlFillTertiary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlFillTertiaryBrushKey => _ControlFillTertiaryBrushKey ??= new ThemeResourceKey(Category, "ControlFillTertiary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlFillTransparentColorKey => _ControlFillTransparentColorKey ??= new ThemeResourceKey(Category, "ControlFillTransparent", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlFillTransparentBrushKey => _ControlFillTransparentBrushKey ??= new ThemeResourceKey(Category, "ControlFillTransparent", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlOnImageFillDefaultColorKey => _ControlOnImageFillDefaultColorKey ??= new ThemeResourceKey(Category, "ControlOnImageFillDefault", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlOnImageFillDefaultBrushKey => _ControlOnImageFillDefaultBrushKey ??= new ThemeResourceKey(Category, "ControlOnImageFillDefault", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlOnImageFillDisabledColorKey => _ControlOnImageFillDisabledColorKey ??= new ThemeResourceKey(Category, "ControlOnImageFillDisabled", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlOnImageFillDisabledBrushKey => _ControlOnImageFillDisabledBrushKey ??= new ThemeResourceKey(Category, "ControlOnImageFillDisabled", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlOnImageFillSecondaryColorKey => _ControlOnImageFillSecondaryColorKey ??= new ThemeResourceKey(Category, "ControlOnImageFillSecondary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlOnImageFillSecondaryBrushKey => _ControlOnImageFillSecondaryBrushKey ??= new ThemeResourceKey(Category, "ControlOnImageFillSecondary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlOnImageFillTertiaryColorKey => _ControlOnImageFillTertiaryColorKey ??= new ThemeResourceKey(Category, "ControlOnImageFillTertiary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlOnImageFillTertiaryBrushKey => _ControlOnImageFillTertiaryBrushKey ??= new ThemeResourceKey(Category, "ControlOnImageFillTertiary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlSolidFillDefaultColorKey => _ControlSolidFillDefaultColorKey ??= new ThemeResourceKey(Category, "ControlSolidFillDefault", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlSolidFillDefaultBrushKey => _ControlSolidFillDefaultBrushKey ??= new ThemeResourceKey(Category, "ControlSolidFillDefault", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlStrokeDefaultColorKey => _ControlStrokeDefaultColorKey ??= new ThemeResourceKey(Category, "ControlStrokeDefault", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlStrokeDefaultBrushKey => _ControlStrokeDefaultBrushKey ??= new ThemeResourceKey(Category, "ControlStrokeDefault", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlStrokeForStrongFillWhenOnImageColorKey => _ControlStrokeForStrongFillWhenOnImageColorKey ??= new ThemeResourceKey(Category, "ControlStrokeForStrongFillWhenOnImage", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlStrokeForStrongFillWhenOnImageBrushKey => _ControlStrokeForStrongFillWhenOnImageBrushKey ??= new ThemeResourceKey(Category, "ControlStrokeForStrongFillWhenOnImage", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlStrokeOnAccentDefaultColorKey => _ControlStrokeOnAccentDefaultColorKey ??= new ThemeResourceKey(Category, "ControlStrokeOnAccentDefault", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlStrokeOnAccentDefaultBrushKey => _ControlStrokeOnAccentDefaultBrushKey ??= new ThemeResourceKey(Category, "ControlStrokeOnAccentDefault", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlStrokeOnAccentDisabledColorKey => _ControlStrokeOnAccentDisabledColorKey ??= new ThemeResourceKey(Category, "ControlStrokeOnAccentDisabled", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlStrokeOnAccentDisabledBrushKey => _ControlStrokeOnAccentDisabledBrushKey ??= new ThemeResourceKey(Category, "ControlStrokeOnAccentDisabled", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlStrokeOnAccentSecondaryColorKey => _ControlStrokeOnAccentSecondaryColorKey ??= new ThemeResourceKey(Category, "ControlStrokeOnAccentSecondary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlStrokeOnAccentSecondaryBrushKey => _ControlStrokeOnAccentSecondaryBrushKey ??= new ThemeResourceKey(Category, "ControlStrokeOnAccentSecondary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlStrokeOnAccentTertiaryColorKey => _ControlStrokeOnAccentTertiaryColorKey ??= new ThemeResourceKey(Category, "ControlStrokeOnAccentTertiary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlStrokeOnAccentTertiaryBrushKey => _ControlStrokeOnAccentTertiaryBrushKey ??= new ThemeResourceKey(Category, "ControlStrokeOnAccentTertiary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlStrokeSecondaryColorKey => _ControlStrokeSecondaryColorKey ??= new ThemeResourceKey(Category, "ControlStrokeSecondary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlStrokeSecondaryBrushKey => _ControlStrokeSecondaryBrushKey ??= new ThemeResourceKey(Category, "ControlStrokeSecondary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlStrokeTransparentColorKey => _ControlStrokeTransparentColorKey ??= new ThemeResourceKey(Category, "ControlStrokeTransparent", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlStrokeTransparentBrushKey => _ControlStrokeTransparentBrushKey ??= new ThemeResourceKey(Category, "ControlStrokeTransparent", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlStrongFillDefaultColorKey => _ControlStrongFillDefaultColorKey ??= new ThemeResourceKey(Category, "ControlStrongFillDefault", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlStrongFillDefaultBrushKey => _ControlStrongFillDefaultBrushKey ??= new ThemeResourceKey(Category, "ControlStrongFillDefault", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlStrongFillDisabledColorKey => _ControlStrongFillDisabledColorKey ??= new ThemeResourceKey(Category, "ControlStrongFillDisabled", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlStrongFillDisabledBrushKey => _ControlStrongFillDisabledBrushKey ??= new ThemeResourceKey(Category, "ControlStrongFillDisabled", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlStrongStrokeDefaultColorKey => _ControlStrongStrokeDefaultColorKey ??= new ThemeResourceKey(Category, "ControlStrongStrokeDefault", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlStrongStrokeDefaultBrushKey => _ControlStrongStrokeDefaultBrushKey ??= new ThemeResourceKey(Category, "ControlStrongStrokeDefault", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ControlStrongStrokeDisabledColorKey => _ControlStrongStrokeDisabledColorKey ??= new ThemeResourceKey(Category, "ControlStrongStrokeDisabled", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ControlStrongStrokeDisabledBrushKey => _ControlStrongStrokeDisabledBrushKey ??= new ThemeResourceKey(Category, "ControlStrongStrokeDisabled", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey DividerStrokeDefaultColorKey => _DividerStrokeDefaultColorKey ??= new ThemeResourceKey(Category, "DividerStrokeDefault", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey DividerStrokeDefaultBrushKey => _DividerStrokeDefaultBrushKey ??= new ThemeResourceKey(Category, "DividerStrokeDefault", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey FocusStrokeInnerColorKey => _FocusStrokeInnerColorKey ??= new ThemeResourceKey(Category, "FocusStrokeInner", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey FocusStrokeInnerBrushKey => _FocusStrokeInnerBrushKey ??= new ThemeResourceKey(Category, "FocusStrokeInner", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey FocusStrokeOuterColorKey => _FocusStrokeOuterColorKey ??= new ThemeResourceKey(Category, "FocusStrokeOuter", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey FocusStrokeOuterBrushKey => _FocusStrokeOuterBrushKey ??= new ThemeResourceKey(Category, "FocusStrokeOuter", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey HyperlinkFillDisabledColorKey => _HyperlinkFillDisabledColorKey ??= new ThemeResourceKey(Category, "HyperlinkFillDisabled", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey HyperlinkFillDisabledBrushKey => _HyperlinkFillDisabledBrushKey ??= new ThemeResourceKey(Category, "HyperlinkFillDisabled", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey HyperlinkFillPrimaryColorKey => _HyperlinkFillPrimaryColorKey ??= new ThemeResourceKey(Category, "HyperlinkFillPrimary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey HyperlinkFillPrimaryBrushKey => _HyperlinkFillPrimaryBrushKey ??= new ThemeResourceKey(Category, "HyperlinkFillPrimary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey HyperlinkFillSecondaryColorKey => _HyperlinkFillSecondaryColorKey ??= new ThemeResourceKey(Category, "HyperlinkFillSecondary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey HyperlinkFillSecondaryBrushKey => _HyperlinkFillSecondaryBrushKey ??= new ThemeResourceKey(Category, "HyperlinkFillSecondary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey HyperlinkFillTertiaryColorKey => _HyperlinkFillTertiaryColorKey ??= new ThemeResourceKey(Category, "HyperlinkFillTertiary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey HyperlinkFillTertiaryBrushKey => _HyperlinkFillTertiaryBrushKey ??= new ThemeResourceKey(Category, "HyperlinkFillTertiary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey LayerFillAltColorKey => _LayerFillAltColorKey ??= new ThemeResourceKey(Category, "LayerFillAlt", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey LayerFillAltBrushKey => _LayerFillAltBrushKey ??= new ThemeResourceKey(Category, "LayerFillAlt", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey LayerFillDefaultColorKey => _LayerFillDefaultColorKey ??= new ThemeResourceKey(Category, "LayerFillDefault", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey LayerFillDefaultBrushKey => _LayerFillDefaultBrushKey ??= new ThemeResourceKey(Category, "LayerFillDefault", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey ShadowFlyoutColorKey => _ShadowFlyoutColorKey ??= new ThemeResourceKey(Category, "ShadowFlyout", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey ShadowFlyoutBrushKey => _ShadowFlyoutBrushKey ??= new ThemeResourceKey(Category, "ShadowFlyout", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SmokeFillDefaultColorKey => _SmokeFillDefaultColorKey ??= new ThemeResourceKey(Category, "SmokeFillDefault", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SmokeFillDefaultBrushKey => _SmokeFillDefaultBrushKey ??= new ThemeResourceKey(Category, "SmokeFillDefault", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SmokeFillInverseColorKey => _SmokeFillInverseColorKey ??= new ThemeResourceKey(Category, "SmokeFillInverse", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SmokeFillInverseBrushKey => _SmokeFillInverseBrushKey ??= new ThemeResourceKey(Category, "SmokeFillInverse", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SolidBackgroundFillBaseColorKey => _SolidBackgroundFillBaseColorKey ??= new ThemeResourceKey(Category, "SolidBackgroundFillBase", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SolidBackgroundFillBaseBrushKey => _SolidBackgroundFillBaseBrushKey ??= new ThemeResourceKey(Category, "SolidBackgroundFillBase", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SolidBackgroundFillBaseAltColorKey => _SolidBackgroundFillBaseAltColorKey ??= new ThemeResourceKey(Category, "SolidBackgroundFillBaseAlt", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SolidBackgroundFillBaseAltBrushKey => _SolidBackgroundFillBaseAltBrushKey ??= new ThemeResourceKey(Category, "SolidBackgroundFillBaseAlt", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SolidBackgroundFillQuaternaryColorKey => _SolidBackgroundFillQuaternaryColorKey ??= new ThemeResourceKey(Category, "SolidBackgroundFillQuaternary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SolidBackgroundFillQuaternaryBrushKey => _SolidBackgroundFillQuaternaryBrushKey ??= new ThemeResourceKey(Category, "SolidBackgroundFillQuaternary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SolidBackgroundFillQuinaryColorKey => _SolidBackgroundFillQuinaryColorKey ??= new ThemeResourceKey(Category, "SolidBackgroundFillQuinary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SolidBackgroundFillQuinaryBrushKey => _SolidBackgroundFillQuinaryBrushKey ??= new ThemeResourceKey(Category, "SolidBackgroundFillQuinary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SolidBackgroundFillSecondaryColorKey => _SolidBackgroundFillSecondaryColorKey ??= new ThemeResourceKey(Category, "SolidBackgroundFillSecondary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SolidBackgroundFillSecondaryBrushKey => _SolidBackgroundFillSecondaryBrushKey ??= new ThemeResourceKey(Category, "SolidBackgroundFillSecondary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SolidBackgroundFillSenaryColorKey => _SolidBackgroundFillSenaryColorKey ??= new ThemeResourceKey(Category, "SolidBackgroundFillSenary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SolidBackgroundFillSenaryBrushKey => _SolidBackgroundFillSenaryBrushKey ??= new ThemeResourceKey(Category, "SolidBackgroundFillSenary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SolidBackgroundFillTertiaryColorKey => _SolidBackgroundFillTertiaryColorKey ??= new ThemeResourceKey(Category, "SolidBackgroundFillTertiary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SolidBackgroundFillTertiaryBrushKey => _SolidBackgroundFillTertiaryBrushKey ??= new ThemeResourceKey(Category, "SolidBackgroundFillTertiary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SubtleFillDisabledColorKey => _SubtleFillDisabledColorKey ??= new ThemeResourceKey(Category, "SubtleFillDisabled", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SubtleFillDisabledBrushKey => _SubtleFillDisabledBrushKey ??= new ThemeResourceKey(Category, "SubtleFillDisabled", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SubtleFillSecondaryColorKey => _SubtleFillSecondaryColorKey ??= new ThemeResourceKey(Category, "SubtleFillSecondary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SubtleFillSecondaryBrushKey => _SubtleFillSecondaryBrushKey ??= new ThemeResourceKey(Category, "SubtleFillSecondary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SubtleFillTertiaryColorKey => _SubtleFillTertiaryColorKey ??= new ThemeResourceKey(Category, "SubtleFillTertiary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SubtleFillTertiaryBrushKey => _SubtleFillTertiaryBrushKey ??= new ThemeResourceKey(Category, "SubtleFillTertiary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SubtleFillTransparentColorKey => _SubtleFillTransparentColorKey ??= new ThemeResourceKey(Category, "SubtleFillTransparent", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SubtleFillTransparentBrushKey => _SubtleFillTransparentBrushKey ??= new ThemeResourceKey(Category, "SubtleFillTransparent", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SurfaceBackgroundFillDefaultColorKey => _SurfaceBackgroundFillDefaultColorKey ??= new ThemeResourceKey(Category, "SurfaceBackgroundFillDefault", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SurfaceBackgroundFillDefaultBrushKey => _SurfaceBackgroundFillDefaultBrushKey ??= new ThemeResourceKey(Category, "SurfaceBackgroundFillDefault", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SurfaceStrokeDefaultColorKey => _SurfaceStrokeDefaultColorKey ??= new ThemeResourceKey(Category, "SurfaceStrokeDefault", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SurfaceStrokeDefaultBrushKey => _SurfaceStrokeDefaultBrushKey ??= new ThemeResourceKey(Category, "SurfaceStrokeDefault", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SurfaceStrokeFlyoutColorKey => _SurfaceStrokeFlyoutColorKey ??= new ThemeResourceKey(Category, "SurfaceStrokeFlyout", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SurfaceStrokeFlyoutBrushKey => _SurfaceStrokeFlyoutBrushKey ??= new ThemeResourceKey(Category, "SurfaceStrokeFlyout", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SystemFillAttentionColorKey => _SystemFillAttentionColorKey ??= new ThemeResourceKey(Category, "SystemFillAttention", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SystemFillAttentionBrushKey => _SystemFillAttentionBrushKey ??= new ThemeResourceKey(Category, "SystemFillAttention", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SystemFillAttentionBackgroundColorKey => _SystemFillAttentionBackgroundColorKey ??= new ThemeResourceKey(Category, "SystemFillAttentionBackground", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SystemFillAttentionBackgroundBrushKey => _SystemFillAttentionBackgroundBrushKey ??= new ThemeResourceKey(Category, "SystemFillAttentionBackground", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SystemFillCautionColorKey => _SystemFillCautionColorKey ??= new ThemeResourceKey(Category, "SystemFillCaution", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SystemFillCautionBrushKey => _SystemFillCautionBrushKey ??= new ThemeResourceKey(Category, "SystemFillCaution", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SystemFillCautionBackgroundColorKey => _SystemFillCautionBackgroundColorKey ??= new ThemeResourceKey(Category, "SystemFillCautionBackground", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SystemFillCautionBackgroundBrushKey => _SystemFillCautionBackgroundBrushKey ??= new ThemeResourceKey(Category, "SystemFillCautionBackground", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SystemFillCriticalColorKey => _SystemFillCriticalColorKey ??= new ThemeResourceKey(Category, "SystemFillCritical", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SystemFillCriticalBrushKey => _SystemFillCriticalBrushKey ??= new ThemeResourceKey(Category, "SystemFillCritical", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SystemFillCriticalBackgroundColorKey => _SystemFillCriticalBackgroundColorKey ??= new ThemeResourceKey(Category, "SystemFillCriticalBackground", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SystemFillCriticalBackgroundBrushKey => _SystemFillCriticalBackgroundBrushKey ??= new ThemeResourceKey(Category, "SystemFillCriticalBackground", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SystemFillNeutralColorKey => _SystemFillNeutralColorKey ??= new ThemeResourceKey(Category, "SystemFillNeutral", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SystemFillNeutralBrushKey => _SystemFillNeutralBrushKey ??= new ThemeResourceKey(Category, "SystemFillNeutral", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SystemFillNeutralBackgroundColorKey => _SystemFillNeutralBackgroundColorKey ??= new ThemeResourceKey(Category, "SystemFillNeutralBackground", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SystemFillNeutralBackgroundBrushKey => _SystemFillNeutralBackgroundBrushKey ??= new ThemeResourceKey(Category, "SystemFillNeutralBackground", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SystemFillSolidAttentionBackgroundColorKey => _SystemFillSolidAttentionBackgroundColorKey ??= new ThemeResourceKey(Category, "SystemFillSolidAttentionBackground", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SystemFillSolidAttentionBackgroundBrushKey => _SystemFillSolidAttentionBackgroundBrushKey ??= new ThemeResourceKey(Category, "SystemFillSolidAttentionBackground", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SystemFillSolidNeutralColorKey => _SystemFillSolidNeutralColorKey ??= new ThemeResourceKey(Category, "SystemFillSolidNeutral", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SystemFillSolidNeutralBrushKey => _SystemFillSolidNeutralBrushKey ??= new ThemeResourceKey(Category, "SystemFillSolidNeutral", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SystemFillSolidNeutralBackgroundColorKey => _SystemFillSolidNeutralBackgroundColorKey ??= new ThemeResourceKey(Category, "SystemFillSolidNeutralBackground", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SystemFillSolidNeutralBackgroundBrushKey => _SystemFillSolidNeutralBackgroundBrushKey ??= new ThemeResourceKey(Category, "SystemFillSolidNeutralBackground", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SystemFillSuccessColorKey => _SystemFillSuccessColorKey ??= new ThemeResourceKey(Category, "SystemFillSuccess", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SystemFillSuccessBrushKey => _SystemFillSuccessBrushKey ??= new ThemeResourceKey(Category, "SystemFillSuccess", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey SystemFillSuccessBackgroundColorKey => _SystemFillSuccessBackgroundColorKey ??= new ThemeResourceKey(Category, "SystemFillSuccessBackground", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey SystemFillSuccessBackgroundBrushKey => _SystemFillSuccessBackgroundBrushKey ??= new ThemeResourceKey(Category, "SystemFillSuccessBackground", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey TextFillDisabledColorKey => _TextFillDisabledColorKey ??= new ThemeResourceKey(Category, "TextFillDisabled", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey TextFillDisabledBrushKey => _TextFillDisabledBrushKey ??= new ThemeResourceKey(Category, "TextFillDisabled", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey TextFillPrimaryColorKey => _TextFillPrimaryColorKey ??= new ThemeResourceKey(Category, "TextFillPrimary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey TextFillPrimaryBrushKey => _TextFillPrimaryBrushKey ??= new ThemeResourceKey(Category, "TextFillPrimary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey TextFillSecondaryColorKey => _TextFillSecondaryColorKey ??= new ThemeResourceKey(Category, "TextFillSecondary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey TextFillSecondaryBrushKey => _TextFillSecondaryBrushKey ??= new ThemeResourceKey(Category, "TextFillSecondary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey TextFillTertiaryColorKey => _TextFillTertiaryColorKey ??= new ThemeResourceKey(Category, "TextFillTertiary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey TextFillTertiaryBrushKey => _TextFillTertiaryBrushKey ??= new ThemeResourceKey(Category, "TextFillTertiary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey TextOnAccentFillDisabledColorKey => _TextOnAccentFillDisabledColorKey ??= new ThemeResourceKey(Category, "TextOnAccentFillDisabled", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey TextOnAccentFillDisabledBrushKey => _TextOnAccentFillDisabledBrushKey ??= new ThemeResourceKey(Category, "TextOnAccentFillDisabled", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey TextOnAccentFillPrimaryColorKey => _TextOnAccentFillPrimaryColorKey ??= new ThemeResourceKey(Category, "TextOnAccentFillPrimary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey TextOnAccentFillPrimaryBrushKey => _TextOnAccentFillPrimaryBrushKey ??= new ThemeResourceKey(Category, "TextOnAccentFillPrimary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey TextOnAccentFillSecondaryColorKey => _TextOnAccentFillSecondaryColorKey ??= new ThemeResourceKey(Category, "TextOnAccentFillSecondary", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey TextOnAccentFillSecondaryBrushKey => _TextOnAccentFillSecondaryBrushKey ??= new ThemeResourceKey(Category, "TextOnAccentFillSecondary", ThemeResourceKeyType.BackgroundBrush);

    public static ThemeResourceKey TextOnAccentFillSelectedTextColorKey => _TextOnAccentFillSelectedTextColorKey ??= new ThemeResourceKey(Category, "TextOnAccentFillSelectedText", ThemeResourceKeyType.BackgroundColor);

    public static ThemeResourceKey TextOnAccentFillSelectedTextBrushKey => _TextOnAccentFillSelectedTextBrushKey ??= new ThemeResourceKey(Category, "TextOnAccentFillSelectedText", ThemeResourceKeyType.BackgroundBrush);
}
#nullable restore