#nullable enable
namespace XrmTools.Options;

using System;
using System.ComponentModel;
using System.Globalization;

class EnumDescriptionConverter(Type enumType) : EnumConverter(enumType)
{
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destType) => destType == typeof(string);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                     object value, Type destType)
    {
        if (value == null) return string.Empty;
        var descriptionAttr = (DescriptionAttribute)Attribute.GetCustomAttribute(
            EnumType.GetField(Enum.GetName(EnumType, value)), 
            typeof(DescriptionAttribute));
        return descriptionAttr != null
            ? descriptionAttr.Description 
            : base.ConvertTo(context, culture, value, destType);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType) => srcType == typeof(string);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        foreach (var fieldInfo in EnumType.GetFields())
        {
            var descriptionAttr = 
                (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
            if ((descriptionAttr != null) && ((string)value == descriptionAttr.Description))
            {
                return Enum.Parse(EnumType, fieldInfo.Name);
            }
        }
        return base.ConvertFrom(context, culture, value);
    }
}
#nullable restore