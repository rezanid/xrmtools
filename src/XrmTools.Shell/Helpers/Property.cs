#nullable enable
namespace XrmTools.Shell.Helpers;

using System.Windows;

public static class Property
{
    public static DependencyProperty RegisterAttached<TOwner, TProperty>(
      string name,
      TProperty? defaultValue = default,
      PropertyChangedCallback? propertyChanged = null,
      CoerceValueCallback? coerceValue = null)
    {
        FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(Boxes.Box(defaultValue), propertyChanged, coerceValue);
        return RegisterAttachedCore<TOwner, TProperty>(name, metadata);
    }

    private static DependencyProperty RegisterAttachedCore<TOwner, TProperty>(
      string name,
      FrameworkPropertyMetadata metadata)
    {
        return DependencyProperty.RegisterAttached(name, typeof(TProperty), typeof(TOwner), metadata);
    }

    public static DependencyProperty Register<TOwner, TProperty>(
      string name,
      TProperty? defaultValue = default,
      PropertyChangedCallback? propertyChanged = null,
      CoerceValueCallback? coerceValue = null)
    {
        FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(Boxes.Box(defaultValue), propertyChanged, coerceValue);
        return RegisterCore<TOwner, TProperty>(name, metadata);
    }

    public static DependencyProperty RegisterMeasure<TOwner, TProperty>(
      string name,
      TProperty? defaultValue = default,
      PropertyChangedCallback? propertyChanged = null,
      CoerceValueCallback? coerceValue = null)
    {
        FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(Boxes.Box(defaultValue), FrameworkPropertyMetadataOptions.AffectsMeasure, propertyChanged, coerceValue);
        return RegisterCore<TOwner, TProperty>(name, metadata);
    }

    public static DependencyProperty RegisterArrange<TOwner, TProperty>(
      string name,
      TProperty? defaultValue = default,
      PropertyChangedCallback? propertyChanged = null,
      CoerceValueCallback? coerceValue = null)
    {
        FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(Boxes.Box(defaultValue), FrameworkPropertyMetadataOptions.AffectsArrange, propertyChanged, coerceValue);
        return RegisterCore<TOwner, TProperty>(name, metadata);
    }

    public static DependencyProperty RegisterFull<TOwner, TProperty>(
      string name,
      TProperty? defaultValue = default,
      PropertyChangedCallback? propertyChanged = null,
      CoerceValueCallback? coerceValue = null)
    {
        FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(Boxes.Box(defaultValue), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender, propertyChanged, coerceValue);
        return RegisterCore<TOwner, TProperty>(name, metadata);
    }

    private static DependencyProperty RegisterCore<TOwner, TProperty>(
      string name,
      FrameworkPropertyMetadata metadata)
    {
        return DependencyProperty.Register(name, typeof(TProperty), typeof(TOwner), metadata);
    }

    public static DependencyPropertyKey RegisterReadOnly<TOwner, TProperty>(
      string name,
      TProperty? defaultValue = default,
      PropertyChangedCallback? propertyChanged = null,
      CoerceValueCallback? coerceValue = null)
    {
        FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(Boxes.Box(defaultValue), propertyChanged, coerceValue);
        return RegisterReadOnlyCore<TOwner, TProperty>(name, metadata);
    }

    public static DependencyPropertyKey RegisterReadOnlyFull<TOwner, TProperty>(
      string name,
      TProperty? defaultValue = default,
      PropertyChangedCallback? propertyChanged = null,
      CoerceValueCallback? coerceValue = null)
    {
        FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(Boxes.Box(defaultValue), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender, propertyChanged, coerceValue);
        return RegisterReadOnlyCore<TOwner, TProperty>(name, metadata);
    }

    private static DependencyPropertyKey RegisterReadOnlyCore<TOwner, TProperty>(
      string name,
      FrameworkPropertyMetadata metadata)
    {
        return DependencyProperty.RegisterReadOnly(name, typeof(TProperty), typeof(TOwner), metadata);
    }
}

