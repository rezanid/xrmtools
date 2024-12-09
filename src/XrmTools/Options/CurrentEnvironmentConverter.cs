#nullable enable
namespace XrmTools.Options;
using System;
using System.ComponentModel;
using System.Linq;

public class CurrentEnvironmentConverter : TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        // Indicate that the converter provides a list of standard values.
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        // Indicate that the list of values is exclusive (dropdown style).
        return true;
    }

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        // Get the instance of the GeneralOptions object
        if (context.Instance is GeneralOptions options && options.Environments != null)
        {
            // Return the environments as standard values
            return new StandardValuesCollection(options.Environments);
        }

        return base.GetStandardValues(context);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }
        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value is string environmentString && context.Instance is GeneralOptions options)
        {
            if (string.IsNullOrEmpty(environmentString) || (environmentString == "Empty"))
            {
                return DataverseEnvironment.Empty;
            }
            // Match the string format, extract the URL, and find the environment
            var extractedUrl = ExtractUrlFromDisplayString(environmentString);
            return options.Environments.FirstOrDefault(e => e.Url == extractedUrl);
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is DataverseEnvironment environment)
        {
            // Format the output to: "Name (Url)"
            string name = environment.Name ?? "no name";
            return $"{name} ({environment.Url})";
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    private string ExtractUrlFromDisplayString(string displayString)
    {
        // Assuming the format is "Name (Url)", extract the URL between parentheses
        var startIndex = displayString.IndexOf('(') + 1;
        var endIndex = displayString.IndexOf(')');
        return displayString[startIndex..endIndex];
    }
}
#nullable restore