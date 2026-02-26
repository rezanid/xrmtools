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
        FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(Boxes.Box<TProperty>(defaultValue), propertyChanged, coerceValue);
        return Property.RegisterAttachedCore<TOwner, TProperty>(name, metadata);
    }

    private static DependencyProperty RegisterAttachedCore<TOwner, TProperty>(
      string name,
      FrameworkPropertyMetadata metadata)
    {
        return DependencyProperty.RegisterAttached(name, typeof(TProperty), typeof(TOwner), (PropertyMetadata)metadata);
    }

    public static DependencyProperty Register<TOwner, TProperty>(
      string name,
      TProperty? defaultValue = default,
      PropertyChangedCallback? propertyChanged = null,
      CoerceValueCallback? coerceValue = null)
    {
        FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(Boxes.Box<TProperty>(defaultValue), propertyChanged, coerceValue);
        return Property.RegisterCore<TOwner, TProperty>(name, metadata);
    }

    public static DependencyProperty RegisterMeasure<TOwner, TProperty>(
      string name,
      TProperty? defaultValue = default,
      PropertyChangedCallback? propertyChanged = null,
      CoerceValueCallback? coerceValue = null)
    {
        FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(Boxes.Box<TProperty>(defaultValue), FrameworkPropertyMetadataOptions.AffectsMeasure, propertyChanged, coerceValue);
        return Property.RegisterCore<TOwner, TProperty>(name, metadata);
    }

    public static DependencyProperty RegisterArrange<TOwner, TProperty>(
      string name,
      TProperty? defaultValue = default,
      PropertyChangedCallback? propertyChanged = null,
      CoerceValueCallback? coerceValue = null)
    {
        FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(Boxes.Box<TProperty>(defaultValue), FrameworkPropertyMetadataOptions.AffectsArrange, propertyChanged, coerceValue);
        return Property.RegisterCore<TOwner, TProperty>(name, metadata);
    }

    public static DependencyProperty RegisterFull<TOwner, TProperty>(
      string name,
      TProperty? defaultValue = default,
      PropertyChangedCallback? propertyChanged = null,
      CoerceValueCallback? coerceValue = null)
    {
        FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(Boxes.Box<TProperty>(defaultValue), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender, propertyChanged, coerceValue);
        return Property.RegisterCore<TOwner, TProperty>(name, metadata);
    }

    private static DependencyProperty RegisterCore<TOwner, TProperty>(
      string name,
      FrameworkPropertyMetadata metadata)
    {
        return DependencyProperty.Register(name, typeof(TProperty), typeof(TOwner), (PropertyMetadata)metadata);
    }

    public static DependencyPropertyKey RegisterReadOnly<TOwner, TProperty>(
      string name,
      TProperty? defaultValue = default,
      PropertyChangedCallback? propertyChanged = null,
      CoerceValueCallback? coerceValue = null)
    {
        FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(Boxes.Box<TProperty>(defaultValue), propertyChanged, coerceValue);
        return Property.RegisterReadOnlyCore<TOwner, TProperty>(name, metadata);
    }

    public static DependencyPropertyKey RegisterReadOnlyFull<TOwner, TProperty>(
      string name,
      TProperty? defaultValue = default,
      PropertyChangedCallback? propertyChanged = null,
      CoerceValueCallback? coerceValue = null)
    {
        FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(Boxes.Box<TProperty>(defaultValue), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender, propertyChanged, coerceValue);
        return Property.RegisterReadOnlyCore<TOwner, TProperty>(name, metadata);
    }

    private static DependencyPropertyKey RegisterReadOnlyCore<TOwner, TProperty>(
      string name,
      FrameworkPropertyMetadata metadata)
    {
        return DependencyProperty.RegisterReadOnly(name, typeof(TProperty), typeof(TOwner), (PropertyMetadata)metadata);
    }
}

